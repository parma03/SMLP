/*
* Sistem Manajemen Loyalitas Pelanggan (SMLP)
*/

namespace Smlp.Bsui.Common.Constants;

public static class CommonRouteFor
{
    // Razor Pages
    public const string Host = "/_Host";

    // Common Pages
    public const string Index = "/";
    public const string Dashboard = "/";
    public const string About = "/About";
    public const string Profile = "/Profile";
    public const string MySession = "/MySession";
    public const string Login = "/Login";

    // CRM - Customer Management
    public const string Customers = "/Customers";
    public const string CustomerCreate = "/CustomerCreate";
    public static string CustomerDetail(Guid id) => $"/Customers/{id}";
    public static string CustomerEdit(Guid id) => $"/Customers/{id}/Edit";

    // Transactions
    public const string Transactions = "/Transactions";
    public const string TransactionCreate = "/Transactions/Create";
    public static string TransactionDetail(Guid id) => $"/Transactions/{id}";

    // Point Adjustments
    public const string PointAdjustments = "/PointAdjustments";

    // Rewards & Redeem
    public const string Rewards = "/Rewards";
    public const string RewardCreate = "/Rewards/Create";
    public static string RewardEdit(Guid id) => $"/Rewards/{id}/Edit";
    public const string RedeemHistories = "/RedeemHistories";

    // Analytics & Reports
    public const string Analytics = "/Analytics";
    public const string CustomerInsights = "/CustomerInsights";
    public const string LoyaltyAnalysis = "/LoyaltyAnalysis";
    public const string Reports = "/Reports";

    // Master Data
    public const string PointRules = "/PointRules";
    public const string TierRules = "/TierRules";
    public const string Products = "/Products";
    public const string ProductCreate = "/Products/Create";

    // Administrator
    public const string SiteAdministrator = "/SiteAdministrator";
    public const string Users = "/Users";
    public const string UserCreate = "/Users/Create";
    public static string UserEdit(Guid id) => $"/Users/{id}/Edit";

    // Audit
    public const string Audits = "/Audits";
    public static string AuditDetails(Guid id) => $"/Audits/Details/{id}";

    // Documents
    public const string Documents = "/Documents";

    // Error Pages
    public static class Errors
    {
        public const string NotFound = "/Errors/NotFound";
        public const string Forbidden = "/Errors/Forbidden";
        public const string ServiceUnavailable = "/Errors/ServiceUnavailable";
        public const string NotAvailableDocument = "/Errors/NotAvailableDocument";
        public const string CannotReachAuthenticationProvider = "/Errors/CannotReachAuthenticationProvider";
        public const string CannotReachAuthorizationProvider = "/Errors/CannotReachAuthorizationProvider";
    }
}
