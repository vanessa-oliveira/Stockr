using MediatR;
using Stockr.Domain.Enums;

namespace Stockr.Application.Commands.Users;

public class CreateUserCommand : IRequest<Unit>
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public UserRole Role { get; set; }
}