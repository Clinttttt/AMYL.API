using AMYL.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AMYL.Api.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<Users>
{
    public void Configure(EntityTypeBuilder<Users> builder)
    {
        builder
            .HasIndex(user => user.Email)
            .IsUnique();

        builder
            .HasIndex(user => user.UserName)
            .IsUnique();
    }
}
