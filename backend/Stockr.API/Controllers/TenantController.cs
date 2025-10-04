using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stockr.Application.Commands.Tenants;

namespace Stockr.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TenantController : ControllerBase
{
    private readonly IMediator _mediator;

    public TenantController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpPost("signup")]
    [AllowAnonymous]
    public async Task<IActionResult> Signup([FromBody] TenantSignupCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                return Ok(new
                {
                    success = true,
                    message = "Tenant criado com sucesso",
                    user = result.User,
                    token = result.Token,
                    tokenExpiration = result.TokenExpiration
                });
            }

            return BadRequest(new
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
                message = "Ocorreu um erro ao criar o tenant",
                details = ex.Message
            });
        }
    }
}