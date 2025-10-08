using Mapster;
using MediatR;
using Stockr.Application.Models;
using Stockr.Application.Queries.Users;
using Stockr.Domain.Common;
using Stockr.Infrastructure.Repositories;

namespace Stockr.Application.Handlers.Queries;

public class UserQueryHandler :
    IRequestHandler<GetAllUsersQuery, IEnumerable<UserViewModel>>,
    IRequestHandler<GetUsersPagedQuery, PagedResult<UserViewModel>>,
    IRequestHandler<GetUserByIdQuery, UserViewModel?>,
    IRequestHandler<GetUserByEmailQuery, UserViewModel?>,
    IRequestHandler<GetBlockedUsersQuery, IEnumerable<UserViewModel>>
{
    private readonly IUserRepository _userRepository;

    public UserQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<UserViewModel>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _userRepository.GetAllAsync();
        return users.Adapt<IEnumerable<UserViewModel>>();
    }
    
    public async Task<PagedResult<UserViewModel>> Handle(GetUsersPagedQuery request, CancellationToken cancellationToken)
    {
        var paginationParams = new PaginationParams()
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
        
        var users = await _userRepository.GetPagedAsync(paginationParams);
        return users.Adapt<PagedResult<UserViewModel>>();
    }

    public async Task<UserViewModel?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.Id);
        return user.Adapt<UserViewModel>();
    }

    public async Task<UserViewModel?> Handle(GetUserByEmailQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        return user.Adapt<UserViewModel>();
    }

    public async Task<IEnumerable<UserViewModel>> Handle(GetBlockedUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _userRepository.GetAllAsync();
        var blockedUsers = users.Where(u => u.IsBlocked);
        return blockedUsers.Adapt<IEnumerable<UserViewModel>>();
    }
}