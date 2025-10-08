using KeyKeepers.BLL.Commands.Users.Create;
using KeyKeepers.BLL.Services;
using KeyKeepers.DAL.Data;
using KeyKeepers.DAL.Repositories.Realizations.Base;
using KeyKeepersClient.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace KeyKeepersClient;

public partial class App : Application
{
    public static IServiceProvider ServiceProvider { get; private set; } = null!;

    public static IConfiguration Configuration { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        var services = new ServiceCollection();

        Configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        ConfigureServices(services);

        ServiceProvider = services.BuildServiceProvider();

        var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();

        base.OnStartup(e);
    }

    private void ConfigureServices(IServiceCollection services)
    {
        var connectionString = Configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<KeyKeepersDbContext>(options =>
            options.UseNpgsql(connectionString, npgsqlOptions =>
                npgsqlOptions.MigrationsAssembly(typeof(KeyKeepersDbContext).Assembly.FullName)));

        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

        services.AddRepositoriesFromAssembly(typeof(RepositoryWrapper).Assembly);
        services.AddServicesFromAssembly(typeof(TokenService).Assembly);
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateUserCommand).Assembly));

        services.AddSingleton<MainWindow>();
    }
}
