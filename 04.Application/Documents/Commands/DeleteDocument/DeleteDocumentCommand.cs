using MediatR;
using Microsoft.EntityFrameworkCore;
using Smlp.Application.Common.Attributes;
using Smlp.Application.Common.Exceptions;
using Smlp.Application.Services.Persistence;
using Smlp.Shared.Documents.Constants;
using Smlp.Shared.Services.Authorization.Constants;

namespace Smlp.Application.Documents.Commands.DeleteDocument;

[Authorize(Policy = Permissions.Smlp_Document_Delete)]
public class DeleteDocumentCommand : IRequest
{
    public Guid DocumentId { get; set; }
}

public class DeleteDocumentCommandHandler : IRequestHandler<DeleteDocumentCommand>
{
    private readonly ISmlpDbContext _context;

    public DeleteDocumentCommandHandler(ISmlpDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteDocumentCommand request, CancellationToken cancellationToken)
    {
        var document = await _context.Documents
            .Where(x => !x.IsDeleted && x.Id == request.DocumentId)
            .SingleOrDefaultAsync(cancellationToken);

        if (document is null)
        {
            throw new NotFoundException(DisplayTextFor.Document, request.DocumentId);
        }

        document.IsDeleted = true;

        await _context.SaveChangesAsync(this, cancellationToken);

        return Unit.Value;
    }
}
