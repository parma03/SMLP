namespace Psmp.Bsui.Services.FrontEnd;

public static class DependencyInjection
{
    public static IServiceCollection AddFrontEndService(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<FrontEndOptions>(configuration.GetSection(FrontEndOptions.SectionKey));

        return services;
    }

    public static IApplicationBuilder UseFrontEndService(this IApplicationBuilder app, IConfiguration configuration)
    {
        var frontEndOptions = configuration.GetSection(FrontEndOptions.SectionKey).Get<FrontEndOptions>();

        if (!string.IsNullOrWhiteSpace(frontEndOptions.BasePath))
        {
            app.UsePathBase(frontEndOptions.BasePath);
        }

        return app;
    }
}
