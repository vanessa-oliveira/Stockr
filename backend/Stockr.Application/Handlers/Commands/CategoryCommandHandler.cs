using MediatR;
using Stockr.Application.Commands.Categories;
using Stockr.Domain.Entities;
using Stockr.Infrastructure.Repositories;

namespace Stockr.Application.Handlers.Commands;

public class CategoryCommandHandler : 
    IRequestHandler<CreateCategoryCommand, Guid>,
    IRequestHandler<UpdateCategoryCommand, Unit>,
    IRequestHandler<DeleteCategoryCommand, Unit>
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryCommandHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }
    
    public async Task<Guid> Handle(CreateCategoryCommand command, CancellationToken cancellationToken)
    {
        var category = new Category()
        {
            Name = command.Name,
            Description = command.Description,
        };
        
        await _categoryRepository.AddAsync(category);
        return category.Id;
    }

    public async Task<Unit> Handle(UpdateCategoryCommand command, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(command.Id);
        if (category == null)
        {
            throw new ArgumentException("Category not found");
        }
        
        category.Name = command.Name;
        category.Description = command.Description;
        
        await _categoryRepository.UpdateAsync(category);
        return Unit.Value;
    }

    public async Task<Unit> Handle(DeleteCategoryCommand command, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(command.Id);
        if (category == null)
        {
            throw new ArgumentException("Category not found");
        }
        
        await _categoryRepository.DeleteAsync(category);
        return Unit.Value;
    }
}