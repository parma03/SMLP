using MediatR;
using Microsoft.EntityFrameworkCore;
using Smlp.Application.Services.Persistence;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Master.Users.Queries;

namespace Smlp.Application.Users.Queries;

public class GetUserByListQuery : IRequest<PaginatedListResponse<GetUserResponse>>
{
    public GetUserRequest Request { get; set; } = default!;
}

public class GetUserByListQueryHandler : IRequestHandler<GetUserByListQuery, PaginatedListResponse<GetUserResponse>>
{
    private readonly ISmlpDbContext _context;

    public GetUserByListQueryHandler(ISmlpDbContext context) => _context = context;

    public async Task<PaginatedListResponse<GetUserResponse>> Handle(GetUserByListQuery request, CancellationToken cancellationToken)
    {
        var req = request.Request;
        var query = _context.Users.Include(u => u.ProfilePicture).Where(u => !u.IsDeleted);

        if (!string.IsNullOrWhiteSpace(req.Username))
            query = query.Where(u => u.Username.Contains(req.Username));
        if (!string.IsNullOrWhiteSpace(req.DisplayName))
            query = query.Where(u => u.DisplayName.Contains(req.DisplayName));
        if (req.Role.HasValue)
            query = query.Where(u => u.Role == req.Role);
        if (req.IsActive.HasValue)
            query = query.Where(u => u.IsActive == req.IsActive);

        var total = await query.CountAsync(cancellationToken);
        var users = await query
            .OrderBy(u => u.DisplayName)
            .ToListAsync(cancellationToken);

        var items = users.Select(GetUserByIdQueryHandler.MapToResponse).ToList();

        return new PaginatedListResponse<GetUserResponse> { Items = items, TotalCount = total };
    }
}
