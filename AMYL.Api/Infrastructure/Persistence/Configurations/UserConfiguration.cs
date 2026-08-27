using AMYL.Api.Shared.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AMYL.Api.Infrastructure.Persistence.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<Users>
    {
        public void Configure(EntityTypeBuilder<Users> builder)
        {
      
            builder
                .HasIndex(x => x.Email)
                .IsUnique();

            builder
                .HasIndex(x => x.UserName)
                .IsUnique();
        }

    
    }
}
