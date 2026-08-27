using Microsoft.OpenApi;

namespace AMYL.Api.Shared.Extensions;

public static class OpenApiServices
{
    public static IServiceCollection AddApiOpenApi(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddOpenApi();

        services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Description = "Please enter a valid token",
                Scheme = "bearer",
                BearerFormat = "JWT",
                Name = "Authorization"
            });

            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = []
            });
        });

        return services;
    }

    public static IServiceCollection AddApiCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("Default", policy =>
            {
                policy.AllowAnyHeader();
                policy.AllowAnyMethod();
            });
        });

        return services;
    }
}
