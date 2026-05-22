using MediatR;
using Microsoft.EntityFrameworkCore;
using Smlp.Application.Common.Exceptions;
using Smlp.Application.Services.Persistence;
using Smlp.Domain.Entities.Master;

namespace Smlp.Application.Products.Commands.Delete;

public class DeleteProductCommand : IRequest
{
    public Guid Id { get; set; }
}

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand>
{
    private readonly ISmlpDbContext _context;

    public DeleteProductCommandHandler(ISmlpDbContext context) => _context = context;

    public async Task<Unit> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == request.Id && !p.IsDeleted, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), request.Id);

        _context.Products.Remove(product);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
