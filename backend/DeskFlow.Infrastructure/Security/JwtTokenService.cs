using DeskFlow.Application.Abstractions;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DeskFlow.Domain.Models;
using Microsoft.IdentityModel.Tokens;

namespace DeskFlow.Infrastructure.Security;

public class JwtTokenService(IConfiguration configuration) : ITokenService
{
    public (string Token, DateTime ExpiresAt) Create(User user)
    {
        var key = configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key is not configured.");
        var issuer = configuration["Jwt:Issuer"] ?? "DeskFlow";
        var audience = configuration["Jwt:Audience"] ?? "DeskFlow.Web";
        var expirationMinutes = configuration.GetValue("Jwt:ExpirationMinutes", 480);
        var expiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(issuer, audience, claims, expires: expiresAt, signingCredentials: credentials);
        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
