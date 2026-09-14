using System.Reflection;
using AMYL.Api.Abstractions.Endpoints;
using MediatR;

namespace AMYL.Tests;

/// <summary>
/// Dispatch and routing are resolved at runtime: a message with no handler throws on
/// <c>Send</c>, and an endpoint the scan misses is a silent 404. Nothing else in the suite
/// catches either, so these two tests are the safety net.
/// </summary>
public sealed class WiringTests
{
    private static readonly Assembly Api = typeof(AMYL.Api.Abstractions.Endpoints.IEndpoint).Assembly;

    [Fact]
    public void EveryMessage_HasExactlyOneHandler()
    {
        var handlerInterfaces = Api.GetTypes()
            .Where(type => type is { IsAbstract: false, IsInterface: false })
            .SelectMany(type => type.GetInterfaces())
            .Where(i => i.IsGenericType
                        && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))
            .ToArray();

        var messages = Api.GetTypes()
            .Where(type => type is { IsAbstract: false, IsInterface: false })
            .SelectMany(type => type.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>))
                .Select(i => new { Message = type, Response = i.GenericTypeArguments[0] }))
            .ToArray();

        Assert.NotEmpty(messages);

        var orphans = messages
            .Where(m => handlerInterfaces.Count(h =>
                h.GenericTypeArguments[0] == m.Message &&
                h.GenericTypeArguments[1] == m.Response) != 1)
            .Select(m => m.Message.FullName)
            .ToArray();

        Assert.Empty(orphans);
    }

    [Fact]
    public void EveryUseCase_HasAnEndpoint()
    {
        var endpoints = Api.GetTypes()
            .Where(type => type is { IsAbstract: false, IsInterface: false }
                           && typeof(IEndpoint).IsAssignableFrom(type))
            .ToArray();

        // One endpoint per use-case folder under Features/.
        var useCaseNamespaces = Api.GetTypes()
            .Where(type => type is { IsAbstract: false, IsInterface: false })
            .Where(type => type.GetInterfaces().Any(i =>
                i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>)))
            .Select(type => type.Namespace)
            .Distinct()
            .ToArray();

        var endpointNamespaces = endpoints.Select(type => type.Namespace).ToHashSet();

        var missing = useCaseNamespaces
            .Where(ns => !endpointNamespaces.Contains(ns))
            .ToArray();

        Assert.Empty(missing);
        Assert.Equal(useCaseNamespaces.Length, endpoints.Length);
    }
}
