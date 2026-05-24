/*
* Sistem Manajemen Loyalitas Pelanggan (SMLP)
* Copyright (c) 2025-2026
*/

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Smlp.Application.Services.CurrentUser;
using Smlp.Application.Services.DateAndTime;
using Smlp.Application.Services.DomainEvent;
using Smlp.Application.Services.Persistence;
using Smlp.Domain.Entities;
using Smlp.Domain.Entities.Master;
using Smlp.Domain.Entities.Transaction;
using Smlp.Domain.Events;
using Smlp.Domain.Interfaces;
using Smlp.Infrastructure.Persistence.Common.Extensions;
using Smlp.Infrastructure.Persistence.Common.Models;

namespace Smlp.Infrastructure.Persistence;

public class SmlpDbContext : DbContext, ISmlpDbContext
{
    protected ICurrentUserService _currentUser = default!;
    protected IDateAndTimeService _dateTime = default!;
    protected IDomainEventService _domainEvent = default!;

    #region Common Entities

    public DbSet<Audit> Audits => Set<Audit>();
    public DbSet<Document> Documents => Set<Document>();

    #endregion Common Entities

    #region Master Data Entities

    public DbSet<User> Users => Set<User>();
    public DbSet<UserProfilePicture> UserProfilePictures => Set<UserProfilePicture>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<PointRule> PointRules => Set<PointRule>();
    public DbSet<TierRule> TierRules => Set<TierRule>();
    public DbSet<Reward> Rewards => Set<Reward>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<AppSetting> AppSettings => Set<AppSetting>();

    #endregion Master Data Entities

    #region Transaction Entities

    public DbSet<Transaksi> Transactions => Set<Transaksi>();
    public DbSet<TransactionItem> TransactionItems => Set<TransactionItem>();
    public DbSet<TransactionPayment> TransactionPayments => Set<TransactionPayment>();
    public DbSet<RedeemHistory> RedeemHistories => Set<RedeemHistory>();
    public DbSet<PointAdjustment> PointAdjustments => Set<PointAdjustment>();

    #endregion Transaction Entities

    public SmlpDbContext(
        DbContextOptions<SmlpDbContext> options,
        ICurrentUserService currentUser,
        IDateAndTimeService dateTime,
        IDomainEventService domainEvent) : base(options)
    {
        _currentUser = currentUser;
        _dateTime = dateTime;
        _domainEvent = domainEvent;
    }

    protected SmlpDbContext(DbContextOptions options) : base(options)
    {
    }

    public IDbContextTransaction BeginTransaction()
    {
        return Database.BeginTransaction();
    }

    public async Task<int> SaveChangesAsync<THandler>(THandler handler, CancellationToken cancellationToken = default) where THandler : notnull
    {
        await OnBeforeSavingAsync(handler, cancellationToken);
        var result = await base.SaveChangesAsync(cancellationToken);
        await OnAfterSavingAsync(cancellationToken);
        return result;
    }

    private Task OnBeforeSavingAsync<THandler>(THandler handler, CancellationToken cancellationToken) where THandler : notnull
    {
        var now = _dateTime?.Now ?? DateTimeOffset.UtcNow;
        var user = _currentUser?.Username ?? handler.ToString() ?? "System";
        var auditEntries = new List<AuditEntry>();

        foreach (var entry in ChangeTracker.Entries().ToList())
        {
            if (entry.Entity is IDeletable deletable && entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                deletable.IsDeleted = true;
                deletable.DeletedAt = now;
                deletable.DeletedBy = user;
            }

            if (entry.Entity is ICreatable creatable && entry.State == EntityState.Added)
            {
                creatable.CreatedAt = now;
                creatable.CreatedBy = user;
            }

            if (entry.Entity is IModifiable modifiable && entry.State == EntityState.Modified)
            {
                modifiable.ModifiedAt = now;
                modifiable.ModifiedBy = user;
            }

            if (entry.Entity is not Audit &&
                (entry.State == EntityState.Added || entry.State == EntityState.Modified || entry.State == EntityState.Deleted))
            {
                var auditEntry = new AuditEntry(entry)
                {
                    ClientApplicationId = handler?.ToString() ?? "System",
                    CreatedBy = user
                };
                auditEntries.Add(auditEntry);
            }
        }

        foreach (var auditEntry in auditEntries)
        {
            Audits.Add(auditEntry.ToAudit());
        }

        return Task.CompletedTask;
    }

    private async Task OnAfterSavingAsync(CancellationToken cancellationToken)
    {
        if (_domainEvent is null) return;

        var domainEvents = ChangeTracker
            .Entries<IHasDomainEvent>()
            .SelectMany(e => e.Entity.DomainEvents)
            .ToList();

        foreach (var domainEvent in domainEvents)
        {
            await _domainEvent.Publish(domainEvent, cancellationToken);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SmlpDbContext).Assembly);
        modelBuilder.SetSoftDeleteQueryFilter();
    }
}
