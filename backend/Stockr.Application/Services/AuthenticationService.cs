using Mapster;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Stockr.Application.Dtos;
using Stockr.Application.Models;
using Stockr.Domain.Entities;
using Stockr.Infrastructure.Repositories;

namespace Stockr.Application.Services;

public interface IAuthenticationService
{
    /// <summary>
    /// Realiza login do usuário com email e senha
    /// </summary>
    /// <param name="request">Dados da requisição de login</param>
    /// <returns>Resultado da autenticação</returns>
    Task<AuthenticationResult> LoginAsync(LoginRequest request);
    
    /// <summary>
    /// Realiza logout do usuário
    /// </summary>
    /// <param name="userId">ID do usuário</param>
    /// <param name="token">Token JWT (opcional)</param>
    /// <returns>Sucesso do logout</returns>
    bool Logout(Guid userId, string? token = null);
}

public class AuthenticationOptions
{
    public int MaxLoginAttempts { get; set; } = 5;
    public int BlockDurationMinutes { get; set; } = 30;
    public int TokenExpirationHours { get; set; } = 8;
}

public class AuthenticationService : IAuthenticationService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<AuthenticationService> _logger;
    private readonly AuthenticationOptions _options;
    
    private static class Messages
    {
        public const string InvalidCredentials = "Invalid credentials";
        public const string UserBlocked = "User account is blocked";
        public const string TooManyAttempts = "Too many failed login attempts. Account blocked for {0} minutes.";
        public const string AuthenticationFailed = "Authentication failed";
    }

    public AuthenticationService(
        IUserRepository userRepository,
        IPasswordService passwordService,
        IJwtTokenService jwtTokenService,
        ILogger<AuthenticationService> logger,
        IOptions<AuthenticationOptions> options)
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<AuthenticationResult> LoginAsync(LoginRequest request)
    {
        if (!IsValidLoginRequest(request))
        {
            return AuthenticationResult.Failure(Messages.InvalidCredentials);
        }

        try
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null)
            {
                return AuthenticationResult.Failure(Messages.InvalidCredentials);
            }

            // Verifica e trata bloqueio do usuário
            var blockCheckResult = await HandleUserBlockStatusAsync(user, request);
            if (!blockCheckResult.IsSuccess)
            {
                return blockCheckResult;
            }

            // Verifica a senha
            var passwordResult = await ValidatePasswordAsync(user, request);
            if (!passwordResult.IsSuccess)
            {
                return passwordResult;
            }

            // Autenticação bem-sucedida
            return await CreateSuccessfulAuthenticationAsync(user, request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during authentication for user {Email}", request.Email);
            return AuthenticationResult.Failure(Messages.AuthenticationFailed);
        }
    }

    public bool Logout(Guid userId, string? token = null)
    {
        try
        {
            //TODO: trabalhar com invalidação/revogação de tokens
            _logger.LogInformation("User {UserId} logged out successfully", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout for user {UserId}", userId);
            return false;
        }
    }

    #region Private Methods

    private static bool IsValidLoginRequest(LoginRequest request)
    {
        return !string.IsNullOrWhiteSpace(request.Email) && 
               !string.IsNullOrWhiteSpace(request.Password);
    }

    private async Task<AuthenticationResult> HandleUserBlockStatusAsync(User user, LoginRequest request)
    {
        if (!user.IsBlocked)
        {
            return AuthenticationResult.Success();
        }

        var hasBlockExpired = user.BlockedUntil.HasValue && user.BlockedUntil.Value <= DateTime.UtcNow;

        if (hasBlockExpired)
        {
            await UnblockUserAsync(user);
            return AuthenticationResult.Success();
        }
        
        return AuthenticationResult.Blocked(Messages.UserBlocked);
    }

    private async Task UnblockUserAsync(User user)
    {
        await _userRepository.UnblockUserAsync(user.Id);
        user.IsBlocked = false;
        user.BlockedUntil = null;
        user.LoginAttempts = 0;
        
        _logger.LogInformation("User {UserId} was automatically unblocked", user.Id);
    }

    private async Task<AuthenticationResult> ValidatePasswordAsync(User user, LoginRequest request)
    {
        if (await _passwordService.VerifyPassword(request.Password, user.Password))
        {
            return AuthenticationResult.Success();
        }

        return await HandleFailedPasswordAttemptAsync(user, request);
    }

    private async Task<AuthenticationResult> HandleFailedPasswordAttemptAsync(User user, LoginRequest request)
    {
        user.LoginAttempts++;
        user.LastFailedLogin = DateTime.UtcNow;

        if (user.LoginAttempts >= _options.MaxLoginAttempts)
        {
            await BlockUserAsync(user);
            
            var message = string.Format(Messages.TooManyAttempts, _options.BlockDurationMinutes);
            return AuthenticationResult.Blocked(message);
        }

        await _userRepository.UpdateAsync(user);
        
        return AuthenticationResult.Failure(Messages.InvalidCredentials);
    }

    private async Task BlockUserAsync(User user)
    {
        user.IsBlocked = true;
        user.BlockedUntil = DateTime.UtcNow.AddMinutes(_options.BlockDurationMinutes);
        await _userRepository.UpdateAsync(user);
        
        _logger.LogWarning("User {UserId} blocked due to {MaxAttempts} failed login attempts", 
            user.Id, _options.MaxLoginAttempts);
    }

    private async Task<AuthenticationResult> CreateSuccessfulAuthenticationAsync(User user, LoginRequest request)
    {
        await ResetLoginAttemptsAsync(user);
        
        var userViewModel = user.Adapt<UserViewModel>();
        var token = _jwtTokenService.GenerateToken(userViewModel);

        return AuthenticationResult.Success(
            user: userViewModel,
            token: token,
            tokenExpiration: DateTime.UtcNow.AddHours(_options.TokenExpirationHours)
        );
    }

    private async Task ResetLoginAttemptsAsync(User user)
    {
        user.LoginAttempts = 0;
        user.LastLoginDate = DateTime.UtcNow;
        user.LastFailedLogin = null;
        await _userRepository.UpdateAsync(user);
    }

    #endregion
}

public class AuthenticationResult
{
    public bool IsSuccess { get; private init; }
    public UserViewModel? User { get; private init; }
    public string? Token { get; private init; }
    public string? Message { get; private init; }
    public bool IsBlocked { get; private init; }
    public DateTime? TokenExpiration { get; private init; }

    private AuthenticationResult() { }

    public static AuthenticationResult Success(UserViewModel? user = null, string? token = null, DateTime? tokenExpiration = null)
        => new()
        {
            IsSuccess = true,
            User = user,
            Token = token,
            TokenExpiration = tokenExpiration
        };

    public static AuthenticationResult Failure(string message)
        => new()
        {
            IsSuccess = false,
            Message = message,
            IsBlocked = false
        };

    public static AuthenticationResult Blocked(string message)
        => new()
        {
            IsSuccess = false,
            Message = message,
            IsBlocked = true
        };
}