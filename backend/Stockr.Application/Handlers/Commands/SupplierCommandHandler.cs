using MediatR;
using Stockr.Application.Commands.Suppliers;
using Stockr.Domain.Entities;
using Stockr.Infrastructure.Repositories;

namespace Stockr.Application.Handlers.Commands;

public class SupplierCommandHandler : 
    IRequestHandler<CreateSupplierCommand, Unit>,
    IRequestHandler<UpdateSupplierCommand, Unit>,
    IRequestHandler<DeleteSupplierCommand, Unit>
{
    private readonly ISupplierRepository _supplierRepository;

    public SupplierCommandHandler(ISupplierRepository supplierRepository)
    {
        _supplierRepository = supplierRepository;
    }

    public async Task<Unit> Handle(CreateSupplierCommand command, CancellationToken cancellationToken)
    {
        var supplier = new Supplier
        {
            Name = command.Name,
            Phone = command.Phone
        };

        await _supplierRepository.AddAsync(supplier);
        return Unit.Value;
    }

    public async Task<Unit> Handle(UpdateSupplierCommand command, CancellationToken cancellationToken)
    {
        var supplier = await _supplierRepository.GetByIdAsync(command.Id);
        if (supplier == null)
        {
            throw new ArgumentException("Supplier not found");
        }

        supplier.Name = command.Name;
        supplier.Phone = command.Phone;
        supplier.UpdatedAt = DateTime.UtcNow;

        await _supplierRepository.UpdateAsync(supplier);
        return Unit.Value;
    }

    public async Task<Unit> Handle(DeleteSupplierCommand command, CancellationToken cancellationToken)
    {
        var supplier = await _supplierRepository.GetByIdAsync(command.Id);
        if (supplier == null)
        {
            throw new ArgumentException("Supplier not found");
        }

        // Verifica se o fornecedor tem produtos associados
        var hasProducts = await _supplierRepository.HasProductsAsync(command.Id);
        if (hasProducts)
        {
            throw new InvalidOperationException("Cannot delete supplier that has associated products");
        }

        await _supplierRepository.DeleteAsync(supplier);
        return Unit.Value;
    }
}