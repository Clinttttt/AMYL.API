namespace AMYL.Api.Abstractions.Endpoints;

/// <summary>
/// Implemented once per use case. Discovered by assembly scan in
/// <c>Extensions/EndpointExtensions.cs</c>, so adding a slice edits no shared file.
/// </summary>
public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}
