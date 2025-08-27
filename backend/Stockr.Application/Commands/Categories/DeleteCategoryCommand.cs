using MediatR;

namespace Stockr.Application.Commands.Categories;

public class DeleteCategoryCommand : IRequest<Unit>
{
    public Guid Id { get; set; }
}