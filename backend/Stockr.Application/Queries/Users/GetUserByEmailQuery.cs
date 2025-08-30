using MediatR;
using Stockr.Application.Models;

namespace Stockr.Application.Queries.Users;

public class GetUserByEmailQuery : IRequest<UserViewModel?>
{
    public string Email { get; set; }
}