/*
* Sistem Manajemen Loyalitas Pelanggan (SMLP)
* Copyright (c) 2025-2026
*/

using Smlp.Domain.Entities;
using Smlp.Shared.Audits.Queries.GetAudits;
using Smlp.Shared.Common.Responses;

namespace Smlp.Application.Audits;

/// <summary>
/// Dedicated repository untuk operasi Audit, memisahkan concern dari ISmlpDbContext.
/// </summary>
public interface IAuditRepository
{
    /// <summary>
    /// Ambil daftar audit log dengan filter dan paginasi.
    /// </summary>
    Task<PaginatedListResponse<GetAuditsAudit>> GetAuditsAsync(GetAuditsRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ambil audit log tunggal berdasarkan ID.
    /// </summary>
    Task<Audit?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ambil daftar audit berdasarkan ID untuk keperluan export.
    /// </summary>
    Task<IList<Audit>> GetAuditsByIdsAsync(IList<Guid> ids, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tambah audit baru ke repository.
    /// </summary>
    Task AddAsync(Audit audit, CancellationToken cancellationToken = default);
}
