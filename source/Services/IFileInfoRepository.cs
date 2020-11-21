using System.Threading.Tasks;

namespace ExportFromFTP
{
    public interface IFileInfoRepository
    {
        Task<FileInfo> GetAsync(string path);
        Task SaveAsync(FileInfo fileInfo);
    }
}