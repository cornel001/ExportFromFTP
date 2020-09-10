using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
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
        public Worker(ILogger<Worker> logger, IOptions<WinscpOptions> sessionOptions)
        {
            _logger = logger;
            _sessionOptions = sessionOptions.Value;
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
                RemoteDirectoryInfo directory = 
                    _session.ListDirectory("/");

                foreach (RemoteFileInfo fileInfo in directory.Files)
                {
                    if (!fileInfo.IsDirectory)
                        {
                            _logger.LogInformation("{filename}", fileInfo.Name);
                            Console.WriteLine($@"{fileInfo.Name} has {fileInfo.Length} bytes and has been created at {fileInfo.LastWriteTime}");
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
            _session.Dispose();
            _logger.LogInformation("Am inchis conexiunea FTP.");
        }
    }
}
