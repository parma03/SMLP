using AutoMapper;
using AutoMapper.QueryableExtensions;
using Smlp.Application.Common.Models;
using Smlp.Shared.Common.Responses;

namespace Smlp.Application.Common.Extensions;

public static class PaginatedListExtensions
{
    public static PaginatedListResponse<T> ToPaginatedListResponse<T>(this PaginatedList<T> source)
    {
        var mapperConfig = new MapperConfiguration(configuration => configuration.CreateMap<PaginatedList<T>, PaginatedListResponse<T>>());

        var mapper = new Mapper(mapperConfig);

        return mapper.Map<PaginatedList<T>, PaginatedListResponse<T>>(source);
    }

    public static PaginatedListResponse<TDestination> AsPaginatedListResponse<TDestination, TSource>(this PaginatedList<TSource> source, IConfigurationProvider configurationProvider)
    {
        var projectedItems = source.Items.AsQueryable().ProjectTo<TDestination>(configurationProvider).ToList();

        return new PaginatedListResponse<TDestination>
        {
            Items = projectedItems,
            TotalCount = source.TotalCount
        };
    }
}
