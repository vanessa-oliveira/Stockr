using Mapster;
using MediatR;
using Stockr.Application.Models;
using Stockr.Application.Queries.Sales;
using Stockr.Domain.Common;
using Stockr.Domain.Entities;
using Stockr.Infrastructure.Repositories;

namespace Stockr.Application.Handlers.Queries;

public class SaleQueryHandler :
    IRequestHandler<GetSaleByIdQuery, SaleViewModel?>,
    IRequestHandler<GetSalesByCustomerQuery, IEnumerable<SaleViewModel>>,
    IRequestHandler<GetSalesByPeriodQuery, IEnumerable<SaleViewModel>>,
    IRequestHandler<GetSalesBySalespersonQuery, IEnumerable<SaleViewModel>>,
    IRequestHandler<GetSalesPagedQuery, PagedResult<SaleViewModel>>
{
    private readonly ISaleRepository _saleRepository;

    public SaleQueryHandler(ISaleRepository saleRepository)
    {
        _saleRepository = saleRepository;
    }

    public async Task<SaleViewModel?> Handle(GetSaleByIdQuery request, CancellationToken cancellationToken)
    {
        var sale = await _saleRepository.GetWithItemsAsync(request.Id);
        return sale.Adapt<SaleViewModel>();
    }

    public async Task<IEnumerable<SaleViewModel>> Handle(GetSalesByCustomerQuery request, CancellationToken cancellationToken)
    {
        var sales = await _saleRepository.GetByCustomerAsync(request.CustomerId);
        return sales.Adapt<IEnumerable<SaleViewModel>>();
    }

    public async Task<IEnumerable<SaleViewModel>> Handle(GetSalesByPeriodQuery request, CancellationToken cancellationToken)
    {
        var sales = await _saleRepository.GetByPeriodAsync(request.StartDate, request.EndDate);
        return sales.Adapt<IEnumerable<SaleViewModel>>();
    }

    public async Task<IEnumerable<SaleViewModel>> Handle(GetSalesBySalespersonQuery request, CancellationToken cancellationToken)
    {
        var sales = await _saleRepository.GetBySalespersonAsync(request.UserId);
        return sales.Adapt<IEnumerable<SaleViewModel>>();
    }

    public async Task<PagedResult<SaleViewModel>> Handle(GetSalesPagedQuery request, CancellationToken cancellationToken)
    {
        var paginationParams = new PaginationParams
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
        
        var sales = await _saleRepository.GetPagedAsync(paginationParams);
        return sales.Adapt<PagedResult<SaleViewModel>>();
    }
}