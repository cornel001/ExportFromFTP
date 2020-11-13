using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ExportFromFTP
{
    public class ExportWorker : BackgroundService
    {
        private readonly ILogger<ExportWorker> _logger;
        private IFtpService _ftpService;
        private IServiceProvider _serviceProvider;
        private IExportService _exportService;

        public ExportWorker(ILogger<ExportWorker> logger, 
                      IFtpService ftpService,
                      IServiceProvider serviceProvider,
                      IExportService exportService)
        {
            _logger = logger;
            _ftpService = ftpService;
            _serviceProvider = serviceProvider;
            _exportService = exportService;
        }

        public override Task StartAsync(CancellationToken stoppingToken)
        {
            return base.StartAsync(stoppingToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var fileList = _ftpService.GetFilesInfo();

            foreach (var (remotePath, remoteWriteTime) in fileList)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var _repository = scope.ServiceProvider.GetRequiredService<FileInfoRepository>(); 
                    var fileInfo =_repository.Get(remotePath) ?? new FileInfo(remotePath,remoteWriteTime);

                    //if status = finished then file has been processed completed already
                    //and we do not export it again 
                    //only record when it showed on FTP again, and remove it
                    if (fileInfo.Status == FileStatus.Finished)
                    {
                        fileInfo.UpdateWriteTime(remoteWriteTime);
                        _ftpService.DeleteFile(fileInfo.Path);
                    }

                    if (fileInfo.Status == FileStatus.Initial)
                    {
                        var fileBytes = _ftpService.GetFile(fileInfo.Path);
                        if (!(fileBytes is null))
                            if (_exportService.Export(fileBytes))
                                fileInfo.UpdateStatus();
                    }

                    if (fileInfo.Status == FileStatus.Sent)
                    {
                        if (_ftpService.DeleteFile(fileInfo.Path))
                            fileInfo.UpdateStatus();
                    }

                    _repository.Save(fileInfo);
                }
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
            _logger.LogInformation("Service is stopping.");
            await Task.CompletedTask;
        }

        public override void Dispose()
        {
            _ftpService.Dispose();
            _logger.LogInformation("FTP connection closed.");
        }
    }
}
