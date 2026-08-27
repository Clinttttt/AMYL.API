using Microsoft.AspNetCore.Identity;

namespace AMYL.Api.Features.Authentication;

public interface IPasswordHasherService
{
    string HashPassword(string password);
    bool VerifyPassword(string hashedPassword, string providedPassword);
}

/// <summary>
/// Provider owned by the Authentication feature. The "Service" suffix is kept
/// deliberately: the bare name would collide with
/// <see cref="PasswordHasher{TUser}"/> from ASP.NET Core Identity.
/// </summary>
public sealed class PasswordHasherService(PasswordHasher<object> passwordHasher)
    : IPasswordHasherService
{
    public string HashPassword(string password) =>
        passwordHasher.HashPassword(null!, password);

    public bool VerifyPassword(string hashedPassword, string providedPassword) =>
        passwordHasher.VerifyHashedPassword(null!, hashedPassword, providedPassword)
            == PasswordVerificationResult.Success;
}
