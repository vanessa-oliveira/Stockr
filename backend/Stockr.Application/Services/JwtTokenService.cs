using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Stockr.Application.Configuration;
using Stockr.Application.Models;

namespace Stockr.Application.Services;

public interface IJwtTokenService
{
    string GenerateTokenAsync(UserViewModel user);
}

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtConfiguration _jwtConfig;
    private readonly HashSet<string> _revokedTokens;

    public JwtTokenService(IOptions<JwtConfiguration> jwtConfig)
    {
        _jwtConfig = jwtConfig.Value;
        _revokedTokens = new HashSet<string>();
    }

    public string GenerateTokenAsync(UserViewModel user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtConfig.SecretKey);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(_jwtConfig.ExpirationInHours),
            Issuer = _jwtConfig.Issuer,
            Audience = _jwtConfig.Audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}