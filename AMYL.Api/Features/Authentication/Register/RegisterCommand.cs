using AMYL.Api.Shared.Abstractions;
using MediatR;

namespace AMYL.Api.Features.Authentication.Register
{
    public sealed record RegisterCommand(
        string UserName,
        string FullName,
        string Password,
        string Email) : IRequest<Result>;

}
