using Mapster;
using MediatR;
using Stockr.Application.Models;
using Stockr.Application.Queries.Products;
using Stockr.Domain.Common;
using Stockr.Infrastructure.Helpers;
using Stockr.Infrastructure.Interfaces;
using Stockr.Infrastructure.Repositories;
using Stockr.Infrastructure.Services;

namespace Stockr.Application.Handlers.Queries;

public class ProductQueryHandler :
    IRequestHandler<GetAllProductsQuery, IEnumerable<ProductViewModel>>,
    IRequestHandler<GetProductByIdQuery, ProductViewModel?>,
    IRequestHandler<GetProductsPagedQuery, PagedResult<ProductViewModel>>
{
    private readonly IProductRepository _productRepository;
    private readonly ICacheService _cacheService;
    private readonly ITenantContext _tenantContext;

    public ProductQueryHandler(
        IProductRepository productRepository,
        ICacheService cacheService,
        ITenantContext tenantContext)
    {
        _productRepository = productRepository;
        _cacheService = cacheService;
        _tenantContext = tenantContext;
    }

    public async Task<IEnumerable<ProductViewModel>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        var tenantId = ValidateAndGetTenantId();

        var cacheKey = CacheKeyHelper.ProductsList(tenantId);
        
        var cachedProducts = await _cacheService.GetAsync<List<ProductViewModel>>(cacheKey);
        if (cachedProducts != null)
        {
            return cachedProducts;
        }
        
        var products = await _productRepository.GetAllAsync();
        var productViewModels = products.Adapt<List<ProductViewModel>>();
        
        await _cacheService.SetAsync(cacheKey, productViewModels, TimeSpan.FromMinutes(10));

        return productViewModels;
    }

    public async Task<ProductViewModel?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var tenantId = ValidateAndGetTenantId();

        var cacheKey = CacheKeyHelper.ProductById(tenantId, request.Id);
        
        var cachedProduct = await _cacheService.GetAsync<ProductViewModel>(cacheKey);
        if (cachedProduct != null)
        {
            return cachedProduct;
        }
        
        var product = await _productRepository.GetByIdAsync(request.Id);
        if (product == null)
        {
            return null;
        }

        var productViewModel = product.Adapt<ProductViewModel>();
        
        await _cacheService.SetAsync(cacheKey, productViewModel, TimeSpan.FromMinutes(10));

        return productViewModel;
    }

    public async Task<PagedResult<ProductViewModel>> Handle(GetProductsPagedQuery request, CancellationToken cancellationToken)
    {
        var tenantId = ValidateAndGetTenantId();

        var cacheKey = CacheKeyHelper.ProductsPaged(tenantId, request.PageNumber, request.PageSize);
        
        var cachedProducts = await _cacheService.GetAsync<PagedResult<ProductViewModel>>(cacheKey);
        if (cachedProducts != null)
        {
            return cachedProducts;
        }
        
        var paginationParams = new PaginationParams
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        var products = await _productRepository.GetPagedAsync(paginationParams);
        var pagedResult = products.Adapt<PagedResult<ProductViewModel>>();
        
        await _cacheService.SetAsync(cacheKey, pagedResult, TimeSpan.FromMinutes(10));

        return pagedResult;
    }
    
    private Guid ValidateAndGetTenantId()
    {
        var tenantId = _tenantContext.GetCurrentTenantId();
        if (!tenantId.HasValue)
        {
            throw new UnauthorizedAccessException("User must belong to a tenant");
        }
        return tenantId.Value;
    }
}