using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace ExportFromFTP
{
    public class ExportService : IExportService
    {
        private ILogger<ExportService> _logger;
        public ExportService(ILogger<ExportService> logger)
        {
            _logger = logger;
        }
        public bool Export(ICollection<byte> file)
        {
            _logger.LogInformation("Exported {count} bytes", file.Count);
            return true;
        }
    }
}