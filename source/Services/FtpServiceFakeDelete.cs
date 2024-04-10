using System;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace ExportFromFTP
{
    public class FtpServiceFakeDelete : FtpService
    {
        public FtpServiceFakeDelete(IOptions<WinscpOptions> sessionOptions, 
                                    ILogger<FtpService> logger,
                                    Boolean withFileTransfer )
            : base(sessionOptions, logger, withFileTransfer) {}
        
        public override bool DeleteFile(string path)
        {
            return true;
        }
    } 
}