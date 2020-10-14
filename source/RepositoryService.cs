using System;

namespace ExportFromFTP
{
    public class RepositoryService
    {
        private FileContext _context;

        public RepositoryService(FileContext context)
        {
            _context = context;
        }

        public FileInfo GetFromRepository(string path)
        {
            return _context.Find<FileInfo>(path);
        }
        
        public FileInfo AddToRepository(FileInfo fileInfo)
        {
            _context.Add<FileInfo>(fileInfo);
            _context.SaveChanges();
            return fileInfo;
        }

        public void UpdateInRepository(string path, FileStatus fileStatus)
        {
            var fileInfo = _context.Find<FileInfo>(path);

            if (fileInfo == null)
                throw new Microsoft.EntityFrameworkCore.DbUpdateException(); 

            fileInfo.Status = fileStatus;
            _context.SaveChanges();
        }

        public void UpdateInRepository(string path, DateTime writeTime)
        {
            //throw new NotImplementedException();
            var fileInfo = _context.Find<FileInfo>(path);

            if (fileInfo == null)
                throw new Microsoft.EntityFrameworkCore.DbUpdateException();

            fileInfo.LastWriteTime = writeTime;
            _context.SaveChanges();
        }
    }
}