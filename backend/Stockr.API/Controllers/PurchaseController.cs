using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stockr.Application.Commands.Purchase;
using Stockr.Application.Queries.Purchase;

namespace Stockr.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PurchaseController : ControllerBase
{
    private readonly IMediator _mediator;

    public PurchaseController(IMediator mediator)
    {
        _mediator = mediator;
    }

    #region Queries

    [HttpGet]
    public async Task<IActionResult> GetAllPurchases()
    {
        var query = new GetAllPurchasesQuery();
        var purchases = await _mediator.Send(query);
        return Ok(purchases);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetPurchaseByIdQuery { Id = id };
        var purchase = await _mediator.Send(query);
        
        if (purchase == null)
        {
            return NotFound();
        }

        return Ok(purchase);
    }

    [HttpGet("supplier/{supplierId}")]
    public async Task<IActionResult> GetBySupplier(Guid supplierId)
    {
        var query = new GetPurchasesBySupplierQuery { SupplierId = supplierId };
        var purchases = await _mediator.Send(query);
        return Ok(purchases);
    }

    [HttpGet("period")]
    public async Task<IActionResult> GetByPeriod([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var query = new GetPurchasesByPeriodQuery { StartDate = startDate, EndDate = endDate };
        var purchases = await _mediator.Send(query);
        return Ok(purchases);
    }

    [HttpGet("invoice/{invoiceNumber}")]
    public async Task<IActionResult> GetByInvoiceNumber(string invoiceNumber)
    {
        var query = new GetPurchasesByInvoiceNumberQuery { InvoiceNumber = invoiceNumber };
        var purchases = await _mediator.Send(query);
        return Ok(purchases);
    }

    #endregion

    #region Commands

    [HttpPost]
    public async Task<IActionResult> CreatePurchase([FromBody] CreatePurchaseCommand command)
    {
        await _mediator.Send(command);
        return Ok(new { message = "Purchase created successfully" });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePurchase(Guid id, [FromBody] UpdatePurchaseCommand command)
    {
        command.Id = id;
        await _mediator.Send(command);
        return Ok(new { message = "Purchase updated successfully" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePurchase(Guid id)
    {
        var command = new DeletePurchaseCommand { Id = id };
        await _mediator.Send(command);
        return Ok(new { message = "Purchase deleted successfully" });
    }

    #endregion
}