using MediatR;

namespace Stockr.Application.Commands.Users;

public class UpdatePersonalInfoCommand : IRequest<Unit>
{
    public Guid UserId { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
}