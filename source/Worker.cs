using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WinSCP;

namespace ExportFromFTP
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly WinscpOptions _sessionOptions;
        private Session _session = new Session();
        private IServiceProvider _serviceProvider;

        public Worker(ILogger<Worker> logger, 
                      IOptions<WinscpOptions> sessionOptions,
                      IServiceProvider serviceProvider)
        {
            _logger = logger;
            _sessionOptions = sessionOptions.Value;
            _serviceProvider = serviceProvider;
        }

        public override Task StartAsync(CancellationToken stoppingToken)
        {
            SessionOptions sessionOptions;
            try
            {
                sessionOptions = new SessionOptions()
                {
                    Protocol = Enum.Parse<Protocol>(_sessionOptions.Protocol),
                    HostName = _sessionOptions.HostName,
                    UserName = _sessionOptions.UserName,
                    Password = _sessionOptions.Password
                };
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, 
                    "Error loading configuration for FTP session: {message}", e.Message);
                throw;
            }

            try
            {
                _session.Open(sessionOptions);
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, 
                    "Error opening FTP session: {message}", e.Message);
                throw;
            }

            return base.StartAsync(stoppingToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                IEnumerable<RemoteFileInfo> remoteFileList = 
                    _session.EnumerateRemoteFiles("/","*.*",EnumerationOptions.None);

                foreach (RemoteFileInfo remoteFileInfo in remoteFileList)
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var _repository = scope.ServiceProvider.GetRequiredService<FileInfoRepository>(); 
                        var fileInfo =_repository.Get(remoteFileInfo.FullName);
                        Console.WriteLine(fileInfo?.Path ?? "notfound");
                    }
                    //_logger.LogInformation("{filename}", remoteFileInfo.Name);
                    //Console.WriteLine($@"{remoteFileInfo.Name} has {remoteFileInfo.Fullname} path and has been created at {remoteFileInfo.LastWriteTime}");
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
            _session.Dispose();
            _logger.LogInformation("Am inchis conexiunea FTP.");
        }
    }
}
