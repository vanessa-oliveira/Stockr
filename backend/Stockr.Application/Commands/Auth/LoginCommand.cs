using MediatR;
using AuthenticationResult = Stockr.Application.Services.AuthenticationResult;

namespace Stockr.Application.Commands.Auth;

public class LoginCommand : IRequest<AuthenticationResult>
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}