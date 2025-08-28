using MediatR;
using Stockr.Application.Commands.Sales;
using Stockr.Domain.Entities;
using Stockr.Domain.Enums;
using Stockr.Infrastructure.Repositories;

namespace Stockr.Application.Handlers.Commands;

public class SaleCommandHandler : 
    IRequestHandler<CreateSaleCommand, Unit>,
    IRequestHandler<UpdateSaleCommand, Unit>,
    IRequestHandler<DeleteSaleCommand, Unit>,
    IRequestHandler<ChangeSaleStatusCommand, Unit>
{
    private readonly ISaleRepository _saleRepository;
    private readonly ISaleItemRepository _saleItemRepository;
    private readonly IProductRepository _productRepository;

    public SaleCommandHandler(ISaleRepository saleRepository, ISaleItemRepository saleItemRepository, IProductRepository productRepository)
    {
        _saleRepository = saleRepository;
        _saleItemRepository = saleItemRepository;
        _productRepository = productRepository;
    }

    public async Task<Unit> Handle(CreateSaleCommand command, CancellationToken cancellationToken)
    {
        var sale = new Sale
        {
            CustomerId = command.CustomerId,
            SalesPersonId = command.SalespersonId,
            SaleStatus = Enum.Parse<SaleStatus>(command.SaleStatus),
            TotalAmount = command.TotalAmount,
            SaleDate = command.SaleDate
        };
        
        var saleItems = new List<SaleItem>();
        decimal totalAmount = 0;

        foreach (var item in command.SaleItems)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId);
            if (product == null)
            {
                throw new ArgumentException($"Product with ID {item.ProductId} not found");
            }
            
            var unitPrice = item.UnitPrice ?? product.SalePrice;
            var totalPrice = unitPrice * item.Quantity;
            
            var saleItem = new SaleItem
            {
                SaleId = sale.Id,
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = unitPrice,
                TotalPrice = totalPrice
            };
            saleItems.Add(saleItem);
            totalAmount += totalPrice;
        }
        
        sale.TotalAmount = totalAmount;
        
        await _saleRepository.AddAsync(sale);
        await _saleItemRepository.AddRangeAsync(saleItems);
        
        return Unit.Value;
    }

    public async Task<Unit> Handle(UpdateSaleCommand command, CancellationToken cancellationToken)
    {
        var existingSale = await _saleRepository.GetByIdAsync(command.Id);
        if (existingSale == null)
        {
            throw new ArgumentException("Sale not found");
        }

        existingSale.SaleDate = command.SaleDate;
        existingSale.CustomerId = command.CustomerId;
        existingSale.SalesPersonId = command.SalesPersonId;
        existingSale.SaleStatus = Enum.Parse<SaleStatus>(command.SaleStatus);
        existingSale.SalesPersonId = command.SalesPersonId;
        existingSale.UpdatedAt = DateTime.UtcNow;
        
        // Gerenciar itens de venda
        var existingItems = await _saleItemRepository.GetBySaleAsync(command.Id);
        
        // Marcar itens para deletar
        var itemsToDelete = command.SaleItems.Where(x => x.ToDelete && x.Id.HasValue).ToList();
        foreach (var itemToDelete in itemsToDelete)
        {
            var existingItem = existingItems?.FirstOrDefault(x => x.Id == itemToDelete.Id);
            if (existingItem != null)
            {
                await _saleItemRepository.DeleteAsync(existingItem);
            }
        }

        decimal totalAmount = 0;
        var itemsToProcess = command.SaleItems.Where(x => !x.ToDelete).ToList();

        foreach (var item in itemsToProcess)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId);
            if (product == null)
            {
                throw new ArgumentException($"Product with ID {item.ProductId} not found");
            }

            var unitPrice = item.UnitPrice ?? product.SalePrice;
            var totalPrice = unitPrice * item.Quantity;

            if (item.Id.HasValue)
            {
                // Atualizar item existente
                var existingItem = existingItems?.FirstOrDefault(x => x.Id == item.Id);
                if (existingItem != null)
                {
                    existingItem.SaleId = existingSale.Id;
                    existingItem.ProductId = item.ProductId;
                    existingItem.Quantity = item.Quantity;
                    existingItem.UnitPrice = unitPrice;
                    existingItem.TotalPrice = totalPrice;
                    await _saleItemRepository.UpdateAsync(existingItem);
                }
            }
            else
            {
                // Criar novo item
                var newSaleItem = new SaleItem
                {
                    SaleId = existingSale.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = unitPrice,
                    TotalPrice = totalPrice
                };
                await _saleItemRepository.AddAsync(newSaleItem);
            }

            totalAmount += totalPrice;
        }
        
        existingSale.TotalAmount = totalAmount;
        await _saleRepository.UpdateAsync(existingSale);
        return Unit.Value;
    }

    public async Task<Unit> Handle(DeleteSaleCommand command, CancellationToken cancellationToken)
    {
        var sale = await _saleRepository.GetByIdAsync(command.Id);
        if (sale == null)
        {
            throw new ArgumentException("Sale not found");
        }
        
        // Deletar itens de venda primeiro
        var saleItems = await _saleItemRepository.GetBySaleAsync(command.Id);
        foreach (var item in saleItems)
        {
            await _saleItemRepository.DeleteAsync(item);
        }
        
        await _saleRepository.DeleteAsync(sale);
        return Unit.Value;
    }

    public async Task<Unit> Handle(ChangeSaleStatusCommand command, CancellationToken cancellationToken)
    {
        var sale = await _saleRepository.GetByIdAsync(command.Id);
        if (sale == null)
        {
            throw new ArgumentException("Sale not found");
        }
        
        sale.SaleStatus = Enum.Parse<SaleStatus>(command.NewStatus);
        
        await _saleRepository.UpdateAsync(sale);
        return Unit.Value;
    }
}