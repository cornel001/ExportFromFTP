using System.ComponentModel.DataAnnotations;

namespace ExportFromFTP
{
    public class FileInfo
    {
        [Key]
        public string Path {get; set;} = null!;
        public string Type {get; set;} = null!;
        public System.DateTime WriteTime {get; set;} 
        public System.DateTime? LastWriteTime {get; set;}
        public FileStatus Status {get; set;}
    }
}