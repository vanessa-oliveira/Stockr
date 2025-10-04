using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stockr.Application.Commands.Suppliers;
using Stockr.Application.Queries.Suppliers;

namespace Stockr.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SupplierController : ControllerBase
{
    private readonly IMediator _mediator;

    public SupplierController(IMediator mediator)
    {
        _mediator = mediator;
    }

    #region Queries

    [HttpGet]
    public async Task<IActionResult> GetAllSuppliers()
    {
        var query = new GetAllSuppliersQuery();
        var suppliers = await _mediator.Send(query);
        return Ok(suppliers);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetSupplierByIdQuery { Id = id };
        var supplier = await _mediator.Send(query);
        
        if (supplier == null)
        {
            return NotFound();
        }

        return Ok(supplier);
    }

    #endregion

    #region Commands

    [HttpPost]
    public async Task<IActionResult> CreateSupplier([FromBody] CreateSupplierCommand command)
    {
        try
        {
            var supplierId = await _mediator.Send(command);
            return Created("", new { id = supplierId, message = "Supplier created successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while creating the supplier", details = ex.Message });
        }
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSupplier(Guid id, [FromBody] UpdateSupplierCommand command)
    {
        try
        {
            command.Id = id;
            await _mediator.Send(command);
            return Ok(new { message = "Supplier updated successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while updating the supplier", details = ex.Message });
        }
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var command = new DeleteSupplierCommand { Id = id };
            await _mediator.Send(command);
            return Ok(new { message = "Supplier deleted successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while deleting the supplier", details = ex.Message });
        }
    }

    #endregion
}