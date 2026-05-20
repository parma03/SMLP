/*
 * Sistem Manajemen Loyalitas Pelanggan (SMLP)
 * Copyright (c) 2025-2026
 */

using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using Smlp.Bsui.Common.Constants;
using Smlp.Bsui.Services;
using Smlp.Bsui.Services.Authentication;
using Smlp.Bsui.Services.Authorization;
using Smlp.Bsui.Services.Logging;
using Smlp.Bsui.Services.Security;
using Smlp.Client;
using Smlp.Shared.Common.Constants;
using Smlp.Shared.Localization;
using Smlp.Shared;

Console.WriteLine($"Starting {CommonValueFor.EntryAssemblySimpleName}...");

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseLoggingService();
builder.Services.AddHttpContextAccessor();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddServerSideBlazor().AddHubOptions(o => { o.MaximumReceiveMessageSize = 2147483647; });
builder.Services.AddShared(builder.Configuration);
builder.Services.AddClient(builder.Configuration);
builder.Services.AddBsui(builder.Configuration);

StaticWebAssetsLoader.UseStaticWebAssets(builder.Environment, builder.Configuration);

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
