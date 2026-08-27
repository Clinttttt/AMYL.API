using AMYL.Api.Features.Authentication;
using AMYL.Api.Features.Memories;
using AMYL.Api.Infrastructure.Caching;
using AMYL.Api.Infrastructure.Identity;
using AMYL.Api.Infrastructure.Persistence;
using AMYL.Api.Infrastructure.Storage;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AMYL.Api.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IFileService, FileService>();
        services.AddScoped<PasswordHasher<object>>();
        services.AddScoped<IPasswordHasherService, PasswordHasherService>();
        services.AddSingleton(TimeProvider.System);
        services.AddCaching(configuration);

        return services;
    }
}
