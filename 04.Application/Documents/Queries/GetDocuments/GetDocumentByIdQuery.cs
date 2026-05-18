
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Smlp.Application.Common.Exceptions;
using Smlp.Application.Common.Mappings;
using Smlp.Application.Services.Persistence;
using Smlp.Shared.Documents.Constants;
using Smlp.Shared.Documents.Queries.GetDocuments;

namespace Smlp.Application.Documents.Queries.GetDocuments;
public class GetDocumentByIdQuery : IRequest<GetDocumentByIdResponse>
{
    public Guid? DocumentId { get; set; } = default!;
}
public class GetDocumentByIdQueryMapping : IMapFrom<Domain.Entities.Document, GetDocumentByIdResponse>
{
}
public class GetDocumentByIdQueryHandler : IRequestHandler<GetDocumentByIdQuery, GetDocumentByIdResponse>
{
    private readonly ISmlpDbContext _context;
    private readonly IMapper _mapper;

    public GetDocumentByIdQueryHandler(ISmlpDbContext dbContext, IMapper mapper)
    {
        _context = dbContext;
        _mapper = mapper;
    }

    public async Task<GetDocumentByIdResponse> Handle(GetDocumentByIdQuery request, CancellationToken cancellationToken)
    {
        var result = new GetDocumentByIdResponse();
        var query = await _context.Documents
            .Where(x => !x.IsDeleted && x.Id == request.DocumentId).SingleOrDefaultAsync(cancellationToken);

        if (query is null)
        {
            throw new NotFoundException(DisplayTextFor.Document, request.DocumentId!);
        }

        result.Id = query.Id;
        result.FileName = query.FileName;
        result.DocumentType = query.DocumentType;
        result.FileContentType = query.FileContentType;
        result.FileSize = query.FileSize;

        return result;
    }
}
