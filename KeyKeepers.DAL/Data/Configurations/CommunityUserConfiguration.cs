using KeyKeepers.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KeyKeepers.DAL.Data.Configurations;

public class CommunityUserConfiguration : IEntityTypeConfiguration<CommunityUser>
{
    public void Configure(EntityTypeBuilder<CommunityUser> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.UserName)
            .IsRequired();

        builder.HasIndex(x => x.UserName)
            .IsUnique();

        builder.Property(x => x.PasswordHash)
            .IsRequired();

        builder.Property(x => x.Role)
            .IsRequired();

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.HasOne(x => x.User)
            .WithMany(x => x.CommunityUsers)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Community)
            .WithMany(x => x.CommunityUsers)
            .HasForeignKey(x => x.CommunityId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.CreatedAt)
            .IsRequired()
            .ValueGeneratedOnAdd();

        builder.HasMany(x => x.PrivateCategories)
            .WithOne(x => x.CommunityUser)
            .HasForeignKey(x => x.CommunityUserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
