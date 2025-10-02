using KeyKeepers.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KeyKeepers.DAL.Data.Configurations;

public class BaseCategoryConfiguration : IEntityTypeConfiguration<BaseCategory>
{
    public void Configure(EntityTypeBuilder<BaseCategory> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Name)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired()
            .ValueGeneratedOnAdd();

        builder.Property(x => x.CommunityId)
            .IsRequired();

        builder.HasOne(x => x.Community)
            .WithMany(x => x.Categories)
            .HasForeignKey(x => x.CommunityId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.CredentialsCollection)
            .WithOne(x => x.Category)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
