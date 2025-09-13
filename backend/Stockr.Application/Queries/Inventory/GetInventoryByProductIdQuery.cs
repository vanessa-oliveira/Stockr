using MediatR;
using Stockr.Application.Models;

namespace Stockr.Application.Queries.Inventory;

public class GetInventoryByProductIdQuery : IRequest<InventoryViewModel?>
{
    public Guid ProductId { get; set; }
}