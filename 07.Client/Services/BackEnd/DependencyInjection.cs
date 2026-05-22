/*
* Sistem Manajemen Loyalitas Pelanggan (SMLP)
* Copyright (c) 2025-2026
*/

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Smlp.Client.Services.BackEnd.Auth;
using Smlp.Client.Services.BackEnd.Masters;
using Smlp.Client.Services.BackEnd.Transactions;

namespace Smlp.Client.Services.BackEnd;

public static class DependencyInjection
{
    public static IServiceCollection AddBackEndService(this IServiceCollection services, IConfiguration configuration)
    {
        if (configuration != null)
        {
            services.Configure<BackEndOptions>(configuration.GetSection(BackEndOptions.SectionKey));
        }

        #region Essential Services

        services.AddTransient<AuthService>();
        services.AddTransient<AuditService>();
        services.AddTransient<UsersService>();

        #endregion Essential Services

        #region Business Services

        services.AddTransient<DocumentService>();
        services.AddTransient<EmailService>();

        #endregion Business Services

        #region Master Services

        services.AddTransient<CustomerService>();
        services.AddTransient<ProductService>();
        services.AddTransient<RewardService>();
        services.AddTransient<PointRuleService>();
        services.AddTransient<TierRuleService>();

        #endregion Master Services

        #region Transaction Services

        services.AddTransient<TransactionService>();
        services.AddTransient<RedeemService>();
        services.AddTransient<PointAdjustmentService>();

        #endregion Transaction Services

        #region Analytics & Reports

        services.AddTransient<AnalyticsService>();
        services.AddTransient<ReportService>();

        #endregion Analytics & Reports

        return services;
    }
}
