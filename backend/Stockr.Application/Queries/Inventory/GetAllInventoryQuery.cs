using MediatR;
using Stockr.Application.Models;

namespace Stockr.Application.Queries.Inventory;

public class GetAllInventoryQuery : IRequest<IEnumerable<InventoryViewModel>>
{
}