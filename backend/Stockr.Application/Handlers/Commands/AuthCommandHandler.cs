using MediatR;
using Stockr.Application.Commands.Auth;
using Stockr.Application.Dtos;
using Stockr.Application.Services;

namespace Stockr.Application.Handlers.Commands;

public class AuthCommandHandler :
    IRequestHandler<LoginCommand, AuthenticationResult>,
    IRequestHandler<LogoutCommand, bool>
{
    private readonly IAuthenticationService _authenticationService;

    public AuthCommandHandler(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    public async Task<AuthenticationResult> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        return await _authenticationService.LoginAsync(new LoginRequest
        {
            Email = command.Email,
            Password = command.Password
        });
    }

    public async Task<bool> Handle(LogoutCommand command, CancellationToken cancellationToken)
    {
        return _authenticationService.Logout(command.UserId, command.Token);
    }
}