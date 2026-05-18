using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Smlp.Application.Common.Attributes;
using Smlp.Application.Common.Extensions;
using Smlp.Application.Common.Mappings;
using Smlp.Application.Services.Persistence;
using Smlp.Domain.Entities;
using Smlp.Shared.Common.Enums;
using Smlp.Shared.Common.Requests;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Documents.Queries.GetDocuments;
using Smlp.Shared.Services.Authorization.Constants;

namespace Smlp.Application.Documents.Queries.GetDocuments;

[Authorize(Policy = Permissions.Smlp_Document_Index)]
public class GetDocumentsQuery : PaginatedListRequest, IRequest<PaginatedListResponse<GetDocumentsDocument>>
{
}

public class GetDocumentsQueryValidator : AbstractValidator<GetDocumentsQuery>
{
    public GetDocumentsQueryValidator()
    {
        Include(new PaginatedListRequestValidator());
    }
}

public class GetDocumentsDocumentMapping : IMapFrom<Document, GetDocumentsDocument>
{
}

public class GetDocumentsQueryHandler : IRequestHandler<GetDocumentsQuery, PaginatedListResponse<GetDocumentsDocument>>
{
    private readonly ISmlpDbContext _context;
    private readonly IMapper _mapper;

    public GetDocumentsQueryHandler(ISmlpDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PaginatedListResponse<GetDocumentsDocument>> Handle(GetDocumentsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Documents
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .ApplySearch(request.SearchText, typeof(GetDocumentsDocument), _mapper.ConfigurationProvider)
            .ApplyOrder(request.SortField, request.SortOrder,
                typeof(GetDocumentsDocument),
                _mapper.ConfigurationProvider,
                nameof(GetDocumentsDocument.Title),
                SortOrder.Asc);

        var result = await query
            .ProjectTo<GetDocumentsDocument>(_mapper.ConfigurationProvider)
            .ToPaginatedListAsync(request.Page, request.PageSize, cancellationToken);

        return result.ToPaginatedListResponse();
    }
}
