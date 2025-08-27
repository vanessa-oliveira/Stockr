using MediatR;

namespace Stockr.Application.Commands.Categories;

public class CreateCategoryCommand : IRequest<Guid>
{
    public string Name { get; set; }
    public string Description { get; set; }
}