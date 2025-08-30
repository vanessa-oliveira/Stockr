using MediatR;
using Stockr.Application.Models;

namespace Stockr.Application.Queries.Users;

public class GetBlockedUsersQuery : IRequest<IEnumerable<UserViewModel>>
{
}