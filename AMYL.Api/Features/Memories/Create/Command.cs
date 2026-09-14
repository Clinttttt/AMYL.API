using AMYL.Api.Abstractions.Messaging;
using AMYL.Api.Domain;

namespace AMYL.Api.Features.Memories.Create;

// MIGRATION ITEM: IFormFile on a command keeps a transport type in the message.
// The design calls for binding multipart at the endpoint and mapping to a
// feature-owned upload model first. Carried over unchanged here so this
// restructure introduces no behavioural change; fix it in its own commit.
public sealed record Command(
    string Title,
    string Description,
    MemoryType MemoryType,
    IFormFile VideoUrl,
    IFormFile? ImageUrl,
    IFormFile? AudioUrl,
    string? SecretNote) : ICommand;
