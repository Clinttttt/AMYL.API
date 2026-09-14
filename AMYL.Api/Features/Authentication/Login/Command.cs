using AMYL.Api.Abstractions.Messaging;

namespace AMYL.Api.Features.Authentication.Login;

public sealed record Command(string UserName, string Password) : ICommand<TokenResponse>;
