using AMYL.Api.Abstractions.Messaging;

// Stand-ins for two slices whose request types share a name, exactly as real slices do:
// AMYL.Api.Features.Memories.Create.Command and ...Delete.Command are both named "Command".
// LoggingBehaviorTests uses them to prove the span name identifies the slice, not the type.

namespace AMYL.Tests.Fakes.Create
{
    public sealed record Command : ICommand;
}

namespace AMYL.Tests.Fakes.Delete
{
    public sealed record Command : ICommand;
}
