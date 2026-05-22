using MediatR;
using Microsoft.EntityFrameworkCore;
using Smlp.Application.Common.Exceptions;
using Smlp.Application.Services.Persistence;
using Smlp.Domain.Entities.Master;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Master.Products.Commands.Update;

namespace Smlp.Application.Products.Commands.Update;

public class UpdateProductCommand : IRequest<CommonUpdateResponse>
{
    public Guid Id { get; set; }
    public UpdateProductRequest Request { get; set; } = default!;
}

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, CommonUpdateResponse>
{
    private readonly ISmlpDbContext _context;

    public UpdateProductCommandHandler(ISmlpDbContext context) => _context = context;

    public async Task<CommonUpdateResponse> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == request.Id && !p.IsDeleted, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), request.Id);

        var req = request.Request;
        product.Name = req.Name;
        product.Description = req.Description;
        product.Price = req.Price;
        product.IsActive = req.IsActive;
        product.ProductId = req.CategoryId;

        await _context.SaveChangesAsync(this, cancellationToken);

        return new CommonUpdateResponse { Id = product.Id };
    }
}
