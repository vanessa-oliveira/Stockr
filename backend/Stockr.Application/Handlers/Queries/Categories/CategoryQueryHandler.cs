using Mapster;
using MediatR;
using Stockr.Application.Models;
using Stockr.Application.Queries.Categories;
using Stockr.Domain.Entities;
using Stockr.Infrastructure.Repositories;

namespace Stockr.Application.Handlers.Queries.Categories;

public class CategoryQueryHandler :
    IRequestHandler<GetAllCategoriesQuery, IEnumerable<CategoryViewModel>>,
    IRequestHandler<GetCategoryByIdQuery, CategoryViewModel?>
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryQueryHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<IEnumerable<CategoryViewModel>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await _categoryRepository.GetAllAsync();
        return categories.Adapt<IEnumerable<CategoryViewModel>>();
    }

    public async Task<CategoryViewModel?> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetWithProductsAsync(request.Id);
        return category.Adapt<CategoryViewModel>();
    }
}