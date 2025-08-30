using MediatR;

namespace Stockr.Application.Commands.Users;

public class ChangePasswordCommand : IRequest<Unit>
{
    public Guid Id { get; set; }
    public string NewPassword { get; set; }
}