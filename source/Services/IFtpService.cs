using System;
using System.Collections.Generic;

namespace ExportFromFTP
{
    public interface IFtpService: IDisposable
    {
        IEnumerable<FileInfo> GetFilesInfo();
        IEnumerable<byte> GetFile(string path);
        void DeleteFile(string path);
    }
}