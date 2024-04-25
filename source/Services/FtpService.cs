using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using WinSCP;

namespace ExportFromFTP
{
    internal static class FTPSession
    {
        internal static Session OpenNewSession(IOptions<WinscpOptions> sessionOptionsCfg, 
                                               ILogger<FtpService> logger)
        {

            SessionOptions sessionOptions;
            
            try
            {
                sessionOptions = new SessionOptions()
                {
                    Protocol = Enum.Parse<Protocol>(sessionOptionsCfg.Value.Protocol),
                    HostName = sessionOptionsCfg.Value.HostName,
                    UserName = sessionOptionsCfg.Value.UserName,
                    Password = sessionOptionsCfg.Value.Password
                };
            }
            catch (Exception e)
            {
                logger.LogCritical(e, "Error setting configuration for FTP session: {message}", e.Message);
                throw;
            }

            Session session = new Session();

            try
            {
                session.Open(sessionOptions);
            }
            catch (Exception e)
            {
                logger.LogCritical(e, "Error opening FTP session: {message}", e.Message);
                throw;
            }

            return session;
        }

        internal static void Dispose(Session session)
        {
            session.Close();
            session.Dispose();
        }
    }

    public class FtpService : IFtpService
    {
        private readonly ILogger<FtpService> _logger;
        private readonly Session _session;
        internal static readonly char[] chars = "abcdefghijklmnopqrstuvwxyz".ToCharArray();
        private string _localTempPath;

        public FtpService(IOptions<WinscpOptions> sessionOptionsCfg, 
                          ILogger<FtpService> logger)
        {
            _logger = logger;
            _session = FTPSession.OpenNewSession(sessionOptionsCfg,logger);
            _localTempPath = CreateTempFolder(logger);
            _logger.LogDebug("FTP Service constructor ran");
        }

        // If I later switch to async FTP library, I could change
        // IEnumerable<> to IAsyncEnumerable<>
        public IEnumerable<ValueTuple<string, DateTime>> GetFilesInfo()
        {
            // Should be moved later to configuration file.
            const string rootPath = "/";
            const string rootPathNotFoundErrMsg = "Error retrieving list of files from FTP: Directory does not exist.";

            if (!_session.FileExists(rootPath))
            {
                _logger.LogCritical(rootPathNotFoundErrMsg);
                throw new OperationCanceledException(rootPathNotFoundErrMsg);
            }

            return _session.EnumerateRemoteFiles(rootPath, 
                                                 "*.*", 
                                                 WinSCP.EnumerationOptions.AllDirectories)
                .Select(r => (r.FullName, r.LastWriteTime));

        }

        public async Task<ICollection<byte>?> GetFileAsync(string path)
        {
            string? localTempFile = null;

            try
            {
                localTempFile = _session.GetFileToDirectory(path, _localTempPath).Destination;
                return await File.ReadAllBytesAsync(localTempFile);
            }
            catch (Exception e)
            {
                _logger.LogError(e,
                    "Error retrieving remote bytes for file {filepath} : {message}", path, e.Message);
                return await Task.FromResult<ICollection<byte>?>(null);
            }
            finally
            {
                if (localTempFile != null)
                    File.Delete(localTempFile);
            }
        }

        public virtual bool DeleteFile(string path)
        {

            try
            {
                _session.RemoveFile(path);
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e,
                    "Error deleting exported file, from FTP storage: {message}", e.Message);
                return false;
            }
        }

        private static string CreateTempFolder(ILogger<FtpService> logger)
        {
            var localTempPath = Path.GetTempPath() + "ExportFromFTP\\" + GetRandomString(10);

            try
            {
                Directory.CreateDirectory(localTempPath);
            }
            catch (Exception e)
            {
                logger.LogCritical(e, "Error creating temp folder: {message}", e.Message);
            }
        
            return localTempPath;
        }

        private static string GetRandomString(int length)
        {
            if (length < 1) 
                throw new ArgumentOutOfRangeException(nameof(length), length, "Length of the string to be generated cannot be less than 1");
            var randomStringArray = new char[length];
            for (int i = 0; i < length; i ++)
            {
                randomStringArray[i] = chars[RandomNumberGenerator.GetInt32(chars.Length)];
            }
            return string.Join("", randomStringArray);
        }

        public void Dispose()
        {
            _logger.LogInformation("dispose will be called from ftp service's dispose");
            FTPSession.Dispose(_session);
            _logger.LogInformation("dispose was called from ftp service's dispose");
        }
    } 
}