using MediatR;

namespace Stockr.Application.Commands.Inventory;

public class CreateInventoryCommand : IRequest<Unit>
{
    public Guid ProductId { get; set; }
    public int MinStock { get; set; }
    public int CurrentStock { get; set; }
}