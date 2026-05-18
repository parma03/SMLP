/*
* Sistem Manajemen Loyalitas Pelanggan (SMLP)
* Copyright (c) 2025-2026
*/

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Psmp.Infrastructure.Persistence.Common.Extensions;
using Psmp.Infrastructure.Persistence.Common.Models;
using Psmp.InfrastructureCurrentUser;
using Psmp.InfrastructureDateAndTime;
using Psmp.InfrastructureDomainEvent;
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
    private const string SoftDeleted = nameof(SoftDeleted);

    protected ICurrentUserService _currentUser = default!;
    protected IDateAndTimeService _dateTime = default!;
    protected IDomainEventService _domainEvent = default!;

    #region Common Entities

    public DbSet<Audit> Audits => Set<Audit>();
    public DbSet<Document> Documents => Set<Document>();

    #endregion Common Entities

    #region Master Data Entities

    public DbSet<User> Users => Set<User>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<PointRule> PointRules => Set<PointRule>();
    public DbSet<TierRule> TierRules => Set<TierRule>();
    public DbSet<Reward> Rewards => Set<Reward>();
    public DbSet<Product> Products => Set<Product>();

    #endregion Master Data Entities

    #region Transaction Entities

    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<TransactionItem> TransactionItems => Set<TransactionItem>();
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
        foreach (var entry in ChangeTracker.Entries<ICreatable>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedBy = _currentUser.Username;
                    entry.Entity.Created = _dateTime.Now;
                    break;
            }
        }

        foreach (var entry in ChangeTracker.Entries<IModifiable>())
        {
            switch (entry.State)
            {
                case EntityState.Modified:
                    entry.Entity.ModifiedBy = _currentUser.Username;
                    entry.Entity.Modified = _dateTime.Now;
                    break;
            }
        }

        var auditEntries = OnBeforeSaveChanges(handler.GetType().Name);
        var result = await base.SaveChangesAsync(true, cancellationToken);

        await OnAfterSaveChanges(auditEntries);
        await DispatchEvents();

        return result;
    }

    private List<AuditEntry> OnBeforeSaveChanges(string actionName)
    {
        ChangeTracker.DetectChanges();

        var auditEntries = new List<AuditEntry>();

        foreach (var entry in ChangeTracker.Entries())
        {
            var isAuditableEntity = entry.Entity.GetType().IsAuditableEntity();

            if (!isAuditableEntity || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                continue;

            var entityType = Model.FindEntityType(entry.Entity.GetType());

            if (entityType is null)
                throw new InvalidOperationException($"Cannot find Entity Type for {entry.Entity.GetType()}");

            var tableName = $"{entityType.GetSchema()}.{entityType.GetTableName()}";
            var entityName = entry.Entity.GetType().Name;

            var auditEntry = new AuditEntry
            {
                TableName = tableName,
                EntityName = entityName,
                ClientApplicationId = _currentUser.ClientId,
                CreatedBy = _currentUser.Username,
                FromIpAddress = _currentUser.IpAddress,
                FromGeolocation = _currentUser.Geolocation,
                ActionName = actionName,
                ActionType = entry.State.ToString()
            };

            auditEntries.Add(auditEntry);

            foreach (var property in entry.Properties)
            {
                if (property.IsTemporary)
                {
                    auditEntry.TemporaryProperties.Add(property);
                    continue;
                }

                var propertyName = property.Metadata.Name;
                var originalValue = property.OriginalValue;
                var currentValue = property.CurrentValue;

                if (property.Metadata.IsPrimaryKey())
                {
                    if (currentValue is null)
                        continue;

                    if (propertyName == nameof(IHasKey.Id))
                    {
                        var entityId = currentValue.ToString();
                        if (!string.IsNullOrWhiteSpace(entityId))
                            auditEntry.EntityId = new Guid(entityId);
                    }
                    continue;
                }

                switch (entry.State)
                {
                    case EntityState.Added:
                        if (propertyName is nameof(IDeletable.IsDeleted) or nameof(IModifiable.Modified) or nameof(IModifiable.ModifiedBy))
                            continue;
                        auditEntry.NewValues[propertyName] = currentValue;
                        break;

                    case EntityState.Deleted:
                        auditEntry.OldValues[propertyName] = originalValue;
                        break;

                    case EntityState.Modified:
                        if (property.IsModified)
                        {
                            auditEntry.OldValues[propertyName] = originalValue;
                            auditEntry.NewValues[propertyName] = currentValue;

                            if (propertyName is nameof(IDeletable.IsDeleted) && (bool)currentValue! is true)
                                auditEntry.ActionType = SoftDeleted;
                        }
                        break;
                }
            }
        }

        foreach (var auditEntry in auditEntries.Where(x => !x.HasTemporaryProperties))
            Audits.Add(auditEntry.ToAudit());

        return auditEntries.Where(x => x.HasTemporaryProperties).ToList();
    }

    private Task OnAfterSaveChanges(List<AuditEntry> auditEntries)
    {
        if (auditEntries is null || auditEntries.Count == 0)
            return Task.CompletedTask;

        foreach (var auditEntry in auditEntries)
        {
            foreach (var property in auditEntry.TemporaryProperties)
            {
                var currentValue = property.CurrentValue;
                var propertyName = property.Metadata.Name;

                if (currentValue is null)
                    continue;

                if (property.Metadata.IsPrimaryKey())
                {
                    if (propertyName == nameof(IHasKey.Id))
                    {
                        var entityId = currentValue.ToString();
                        if (!string.IsNullOrWhiteSpace(entityId))
                            auditEntry.EntityId = new Guid(entityId);
                    }
                }
                else
                {
                    auditEntry.NewValues[propertyName] = currentValue;
                }
            }

            Audits.Add(auditEntry.ToAudit());
        }

        return SaveChangesAsync();
    }

    private async Task DispatchEvents()
    {
        while (true)
        {
            var domainEventEntity = ChangeTracker.Entries<IHasDomainEvent>()
                .Select(x => x.Entity.DomainEvents)
                .SelectMany(x => x)
                .Where(domainEvent => !domainEvent.IsPublished)
                .FirstOrDefault();

            if (domainEventEntity is null)
                break;

            domainEventEntity.IsPublished = true;
            await _domainEvent.Publish(domainEventEntity);
        }
    }
}
