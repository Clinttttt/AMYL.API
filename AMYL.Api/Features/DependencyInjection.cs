using AMYL.Api.Shared.Behaviors;
using FluentValidation;

namespace AMYL.Api.Features;

public static class DependencyInjection
{
    public static IServiceCollection AddFeatures(this IServiceCollection services)
    {
        var assembly = typeof(AssemblyReference).Assembly;

        services.AddValidatorsFromAssembly(assembly);
        services.AddMediatR(options =>
        {
            options.RegisterServicesFromAssembly(assembly);
            options.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        return services;
    }
}
