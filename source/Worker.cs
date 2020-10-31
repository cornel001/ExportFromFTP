using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ExportFromFTP
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private IFtpService _ftpService;
        private IServiceProvider _serviceProvider;

        public Worker(ILogger<Worker> logger, 
                      IFtpService ftpService,
                      IServiceProvider serviceProvider)
        {
            _logger = logger;
            _ftpService = ftpService;
            _serviceProvider = serviceProvider;
        }

        public override Task StartAsync(CancellationToken stoppingToken)
        {
            return base.StartAsync(stoppingToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                IEnumerable<FileInfo> fileList = _ftpService.GetFiles();

                foreach (FileInfo fileInfo in fileList)
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var _repository = scope.ServiceProvider.GetRequiredService<FileInfoRepository>(); 
                        var fileInfo2 =_repository.Get(fileInfo.Path);
                        Console.WriteLine(fileInfo2?.Path ?? "notfound");
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, 
                    "Error occured while processing files: {message}", e.Message);
            }

            await Task.CompletedTask;
/*             while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            } */
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Serviciul se opreste");
            await Task.CompletedTask;
        }

        public override void Dispose()
        {
            _ftpService.Dispose();
            _logger.LogInformation("Am inchis conexiunea FTP.");
        }
    }
}
