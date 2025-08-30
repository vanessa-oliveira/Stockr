using MediatR;

namespace Stockr.Application.Commands.Users;

public class DeleteUserCommand : IRequest<Unit>
{
    public Guid Id { get; set; }
}