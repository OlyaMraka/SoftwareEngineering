using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace KeyKeepersClient.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddServicesFromAssembly(this IServiceCollection services, Assembly assembly, ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        var types = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .ToList();

        foreach (var type in types)
        {
            var interfaces = type.GetInterfaces();

            foreach (var @interface in interfaces)
            {
                if (@interface.Name == $"I{type.Name}")
                {
                    services.Add(new ServiceDescriptor(@interface, type, lifetime));
                    break;
                }
            }
        }
    }

    public static void AddRepositoriesFromAssembly(this IServiceCollection services, Assembly assembly, ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        var types = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract);

        foreach (var type in types)
        {
            var repositoryInterfaces = type.GetInterfaces()
                .Where(i => i.Name.EndsWith("Repository")); // шукаємо інтерфейси репозиторіїв

            foreach (var iface in repositoryInterfaces)
            {
                services.Add(new ServiceDescriptor(iface, type, lifetime));
            }
        }
    }
}
