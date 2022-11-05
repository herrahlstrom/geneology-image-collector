using GenPhoto.Data;
using GenPhoto.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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

                services.AddSingleton<AppSettings>();

                // View models
                services
                       .AddSingleton<DisplayViewModelRepository>()
                       .AddTransient<IMainViewModel, MainViewModel>();

                // Helpers etc.
                services
                    .AddSingleton<ImageLoader>()
                    .AddSingleton<FileManagement>();
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

         MainWindow = new MainWindow();
         MainWindow.Show();

         base.OnStartup(e);
      }

      private Task StartRutine()
      {
         var fileManagement = AppHost.Services.GetRequiredService<FileManagement>();

         _ = Task.Run(async () =>
         {
            await fileManagement.FindNewFilesAsync();
            await fileManagement.FindMissingFilesAsync();
         });

         return Task.CompletedTask;
      }
   }
}