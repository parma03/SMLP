using MediatR;
using Microsoft.EntityFrameworkCore;
using Smlp.Application.Common.Exceptions;
using Smlp.Application.Services.Persistence;
using Smlp.Domain.Entities.Master;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Master.Products.Commands.Create;

namespace Smlp.Application.Products.Commands.Create;

public class CreateProductCommand : IRequest<CommonCreateResponse>
{
    public CreateProductRequest Request { get; set; } = default!;
}

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, CommonCreateResponse>
{
    private readonly ISmlpDbContext _context;

    public CreateProductCommandHandler(ISmlpDbContext context) => _context = context;

    public async Task<CommonCreateResponse> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var req = request.Request;

        var exists = await _context.Products.AnyAsync(p => p.Name == req.Name && !p.IsDeleted, cancellationToken);
        if (exists)
            throw new AlreadyExistsExceptions(nameof(Product), req.Name);

        var product = new Product
        {
            Name = req.Name,
            Description = req.Description,
            Price = req.Price,
            IsActive = req.IsActive,
            ProductId = req.CategoryId,
        };

        await _context.Products.AddAsync(product, cancellationToken);
        await _context.SaveChangesAsync(this, cancellationToken);

        return new CommonCreateResponse { Id = product.Id };
    }
}
