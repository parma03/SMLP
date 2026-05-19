using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Smlp.Application.Services.Persistence.Seeders;
using Smlp.Application.Common.Behaviours;

namespace Smlp.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(DependencyInjection).Assembly);
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddMediatR(typeof(DependencyInjection).Assembly);

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehaviour<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>));

        #region Non-MediatR Services

        services.AddTransient<UserSeeder>();

        #endregion Non-MediatR Services

        #region Background Job

        #endregion Background Job

        return services;
    }
}
