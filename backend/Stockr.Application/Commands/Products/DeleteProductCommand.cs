using MediatR;

namespace Stockr.Application.Commands.Products;

public class DeleteProductCommand : IRequest<Unit>
{
    public Guid Id { get; set; }
}