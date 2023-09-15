using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System;
using Microsoft.Extensions.Logging;

namespace ExportFromFTP
{
    public class ExportService : IExportService
    {
        private readonly ILogger<ExportService> _logger;
        
        public ExportService(ILogger<ExportService> logger)
        {
            _logger = logger;
            File.Delete("ExportService.txt");
        }
        
        public async Task<bool> Export(ICollection<byte> file)
        {
            try
            {
                await using var stream = File.Open("ExportService.txt",FileMode.Append,FileAccess.Write, FileShare.Write);
                await using var writer = new StreamWriter(stream);
                await writer.WriteLineAsync($"Exported {file.Count} bytes");
            }
            catch (IOException e)
            {
                _logger.LogCritical(e, "Export Service Malfunction", e.Message);
                throw;
            }

            //_logger.LogInformation("Exported {count} bytes", file.Count);
            return await Task.FromResult(true);
        }
    }
}