using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Smlp.Application.Services.Authorization;
using Smlp.Application.Services.Authentication;
using Smlp.Application.Services.Persistence.Seeders;
using Smlp.Application.Common.Behaviours;

namespace Smlp.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AuthenticationOptions>(configuration.GetSection(AuthenticationOptions.SectionKey));
        services.Configure<AuthorizationOptions>(configuration.GetSection(AuthorizationOptions.SectionKey));

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

        return services;
    }
}
