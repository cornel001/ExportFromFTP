using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using WinSCP;

namespace ExportFromFTP
{
    public class FtpService : IFtpService
    {
        private readonly ILogger<FtpService> _logger;
        private readonly WinscpOptions _sessionOptions;
        private Session _session = new Session();

        public FtpService(IOptions<WinscpOptions> sessionOptions, 
                          ILogger<FtpService> logger)
        {
            _sessionOptions = sessionOptions.Value;
            _logger = logger;
        }
        
        private void OpenSession()
        {
            if (_session.Opened) return;

            SessionOptions sessionOptions;
            
            try
            {
                sessionOptions = new SessionOptions()
                {
                    Protocol = Enum.Parse<Protocol>(_sessionOptions.Protocol),
                    HostName = _sessionOptions.HostName,
                    UserName = _sessionOptions.UserName,
                    Password = _sessionOptions.Password
                };
            }
            catch (Exception e)
            {
                _logger.LogCritical(e,
                    "Error setting configuration for FTP session: {message}", e.Message);
                throw;
            }

            try
            {
                _session.Open(sessionOptions);
            }
            catch (Exception e)
            {
                _logger.LogCritical(e,
                    "Error opening FTP session: {message}", e.Message);
                throw;
            }
        }

        public IEnumerable<ValueTuple<string, DateTime>> GetFilesInfo()
        {
            OpenSession();
            try
            {
                return _session.EnumerateRemoteFiles("/", 
                                                     "*.*", 
                                                     WinSCP.EnumerationOptions.AllDirectories)
                    .Select(r => (r.FullName, r.LastWriteTime));
            }
            catch (Exception e)
            {
                _logger.LogCritical(e,
                    "Error retrieving list of files from FTP: {message}", e.Message);
                throw;
            }
        }

        public async Task<ICollection<byte>?> GetFileAsync(string path)
        {
            string? localTempFile = null;

            OpenSession();
            try
            {
                localTempFile = _session.GetFileToDirectory(path, Path.GetTempPath())
                    .Destination;

                return await File.ReadAllBytesAsync(localTempFile);
            }
            catch (Exception e)
            {
                _logger.LogError(e,
                    "Error retrieving remote bytes for file {filepath} : {message}",
                    path, e.Message);
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
            OpenSession();
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

        public void Dispose()
        {
            _session.Dispose();
        }
    } 
}