/*
* Sistem Manajemen Loyalitas Pelanggan (SMLP)
* Copyright (c) 2025-2026
*/

namespace Smlp.Shared.Services.Authorization.Constants;

public static class Permissions
{
    #region Essential Permissions

    public const string Smlp_Audit_Index = "smlp.audit.index";
    public const string Smlp_Audit_View = "smlp.audit.view";
    public const string Smlp_HealthCheck_View = "smlp.healthcheck.view";

    #endregion Essential Permissions

    #region Document Permissions

    public const string Smlp_Document_Create = "smlp.document.create";
    public const string Smlp_Document_Delete = "smlp.document.delete";
    public const string Smlp_Document_Index = "smlp.document.index";
    public const string Smlp_Document_Download = "smlp.document.download";

    #endregion Document Permissions

    #region Customer Permissions

    public const string Smlp_Customer_Index = "smlp.customer.index";
    public const string Smlp_Customer_View = "smlp.customer.view";
    public const string Smlp_Customer_Create = "smlp.customer.create";
    public const string Smlp_Customer_Edit = "smlp.customer.edit";
    public const string Smlp_Customer_Delete = "smlp.customer.delete";

    #endregion Customer Permissions

    #region Transaction Permissions

    public const string Smlp_Transaction_Index = "smlp.transaction.index";
    public const string Smlp_Transaction_View = "smlp.transaction.view";
    public const string Smlp_Transaction_Create = "smlp.transaction.create";
    public const string Smlp_Transaction_Edit = "smlp.transaction.edit";

    #endregion Transaction Permissions

    #region Reward Permissions

    public const string Smlp_Reward_Index = "smlp.reward.index";
    public const string Smlp_Reward_View = "smlp.reward.view";
    public const string Smlp_Reward_Create = "smlp.reward.create";
    public const string Smlp_Reward_Edit = "smlp.reward.edit";
    public const string Smlp_Reward_Delete = "smlp.reward.delete";

    #endregion Reward Permissions

    #region Redeem Permissions

    public const string Smlp_Redeem_Index = "smlp.redeem.index";
    public const string Smlp_Redeem_Create = "smlp.redeem.create";
    public const string Smlp_Redeem_Verify = "smlp.redeem.verify";

    #endregion Redeem Permissions

    #region Analytics Permissions

    public const string Smlp_Analytics_View = "smlp.analytics.view";
    public const string Smlp_CustomerInsights_View = "smlp.customerinsights.view";
    public const string Smlp_LoyaltyAnalysis_View = "smlp.loyaltyanalysis.view";
    public const string Smlp_Report_View = "smlp.report.view";
    public const string Smlp_Report_Export = "smlp.report.export";

    #endregion Analytics Permissions

    #region Master Data Permissions

    public const string Smlp_PointRule_Index = "smlp.pointrule.index";
    public const string Smlp_PointRule_Edit = "smlp.pointrule.edit";
    public const string Smlp_TierRule_Index = "smlp.tierrule.index";
    public const string Smlp_TierRule_Edit = "smlp.tierrule.edit";
    public const string Smlp_Product_Index = "smlp.product.index";
    public const string Smlp_Product_Create = "smlp.product.create";
    public const string Smlp_Product_Edit = "smlp.product.edit";

    #endregion Master Data Permissions

    #region User Management

    public const string Smlp_User_Index = "smlp.user.index";
    public const string Smlp_User_Create = "smlp.user.create";
    public const string Smlp_User_Edit = "smlp.user.edit";
    public const string Smlp_User_Delete = "smlp.user.delete";

    #endregion User Management

