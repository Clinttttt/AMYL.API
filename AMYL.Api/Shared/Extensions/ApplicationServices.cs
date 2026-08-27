using AMYL.Api.Shared.Authorization;
using AMYL.Api.Shared.Behaviors;
using AMYL.Api.Shared.Middleware;
using FluentValidation;

namespace AMYL.Api.Shared.Extensions;

public static class ApplicationServices
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(Program).Assembly;

        services.AddValidatorsFromAssembly(assembly);
        services.AddMediatR(options =>
        {
            options.RegisterServicesFromAssembly(assembly);
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
