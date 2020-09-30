using Microsoft.EntityFrameworkCore;

namespace ExportFromFTP
{
    public class FileContext: DbContext
    {
        public FileContext(DbContextOptions<FileContext> options) : base(options) { }
        public DbSet<FileInfo> FilesInfo {get; set;} = null!;
    }
}