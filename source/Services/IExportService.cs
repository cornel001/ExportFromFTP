using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExportFromFTP
{
    public interface IExportService
    {
        Task<bool> Export(ICollection<byte> file);
    }
}