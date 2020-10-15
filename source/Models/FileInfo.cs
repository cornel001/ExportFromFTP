using System;
using System.ComponentModel.DataAnnotations;

namespace ExportFromFTP
{
    public class FileInfo
    {
        public FileInfo(string path, string type, DateTime writeTime)
        {
            Path = path;
            Type = type;
            WriteTime = writeTime;
            Status = FileStatus.Initial;
        }

        [Key]
        public string Path {get; set;} = null!;
        public string Type {get; set;} = null!;
        public DateTime WriteTime {get; set;} 
        public DateTime? LastWriteTime {get; set;}
        public FileStatus Status {get; set;}
    }
}