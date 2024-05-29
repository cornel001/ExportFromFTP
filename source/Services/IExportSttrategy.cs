using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExportFromFTP
{
    public interface IExportStrategy
    {
        Task ExecuteExportAsync(IEnumerable<(string, DateTime)> source, int dop);
    }
}