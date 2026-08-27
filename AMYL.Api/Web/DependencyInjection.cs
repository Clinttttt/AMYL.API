using AMYL.Api.Shared.Security;
using AMYL.Api.Web.ExceptionHandling;
using AMYL.Api.Web.Extensions;
using AMYL.Api.Web.Security;

namespace AMYL.Api.Web;

public static class DependencyInjection
{
    public static IServiceCollection AddWeb(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddControllers();
        services.AddOpenApi();
        services.RateLimiter();
        services.AddApiAuthorization();
        services.AddApiAuthentication(configuration);
        services.AddSwagger();
        services.AddApiCors();
        services.AddHealthChecks();
        services.AddApiOpenTelemetry();
        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        return services;
    }
}
