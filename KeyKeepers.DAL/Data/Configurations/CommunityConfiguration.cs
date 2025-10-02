using KeyKeepers.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KeyKeepers.DAL.Data.Configurations;

public class CommunityConfiguration : IEntityTypeConfiguration<Community>
{
    public void Configure(EntityTypeBuilder<Community> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .ValueGeneratedOnAdd();

        builder.Property(c => c.Name)
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .IsRequired()
            .ValueGeneratedOnAdd();

        builder.HasMany(x => x.CommunityUsers)
            .WithOne(x => x.Community)
            .HasForeignKey(x => x.CommunityId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Categories)
            .WithOne(x => x.Community)
            .HasForeignKey(x => x.CommunityId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
