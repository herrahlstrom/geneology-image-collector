using GenPhoto.Data;
using GenPhoto.Models;
using GenPhoto.Repositories;
using GenPhoto.Tools;
using GenPhoto.ViewModels;
using GenPhoto.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Windows;

namespace GenPhoto
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, IServiceProvider
    {
        public App()
        {
            AppHost = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((_, builder) =>
                {
                    builder
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile($"appsettings.json", optional: false)
                        .AddJsonFile($"appsettings.{Environment.MachineName}.json", optional: false);
                })
                .ConfigureServices((_, services) =>
                {
                    services.AddLogging(builder =>
                    {
                        builder.AddDebug();
                    });

                    services.AddMemoryCache();

                    services.AddDbContextFactory<AppDbContext>((services, optionsBuilder) =>
                    {
                        var config = services.GetRequiredService<IConfiguration>();
                        var connString = config.GetConnectionString("Sqlite");
                        optionsBuilder.UseSqlite(connString, contextOptionsBuilder => contextOptionsBuilder.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
                    });

                    services.AddSingleton<AppSettings>();

                    // View models
                    services
                        .AddTransient<MainViewModel>();

                    // Helpers etc.
                    services
                        .AddSingleton<EntityRepositoryFactory>()
                        .AddTransient<Api>()
                        .AddSingleton<AppState>()
                        .AddSingleton<Maintenance>();
                }).Build();
        }

        public IHost AppHost { get; }

        public object? GetService(Type serviceType) => AppHost.Services.GetService(serviceType);

        protected override async void OnExit(ExitEventArgs e)
        {
            await AppHost.StopAsync();

            base.OnExit(e);
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await AppHost.StartAsync();

            await StartRutine();

            MainWindow = new NewMainWindow();
            MainWindow.Show();

            base.OnStartup(e);
        }

        private Task StartRutine()
        {
            var fileManagement = AppHost.Services.GetRequiredService<Maintenance>();

            _ = Task.Run(async () =>
            {
                await Task.Delay(1);
                //await fileManagement.OneTimeFix();
                //await fileManagement.FindNewFilesAsync();
                await fileManagement.DetectMissingFilesAsync();
            });

            return Task.CompletedTask;
        }
    }
}