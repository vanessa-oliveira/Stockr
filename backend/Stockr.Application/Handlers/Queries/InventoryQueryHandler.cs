using Mapster;
using MediatR;
using Stockr.Application.Models;
using Stockr.Application.Queries.Inventory;
using Stockr.Domain.Common;
using Stockr.Infrastructure.Repositories;

namespace Stockr.Application.Handlers.Queries;

public class InventoryQueryHandler :
    IRequestHandler<GetInventoriesPagedQuery, PagedResult<InventoryViewModel>>,
    IRequestHandler<GetInventoryByIdQuery, InventoryViewModel?>,
    IRequestHandler<GetInventoryByProductIdQuery, InventoryViewModel?>
{
    private readonly IInventoryRepository _inventoryRepository;

    public InventoryQueryHandler(IInventoryRepository inventoryRepository)
    {
        _inventoryRepository = inventoryRepository;
    }
    public async Task<PagedResult<InventoryViewModel>> Handle(GetInventoriesPagedQuery request, CancellationToken cancellationToken)
    {
        var paginationParams = new PaginationParams
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        var inventories = await _inventoryRepository.GetPagedAsync(paginationParams);
        return inventories.Adapt<PagedResult<InventoryViewModel>>();
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