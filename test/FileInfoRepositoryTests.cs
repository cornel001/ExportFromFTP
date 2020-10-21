using System;
using Microsoft.EntityFrameworkCore;
using Xunit;
using FluentAssertions; 

namespace ExportFromFTP.Tests
{
    public class FileInfoRepositoryTests
    {
        protected FileInfoContext RepositoryContext {get;}
        protected FileInfoRepository Repository {get;}
        protected FileInfoContext ArrangeContext {get;}
        protected FileInfo FirstFileInfo {get;} = new FileInfo(
            "/IMG_20160104_230848.jpg",
            new DateTime(2020,10,01,14,13,21)
        );
        protected FileInfo AnotherFileInfo {get;} = new FileInfo(
            "/IMG_20160104_230850.jpg",
            new DateTime(2020,10,01,14,13,23)
        );
        protected DateTime newWriteTime {get;} = new DateTime(2020,10,10,14,10,12);

        public FileInfoRepositoryTests()
        {
            var contextOptions = new DbContextOptionsBuilder<FileInfoContext>()
                .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=ExportFromFTP;Trusted_Connection=True;")
                .Options;
            RepositoryContext = new FileInfoContext(contextOptions);
            Repository = new FileInfoRepository(RepositoryContext);
            ArrangeContext = new FileInfoContext(contextOptions);
            
            using (var setupContext = new FileInfoContext(contextOptions))
            {
                setupContext.RemoveRange(setupContext.FilesInfo.ToListAsync().Result);
                setupContext.Add(FirstFileInfo);            
                setupContext.SaveChanges();
            }            
        }

        public void Dispose()
        {            
            RepositoryContext.Dispose();
            ArrangeContext.Dispose();
        }

        [Fact]
        public void Get_ExistingPath_ReturnsItem()
        {
            var expected = FirstFileInfo;

            var actual = Repository.Get(FirstFileInfo.Path);

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Get_NonExistingPath_ReturnsNull()
        {
            var actual = Repository.Get("thispathisnotthere");

            Assert.Null(actual);
        }

        [Fact]
        public void Add_NewPath_ReturnsNewFileInfo()
        {
            var expected = Repository.Add(AnotherFileInfo);

            var actual = ArrangeContext.Find<FileInfo>(AnotherFileInfo.Path);

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Add_ExistingPath_ThrowsDbUpdateException()
        {
            Action action = () => Repository.Add(FirstFileInfo);

            Assert.Throws<DbUpdateException>(action);
        }

        [Fact]
        public void UpdateStatus_ExistingPath_UpdatesStatus()
        {
            var expected = FileStatus.Sent;

            Repository.UpdateStatus(FirstFileInfo.Path, FileStatus.Sent);
            
            var fileInfo = ArrangeContext.Find<FileInfo>(FirstFileInfo.Path);
            var actual = fileInfo.Status;

            Assert.Equal(expected, actual); 
        }

        [Fact]
        public void UpdateStatus_NonExistingPath_ThrowsDbUpdateException()
        {
            Action action = () => Repository.UpdateStatus("thispathisnotthere", FileStatus.Sent);

            Assert.Throws<DbUpdateException>(action);
        }

        [Fact]
        public void UpdateWriteTime_ExistingPath_UpdatesTime()
        {
            var expected = newWriteTime;

            Repository.UpdateWriteTime(FirstFileInfo.Path, newWriteTime);

            var fileInfo = ArrangeContext.Find<FileInfo>(FirstFileInfo.Path);
            var actual = fileInfo.LastWriteTime;

            Assert.Equal(expected,actual);
        }

        [Fact]
        public void UpdateWriteTime_NonExistingPath_ThrowsDbUpdateException()
        {
            Action action = () => Repository.UpdateWriteTime("thispathisnotthere", newWriteTime);

            Assert.Throws<DbUpdateException>(action);
        }
    }
}
