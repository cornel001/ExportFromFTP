using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Xunit;
using FluentAssertions; 

namespace ExportFromFTP.Tests
{
    public class FileInfoRepositoryTests
    {
        protected DbContextOptions<FileInfoContext> ContextOptions {get;} = 
            new DbContextOptionsBuilder<FileInfoContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=ExportFromFTP;Trusted_Connection=True;")
            .Options;
        protected FileInfoContext RepositoryContext {get;}
        protected FileInfoRepository Repository {get;}
        private FileInfo[] _sampleList = {
            new FileInfo("/IMG_20160104_230848.jpg", new DateTime(2020,10,01,14,13,21)),
            new FileInfo("/IMG_20160104_230850.jpg", new DateTime(2020,10,01,14,13,23))
        };

        public FileInfoRepositoryTests()
        {
            RepositoryContext = GetNewContext();
            Repository = new FileInfoRepository(RepositoryContext, null);
            
            using (var setupContext = new FileInfoContext(ContextOptions))
            {
                setupContext.RemoveRange(setupContext.FilesInfo.ToList());
                setupContext.Add(_sampleList[0]);            
                setupContext.SaveChanges();
            }            
        }

        public void Dispose()
        {            
            RepositoryContext.Dispose();
        }

        private FileInfoContext GetNewContext()
        {
            return new FileInfoContext(ContextOptions);
        }

        [Fact]
        public void Get_ExistingPath_ReturnsItem()
        {
            var expected = _sampleList[0];

            var actual = Repository.Get(expected.Path);

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Get_NonExistingPath_ReturnsNull()
        {
            var actual = Repository.Get("thispathisnotthere");

            Assert.Null(actual);
        }

        [Fact]
        public void Save_NewPath_ItemSavedAsNew()
        {
            FileInfo expected = _sampleList[1];
            FileInfo actual;

            Repository.Save(expected);

            using (var context = GetNewContext())
            {
                actual = context.FilesInfo.Find(expected.Path);
            }

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Save_ExistingPath_ItemSavedAsUpdated()
        {
            FileInfo expected = RepositoryContext.FilesInfo.Find(_sampleList[0].Path);
            FileInfo actual;
            expected.Status = FileStatus.Sent;
            expected.LastWriteTime = new DateTime(2020,10,10,14,10,12);;
            
            Repository.Save(expected);

            using (var context = GetNewContext())
            {
                actual = context.FilesInfo.Find(expected.Path);
            }

            actual.Should().BeEquivalentTo(expected);
        }
    }
}
