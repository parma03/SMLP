using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using Psmp.Infrastructure.AppInfo;
using Psmp.Shared.Common.Constants;
using Psmp.Shared.Common.Extensions;
using Psmp.WebApi.Services.BackEnd;
using Psmp.WebApi.Services.Documentation.Swagger.Filters;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace Psmp.WebApi.Services.Documentation.Swagger;

public static class DependencyInjection
{
    public static IServiceCollection AddSwaggerDocumentationService(this IServiceCollection services, IConfiguration configuration)
    {
        var appInfoOptions = configuration.GetSection(AppInfoOptions.SectionKey).Get<AppInfoOptions>();
        var swaggerDocumentationOptions = configuration.GetSection(SwaggerDocumentationOptions.SectionKey).Get<SwaggerDocumentationOptions>();
        var description = swaggerDocumentationOptions.Description;

        if (!string.IsNullOrWhiteSpace(swaggerDocumentationOptions.DescriptionMarkdownFile))
        {
            var markdownContent = File.ReadAllText(Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                nameof(Services),
                nameof(Documentation),
                swaggerDocumentationOptions.DescriptionMarkdownFile));

            description = markdownContent
                    .Replace("{EnvironmentName}", CommonValueFor.EnvironmentName)
                    .Replace("{OperatingSystem}", CommonValueFor.OperatingSystemDescription)
                    .Replace("{AspNetVersion}", CommonValueFor.EntryAssemblyFrameworkName)
                    .Replace("{SemanticVersion}", CommonValueFor.EntryAssemblyInformationalVersion)
                    .Replace("{AppAssemblyLastBuildDate}", CommonValueFor.EntryAssemblyLastBuild.ToCompleteDateTimeDisplayText());
        }

        var executingAssembly = Assembly.GetExecutingAssembly();
        var executingAssemblyLastBuild = File.GetLastWriteTime(executingAssembly.Location);

        services.AddSwaggerGen(setupAction =>
        {
            foreach (var apiVersion in swaggerDocumentationOptions.ApiVersions)
            {
                setupAction.SwaggerDoc(apiVersion, new OpenApiInfo
                {
                    Title = appInfoOptions.FullName,
                    Version = apiVersion,
                    Description = description,
                    TermsOfService = new Uri(swaggerDocumentationOptions.TermsOfServiceUrl),
                    Contact = new OpenApiContact
                    {
                        Name = swaggerDocumentationOptions.Contact.Name,
                        Email = swaggerDocumentationOptions.Contact.Email,
                        Url = new Uri(swaggerDocumentationOptions.Contact.Url)
                    },
                    License = new OpenApiLicense
                    {
                        Name = swaggerDocumentationOptions.License.Name,
                        Url = new Uri(swaggerDocumentationOptions.License.Url)
                    },
                    Extensions = new Dictionary<string, IOpenApiExtension>
                        {
                              {"x-logo", new OpenApiObject
                                {
                                   {"url", new OpenApiString(swaggerDocumentationOptions.Logo.Url)},
                                   { "altText", new OpenApiString(swaggerDocumentationOptions.Logo.Text)}
                                }
                              }
                        }
                });
            }

            setupAction.OperationFilter<OpenApiContentTypeOperationFilter>();
            setupAction.CustomOperationIds(d => (d.ActionDescriptor as ControllerActionDescriptor)?.ActionName);
            setupAction.CustomSchemaIds(DefaultSchemaIdSelector);

            setupAction.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
            {
                Description = $"JWT Authorization header using the {JwtBearerDefaults.AuthenticationScheme} scheme. Example: \"Authorization: Bearer {{token}}\"",
                Name = HttpHeaderName.Authorization,
                Type = SecuritySchemeType.Http,
                Scheme = JwtBearerDefaults.AuthenticationScheme,
                BearerFormat = JwtConstants.TokenType,
                In = ParameterLocation.Header
            });

            setupAction.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = JwtBearerDefaults.AuthenticationScheme
                        },
                        Scheme = JwtBearerDefaults.AuthenticationScheme,
                        Name = HttpHeaderName.Authorization,
                        In = ParameterLocation.Header,
                    },
                    new List<string>()
                }
            });
        });

        return services;
    }

    public static IApplicationBuilder UseSwaggerDocumentationService(this IApplicationBuilder app, IConfiguration configuration)
    {
        var appInfoOptions = configuration.GetSection(AppInfoOptions.SectionKey).Get<AppInfoOptions>();
        var backEndOptions = configuration.GetSection(BackEndOptions.SectionKey).Get<BackEndOptions>();
        var swaggerDocumentationOptions = configuration.GetSection(SwaggerDocumentationOptions.SectionKey).Get<SwaggerDocumentationOptions>();

        app.UseSwagger(c => c.PreSerializeFilters.Add((swagger, httpRequest) => swagger.Servers = new List<OpenApiServer> { new() { Url = $"{httpRequest.Scheme}://{httpRequest.Host.Value}{backEndOptions.BasePath}" } }));

        app.UseSwaggerUI(setupAction =>
        {
            foreach (var apiVersion in swaggerDocumentationOptions.ApiVersions)
            {
                var jsonEndpoint = string.Concat(backEndOptions.BasePath, swaggerDocumentationOptions.JsonEndpoint).Replace("$version$", apiVersion);

                setupAction.SwaggerEndpoint(jsonEndpoint, apiVersion);
            }

            setupAction.RoutePrefix = swaggerDocumentationOptions.SwaggerPrefix;
            setupAction.DocumentTitle = appInfoOptions.FullName;
            setupAction.DefaultModelExpandDepth(2);
            setupAction.DefaultModelRendering(ModelRendering.Model);
            setupAction.DefaultModelsExpandDepth(-1);
            setupAction.DisplayOperationId();
            setupAction.DisplayRequestDuration();
            setupAction.DocExpansion(DocExpansion.None);
            setupAction.EnableDeepLinking();
            setupAction.EnableFilter();
            setupAction.EnableValidator();
            setupAction.ShowExtensions();
        });

        return app;
    }

    private static string DefaultSchemaIdSelector(Type modelType)
    {
        if (!modelType.IsConstructedGenericType)
        {
            return modelType.Name;
        }

        var suffix = modelType.GetGenericArguments()
            .Select(DefaultSchemaIdSelector)
            .Aggregate((previous, current) => previous + current);

        return $"{modelType.Name.Split('`').First()}Of{suffix}";
    }
}
