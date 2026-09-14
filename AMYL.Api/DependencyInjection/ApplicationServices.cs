using AMYL.Api.Behaviors;
using AMYL.Api.Extensions;
using AMYL.Api.Infrastructure.Authentication;
using AMYL.Api.Middleware;
using FluentValidation;

namespace AMYL.Api.DependencyInjection;

public static class ApplicationServices
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(Program).Assembly;

        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);
        services.AddEndpoints(assembly);

        services.AddMediatR(options =>
        {
            options.RegisterServicesFromAssembly(assembly);

            // Registration order is execution order, outermost first.
            // Logging wraps validation so rejected commands are still logged and traced.
            options.AddOpenBehavior(typeof(LoggingBehavior<,>));
            options.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddSingleton(TimeProvider.System);
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddHealthChecks();

        return services;
    }
}
