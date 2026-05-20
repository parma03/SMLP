/*
* Sistem Manajemen Loyalitas Pelanggan (SMLP)
*/

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Smlp.Domain.Entities;
using Smlp.Domain.Entities.Master;
using Smlp.Domain.Entities.Transaction;

namespace Smlp.Application.Services.Persistence;

public interface ISmlpDbContext
{
    #region Common Entities

    public DbSet<Audit> Audits { get; }
    public DbSet<Document> Documents { get; }

    #endregion Common Entities

    #region Master Data Entities

    public DbSet<User> Users { get; }
    public DbSet<Customer> Customers { get; }
    public DbSet<PointRule> PointRules { get; }
    public DbSet<TierRule> TierRules { get; }
    public DbSet<Reward> Rewards { get; }
    public DbSet<Product> Products { get; }

    #endregion Master Data Entities

    #region Transaction Entities

    public DbSet<Transaksi> Transactions { get; }
    public DbSet<TransactionItem> TransactionItems { get; }
    public DbSet<RedeemHistory> RedeemHistories { get; }
    public DbSet<PointAdjustment> PointAdjustments { get; }

    #endregion Transaction Entities

    public IDbContextTransaction BeginTransaction();

    public Task<int> SaveChangesAsync<THandler>(THandler handler, CancellationToken cancellationToken = default) where THandler : notnull;
}