using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stockr.Application.Commands.Inventory;
using Stockr.Application.Queries.Inventory;
using Stockr.Domain.Enums;

namespace Stockr.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InventoryController : ControllerBase
{
    private readonly IMediator _mediator;

    public InventoryController(IMediator mediator)
    {
        _mediator = mediator;
    }

    #region Queries

    [HttpGet]
    public async Task<IActionResult> GetInventoriesPaged([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var query = new GetInventoriesPagedQuery()
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetInventoryByIdQuery { Id = id };
        var inventory = await _mediator.Send(query);
        
        if (inventory == null)
        {
            return NotFound();
        }

        return Ok(inventory);
    }

    [HttpGet("product/{productId}")]
    public async Task<IActionResult> GetByProductId(Guid productId)
    {
        var query = new GetInventoryByProductIdQuery { ProductId = productId };
        var inventory = await _mediator.Send(query);
        
        if (inventory == null)
        {
            return NotFound();
        }

        return Ok(inventory);
    }

    #endregion

    #region Commands

    [HttpPost]
    public async Task<IActionResult> CreateInventory([FromBody] CreateInventoryCommand command)
    {
        await _mediator.Send(command);
        return Created("", new { message = "Inventory created successfully" });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateInventory(Guid id, [FromBody] UpdateInventoryCommand command)
    {
        command.Id = id;
        await _mediator.Send(command);
        return Ok(new { message = "Inventory updated successfully" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteInventoryCommand { Id = id };
        await _mediator.Send(command);
        return Ok(new { message = "Inventory deleted successfully" });
    }

    #endregion
}