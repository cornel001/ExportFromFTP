using System;
using System.Diagnostics;

namespace ExportFromFTP
{
    public class DebugStopwatch
    {
        private Stopwatch _stopWatch;

        public DebugStopwatch()
        {
            _stopWatch = new Stopwatch();
        }

        [Conditional("DEBUG")]
        public void Start()
        {
            _stopWatch.Start();
        }

        [Conditional("DEBUG")]
        public void Stop()
        {
            _stopWatch.Stop();
            Elapsed = _stopWatch.Elapsed;
        }

        public TimeSpan Elapsed {get; private set;}
    }
}