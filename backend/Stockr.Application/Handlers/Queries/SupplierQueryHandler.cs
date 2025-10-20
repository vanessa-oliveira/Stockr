using Mapster;
using MediatR;
using Stockr.Application.Models;
using Stockr.Application.Queries.Suppliers;
using Stockr.Domain.Common;
using Stockr.Infrastructure.Repositories;

namespace Stockr.Application.Handlers.Queries;

public class SupplierQueryHandler :
    IRequestHandler<GetSupplierByIdQuery, SupplierViewModel?>,
    IRequestHandler<GetSuppliersPagedQuery, PagedResult<SupplierViewModel>>
{
    private readonly ISupplierRepository _supplierRepository;

    public SupplierQueryHandler(ISupplierRepository supplierRepository)
    {
        _supplierRepository = supplierRepository;
    }

    public async Task<SupplierViewModel?> Handle(GetSupplierByIdQuery request, CancellationToken cancellationToken)
    {
        var supplier = await _supplierRepository.GetByIdAsync(request.Id);
        return supplier.Adapt<SupplierViewModel>();
    }

    public async Task<PagedResult<SupplierViewModel>> Handle(GetSuppliersPagedQuery request, CancellationToken cancellationToken)
    {
        var paginationParams = new PaginationParams
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
        
        var suppliers = await _supplierRepository.GetPagedAsync(paginationParams);

        return suppliers.Adapt<PagedResult<SupplierViewModel>>();
    }
}