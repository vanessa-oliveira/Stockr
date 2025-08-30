using MediatR;
using Microsoft.AspNetCore.Mvc;
using Stockr.Application.Commands.Auth;

namespace Stockr.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            
            if (result.IsSuccess)
            {
                return Ok(new
                {
                    success = true,
                    message = "Login successful",
                    user = result.User,
                    token = result.Token,
                    tokenExpiration = result.TokenExpiration,
                });
            }
            
            return Unauthorized(new
            {
                success = false,
                message = result.Message
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred during authentication",
                details = ex.Message
            });
        }
    }
    
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutCommand command)
    {
        try
        {
            var success = await _mediator.Send(command);
            
            if (success)
            {
                return Ok(new
                {
                    success = true,
                    message = "Logout successful"
                });
            }
            
            return BadRequest(new
            {
                success = false,
                message = "Logout failed"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred during logout",
                details = ex.Message
            });
        }
    }
}