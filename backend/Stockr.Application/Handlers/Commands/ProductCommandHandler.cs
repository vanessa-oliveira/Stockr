using MediatR;
using Stockr.Application.Commands.Products;
using Stockr.Application.Services;
using Stockr.Domain.Entities;
using Stockr.Infrastructure.Repositories;

namespace Stockr.Application.Handlers.Commands;

public class ProductCommandHandler :
    IRequestHandler<CreateProductCommand, Unit>,
    IRequestHandler<UpdateProductCommand, Unit>,
    IRequestHandler<DeleteProductCommand, Unit>
{
    private readonly IProductRepository _productRepository;
    private readonly ITenantService _tenantService;

    public ProductCommandHandler(IProductRepository productRepository, ITenantService tenantService)
    {
        _productRepository = productRepository;
        _tenantService = tenantService;
    }

    public async Task<Unit> Handle(CreateProductCommand command, CancellationToken cancellationToken)
    {
        var currentTenantId = _tenantService.GetCurrentTenantId();
        if (!currentTenantId.HasValue)
        {
            throw new UnauthorizedAccessException("User must belong to a tenant");
        }

        var product = new Product()
        {
            Name = command.Name,
            SKU = command.SKU,
            Description = command.Description,
            CategoryId = command.CategoryId,
            SupplierId = command.SupplierId,
            CostPrice = command.CostPrice,
            SalePrice = command.SalePrice,
            TenantId = currentTenantId.Value
        };

        await _productRepository.AddAsync(product);
        return Unit.Value;
    }

    public async Task<Unit> Handle(UpdateProductCommand command, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(command.Id);
        if (product == null)
        {
            throw new ArgumentException("Product not found");
        }
        
        product.Name = command.Name;
        product.SKU = command.SKU;
        product.Description = command.Description;
        product.CategoryId = command.CategoryId;
        product.SupplierId = command.SupplierId;
        product.CostPrice = command.CostPrice;
        product.SalePrice = command.SalePrice;
        
        await _productRepository.UpdateAsync(product);
        return Unit.Value;
    }

    public async Task<Unit> Handle(DeleteProductCommand command, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(command.Id);
        if (product == null)
        {
            throw new ArgumentException("Product not found");
        }
        
        await _productRepository.DeleteAsync(product);
        return Unit.Value;
    }
}