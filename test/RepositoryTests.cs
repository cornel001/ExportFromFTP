using System;
using Xunit;
using Microsoft.EntityFrameworkCore;
using ExportFromFTP;

namespace test
{
    public class RepositoryTests
    {
        protected DbContextOptions<FileContext> ContextOptions {get;}
        
        public RepositoryTests()
        {
            ContextOptions = new DbContextOptionsBuilder<FileContext>()
                .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=ExportFromFTP;Trusted_Connection=True;")
                .Options;
        }

        [Fact]
        public void CanAdd()
        {
            using (var context = new FileContext(ContextOptions))
            {
                var fileInfo = new FileInfo(FileInfoTests.testPath, 
                                            FileInfoTests.testType,
                                            FileInfoTests.testWriteTime);
                
                context.Add<FileInfo>(fileInfo);
                context.SaveChanges();
            }

            using (var context = new FileContext(ContextOptions))
            {
                var fileInfo = context.Find<FileInfo>(FileInfoTests.testPath);
                            
                Assert.Equal(FileInfoTests.testPath, fileInfo.Path);
                Assert.Equal(FileInfoTests.testType, fileInfo.Type);
                Assert.Equal(FileInfoTests.testWriteTime, fileInfo.WriteTime);
                Assert.Null(fileInfo.LastWriteTime);
                Assert.Equal(FileStatus.Initial, fileInfo.Status);
            }
        }

        [Fact]
        public void CanUpdate()
        {
            using (var context = new FileContext(ContextOptions))
            {
                var fileInfo = context.Find<FileInfo>(FileInfoTests.testPath);
                fileInfo.Status = FileStatus.Sent;
                context.SaveChanges();
            }

            using (var context = new FileContext(ContextOptions))
            {
                var fileInfo = context.Find<FileInfo>(FileInfoTests.testPath);
                Assert.Equal(FileStatus.Sent, fileInfo.Status);
            }
        }

        [Fact]
        public void CanDelete()
        {
            using (var context = new FileContext(ContextOptions))
            {
                var fileInfo = context.Find<FileInfo>(FileInfoTests.testPath);
                context.Remove<FileInfo>(fileInfo);
                context.SaveChanges();
            }

            using (var context = new FileContext(ContextOptions))
            {
                var fileInfo = context.Find<FileInfo>(FileInfoTests.testPath);
                Assert.Null(fileInfo);
            }
        }
    }
}
