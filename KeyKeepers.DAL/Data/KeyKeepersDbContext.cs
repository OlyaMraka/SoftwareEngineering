using KeyKeepers.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace KeyKeepers.DAL.Data;

public class KeyKeepersDbContext : DbContext
{
    public KeyKeepersDbContext(DbContextOptions<KeyKeepersDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }

    public DbSet<Community> Communities { get; set; }

    public DbSet<CommunityUser> CommunityUsers { get; set; }

    public DbSet<BaseCategory> BaseCategories { get; set; }

    public DbSet<PrivateCategory> PrivateCategories { get; set; }

    public DbSet<Credentials> Credentials { get; set; }

    public DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<BaseCategory>().ToTable("BaseCategories");
        modelBuilder.Entity<PrivateCategory>().ToTable("PrivateCategories");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(KeyKeepersDbContext).Assembly);
    }
}
