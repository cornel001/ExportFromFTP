using System;

namespace ExportFromFTP
{
    interface IFileInfoRepository
    {
        bool Exists(string path);
        FileInfo Add(FileInfo fileInfo);
        void UpdateWriteTime(string path, DateTime writeTime);
    }
}