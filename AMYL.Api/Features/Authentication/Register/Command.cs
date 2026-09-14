using AMYL.Api.Abstractions.Messaging;

namespace AMYL.Api.Features.Authentication.Register;

public sealed record Command(
    string UserName,
    string FullName,
    string Password,
    string Email) : ICommand;
