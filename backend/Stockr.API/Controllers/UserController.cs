using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stockr.Application.Commands.Users;
using Stockr.Application.Queries.Users;

namespace Stockr.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserController(IMediator mediator)
    {
        _mediator = mediator;
    }

    #region Queries

    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        var query = new GetAllUsersQuery();
        var users = await _mediator.Send(query);
        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetUserByIdQuery { Id = id };
        var user = await _mediator.Send(query);
        
        if (user == null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    [HttpGet("email/{email}")]
    public async Task<IActionResult> GetByEmail(string email)
    {
        var query = new GetUserByEmailQuery { Email = email };
        var user = await _mediator.Send(query);
        
        if (user == null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    [HttpGet("blocked")]
    public async Task<IActionResult> GetBlockedUsers()
    {
        var query = new GetBlockedUsersQuery();
        var users = await _mediator.Send(query);
        return Ok(users);
    }

    #endregion

    #region Commands

    [HttpPost]
    [Authorize(Policy = "TenantAdmin")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserCommand command)
    {
        try
        {
            await _mediator.Send(command);
            return Created("", new { message = "User created successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while creating the user", details = ex.Message });
        }
    }
    
    [HttpPut("{id}")]
    [Authorize(Policy = "TenantAdmin")]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserCommand command)
    {
        try
        {
            command.Id = id;
            await _mediator.Send(command);
            return Ok(new { message = "User updated successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while updating the user", details = ex.Message });
        }
    }
    
    [HttpDelete("{id}")]
    [Authorize(Policy = "TenantAdmin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var command = new DeleteUserCommand { Id = id };
            await _mediator.Send(command);
            return Ok(new { message = "User deleted successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while deleting the user", details = ex.Message });
        }
    }

    [HttpPatch("{id}/block")]
    [Authorize(Policy = "TenantAdmin")]
    public async Task<IActionResult> BlockUser(Guid id, [FromBody] BlockUserCommand command)
    {
        try
        {
            command.Id = id;
            await _mediator.Send(command);
            return Ok(new { message = "User blocked successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while blocking the user", details = ex.Message });
        }
    }

    [HttpPatch("{id}/unblock")]
    [Authorize(Policy = "TenantAdmin")]
    public async Task<IActionResult> UnblockUser(Guid id)
    {
        try
        {
            var command = new UnblockUserCommand { Id = id };
            await _mediator.Send(command);
            return Ok(new { message = "User unblocked successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while unblocking the user", details = ex.Message });
        }
    }

    [HttpPatch("{id}/change-password")]
    public async Task<IActionResult> ChangePassword(Guid id, [FromBody] ChangePasswordCommand command)
    {
        try
        {
            command.Id = id;
            await _mediator.Send(command);
            return Ok(new { message = "Password changed successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while changing the password", details = ex.Message });
        }
    }

    #endregion
}