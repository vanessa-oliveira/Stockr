using MediatR;
using Stockr.Application.Models;

namespace Stockr.Application.Queries.Users;

public class GetUserByIdQuery : IRequest<UserViewModel?>
{
    public Guid Id { get; set; }
}