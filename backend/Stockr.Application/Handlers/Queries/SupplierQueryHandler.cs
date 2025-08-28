using Mapster;
using MediatR;
using Stockr.Application.Models;
using Stockr.Application.Queries.Suppliers;
using Stockr.Infrastructure.Repositories;

namespace Stockr.Application.Handlers.Queries.Suppliers;

public class SupplierQueryHandler :
    IRequestHandler<GetAllSuppliersQuery, IEnumerable<SupplierViewModel>>,
    IRequestHandler<GetSupplierByIdQuery, SupplierViewModel?>
{
    private readonly ISupplierRepository _supplierRepository;

    public SupplierQueryHandler(ISupplierRepository supplierRepository)
    {
        _supplierRepository = supplierRepository;
    }

    public async Task<IEnumerable<SupplierViewModel>> Handle(GetAllSuppliersQuery request, CancellationToken cancellationToken)
    {
        var suppliers = await _supplierRepository.GetAllAsync();
        return suppliers.Adapt<IEnumerable<SupplierViewModel>>();
    }

    public async Task<SupplierViewModel?> Handle(GetSupplierByIdQuery request, CancellationToken cancellationToken)
    {
        var supplier = await _supplierRepository.GetByIdAsync(request.Id);
        return supplier.Adapt<SupplierViewModel>();
    }
}