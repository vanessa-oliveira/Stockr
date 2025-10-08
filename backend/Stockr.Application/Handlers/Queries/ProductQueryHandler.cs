using Mapster;
using MediatR;
using Stockr.Application.Models;
using Stockr.Application.Queries.Products;
using Stockr.Domain.Common;
using Stockr.Domain.Entities;
using Stockr.Infrastructure.Repositories;

namespace Stockr.Application.Handlers.Queries;

public class ProductQueryHandler :
    IRequestHandler<GetAllProductsQuery, IEnumerable<ProductViewModel>>,
    IRequestHandler<GetProductByIdQuery, ProductViewModel?>,
    IRequestHandler<GetProductsPagedQuery, PagedResult<ProductViewModel>>
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

    public async Task<PagedResult<ProductViewModel>> Handle(GetProductsPagedQuery request, CancellationToken cancellationToken)
    {
        var paginationParams = new PaginationParams
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
        
        var products = await _productRepository.GetPagedAsync(paginationParams);
        return products.Adapt<PagedResult<ProductViewModel>>();
    }
}