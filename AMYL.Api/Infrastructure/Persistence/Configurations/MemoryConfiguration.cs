using AMYL.Api.Shared.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AMYL.Api.Infrastructure.Persistence.Configurations
{
    public class MemoryConfiguration : IEntityTypeConfiguration<AMYL.Api.Shared.Domain.Entities.Memory>
    {
        public void Configure(EntityTypeBuilder<Memory> builder)
        {
            builder.HasOne(s=> s.Users)
                .WithMany(s=> s.Memories)
                .HasForeignKey(s=>s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(s => new
            {
              s.UserId,
              s.CreatedAt
            });

        }
    }
}
