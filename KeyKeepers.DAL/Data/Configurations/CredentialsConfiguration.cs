using Microsoft.EntityFrameworkCore;
using KeyKeepers.DAL.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KeyKeepers.DAL.Data.Configurations;

public class CredentialsConfiguration :  IEntityTypeConfiguration<Credentials>
{
    public void Configure(EntityTypeBuilder<Credentials> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.AppName)
            .IsRequired();

        builder.Property(x => x.Login)
            .IsRequired();
        
        builder.Property(x => x.PasswordHash)
            .IsRequired();

        builder.Property(x => x.LogoUrl)
            .IsRequired();
        
        builder.HasOne(x => x.Category)
            .WithMany(x => x.CredentialsCollection)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}