using System.Linq.Expressions;
using System.Reflection;
using AutoMapper;
using AutoMapper.Internal;
using AutoMapper.QueryableExtensions;
using Smlp.Application.Common.Models;
using Smlp.Domain.Abstracts;
using Smlp.Shared.Common.Enums;
using Smlp.Shared.Common.Extensions;

namespace Smlp.Application.Common.Extensions;

public static class IQueryableExtensions
{
    public static Task<PaginatedList<T>> ToPaginatedListAsync<T>(this IQueryable<T> queryable, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        return PaginatedList<T>.CreateAsync(queryable, pageNumber, pageSize, cancellationToken);
    }

    public static Task<List<T>> ProjectToListAsync<T>(this IQueryable queryable, IConfigurationProvider configuration)
    {
        return Task.Run(() => queryable.ProjectTo<T>(configuration).ToList());
    }

    public static IQueryable<TSource> ApplySearch<TSource>(this IQueryable<TSource> source, string? searchText, Type destinationType, IConfigurationProvider configurationProvider)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return source;
        }

        var keywords = searchText.Split(' ').ToList();
        keywords.Add(searchText);

        var typeMap = ((IGlobalConfiguration)configurationProvider).FindTypeMapFor(typeof(TSource), destinationType);

        if (typeMap is null)
        {
            throw new InvalidOperationException($"Unable to find TypeMap from {typeof(TSource).Name} to {destinationType.Name}");
        }

        var propertyMapNames = new List<string>();

        foreach (var propertyMap in typeMap.PropertyMaps)
        {
            if (propertyMap.CustomMapExpression is not null)
            {
                var bodyExpression = propertyMap.CustomMapExpression.Body.ToString();
                bodyExpression = bodyExpression.Replace($"{bodyExpression.Split(".")[0]}.", string.Empty);
                propertyMapNames.Add(bodyExpression);
            }
            else
            {
                propertyMapNames.Add(propertyMap.DestinationName);
            }
        }

        var parameter = Expression.Parameter(typeof(TSource), "x");

        Expression? expressionBody = null;

        foreach (var keyword in keywords)
        {
            expressionBody = expressionBody is null
                ? BuildSearchExpression(parameter, keyword, propertyMapNames.ToArray())
                : Expression.OrElse(expressionBody, BuildSearchExpression(parameter, keyword, propertyMapNames.ToArray()));
        }

        if (expressionBody is null)
        {
            return source;
        }

        var predicate = Expression.Lambda<Func<TSource, bool>>(expressionBody, parameter);

        return source.Where(predicate);
    }

    private static Expression BuildSearchExpression(Expression target, string keyword, string[] propertyNames)
    {
        Expression? result = null;
        Expression search = Expression.Property(Expression.Constant(new { keyword }), nameof(keyword));

        var properties = target.Type.GetExpressionProperties(propertyNames);

        foreach (var property in properties)
        {
            var prop = property.Value;

            Expression? condition = null;
            var propValue = target;

            foreach (var member in property.Key.Split('.'))
            {
                propValue = Expression.PropertyOrField(propValue, member);
            }

            if (prop.PropertyType == typeof(string))
            {
                var comparand = Expression.Call(propValue, nameof(string.ToLower), Type.EmptyTypes);
                condition = Expression.Call(comparand, nameof(string.Contains), Type.EmptyTypes, search);
            }
            else if (prop.PropertyType.IsEnum)
            {
                foreach (var enumValue in Enum.GetValues(prop.PropertyType))
                {
                    if (((Enum)enumValue).GetDescription().Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    {
                        Expression memberSearch = Expression.Property(Expression.Constant(new { enumValue }), nameof(enumValue));
                        condition = Expression.Call(propValue, nameof(int.Equals), Type.EmptyTypes, memberSearch);

                        if (condition is not null)
                        {
                            result = result is null ? condition : Expression.OrElse(result, condition);
                        }
                    }
                }
            }
            else if (prop.PropertyType == typeof(Guid) || prop.PropertyType == typeof(Guid?))
            {
                var comparand = Expression.Call(propValue, nameof(object.ToString), Type.EmptyTypes);
                condition = Expression.Call(comparand, nameof(string.Contains), Type.EmptyTypes, search);
            }
            else if (prop.PropertyType == typeof(int) || prop.PropertyType == typeof(long) || prop.PropertyType == typeof(double))
            {
                var comparand = Expression.Call(propValue, nameof(object.ToString), Type.EmptyTypes);
                condition = Expression.Call(comparand, nameof(string.Contains), Type.EmptyTypes, search);
            }
            else if (prop.PropertyType == typeof(DateTime) || prop.PropertyType == typeof(DateTimeOffset))
            {
                var comparand = Expression.Call(propValue, nameof(object.ToString), Type.EmptyTypes);
                condition = Expression.Call(comparand, nameof(string.Contains), Type.EmptyTypes, search);
            }
            else
            {
                var method = prop.PropertyType.GetMethods()
                    .Where(x => x.Name == nameof(object.ToString))
                    .FirstOrDefault(x => x.IsGenericMethod);

                if (method is not null)
                {
                    var covertToString = Expression.Call(propValue, nameof(object.ToString), Type.EmptyTypes);
                    var comparand = Expression.Call(covertToString, nameof(string.ToLower), Type.EmptyTypes);
                    condition = Expression.Call(comparand, nameof(string.Contains), Type.EmptyTypes, search);
                }
            }

            if (condition is not null)
            {
                result = result is null ? condition : Expression.OrElse(result, condition);
            }
        }

        if (result is null)
        {
            throw new NullReferenceException("Expression result is null");
        }
        else
        {
            return result;
        }
    }

    private static Dictionary<string, PropertyInfo> GetExpressionProperties(this Type type, string[] propertyNames)
    {
        var results = new Dictionary<string, PropertyInfo>();

        foreach (var propertyName in propertyNames)
        {
            PropertyInfo? typeProperty = null;
            var isFirst = true;

            foreach (var property in propertyName.Split("."))
            {
                if (isFirst)
                {
                    typeProperty = type.GetProperty(property);
                    isFirst = false;
                }
                else
                {
                    typeProperty = typeProperty?.PropertyType.GetProperty(property);
                }
            }

            if (typeProperty is not null && typeProperty.CanRead)
            {
                results.Add(propertyName, typeProperty);
            }
        }

        return results;
    }

    public static IOrderedQueryable<TSource> ApplyOrder<TSource>(
        this IQueryable<TSource> query,
        string? sortFieldName,
        SortOrder? sortOrder,
        Type destinationType,
        IConfigurationProvider configurationProvider,
        string defaultSortFieldName,
        SortOrder defaultSortOrder) where TSource : Entity
    {
        var sortBy = defaultSortOrder;

        if (sortOrder.HasValue)
        {
            sortBy = sortOrder.Value;
        }

        var defaultOrderedQueryable = query.GetOrderedQueryable(defaultSortFieldName, defaultSortOrder, destinationType, configurationProvider);

        if (sortFieldName is null)
        {
            return defaultOrderedQueryable;
        }

        var destinationProperty = destinationType.GetProperty(sortFieldName);

        if (destinationProperty is null)
        {
            throw new ArgumentException($"Property {sortFieldName} does not exist in {destinationType.Name}", nameof(sortFieldName));
        }

        var orderedQueryable = query.GetOrderedQueryable(destinationProperty.Name, sortBy, destinationType, configurationProvider);

        return orderedQueryable;
    }

    private static IOrderedQueryable<TSource> GetOrderedQueryable<TSource>(
        this IQueryable<TSource> query,
        string propertyName,
        SortOrder sortOrder,
        Type destinationType,
        IConfigurationProvider configurationProvider) where TSource : Entity
    {
        var typeMap = ((IGlobalConfiguration)configurationProvider).FindTypeMapFor(typeof(TSource), destinationType);

        if (typeMap is null)
        {
            throw new InvalidOperationException($"Unable to find TypeMap from {typeof(TSource).Name} to {destinationType.Name}");
        }

        var propertyMap = typeMap.PropertyMaps.SingleOrDefault(x => x.DestinationName.Equals(propertyName));

        if (propertyMap is null)
        {
            throw new InvalidOperationException($"Unable to find PropertyMap from {typeof(TSource).Name} to {destinationType.Name} for property {propertyName}");
        }

        var propertyMapName = string.Empty;

        if (propertyMap.CustomMapExpression is not null)
        {
            var bodyExpression = propertyMap.CustomMapExpression.Body.ToString();

            bodyExpression = bodyExpression.Replace($"{bodyExpression.Split(".")[0]}.", string.Empty);
            propertyMapName = bodyExpression;
        }
        else
        {
            propertyMapName = propertyMap.DestinationName;
        }

        var propertyParameter = Expression.Parameter(typeof(TSource), "x");
        Expression propertyExpression = propertyParameter;
        var propertyType = typeof(TSource);

        foreach (var propertyMember in propertyMapName.Split('.'))
        {
            var propertyInfo = propertyType.GetProperty(propertyMember);

            if (propertyInfo is null)
            {
                throw new InvalidOperationException($"Unable to get PropertyInfo: {propertyMember} from {propertyName}");
            }

            propertyExpression = Expression.Property(propertyExpression, propertyInfo);
            propertyType = propertyInfo.PropertyType;
        }

        var delegateType = typeof(Func<,>).MakeGenericType(typeof(TSource), propertyType);
        var lambda = Expression.Lambda(delegateType, propertyExpression, propertyParameter);

        var methodName = sortOrder switch
        {
            SortOrder.Desc => "OrderByDescending",
            _ => "OrderBy"
        };

        var result = typeof(Queryable).GetMethods().Single(
                method => method.Name == methodName
                        && method.IsGenericMethodDefinition
                        && method.GetGenericArguments().Length == 2
                        && method.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(TSource), propertyType)
                .Invoke(null, new object[] { query, lambda });

        if (result is null)
        {
            throw new InvalidOperationException($"Something went wrong with query {lambda}");
        }

        return (IOrderedQueryable<TSource>)result;
    }
}
