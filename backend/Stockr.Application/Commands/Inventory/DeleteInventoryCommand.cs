using MediatR;

namespace Stockr.Application.Commands.Inventory;

public class DeleteInventoryCommand : IRequest<Unit>
{
    public Guid Id { get; set; }
}