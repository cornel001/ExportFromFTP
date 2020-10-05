using System;
using Xunit;
using ExportFromFTP;

namespace test
{
    public class FileInfoTests
    {
        public static readonly string testPath = "ftps://test.user@localhost/IMG_20160104_230848.jpg";
        public static readonly string testType = "jpg";
        public static readonly DateTime testWriteTime = new DateTime(2020,10,01,14,13,21);

        [Fact]
        public void CanCreateByConstructor()
        {
            var fileInfo = new FileInfo(testPath, testType, testWriteTime);

            Assert.Equal(testPath, fileInfo.Path);
            Assert.Equal(testType, fileInfo.Type);
            Assert.Equal(testWriteTime, fileInfo.WriteTime);
            Assert.Null(fileInfo.LastWriteTime);
            Assert.Equal(FileStatus.Initial, fileInfo.Status);
        }
    }
}