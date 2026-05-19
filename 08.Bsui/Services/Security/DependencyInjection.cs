namespace Psmp.Bsui.Services.Security;

public static class DependencyInjection
{
    public static IServiceCollection AddSecurityService(this IServiceCollection services)
    {
        // https://cheatsheetseries.owasp.org/cheatsheets/HTTP_Strict_Transport_Security_Cheat_Sheet.html#examples
        services.AddHsts(options =>
        {
            options.Preload = true;
            options.IncludeSubDomains = true;
            options.MaxAge = TimeSpan.FromDays(30);
        });

        return services;
    }

    public static IApplicationBuilder UseSecurityService(this IApplicationBuilder app, IWebHostEnvironment environment)
    {
        if (environment.IsProduction())
        {
            // https://cheatsheetseries.owasp.org/cheatsheets/Cross_Site_Scripting_Prevention_Cheat_Sheet.html#x-xss-protection-header
            // https://jonathancrozier.com/blog/stepping-up-the-security-of-asp-net-core-web-apps-with-security-headers

            // https://cheatsheetseries.owasp.org/cheatsheets/HTTP_Strict_Transport_Security_Cheat_Sheet.html#examples
            app.UseHsts();

            // https://geekflare.com/http-header-implementation/#anchor-x-content-type-options
            // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/X-Content-Type-Options
            // https://geekflare.com/http-header-implementation/#anchor-x-frame-options
            // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/X-Frame-Options
            app.Use((context, next) =>
            {
                context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
                context.Response.Headers.Add("X-Frame-Options", "SAMEORIGIN");
                return next();
            });

            // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/X-XSS-Protection
            // https://owasp.org/www-community/Security_Headers
            app.Use(async (context, next) =>
            {
                context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
                await next();
            });

            // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Referrer-Policy
            app.Use(async (context, next) =>
            {
                context.Response.Headers.Add("Referrer-Policy", "no-referrer");
                await next();
            });

            // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Feature-Policy
            //app.Use(async (context, next) =>
            //{
            //    context.Response.Headers.Add("Permissions-Policy", "accelerometer=(), camera=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), payment=(), usb=()");
            //    await next();
            //});

            // https://www.w3.org/TR/CSP3/
            // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Content-Security-Policy
            // https://docs.microsoft.com/en-us/aspnet/core/blazor/security/content-security-policy?view=aspnetcore-5.0
            // https://docs.nwebsec.com/en/latest/nwebsec/Configuring-csp.html
            //app.UseCsp(options => options
            //    .BlockAllMixedContent()
            //    .DefaultSources(s => s.Self())
            //    .FontSources(s => s.Self().CustomSources("https://fonts.gstatic.com"))
            //    .FrameAncestors(s => s.None())
            //    .FrameSources(s => s.None())
            //    .UpgradeInsecureRequests()
            //);
        }

        return app;
    }
}
