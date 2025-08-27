using Mapster;
using MediatR;
using Stockr.Application.Models;
using Stockr.Application.Queries.Products;
using Stockr.Infrastructure.Repositories;

namespace Stockr.Application.Handlers.Queries.Products;

public class ProductQueryHandler :
    IRequestHandler<GetAllProductsQuery, IEnumerable<ProductViewModel>>,
    IRequestHandler<GetProductByIdQuery, ProductViewModel?>
{
    private readonly IProductRepository _productRepository;

    public ProductQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<IEnumerable<ProductViewModel>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await _productRepository.GetAllAsync();
        return products.Adapt<IEnumerable<ProductViewModel>>();
    }

    public async Task<ProductViewModel?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.Id);
        return product.Adapt<ProductViewModel>();
    }
}