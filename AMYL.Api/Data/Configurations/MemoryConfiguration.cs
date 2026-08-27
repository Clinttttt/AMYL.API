using AMYL.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AMYL.Api.Data.Configurations;

public sealed class MemoryConfiguration : IEntityTypeConfiguration<Memory>
{
    public void Configure(EntityTypeBuilder<Memory> builder)
    {
        builder.HasOne(memory => memory.Users)
            .WithMany(user => user.Memories)
            .HasForeignKey(memory => memory.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(memory => new
        {
            memory.UserId,
            memory.CreatedAt
        });
    }
}
