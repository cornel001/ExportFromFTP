using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.WindowsServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using System;
using Serilog;

namespace ExportFromFTP
{
    public class Program
    {
        public static IConfiguration Configuration {get;} = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional:false, reloadOnChange:true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")}.json", optional:true)
            .AddEnvironmentVariables()
            .Build();

        public static void Main(string[] args)
        {
            System.IO.Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(Configuration)
                .CreateLogger();

            var host = CreateHostBuilder(args).Build();
            
            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Host built.");
            logger.LogInformation("App is Windows Service: {IsWinService}",
                                   WindowsServiceHelpers.IsWindowsService());
            logger.LogInformation("Environment is {EnvName}",
                                   host.Services.GetRequiredService<IHostEnvironment>().EnvironmentName);
            
            host.Run();

            Log.CloseAndFlush();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) => 
                {
                    services.AddHostedService<ExportWorker>();
                    services.Configure<WinscpOptions>(
                        hostContext.Configuration.GetSection("WinscpOptions"));
                    services.AddDbContext<FileInfoContext>(options => 
                        options.UseSqlServer(Configuration.GetConnectionString("ExportFromFTP")));
                    services.AddScoped<IFileInfoRepository, FileInfoRepository>();
                    services.AddScoped<IFtpService, FtpServiceFakeDelete>();
                    services.AddSingleton<IExportService, ExportService>();
                })
                .ConfigureLogging(logging => logging.ClearProviders())
                .UseSerilog()
                .UseWindowsService();
    }
}
