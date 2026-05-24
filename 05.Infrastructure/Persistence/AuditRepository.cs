/*
* Sistem Manajemen Loyalitas Pelanggan (SMLP)
* Copyright (c) 2025-2026
*/

using Microsoft.EntityFrameworkCore;
using Smlp.Application.Audits;
using Smlp.Application.Services.Persistence;
using Smlp.Domain.Entities;
using Smlp.Shared.Audits.Queries.GetAudits;
using Smlp.Shared.Common.Responses;

namespace Smlp.Infrastructure.Persistence;

/// <summary>
/// Implementasi IAuditRepository menggunakan EF Core melalui ISmlpDbContext.
/// </summary>
public class AuditRepository : IAuditRepository
{
    private readonly ISmlpDbContext _context;

    public AuditRepository(ISmlpDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedListResponse<GetAuditsAudit>> GetAuditsAsync(
        GetAuditsRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Audits.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(a =>
                a.EntityName.Contains(search) ||
                a.ActionType.Contains(search) ||
                a.ActionName.Contains(search) ||
                a.FromIpAddress.Contains(search) ||
                a.CreatedBy.Contains(search));
        }

        if (request.From.HasValue)
            query = query.Where(a => a.CreatedAt >= request.From.Value);

        if (request.To.HasValue)
            query = query.Where(a => a.CreatedAt <= request.To.Value);

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(a => new GetAuditsAudit
            {
                Id = a.Id,
                Created = a.CreatedAt,
                CreatedBy = a.CreatedBy,
                EntityName = a.EntityName,
                EntityId = a.EntityId,
                ActionType = a.ActionType,
                FromIpAddress = a.FromIpAddress,
            })
            .ToListAsync(cancellationToken);

        return new PaginatedListResponse<GetAuditsAudit>
        {
            Items = items,
            TotalCount = total
        };
    }

    public async Task<Audit?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Audits
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<IList<Audit>> GetAuditsByIdsAsync(IList<Guid> ids, CancellationToken cancellationToken = default)
    {
        var query = _context.Audits.AsQueryable();

        if (ids.Any())
            query = query.Where(a => ids.Contains(a.Id));

        return await query
            .OrderByDescending(a => a.CreatedAt)
            .Take(10_000) // safety cap
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Audit audit, CancellationToken cancellationToken = default)
    {
        await _context.Audits.AddAsync(audit, cancellationToken);
    }
}
