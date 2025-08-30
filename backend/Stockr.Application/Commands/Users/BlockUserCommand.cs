using MediatR;

namespace Stockr.Application.Commands.Users;

public class BlockUserCommand : IRequest<Unit>
{
    public Guid Id { get; set; }
    public DateTime? BlockedUntil { get; set; }
}