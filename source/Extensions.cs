using System;

namespace ExportFromFTP
{
    public static class Extensions
    {
        public static void UpdateStatus(this FileInfo fileInfo)
        {
            if (fileInfo.Status == FileStatus.Initial)
            {
                fileInfo.Status = FileStatus.Sent;
                return;
            }

            if (fileInfo.Status == FileStatus.Sent)
                fileInfo.Status = FileStatus.Finished;
        }

        public static void UpdateWriteTime(this FileInfo fileInfo, DateTime lastWriteTime) =>
            fileInfo.LastWriteTime = lastWriteTime;
    }
}