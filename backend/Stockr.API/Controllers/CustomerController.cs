using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stockr.Application.Commands.Customers;
using Stockr.Application.Queries.Customers;

namespace Stockr.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomerController : ControllerBase
{
    private readonly IMediator _mediator;

    public CustomerController(IMediator mediator)
    {
        _mediator = mediator;
    }

    #region Queries
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetCustomerByIdQuery { Id = id };
        var customer = await _mediator.Send(query);

        if (customer == null)
        {
            return NotFound();
        }

        return Ok(customer);
    }

    [HttpGet]
    public async Task<IActionResult> GetCustomersPaged([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var query = new GetCustomersPagedQuery
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
    public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerCommand command)
    {
        try
        {
            var customerId = await _mediator.Send(command);
            return Created("", new { id = customerId, message = "Customer created successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while creating the customer", details = ex.Message });
        }
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCustomer(Guid id, [FromBody] UpdateCustomerCommand command)
    {
        try
        {
            command.Id = id;
            await _mediator.Send(command);
            
            return Ok(new { message = "Customer updated successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while updating the customer", details = ex.Message });
        }
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var command = new DeleteCustomerCommand { Id = id };
            await _mediator.Send(command);
            
            return Ok(new { message = "Customer deleted successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while deleting the customer", details = ex.Message });
        }
    }

    #endregion
}