using System;
using System.Linq;
//using Microsoft.EntityFrameworkCore;

namespace ExportFromFTP
{
    public class FileInfoRepository : IFileInfoRepository
    {
        private FileInfoContext _context;

        public FileInfoRepository(FileInfoContext context)
        {
            _context = context;
        }

        public bool Exists(string path)
        {
            return _context.FilesInfo.Any(i => i.Path == path);
        }

        public FileInfo Get(string path)
        {
            return _context.Find<FileInfo>(path);
        }
        
        public FileInfo Add(FileInfo fileInfo)
        {
            _context.Add<FileInfo>(fileInfo);
            _context.SaveChanges();
            return fileInfo;
        }

        public void UpdateStatus(string path, FileStatus fileStatus)
        {
            var fileInfo = _context.Find<FileInfo>(path);

            if (fileInfo == null)
                throw new Microsoft.EntityFrameworkCore.DbUpdateException(); 

            fileInfo.Status = fileStatus;
            _context.SaveChanges();
        }

        public void UpdateWriteTime(string path, DateTime writeTime)
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