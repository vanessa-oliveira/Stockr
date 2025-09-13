using MediatR;
using Stockr.Application.Models;

namespace Stockr.Application.Queries.Inventory;

public class GetInventoryByIdQuery : IRequest<InventoryViewModel?>
{
    public Guid Id { get; set; }
}