/*
* Pertamina Scholarship Management Portal
* Copyright (c) 2024-2025 PT. Pertamina Persero
*/

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Smlp.Domain.Entities;
using Smlp.Domain.Entities.Master;

namespace Smlp.Application.Services.Persistence;

public interface ISmlpDbContext
{
    #region Essential Entities

    public DbSet<Audit> Audits { get; }
    public DbSet<Document> Documents { get; }

    #endregion Essential Entities

    #region Master Data Entities
    public DbSet<User> Users { get; }

    #endregion Master Data Entities

    #region Transaction Data Entities

    #endregion Transaction Data Entities

    public IDbContextTransaction BeginTransaction();

    public Task<int> SaveChangesAsync<THandler>(THandler handler, CancellationToken cancellationToken = default) where THandler : notnull;
}
