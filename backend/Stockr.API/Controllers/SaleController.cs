using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stockr.Application.Commands.Sales;
using Stockr.Application.Queries.Sales;

namespace Stockr.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SaleController : ControllerBase
{
    private readonly IMediator _mediator;

    public SaleController(IMediator mediator)
    {
        _mediator = mediator;
    }

    #region Queries

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetSaleByIdQuery { Id = id };
        var sale = await _mediator.Send(query);
        
        if (sale == null)
        {
            return NotFound();
        }

        return Ok(sale);
    }

    [HttpGet("customer/{customerId}")]
    public async Task<IActionResult> GetByCustomer(Guid customerId)
    {
        var query = new GetSalesByCustomerQuery { CustomerId = customerId };
        var sales = await _mediator.Send(query);
        return Ok(sales);
    }

    [HttpGet("salesperson/{userId}")]
    public async Task<IActionResult> GetBySalesperson(Guid userId)
    {
        var query = new GetSalesBySalespersonQuery { UserId = userId };
        var sales = await _mediator.Send(query);
        return Ok(sales);
    }

    [HttpGet]
    public async Task<IActionResult> GetSalesPaged([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var query = new GetSalesPagedQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    #endregion

    #region Commands

    [HttpPost]
    public async Task<IActionResult> CreateSale([FromBody] CreateSaleCommand command)
    {
        try
        {
            var saleId = await _mediator.Send(command);
            return Created("", new { id = saleId, message = "Sale created successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while creating the sale", details = ex.Message });
        }
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSale(Guid id, [FromBody] UpdateSaleCommand command)
    {
        try
        {
            command.Id = id;
            await _mediator.Send(command);
            
            return Ok(new { message = "Sale updated successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while updating the sale", details = ex.Message });
        }
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> ChangeSaleStatus(Guid id, [FromBody] ChangeSaleStatusCommand command)
    {
        try
        {
            command.Id = id;
            await _mediator.Send(command);
            
            return Ok(new { message = "Sale status updated successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while updating the sale status", details = ex.Message });
        }
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var command = new DeleteSaleCommand { Id = id };
            await _mediator.Send(command);
            
            return Ok(new { message = "Sale deleted successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while deleting the sale", details = ex.Message });
        }
    }

    #endregion
}