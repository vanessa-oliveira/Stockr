using Mapster;
using MediatR;
using Stockr.Application.Models;
using Stockr.Application.Queries.Inventory;
using Stockr.Infrastructure.Repositories;

namespace Stockr.Application.Handlers.Queries;

public class InventoryQueryHandler :
    IRequestHandler<GetAllInventoryQuery, IEnumerable<InventoryViewModel>>,
    IRequestHandler<GetInventoryByIdQuery, InventoryViewModel?>,
    IRequestHandler<GetInventoryByProductIdQuery, InventoryViewModel?>
{
    private readonly IInventoryRepository _inventoryRepository;

    public InventoryQueryHandler(IInventoryRepository inventoryRepository)
    {
        _inventoryRepository = inventoryRepository;
    }

    public async Task<IEnumerable<InventoryViewModel>> Handle(GetAllInventoryQuery request, CancellationToken cancellationToken)
    {
        var inventories = await _inventoryRepository.GetAllAsync();
        return inventories.Adapt<IEnumerable<InventoryViewModel>>();
    }

    public async Task<InventoryViewModel?> Handle(GetInventoryByIdQuery request, CancellationToken cancellationToken)
    {
        var inventory = await _inventoryRepository.GetByIdAsync(request.Id);
        return inventory.Adapt<InventoryViewModel>();
    }

    public async Task<InventoryViewModel?> Handle(GetInventoryByProductIdQuery request, CancellationToken cancellationToken)
    {
        var inventory = await _inventoryRepository.GetByProductIdAsync(request.ProductId);
        return inventory.Adapt<InventoryViewModel>();
    }
}