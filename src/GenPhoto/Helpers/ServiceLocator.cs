using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace GenPhoto.Helpers;

internal class ServiceLocator : IServiceProvider
{
    public static TService GetService<TService>() where TService : class
    {
        var serviceLocator = Application.Current.FindResource(nameof(ServiceLocator)) as ServiceLocator
                ?? throw new InvalidOperationException($"Can't find {nameof(ServiceLocator)} in current application resources");

        return serviceLocator.GetRequiredService<TService>();
    }

    public object? GetService(Type serviceType)
    {
        var serviceProvider = Application.Current as IServiceProvider
            ?? throw new InvalidOperationException($"Current {nameof(App)} is not implementing {nameof(IServiceProvider)}");

        return serviceProvider.GetService(serviceType);
    }
}