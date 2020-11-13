namespace ExportFromFTP
{
    interface IFileInfoRepository
    {
        FileInfo Get(string path);
        void Save(FileInfo fileInfo);
    }
}