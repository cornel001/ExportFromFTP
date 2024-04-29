using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System;
using Microsoft.Extensions.Logging;

namespace ExportFromFTP
{
    public class ExportService : IExportService
    {
        private readonly ILogger<ExportService> _logger;
        private static readonly SemaphoreSlim semaphore = new SemaphoreSlim(1,1); 
        public ExportService(ILogger<ExportService> logger)
        {
            _logger = logger;
            File.Delete("ExportService.txt");
        }
        
        public async Task<bool> Export(ICollection<byte> file)
        {
            await semaphore.WaitAsync();
            try
            {
                await using var writer = new StreamWriter("ExportService.txt", true);
                await writer.WriteLineAsync($"Exported {file.Count} bytes");
            }
            catch (IOException e)
            {
                _logger.LogError(e, "Export Service Malfunction: {message}", e.Message);
                return false;
            }
            finally
            {
                semaphore.Release();
            }

            return true;
        }
    }
}