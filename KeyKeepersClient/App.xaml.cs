using KeyKeepers.BLL.Commands.Users.Create;
using KeyKeepers.BLL.Services;
using KeyKeepers.DAL.Data;
using KeyKeepers.BLL.Validators.Users;
using KeyKeepers.DAL.Entities;
using KeyKeepers.DAL.Repositories.Interfaces.Base;
using KeyKeepers.DAL.Repositories.Realizations.Base;
using KeyKeepersClient.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Windows;
using KeyKeepers.BLL.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

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

        services.AddSingleton<IConfiguration>(Configuration);

        ConfigureServices(services);

        ServiceProvider = services.BuildServiceProvider();

        var firstWindow = new FirstWindow();
        firstWindow.Show();

        base.OnStartup(e);
    }

    private void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContext<KeyKeepersDbContext>(options =>
            options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection")));

        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

        services.AddRepositoriesFromAssembly(typeof(RepositoryWrapper).Assembly);

        services.AddScoped<IRepositoryWrapper, RepositoryWrapper>();
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddRepositoriesFromAssembly(typeof(RepositoryWrapper).Assembly);
        services.AddServicesFromAssembly(typeof(TokenService).Assembly);
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateUserCommand).Assembly));
        services.AddLogging();
        services.AddValidatorsFromAssembly(typeof(UserRegisterValidator).Assembly);
        services.AddTransient<FirstWindow>();
        services.AddTransient<SignUpWindow>();
    }
}
