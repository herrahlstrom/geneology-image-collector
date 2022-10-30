using GeneologyImageCollector.Data;
using GeneologyImageCollector.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyGen.Data;
using System.IO;
using System.Windows;

namespace GeneologyImageCollector
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
                        .AddJsonFile($"appsettings.{Environment.MachineName}.json", optional: true);
                })
                .ConfigureServices((_, services) =>
                {
                    services.AddLogging();

                    services.AddDbContextFactory<AppDbContext>((services, optionsBuilder) =>
                    {
                        var config = services.GetRequiredService<IConfiguration>();
                        var connString = config.GetConnectionString("Sqlite");
                        optionsBuilder.UseSqlite(connString, contextOptionsBuilder => contextOptionsBuilder.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
                    });

                    services.AddDbContextFactory<MyGenDbContext>((services, optionsBuilder) =>
                    {
                        var config = services.GetRequiredService<IConfiguration>();
                        var connString = config.GetConnectionString("MyGen");
                        optionsBuilder.UseSqlite(connString, contextOptionsBuilder => contextOptionsBuilder.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
                    });

                    services.AddSingleton<AppSettings>();

                    // View models
                    services
                        .AddTransient<IMainViewModel, MainViewModel>();

                    // Helpers
                    services.AddTransient<MyGenImporter>();
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

            await InitDatabase();

            MainWindow = new MainWindow();
            MainWindow.Show();

            base.OnStartup(e);
        }

        private async Task InitDatabase()
        {
            var db = AppHost.Services.GetRequiredService<AppDbContext>();

            //if (Debugger.IsAttached)
            //{
            //    await db.Database.EnsureDeletedAsync();
            //}

            if (await db.Database.EnsureCreatedAsync())
            {
                var importer = AppHost.Services.GetRequiredService<MyGenImporter>();
                await importer.Import();
            }
        }
    }
}