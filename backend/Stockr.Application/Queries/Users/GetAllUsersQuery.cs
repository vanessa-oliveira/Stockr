using MediatR;
using Stockr.Application.Models;

namespace Stockr.Application.Queries.Users;

public class GetAllUsersQuery : IRequest<IEnumerable<UserViewModel>>
{
}