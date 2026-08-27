using AMYL.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace AMYL.Api.Shared.Extensions;

public static class PersistenceServices
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        return services;
    }
}
