using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ExportFromFTP
{
    public class ExportService : IExportService
    {
        private readonly ILogger<ExportService> _logger;
        
        public ExportService(ILogger<ExportService> logger)
        {
            _logger = logger;
        }
        
        public async Task<bool> Export(ICollection<byte> file)
        {
            _logger.LogInformation("Exported {count} bytes", file.Count);
            return await Task.FromResult(true);
        }
    }
}