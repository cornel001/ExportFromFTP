using Microsoft.EntityFrameworkCore;

namespace ExportFromFTP
{
    public class FileInfoContext: DbContext
    {
        public FileInfoContext(DbContextOptions<FileInfoContext> options) : base(options) { }
        public DbSet<FileInfo> FilesInfo {get; set;} = null!;
    }
}