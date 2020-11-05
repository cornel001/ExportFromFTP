using System;
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

        public FtpService(ILogger<FtpService> logger, IOptions<WinscpOptions> sessionOptions)
        {
            _logger = logger;
            _sessionOptions = sessionOptions.Value;
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

        public IEnumerable<FileInfo> GetFilesInfo()
        {
            OpenSession();
            IEnumerable<RemoteFileInfo> remoteFileList = 
                _session.EnumerateRemoteFiles("/","*.*", WinSCP.EnumerationOptions.None);

            return remoteFileList.Select(r => new FileInfo(r.FullName, r.LastWriteTime));
        }

        public IEnumerable<byte> GetFile(string path)
        {
            var localTempFile = Path.GetTempFileName();

            OpenSession();
            try
            {
                _session.GetFileToDirectory(path, localTempFile);

                return File.ReadAllBytes(localTempFile);
            }
            catch (Exception e)
            {
                _logger.LogError(e,
                    "Error retrieving remote bytes for file {filepath} : {message}",
                    path, e.Message);
                throw;
            }
            finally
            {
                File.Delete(localTempFile);
            }
        }

        public void DeleteFile(string path)
        {
            OpenSession();
            try
            {
                _session.RemoveFile(path);
            }
            catch (Exception e)
            {
                _logger.LogError(e,
                    "Error deleting exported file, from FTP storage: {message}", e.Message);
                throw;
            }
        }

        public void Dispose()
        {
            _session.Dispose();
        }
    } 
}