using AMYL.Api.Features.Authentication;
using AMYL.Api.Features.Authentication.Login;
using AMYL.Api.Infrastructure.Persistence;
using AMYL.Api.Shared.Domain.Entities;
using AMYL.Api.Shared.Security;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace AMYL.Api.Infrastructure.Identity;

public sealed class TokenService(
    AppDbContext context,
    IConfiguration configuration,
    TimeProvider timeProvider) : ITokenService
{
    public async Task<TokenResponseDto> CreateTokenResponseAsync(
        Users user,
        CancellationToken cancellationToken)
    {
        var refreshToken = GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = timeProvider.GetUtcNow().UtcDateTime.AddDays(7);

        await context.SaveChangesAsync(cancellationToken);

        return new TokenResponseDto(CreateAccessToken(user), refreshToken);
    }

    private string CreateAccessToken(Users user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName ?? string.Empty),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        claims.AddRange(RolePermissions.GetPermissions(user)
            .Select(permission => new Claim(CustomClaimTypes.Permission, permission)));

        var secretKey = configuration["JwtSettings:SecretKey"]
            ?? throw new InvalidOperationException("JWT secret key is not configured.");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: timeProvider.GetUtcNow().UtcDateTime.AddDays(7),
            signingCredentials: credentials,
            audience: configuration["JwtSettings:Audience"],
            issuer: configuration["JwtSettings:Issuer"]);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(randomBytes);
    }
}
