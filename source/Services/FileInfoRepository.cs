using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ExportFromFTP
{
    public class FileInfoRepository : IFileInfoRepository
    {
        private FileInfoContext _context;
        private ILogger<FileInfoRepository> _logger;
        public FileInfoRepository(FileInfoContext context, 
                                  ILogger<FileInfoRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public FileInfo Get(string path)
        {
            return _context.FilesInfo.Find(path);
        }

        public void Save(FileInfo fileInfo)
        {            
            if (_context.Entry(fileInfo).State == EntityState.Detached)
                _context.Add(fileInfo);
            
            try
            {
                _context.SaveChanges();
            }
            catch (DbUpdateException e)
            {
                _logger.LogCritical(e, "Error saving to repository: {message}", e.Message);
                throw;
            }

        }

    }
}