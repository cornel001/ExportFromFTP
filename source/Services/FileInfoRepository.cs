using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace ExportFromFTP
{
    public class FileInfoRepository : IFileInfoRepository
    {
        private readonly FileInfoContext _context;
        private readonly ILogger<FileInfoRepository> _logger;
        
        public FileInfoRepository(FileInfoContext context, 
                                  ILogger<FileInfoRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<FileInfo> GetAsync(string path)
        {
            return await _context.FilesInfo.FindAsync(path);
        }

        public async Task SaveAsync(FileInfo fileInfo)
        {            
            if (_context.Entry(fileInfo).State == EntityState.Detached)
                await _context.AddAsync(fileInfo);
            
            try
            {
               await _context.SaveChangesAsync();
            }
            catch (DbUpdateException e)
            {
                _logger.LogCritical(e, "Error saving to repository: {message}", e.Message);
                throw;
            }

        }

    }
}