    public static readonly string[] All =
    [
        Smlp_Audit_Index, Smlp_Audit_View, Smlp_HealthCheck_View,
        Smlp_Document_Create, Smlp_Document_Delete, Smlp_Document_Index, Smlp_Document_Download,
        Smlp_Customer_Index, Smlp_Customer_View, Smlp_Customer_Create, Smlp_Customer_Edit, Smlp_Customer_Delete,
        Smlp_Transaction_Index, Smlp_Transaction_View, Smlp_Transaction_Create, Smlp_Transaction_Edit,
        Smlp_Reward_Index, Smlp_Reward_View, Smlp_Reward_Create, Smlp_Reward_Edit, Smlp_Reward_Delete,
        Smlp_Redeem_Index, Smlp_Redeem_Create, Smlp_Redeem_Verify,
        Smlp_Analytics_View, Smlp_CustomerInsights_View, Smlp_LoyaltyAnalysis_View, Smlp_Report_View, Smlp_Report_Export,
        Smlp_PointRule_Index, Smlp_PointRule_Edit, Smlp_TierRule_Index, Smlp_TierRule_Edit,
        Smlp_Product_Index, Smlp_Product_Create, Smlp_Product_Edit,
        Smlp_User_Index, Smlp_User_Create, Smlp_User_Edit, Smlp_User_Delete,
    ];

    /// <summary>Permissions for Admin role</summary>
    public static readonly string[] Admin =
    [
        Smlp_Audit_Index, Smlp_Audit_View, Smlp_HealthCheck_View,
        Smlp_Document_Create, Smlp_Document_Delete, Smlp_Document_Index, Smlp_Document_Download,
        Smlp_Customer_Index, Smlp_Customer_View, Smlp_Customer_Create, Smlp_Customer_Edit, Smlp_Customer_Delete,
        Smlp_Transaction_Index, Smlp_Transaction_View, Smlp_Transaction_Create, Smlp_Transaction_Edit,
        Smlp_Reward_Index, Smlp_Reward_View, Smlp_Reward_Create, Smlp_Reward_Edit, Smlp_Reward_Delete,
        Smlp_Redeem_Index, Smlp_Redeem_Create, Smlp_Redeem_Verify,
        Smlp_Analytics_View, Smlp_CustomerInsights_View, Smlp_LoyaltyAnalysis_View, Smlp_Report_View, Smlp_Report_Export,
        Smlp_PointRule_Index, Smlp_PointRule_Edit, Smlp_TierRule_Index, Smlp_TierRule_Edit,
        Smlp_Product_Index, Smlp_Product_Create, Smlp_Product_Edit,
        Smlp_User_Index, Smlp_User_Create, Smlp_User_Edit, Smlp_User_Delete,
    ];

    /// <summary>Permissions for Owner role</summary>
    public static readonly string[] Owner =
    [
        Smlp_Audit_Index, Smlp_Audit_View, Smlp_HealthCheck_View,
        Smlp_Customer_Index, Smlp_Customer_View,
        Smlp_Transaction_Index, Smlp_Transaction_View,
        Smlp_Reward_Index, Smlp_Reward_View, Smlp_Reward_Create, Smlp_Reward_Edit,
        Smlp_Redeem_Index,
        Smlp_Analytics_View, Smlp_CustomerInsights_View, Smlp_LoyaltyAnalysis_View, Smlp_Report_View, Smlp_Report_Export,
        Smlp_PointRule_Index, Smlp_PointRule_Edit, Smlp_TierRule_Index, Smlp_TierRule_Edit,
        Smlp_Product_Index,
        Smlp_User_Index, Smlp_User_Create, Smlp_User_Edit,
    ];

    /// <summary>Permissions for Kasir role</summary>
    public static readonly string[] Kasir =
    [
        Smlp_Customer_Index, Smlp_Customer_View, Smlp_Customer_Create,
        Smlp_Transaction_Index, Smlp_Transaction_View, Smlp_Transaction_Create,
        Smlp_Reward_Index, Smlp_Reward_View,
        Smlp_Redeem_Index, Smlp_Redeem_Create, Smlp_Redeem_Verify,
    ];

    /// <summary>Permissions for Customer role (self-service)</summary>
    public static readonly string[] Customer =
    [
        Smlp_Reward_Index, Smlp_Reward_View,
        Smlp_Redeem_Index, Smlp_Redeem_Create,
    ];
}
