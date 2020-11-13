using System;
using System.ComponentModel.DataAnnotations;

namespace ExportFromFTP
{
    public class FileInfo
    {
        public FileInfo(string path, DateTime writeTime)
        {
            Path = path;
            WriteTime = writeTime;
            Status = FileStatus.Initial;
        }

        [Key]
        public string Path {get; private set;}
        public DateTime WriteTime {get; private set;} 
        public DateTime? LastWriteTime {get; set;}
        public FileStatus Status {get; set;}
    }
}