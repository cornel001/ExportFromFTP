using System;
using System.Collections.Generic;

namespace ExportFromFTP
{
    public interface IFtpService: IDisposable
    {
        IEnumerable<ValueTuple<string, DateTime>> GetFilesInfo();
        ICollection<byte>? GetFile(string path);
        bool DeleteFile(string path);
    }
}