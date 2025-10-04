using MediatR;
using Stockr.Application.Commands.Inventory;
using Stockr.Application.Services;
using Stockr.Domain.Entities;
using Stockr.Infrastructure.Repositories;

namespace Stockr.Application.Handlers.Commands;

public class InventoryCommandHandler :
    IRequestHandler<CreateInventoryCommand, Unit>,
    IRequestHandler<UpdateInventoryCommand, Unit>,
    IRequestHandler<DeleteInventoryCommand, Unit>
{
    private readonly IInventoryRepository _inventoryRepository;
    private readonly ITenantService _tenantService;

    public InventoryCommandHandler(IInventoryRepository inventoryRepository, ITenantService tenantService)
    {
        _inventoryRepository = inventoryRepository;
        _tenantService = tenantService;
    }

    public async Task<Unit> Handle(CreateInventoryCommand command, CancellationToken cancellationToken)
    {
        var currentTenantId = _tenantService.GetCurrentTenantId();
        if (!currentTenantId.HasValue)
        {
            throw new UnauthorizedAccessException("User must belong to a tenant");
        }
        
        var inventory = new Inventory()
        {
            ProductId = command.ProductId,
            MinStock = command.MinStock,
            CurrentStock = command.CurrentStock,
            TenantId = currentTenantId.Value
        };
        
        await _inventoryRepository.AddAsync(inventory);
        return Unit.Value;
    }

    public async Task<Unit> Handle(UpdateInventoryCommand command, CancellationToken cancellationToken)
    {
        var inventory = await _inventoryRepository.GetByIdAsync(command.Id);
        if (inventory == null)
        {
            throw new ArgumentException("Inventory not found");
        }
        
        inventory.ProductId = command.ProductId;
        inventory.MinStock = command.MinStock;
        
        await _inventoryRepository.UpdateAsync(inventory);
        return Unit.Value;
    }

    public async Task<Unit> Handle(DeleteInventoryCommand command, CancellationToken cancellationToken)
    {
        var inventory = await _inventoryRepository.GetByIdAsync(command.Id);
        if (inventory == null)
        {
            throw new ArgumentException("Inventory not found");
        }
        
        await _inventoryRepository.DeleteAsync(inventory);
        return Unit.Value;
    }

}