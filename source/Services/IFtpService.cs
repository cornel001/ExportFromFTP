using System;
using System.Collections.Generic;

namespace ExportFromFTP
{
    public interface IFtpService: IDisposable
    {
        IEnumerable<FileInfo> GetFiles();
        void DeleteFile(string path);
    }
}