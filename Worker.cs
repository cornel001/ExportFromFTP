using System;
using System.Threading;
using System.Threading.Tasks;
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

        public Worker(ILogger<Worker> logger, IOptions<WinscpOptions> sessionOptions)
        {
            _logger = logger;
            _sessionOptions = sessionOptions.Value;
            
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                SessionOptions sessionOptions = new SessionOptions()
                {
                    Protocol = Enum.Parse<Protocol>(_sessionOptions.Protocol),
                    HostName = _sessionOptions.HostName,
                    UserName = _sessionOptions.UserName,
                    Password = _sessionOptions.Password
                };

                using (Session session = new Session())
                {
                    session.Open(sessionOptions);

                    RemoteDirectoryInfo directory = 
                        session.ListDirectory("/");

                    foreach (RemoteFileInfo fileInfo in directory.Files)
                    {
                        if (!fileInfo.IsDirectory)
                            Console.WriteLine($@"{fileInfo.Name} has {fileInfo.Length} bytes and has been created at {fileInfo.LastWriteTime}");
                    }

                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e}");
            }

            return Task.CompletedTask;
/*             while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            } */
        }
    }
}
