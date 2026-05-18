using System.Text.Json.Serialization;
using FluentValidation.AspNetCore;
using PSMP.Application;
using PSMP.Infrastructure;
using Psmp.Infrastructure.Logging;
using Psmp.Infrastructure.Persistence;
using PSMP.Shared;
using Psmp.Shared.Localization;
using Psmp.Shared.Common.Constants;
using Psmp.WebApi.Common.Filters.ApiException;
using Psmp.WebApi.Common.ModelBindings;
using Psmp.WebApi.Services;
using Psmp.WebApi.Services.BackEnd;
using Psmp.WebApi.Services.Documentation;
using Psmp.Infrastructure.BackgroundJob.Hangfire;

Console.WriteLine($"Starting {CommonValueFor.EntryAssemblySimpleName}...");

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseLoggingService();
builder.Services.AddShared(builder.Configuration);
builder.Services.AddApplication(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddWebApi(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.AddApiVersioning();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddControllers(options =>
{
    options.Filters.Add(new ApiExceptionFilterAttribute());
    options.ModelBinderProviders.Insert(0, new JsonModelBinderProvider());
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

var app = builder.Build();

using var scope = app.Services.CreateScope();
await scope.ServiceProvider.ApplyDatabaseMigrationAsync<Program>();
await scope.ServiceProvider.ApplyDatabaseSeedingAsync();
await scope.ServiceProvider.ApplyDataBackgroundJobsAsync();

var logger = app.Services.GetRequiredService<ILogger<Program>>();

CultureConfigurator.Configure(new LocalizationSettings
{
    CurrencySymbol = "Rp",
    CurrencyDecimalDigits = 0,
    NumberGroupSeparator = ".",
    NumberDecimalSeparator = ","
}, logger);

logger.LogInformation("Running {AssemblyName}", CommonValueFor.EntryAssemblySimpleName);

if (!app.Environment.IsProduction())
{
    app.UseDeveloperExceptionPage();
}

app.UseBackEndService(builder.Configuration);
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseInfrastructure(builder.Configuration);
app.UseDocumentationService(builder.Configuration);
app.MapControllers();
app.Run();
