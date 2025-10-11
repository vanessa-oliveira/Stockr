using MediatR;
using Stockr.Application.Commands.Users;
using Stockr.Application.Services;
using Stockr.Domain.Entities;
using Stockr.Domain.Enums;
using Stockr.Infrastructure.Repositories;

namespace Stockr.Application.Handlers.Commands;

public class UserCommandHandler :
    IRequestHandler<CreateUserCommand, Unit>,
    IRequestHandler<UpdateUserCommand, Unit>,
    IRequestHandler<DeleteUserCommand, Unit>,
    IRequestHandler<BlockUserCommand, Unit>,
    IRequestHandler<UnblockUserCommand, Unit>,
    IRequestHandler<ChangePasswordCommand, Unit>,
    IRequestHandler<UpdatePersonalInfoCommand, Unit>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;
    private readonly ITenantService _tenantService;

    public UserCommandHandler(
        IUserRepository userRepository,
        IPasswordService passwordService,
        ITenantService tenantService)
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
        _tenantService = tenantService;
    }

    public async Task<Unit> Handle(CreateUserCommand command, CancellationToken cancellationToken)
    {
        var currentTenantId = _tenantService.GetCurrentTenantId();
        if (!currentTenantId.HasValue)
        {
            throw new UnauthorizedAccessException("User must belong to a tenant to create internal users");
        }
        
        var emailExists = await _userRepository.EmailExistsAsync(command.Email);
        if (emailExists)
        {
            throw new InvalidOperationException("Email already exists");
        }
        
        if (!_passwordService.IsValidPassword(command.Password))
        {
            throw new InvalidOperationException("Password does not meet security requirements. It must have at least 8 characters, including uppercase, lowercase, number and special character.");
        }
        
        if (command.Role == UserRole.TenantAdmin || command.Role == UserRole.SystemAdmin)
        {
            throw new InvalidOperationException("Cannot create users with TenantAdmin or SystemAdmin role. Use tenant signup for creating tenant admins.");
        }

        var user = new User
        {
            Name = command.Name,
            Email = command.Email,
            Password = await _passwordService.HashPassword(command.Password),
            Role = command.Role,
            TenantId = currentTenantId.Value,
            Active = true,
            IsBlocked = false,
            LoginAttempts = 0,
            LastPasswordChange = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user);
        return Unit.Value;
    }

    public async Task<Unit> Handle(UpdateUserCommand command, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(command.Id);
        if (user == null)
        {
            throw new ArgumentException("User not found");
        }
        
        if (!_tenantService.ValidateTenantAccess(user.TenantId))
        {
            throw new UnauthorizedAccessException("Cannot access users from other tenants");
        }
        
        if (user.Email != command.Email)
        {
            var emailExists = await _userRepository.EmailExistsAsync(command.Email);
            if (emailExists)
            {
                throw new InvalidOperationException("Email already exists");
            }
        }

        user.Name = command.Name;
        user.Email = command.Email;
        user.Role = Enum.Parse<UserRole>(command.Role);
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        return Unit.Value;
    }

    public async Task<Unit> Handle(DeleteUserCommand command, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(command.Id);
        if (user == null)
        {
            throw new ArgumentException("User not found");
        }
        
        if (!_tenantService.ValidateTenantAccess(user.TenantId))
        {
            throw new UnauthorizedAccessException("Cannot delete users from other tenants");
        }

        await _userRepository.DeleteAsync(user);
        return Unit.Value;
    }

    public async Task<Unit> Handle(BlockUserCommand command, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(command.Id);
        if (user == null)
        {
            throw new ArgumentException("User not found");
        }
        
        if (!_tenantService.ValidateTenantAccess(user.TenantId))
        {
            throw new UnauthorizedAccessException("Cannot block users from other tenants");
        }

        var success = await _userRepository.BlockUserAsync(command.Id, command.BlockedUntil);
        if (!success)
        {
            throw new ArgumentException("User could not be blocked");
        }

        return Unit.Value;
    }

    public async Task<Unit> Handle(UnblockUserCommand command, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(command.Id);
        if (user == null)
        {
            throw new ArgumentException("User not found");
        }
        
        if (!_tenantService.ValidateTenantAccess(user.TenantId))
        {
            throw new UnauthorizedAccessException("Cannot unblock users from other tenants");
        }

        var success = await _userRepository.UnblockUserAsync(command.Id);
        if (!success)
        {
            throw new ArgumentException("User could not be unblocked");
        }

        return Unit.Value;
    }

    public async Task<Unit> Handle(ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(command.Id);
        if (user == null)
        {
            throw new ArgumentException("User not found");
        }
        
        if (!_tenantService.ValidateTenantAccess(user.TenantId))
        {
            throw new UnauthorizedAccessException("Cannot change password for users from other tenants");
        }

        if (!await _passwordService.VerifyPassword(command.CurrentPassword, user.Password))
        {
            throw new InvalidOperationException("Password informed does not match current password");
        }
        
        if (!_passwordService.IsValidPassword(command.NewPassword))
        {
            throw new InvalidOperationException("Password does not meet security requirements. It must have at least 8 characters, including uppercase, lowercase, number and special character.");
        }

        user.Password = await _passwordService.HashPassword(command.NewPassword);
        user.LastPasswordChange = DateTime.UtcNow;
        user.LoginAttempts = 0;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        return Unit.Value;
    }

    public async Task<Unit> Handle(UpdatePersonalInfoCommand command, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(command.UserId);
        if (user == null)
        {
            throw new ArgumentException("User not found");
        }

        //Só iremos atualizar se alguma das informações tiverem sido realmente alteradas
        if ((!string.IsNullOrEmpty(command.Name) &&
             !command.Name.Equals(user.Name, StringComparison.InvariantCultureIgnoreCase)) ||
            (!string.IsNullOrEmpty(command.Email) &&
             !command.Email.Equals(user.Email, StringComparison.InvariantCultureIgnoreCase)))
        {
            user.Name = command.Name!;
            user.Email = command.Email!;
            await _userRepository.UpdateAsync(user);
        }
        
        return Unit.Value;
    }
}