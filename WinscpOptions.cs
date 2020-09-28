namespace ExportFromFTP
{
    public class WinscpOptions
    {
        public string Protocol {get; set;} = null!;
        public string HostName {get; set;} = null!;
        public string UserName {get; set;} = null!;
        public string Password {get; set;} = null!;
    }
}
