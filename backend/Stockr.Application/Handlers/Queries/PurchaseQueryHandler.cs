using MediatR;
using Stockr.Application.Models;
using Stockr.Application.Queries.Purchase;
using Stockr.Infrastructure.Repositories;

namespace Stockr.Application.Handlers.Queries;

public class PurchaseQueryHandler : 
    IRequestHandler<GetAllPurchasesQuery, IEnumerable<PurchaseViewModel>>,
    IRequestHandler<GetPurchaseByIdQuery, PurchaseViewModel?>,
    IRequestHandler<GetPurchasesBySupplierQuery, IEnumerable<PurchaseViewModel>>,
    IRequestHandler<GetPurchasesByPeriodQuery, IEnumerable<PurchaseViewModel>>,
    IRequestHandler<GetPurchasesByInvoiceNumberQuery, IEnumerable<PurchaseViewModel>>
{
    private readonly IPurchaseRepository _purchaseRepository;

    public PurchaseQueryHandler(IPurchaseRepository purchaseRepository)
    {
        _purchaseRepository = purchaseRepository;
    }

    public async Task<IEnumerable<PurchaseViewModel>> Handle(GetAllPurchasesQuery request, CancellationToken cancellationToken)
    {
        var purchases = await _purchaseRepository.GetAllAsync();
        return purchases.Select(MapToViewModel);
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

    private static PurchaseViewModel MapToViewModel(Domain.Entities.Purchase purchase)
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