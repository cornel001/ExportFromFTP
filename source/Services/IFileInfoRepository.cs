using System.Threading.Tasks;

namespace ExportFromFTP
{
    interface IFileInfoRepository
    {
        Task<FileInfo> GetAsync(string path);
        Task SaveAsync(FileInfo fileInfo);
    }
}