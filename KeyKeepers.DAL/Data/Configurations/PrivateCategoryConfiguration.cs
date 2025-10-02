using KeyKeepers.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KeyKeepers.DAL.Data.Configurations;

public class PrivateCategoryConfiguration : IEntityTypeConfiguration<PrivateCategory>
{
    public void Configure(EntityTypeBuilder<PrivateCategory> builder)
    {
        builder.HasOne(x => x.CommunityUser)
            .WithMany(x => x.PrivateCategories)
            .HasForeignKey(x => x.CommunityUserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
