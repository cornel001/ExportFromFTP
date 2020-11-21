using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExportFromFTP
{
    public interface IFtpService: IDisposable
    {
        IEnumerable<ValueTuple<string, DateTime>> GetFilesInfo();
        Task<ICollection<byte>?> GetFileAsync(string path);
        bool DeleteFile(string path);
    }
}