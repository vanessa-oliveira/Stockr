using MediatR;
using AuthenticationResult = Stockr.Application.Services.AuthenticationResult;

namespace Stockr.Application.Commands.Auth;

public class LoginCommand : IRequest<AuthenticationResult>
{
    public string Email { get; set; }
    public string Password { get; set; }
}