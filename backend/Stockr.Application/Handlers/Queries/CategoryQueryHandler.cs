using Mapster;
using MediatR;
using Stockr.Application.Models;
using Stockr.Application.Queries.Categories;
using Stockr.Domain.Common;
using Stockr.Infrastructure.Repositories;

namespace Stockr.Application.Handlers.Queries;

public class CategoryQueryHandler :
    IRequestHandler<GetCategoryByIdQuery, CategoryViewModel?>,
    IRequestHandler<GetCategoriesPagedQuery, PagedResult<CategoryViewModel>>
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryQueryHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<CategoryViewModel?> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(request.Id);
        return category.Adapt<CategoryViewModel>();
    }

    public async Task<PagedResult<CategoryViewModel>> Handle(GetCategoriesPagedQuery request, CancellationToken cancellationToken)
    {
        var paginationParams = new PaginationParams
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
        
        var categories = await _categoryRepository.GetPagedAsync(paginationParams);
        return categories.Adapt<PagedResult<CategoryViewModel>>();
    }
}