using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace ExportFromFTP
{
    public class FtpServiceFakeDelete : FtpService
    {
        public FtpServiceFakeDelete(IOptions<WinscpOptions> sessionOptions, 
                                    ILogger<FtpService> logger)
            : base(sessionOptions, logger) {}
        
        public override bool DeleteFile(string path)
        {
            return true;
        }
    } 
}