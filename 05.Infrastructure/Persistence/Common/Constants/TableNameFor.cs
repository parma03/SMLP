/*
* Sistem Manajemen Loyalitas Pelanggan (SMLP)
*/

namespace Smlp.Infrastructure.Persistence.Common.Constants;

public static class TableNameFor
{
    public static readonly string EfMigrationsHistory = "SchemaVersions";

    // Common
    public static readonly string Audit = nameof(Audit);
    public static readonly string Document = nameof(Document);

    // Master
    public static readonly string User = nameof(User);
    public static readonly string UserProfilePicture = nameof(UserProfilePicture);
    public static readonly string Customer = nameof(Customer);
    public static readonly string PointRule = nameof(PointRule);
    public static readonly string TierRule = nameof(TierRule);
    public static readonly string Reward = nameof(Reward);
    public static readonly string Product = nameof(Product);
    public static readonly string ProductCategory = nameof(ProductCategory);
    public static readonly string AppSetting = nameof(AppSetting);

    // Transaction
    public static readonly string Transaction = nameof(Transaction);
    public static readonly string TransactionItem = nameof(TransactionItem);
    public static readonly string RedeemHistory = nameof(RedeemHistory);
    public static readonly string PointAdjustment = nameof(PointAdjustment);
}
