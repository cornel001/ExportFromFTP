using System.Collections.Generic;

namespace ExportFromFTP
{
    public interface IExportService
    {
        bool Export(ICollection<byte> file);
    }
}