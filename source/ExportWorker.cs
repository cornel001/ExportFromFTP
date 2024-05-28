using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ExportFromFTP
{
    public class ExportWorker : BackgroundService
    {
        private readonly ILogger<ExportWorker> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IExportService _exportService;

        public ExportWorker(IExportService exportService,
                            IServiceProvider serviceProvider,
                            ILogger<ExportWorker> logger)
        {
            _exportService = exportService;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public override Task StartAsync(CancellationToken stoppingToken)
        {
            return base.StartAsync(stoppingToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ExecuteAsync started at: {time}", DateTime.Now.ToString());

            using var scope = _serviceProvider.CreateScope();
            var ftpService = scope.ServiceProvider.GetRequiredService<IFtpService>();
            var fileInfoList = ftpService.GetFilesInfo();

            var strategy = ExportStrategyFactory.CreateExportStrategySemaphore(_serviceProvider, _logger, ProcessFile);
            //var strategy = ExportStrategyFactory.CreateExportStrategyPartition(_serviceProvider, _logger, ProcessFile);
            //var strategy = ExportStrategyFactory.CreateExportStrategyPartitionParallel(_serviceProvider, _logger, ProcessFile);
            await strategy.ExecuteExportAsync(fileInfoList, 3);

            _logger.LogInformation("ExecuteAsync stopped at: {time}", DateTime.Now.ToString());

            // await Task.CompletedTask;
            /* while (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                    await Task.Delay(1000, stoppingToken);
                } */
        }

        async Task ProcessFile((string , DateTime) sourceFileInfo, IFtpService ftpService)
            {
                var remotePath = sourceFileInfo.Item1;
                var remoteWriteTime = sourceFileInfo.Item2;
                using var scope = _serviceProvider.CreateScope();
                var _repository = scope.ServiceProvider.GetRequiredService<IFileInfoRepository>(); 
                var fileInfo = await _repository.GetAsync(remotePath) ?? new FileInfo(remotePath, remoteWriteTime);

                // If status = finished then file has been processed complete already and we 
                // do not export it again. Only record when it showed on FTP again, and remove it
                if (fileInfo.Status == FileStatus.Finished)
                {
                    fileInfo.UpdateWriteTime(remoteWriteTime);
                    ftpService.DeleteFile(fileInfo.Path);
                }

                if (fileInfo.Status == FileStatus.Initial)
                {
                    var fileBytes = await ftpService.GetFileAsync(fileInfo.Path);
                    if (fileBytes != null)
                        if (await _exportService.Export(fileBytes))
                            fileInfo.UpdateStatus();
                }

                if (fileInfo.Status == FileStatus.Sent)
                {
                    if (ftpService.DeleteFile(fileInfo.Path))
                        fileInfo.UpdateStatus();
                }

                await _repository.SaveAsync(fileInfo);
            }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Main Service is stopping.");
            await Task.CompletedTask;
        }

    }
}
