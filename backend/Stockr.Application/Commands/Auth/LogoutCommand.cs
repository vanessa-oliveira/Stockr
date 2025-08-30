using MediatR;

namespace Stockr.Application.Commands.Auth;

public class LogoutCommand : IRequest<bool>
{
    public Guid UserId { get; set; }
    public string? Token { get; set; }
}