using MediatR;

namespace Stockr.Application.Commands.Users;

public class UpdateUserCommand : IRequest<Unit>
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
}