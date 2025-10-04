using MediatR;
using Stockr.Application.Commands.Tenants;
using Stockr.Application.Models;
using Stockr.Application.Services;
using Stockr.Domain.Entities;
using Stockr.Domain.Enums;
using Stockr.Infrastructure.Repositories;

namespace Stockr.Application.Handlers.Commands;

public class TenantCommandHandler :
    IRequestHandler<TenantSignupCommand, AuthenticationResult>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;
    private readonly IJwtTokenService _jwtTokenService;

    public TenantCommandHandler(
        ITenantRepository tenantRepository,
        IUserRepository userRepository,
        IPasswordService passwordService,
        IJwtTokenService jwtTokenService)
    {
        _tenantRepository = tenantRepository;
        _userRepository = userRepository;
        _passwordService = passwordService;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthenticationResult> Handle(TenantSignupCommand command, CancellationToken cancellationToken)
    {
        if (!_passwordService.IsValidPassword(command.Password))
        {
            return AuthenticationResult.Failure(
                "A senha deve ter no mínimo 8 caracteres, incluindo maiúsculas, minúsculas, números e caracteres especiais.");
        }
        
        if (await _tenantRepository.DomainExistsAsync(command.TenantDomain))
        {
            return AuthenticationResult.Failure("Este domínio já está em uso. Escolha outro domínio.");
        }
        
        var existingUser = await _userRepository.GetByEmailAsync(command.AdminEmail);
        if (existingUser != null)
        {
            return AuthenticationResult.Failure("Email já cadastrado.");
        }
        
        var tenant = new Tenant
        {
            Name = command.TenantName,
            Domain = command.TenantDomain.ToLowerInvariant().Trim(),
            PlanType = command.PlanType,
            Active = true,
            CreatedAt = DateTime.UtcNow
        };

        await _tenantRepository.AddAsync(tenant);
        
        var hashedPassword = await _passwordService.HashPassword(command.Password);
        var adminUser = new User
        {
            Name = command.AdminName,
            Email = command.AdminEmail.ToLowerInvariant().Trim(),
            Password = hashedPassword,
            Role = UserRole.TenantAdmin,
            TenantId = tenant.Id,
            Active = true,
            IsBlocked = false,
            LoginAttempts = 0,
            CreatedAt = DateTime.UtcNow,
            LastPasswordChange = DateTime.UtcNow
        };

        await _userRepository.AddAsync(adminUser);
        
        var userViewModel = new UserViewModel
        {
            Id = adminUser.Id,
            Name = adminUser.Name,
            Email = adminUser.Email,
            Role = adminUser.Role,
            RoleName = adminUser.Role.ToString(),
            IsBlocked = adminUser.IsBlocked,
            Active = adminUser.Active,
            TenantId = adminUser.TenantId,
            CreatedAt = adminUser.CreatedAt
        };

        var token = _jwtTokenService.GenerateToken(userViewModel);

        return AuthenticationResult.Success(
            userViewModel,
            token,
            DateTime.UtcNow.AddHours(8)
        );
    }
}