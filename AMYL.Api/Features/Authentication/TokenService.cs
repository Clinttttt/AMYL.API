using AMYL.Api.Data;
using AMYL.Api.Domain;
using AMYL.Api.Shared.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace AMYL.Api.Features.Authentication;

public sealed record TokenResponse(string AccessToken, string RefreshToken);

public interface ITokenService
{
    Task<TokenResponse> CreateTokenResponseAsync(Users user, CancellationToken cancellationToken);
}

/// <summary>
/// Provider owned by the Authentication feature: it has exactly one consumer,
/// so it lives beside the slices that use it rather than in a shared folder.
/// </summary>
public sealed class JwtTokenService(
    AppDbContext context,
    IConfiguration configuration,
    TimeProvider timeProvider) : ITokenService
{
    public async Task<TokenResponse> CreateTokenResponseAsync(
        Users user,
        CancellationToken cancellationToken)
    {
        var refreshToken = GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = timeProvider.GetUtcNow().UtcDateTime.AddDays(7);

        await context.SaveChangesAsync(cancellationToken);

        return new TokenResponse(CreateAccessToken(user), refreshToken);
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
