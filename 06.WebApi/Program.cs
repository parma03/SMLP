using System.Text.Json.Serialization;
using Smlp.WebApi.Common.Middleware;
using FluentValidation.AspNetCore;
using Smlp.Application;
using Smlp.Infrastructure;
using Smlp.Infrastructure.Logging;
using Smlp.Infrastructure.Authentication;
using Smlp.Infrastructure.Authentication.Jwt;
using Smlp.Infrastructure.Monitoring;
using Smlp.Shared;
using Smlp.Shared.Localization;
using Smlp.Shared.Common.Constants;
using Smlp.WebApi.Common.Filters.ApiException;
using Smlp.WebApi.Common.ModelBindings;
using Smlp.WebApi.Hubs;
using Smlp.WebApi.Services;
using Smlp.WebApi.Services.Documentation;
using Smlp.Application.Services.PushNotification;
using Smlp.WebApi.Services.PushNotification;

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

// SignalR untuk Push Notification
builder.Services.AddSignalR();
builder.Services.AddScoped<Smlp.Application.Services.PushNotification.IPushNotificationService, SignalRPushNotificationService>();

// JWT Authentication
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionKey));
builder.Services.AddJwtAuthentication(builder.Configuration);

// Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly",       policy => policy.RequireRole("Admin"));
    options.AddPolicy("OwnerOnly",       policy => policy.RequireRole("Owner"));
    options.AddPolicy("KasirOnly",       policy => policy.RequireRole("Kasir"));
    options.AddPolicy("AdminOrOwner",    policy => policy.RequireRole("Admin", "Owner"));
    options.AddPolicy("AdminOrKasir",    policy => policy.RequireRole("Admin", "Kasir"));
    options.AddPolicy("AllRoles",        policy => policy.RequireRole("Admin", "Owner", "Kasir"));
});

builder.Services.AddControllers(options =>
{
    options.Filters.Add(new ApiExceptionFilterAttribute());
    options.ModelBinderProviders.Insert(0, new JsonModelBinderProvider());
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// CORS untuk Blazor frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("SmlpCors", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? new[] { "https://localhost:44992", "https://localhost:5001" };
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
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
    app.UseDeveloperExceptionPage();

app.UseBackEndService(builder.Configuration);
app.UseCors("SmlpCors");
app.UseWebSockets(); // diperlukan untuk SignalR WebSocket transport
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseAuditAction(); // Audit action tracking middleware
app.UseInfrastructure(builder.Configuration);
app.UseDocumentationService(builder.Configuration);
app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notification");
app.MapMonitoringEndpoints(builder.Configuration); // Prometheus scrape endpoint (jika diaktifkan)
app.Run();
