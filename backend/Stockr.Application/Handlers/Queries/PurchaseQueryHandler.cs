using Mapster;
using MediatR;
using Stockr.Application.Models;
using Stockr.Application.Queries.Purchase;
using Stockr.Domain.Common;
using Stockr.Domain.Entities;
using Stockr.Infrastructure.Repositories;

namespace Stockr.Application.Handlers.Queries;

public class PurchaseQueryHandler :
    IRequestHandler<GetPurchaseByIdQuery, PurchaseViewModel?>,
    IRequestHandler<GetPurchasesBySupplierQuery, IEnumerable<PurchaseViewModel>>,
    IRequestHandler<GetPurchasesByPeriodQuery, IEnumerable<PurchaseViewModel>>,
    IRequestHandler<GetPurchasesByInvoiceNumberQuery, IEnumerable<PurchaseViewModel>>,
    IRequestHandler<GetPurchasesPagedQuery, PagedResult<PurchaseViewModel>>
{
    private readonly IPurchaseRepository _purchaseRepository;

    public PurchaseQueryHandler(IPurchaseRepository purchaseRepository)
    {
        _purchaseRepository = purchaseRepository;
    }

    public async Task<PurchaseViewModel?> Handle(GetPurchaseByIdQuery request, CancellationToken cancellationToken)
    {
        var purchase = await _purchaseRepository.GetWithItemsAsync(request.Id);
        return purchase != null ? MapToViewModel(purchase) : null;
    }

    public async Task<IEnumerable<PurchaseViewModel>> Handle(GetPurchasesBySupplierQuery request, CancellationToken cancellationToken)
    {
        var purchases = await _purchaseRepository.GetBySupplierAsync(request.SupplierId);
        return purchases.Select(MapToViewModel);
    }

    public async Task<IEnumerable<PurchaseViewModel>> Handle(GetPurchasesByPeriodQuery request, CancellationToken cancellationToken)
    {
        var purchases = await _purchaseRepository.GetByPeriodAsync(request.StartDate, request.EndDate);
        return purchases.Select(MapToViewModel);
    }

    public async Task<IEnumerable<PurchaseViewModel>> Handle(GetPurchasesByInvoiceNumberQuery request, CancellationToken cancellationToken)
    {
        var purchases = await _purchaseRepository.GetByInvoiceNumberAsync(request.InvoiceNumber);
        return purchases.Select(MapToViewModel);
    }

    public async Task<PagedResult<PurchaseViewModel>> Handle(GetPurchasesPagedQuery request, CancellationToken cancellationToken)
    {
        var paginationParams = new PaginationParams
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
        
        var purchases = await _purchaseRepository.GetPagedAsync(paginationParams);
        return purchases.Adapt<PagedResult<PurchaseViewModel>>();
    }

    private static PurchaseViewModel MapToViewModel(Purchase purchase)
    {
        return new PurchaseViewModel
        {
            Id = purchase.Id,
            SupplierId = purchase.SupplierId,
            SupplierName = purchase.Supplier?.Name ?? "",
            TotalAmount = purchase.TotalAmount,
            PurchaseDate = purchase.PurchaseDate,
            Notes = purchase.Notes,
            InvoiceNumber = purchase.InvoiceNumber,
            PurchaseItems = purchase.PurchaseItems?.Select(item => new PurchaseItemViewModel
            {
                Id = item.Id,
                ProductId = item.ProductId,
                ProductName = item.Product?.Name ?? "",
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                TotalPrice = item.TotalPrice
            }).ToList() ?? new List<PurchaseItemViewModel>()
        };
    }
}