using AMYL.Api.Abstractions.Messaging;

namespace AMYL.Api.Features.Memories.Delete;

public sealed record Command(Guid MemoryId) : ICommand;
