using AMYL.Api.Shared.Abstractions;
using AMYL.Api.Features.Authentication.Login;
using MediatR;

namespace AMYL.Api.Features.Authentication.Login
{
    public sealed record LoginCommand(string UserName, string Password) : IRequest<Result<TokenResponseDto>>;

}
