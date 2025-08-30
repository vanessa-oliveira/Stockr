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
    IRequestHandler<ChangePasswordCommand, Unit>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;

    public UserCommandHandler(IUserRepository userRepository, IPasswordService passwordService)
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
    }

    public async Task<Unit> Handle(CreateUserCommand command, CancellationToken cancellationToken)
    {
        var emailExists = await _userRepository.EmailExistsAsync(command.Email);
        if (emailExists)
        {
            throw new InvalidOperationException("Email already exists");
        }

        // Valida a senha antes de criar o usuário
        // if (!_passwordService.IsValidPassword(command.Password))
        // {
        //     throw new InvalidOperationException("Password does not meet security requirements. It must have at least 8 characters, including uppercase, lowercase, number and special character.");
        // }

        var user = new User
        {
            Name = command.Name,
            Email = command.Email,
            Password = await _passwordService.HashPassword(command.Password),
            Role = Enum.Parse<UserRole>(command.Role),
            LastPasswordChange = DateTime.UtcNow
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

        // Verifica se o novo email já existe em outro usuário
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

        await _userRepository.DeleteAsync(user);
        return Unit.Value;
    }

    public async Task<Unit> Handle(BlockUserCommand command, CancellationToken cancellationToken)
    {
        var success = await _userRepository.BlockUserAsync(command.Id, command.BlockedUntil);
        if (!success)
        {
            throw new ArgumentException("User not found or could not be blocked");
        }

        return Unit.Value;
    }

    public async Task<Unit> Handle(UnblockUserCommand command, CancellationToken cancellationToken)
    {
        var success = await _userRepository.UnblockUserAsync(command.Id);
        if (!success)
        {
            throw new ArgumentException("User not found or could not be unblocked");
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

        // Valida a nova senha antes de alterar
        if (!_passwordService.IsValidPassword(command.NewPassword))
        {
            throw new InvalidOperationException("Password does not meet security requirements. It must have at least 8 characters, including uppercase, lowercase, number and special character.");
        }

        user.Password = await _passwordService.HashPassword(command.NewPassword);
        user.LastPasswordChange = DateTime.UtcNow;
        user.LoginAttempts = 0; // Reset attempts on password change
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        return Unit.Value;
    }
}