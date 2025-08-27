using MediatR;

namespace Stockr.Application.Commands.Categories;

public class UpdateCategoryCommand : IRequest<Unit>
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
}