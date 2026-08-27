using AMYL.Api.Features.Authentication.Login;
using AMYL.Api.Shared.Domain.Entities;

namespace AMYL.Api.Features.Authentication
{
    public interface ITokenService
    {
        Task<TokenResponseDto> CreateTokenResponseAsync(Users user, CancellationToken cancellationToken);
    }
}
