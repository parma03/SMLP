using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using Psmp.Bsui.Common.Constants;
using Psmp.Bsui.Services;
using Psmp.Bsui.Services.Authentication;
using Psmp.Bsui.Services.Authorization;
using Psmp.Bsui.Services.Logging;
using Psmp.Bsui.Services.Security;
using Psmp.Client;
using Psmp.Shared.Common.Constants;
using Psmp.Shared.Localization;
using PSMP.Shared;
using Syncfusion.Blazor;

Console.WriteLine($"Starting {CommonValueFor.EntryAssemblySimpleName}...");

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseLoggingService();
builder.Services.AddHttpContextAccessor();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddServerSideBlazor().AddHubOptions(o => { o.MaximumReceiveMessageSize = 2147483647; });
builder.Services.AddSyncfusionBlazor();
builder.Services.AddShared(builder.Configuration);
builder.Services.AddClient(builder.Configuration);
builder.Services.AddBsui(builder.Configuration);

StaticWebAssetsLoader.UseStaticWebAssets(builder.Environment, builder.Configuration);
builder.Services.AddApplicationInsightsTelemetry();

// temporary custom culture
var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Running {AssemblyName}", CommonValueFor.EntryAssemblySimpleName);

CultureConfigurator.Configure(new LocalizationSettings
{
    CurrencySymbol = "Rp",
    CurrencyDecimalDigits = 0,
    NumberGroupSeparator = ".",
    NumberDecimalSeparator = ","
}, logger);

if (!app.Environment.IsProduction())
{
    app.UseDeveloperExceptionPage();
}

app.UseBsui(app.Configuration);
app.UseSecurityService(app.Environment);
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthenticationService(app.Configuration);
app.UseAuthorizationService(app.Configuration);
app.MapBlazorHub();
app.MapFallbackToPage(CommonRouteFor.Host);
app.Run();
