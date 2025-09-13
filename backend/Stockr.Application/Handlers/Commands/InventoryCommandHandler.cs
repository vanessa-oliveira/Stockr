using MediatR;
using Stockr.Application.Commands.Inventory;
using Stockr.Domain.Entities;
using Stockr.Infrastructure.Repositories;

namespace Stockr.Application.Handlers.Commands;

public class InventoryCommandHandler :
    IRequestHandler<CreateInventoryCommand, Unit>,
    IRequestHandler<UpdateInventoryCommand, Unit>,
    IRequestHandler<DeleteInventoryCommand, Unit>
{
    private readonly IInventoryRepository _inventoryRepository;

    public InventoryCommandHandler(IInventoryRepository inventoryRepository)
    {
        _inventoryRepository = inventoryRepository;
    }
    
    public async Task<Unit> Handle(CreateInventoryCommand command, CancellationToken cancellationToken)
    {
        var inventory = new Inventory()
        {
            ProductId = command.ProductId,
            MinStock = command.MinStock,
            CurrentStock = command.CurrentStock,
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