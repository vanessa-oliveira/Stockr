using MediatR;

namespace Stockr.Application.Commands.Inventory;

public class UpdateInventoryCommand : IRequest<Unit>
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public int MinStock { get; set; }
}