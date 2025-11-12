using KeyKeepers.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KeyKeepers.DAL.Data.Configurations;

public class JoinRequestConfiguration : IEntityTypeConfiguration<JoinRequest>
{
    public void Configure(EntityTypeBuilder<JoinRequest> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Comment);

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.CommunityId)
            .IsRequired();

        builder.HasOne(x => x.Community)
            .WithMany(x => x.JoinRequests)
            .HasForeignKey(x => x.CommunityId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.RecipientId)
            .IsRequired();

        builder.HasOne(x => x.Recipient)
            .WithMany(x => x.JoinRequests)
            .HasForeignKey(x => x.RecipientId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.SenderId)
            .IsRequired();

        builder.HasOne(x => x.Sender)
            .WithMany(x => x.Requests)
            .HasForeignKey(x => x.SenderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
