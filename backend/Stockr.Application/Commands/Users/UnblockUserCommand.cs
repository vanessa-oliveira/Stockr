using MediatR;

namespace Stockr.Application.Commands.Users;

public class UnblockUserCommand : IRequest<Unit>
{
    public Guid Id { get; set; }
}