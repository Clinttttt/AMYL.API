using System.Reflection;
using AMYL.Api.Abstractions.Endpoints;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AMYL.Api.Extensions;

public static class EndpointExtensions
{
    /// <summary>
    /// Registers every <see cref="IEndpoint"/> in the assembly. One reflection pass at startup;
    /// in exchange, a new slice never edits a shared module file.
    /// </summary>
    public static IServiceCollection AddEndpoints(this IServiceCollection services, Assembly assembly)
    {
        ServiceDescriptor[] descriptors = assembly
            .DefinedTypes
            .Where(type => type is { IsAbstract: false, IsInterface: false, IsGenericTypeDefinition: false }
                           && type.IsAssignableTo(typeof(IEndpoint)))
            .Select(type => ServiceDescriptor.Transient(typeof(IEndpoint), type))
            .ToArray();

        services.TryAddEnumerable(descriptors);

        return services;
    }

    public static IApplicationBuilder MapEndpoints(
        this WebApplication app,
        RouteGroupBuilder? routeGroup = null)
    {
        IEndpointRouteBuilder builder = routeGroup is null ? app : routeGroup;

        foreach (IEndpoint endpoint in app.Services.GetRequiredService<IEnumerable<IEndpoint>>())
        {
            endpoint.MapEndpoint(builder);
        }

        return app;
    }
}
