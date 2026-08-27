using AMYL.Api.Features.Authentication;
using Microsoft.AspNetCore.Identity;

namespace AMYL.Api.Infrastructure.Identity
{
    public class PasswordHasherService(PasswordHasher<object> passwordHasher) : IPasswordHasherService
    {
        public string HashPassword(string password)
        {
            return passwordHasher.HashPassword(null!, password);
        }
        public bool VerifyPassword(string hashedPassword, string providedPassword)
        {
            var result = passwordHasher.VerifyHashedPassword(null!, hashedPassword, providedPassword);
            return result == PasswordVerificationResult.Success;
        }

    }
}
