using KeyKeepers.DAL.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace KeyKeepers.DAL
{
    public class KeyKeepersDbContextFactory : IDesignTimeDbContextFactory<KeyKeepersDbContext>
    {
        public KeyKeepersDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<KeyKeepersDbContext>();

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            optionsBuilder.UseNpgsql(
                connectionString,
                npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsAssembly(typeof(KeyKeepersDbContext).Assembly.FullName);
                });

            return new KeyKeepersDbContext(optionsBuilder.Options);
        }
    }
}
