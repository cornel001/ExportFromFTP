using System;
using Microsoft.EntityFrameworkCore;
using Xunit;
using FluentAssertions; 

namespace ExportFromFTP.Tests
{
    public class RepositoryServiceTests
    {
        protected FileContext ServiceContext {get;}
        protected RepositoryService Service {get;}
        protected FileContext ArrangeContext {get;}
        protected FileInfo FirstFileInfo {get;} = new FileInfo(
            "ftps://test.user@localhost/IMG_20160104_230848.jpg",
            "jpg",
            new DateTime(2020,10,01,14,13,21)
        );
        protected FileInfo AnotherFileInfo {get;} = new FileInfo(
            "ftps://test.user@localhost/IMG_20160104_230850.jpg",
            "jpg",
            new DateTime(2020,10,01,14,13,23)
        );
        protected DateTime newWriteTime {get;} = new DateTime(2020,10,10,14,10,12);

        public RepositoryServiceTests()
        {
            var contextOptions = new DbContextOptionsBuilder<FileContext>()
                .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=ExportFromFTP;Trusted_Connection=True;")
                .Options;
            ServiceContext = new FileContext(contextOptions);
            Service = new RepositoryService(ServiceContext);
            ArrangeContext = new FileContext(contextOptions);
            
            using (var setupContext = new FileContext(contextOptions))
            {
                setupContext.RemoveRange(setupContext.FilesInfo.ToListAsync().Result);
                setupContext.Add(FirstFileInfo);            
                setupContext.SaveChanges();
            }            
        }

        public void Dispose()
        {            
            ServiceContext.Dispose();
            ArrangeContext.Dispose();
        }

        [Fact]
        public void GetFromRepository_ExistingPath_ReturnsItem()
        {
            var expected = FirstFileInfo;

            var actual = Service.GetFromRepository(FirstFileInfo.Path);

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void GetFromRepository_NonExistingPath_ReturnsNull()
        {
            var actual = Service.GetFromRepository("thispathisnotthere");

            Assert.Null(actual);
        }

        [Fact]
        public void AddToRepository_NewPath_ReturnsNewFileInfo()
        {
            var expected = Service.AddToRepository(AnotherFileInfo);

            var actual = ArrangeContext.Find<FileInfo>(AnotherFileInfo.Path);

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void AddToRepository_ExistingPath_ThrowsDbUpdateException()
        {
            Action action = () => Service.AddToRepository(FirstFileInfo);

            Assert.Throws<DbUpdateException>(action);
        }

        [Fact]
        public void UpdateInRepository_StatusWithExistingPath_UpdatesStatus()
        {
            var expected = FileStatus.Sent;

            Service.UpdateInRepository(FirstFileInfo.Path, FileStatus.Sent);
            
            var fileInfo = ArrangeContext.Find<FileInfo>(FirstFileInfo.Path);
            var actual = fileInfo.Status;
            ArrangeContext.Dispose();

            Assert.Equal(expected, actual); 
        }

        [Fact]
        public void UpdateInRepository_StatusWithNonExistingPath_ThrowsDbUpdateException()
        {
            Action action = () => Service.UpdateInRepository("thispathisnotthere", FileStatus.Sent);

            Assert.Throws<DbUpdateException>(action);
        }

        [Fact]
        public void UpdateInRepository_LastWriteTimeWithExistingPath_UpdatesTime()
        {
            var expected = newWriteTime;

            Service.UpdateInRepository(FirstFileInfo.Path, newWriteTime);

            var fileInfo = ArrangeContext.Find<FileInfo>(FirstFileInfo.Path);
            var actual = fileInfo.LastWriteTime;
            ArrangeContext.Dispose();

            Assert.Equal(expected,actual);
        }

        [Fact]
        public void UpdateInRepository_LastWriteTimeWithNonExistingPath_ThrowsDbUpdateException()
        {
            Action action = () => Service.UpdateInRepository("thispathisnotthere", newWriteTime);

            Assert.Throws<DbUpdateException>(action);
        }
    }
}
