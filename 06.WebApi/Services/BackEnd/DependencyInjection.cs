namespace Psmp.WebApi.Services.BackEnd;

public static class DependencyInjection
{
    public static IServiceCollection AddBackEndService(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<BackEndOptions>(configuration.GetSection(BackEndOptions.SectionKey));

        return services;
    }

    public static IApplicationBuilder UseBackEndService(this WebApplication app, IConfiguration configuration)
    {
        var backEndOptions = configuration.GetSection(BackEndOptions.SectionKey).Get<BackEndOptions>();

        if (!string.IsNullOrWhiteSpace(backEndOptions.BasePath))
        {
            app.UsePathBase(backEndOptions.BasePath);
        }

        return app;
    }
}
