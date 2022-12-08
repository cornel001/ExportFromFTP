using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;

namespace ExportFromFTP
{
    public static class Extensions
    {
        public static void UpdateStatus(this FileInfo fileInfo)
        {
            if (fileInfo.Status == FileStatus.Initial)
            {
                fileInfo.Status = FileStatus.Sent;
                return;
            }

            if (fileInfo.Status == FileStatus.Sent)
                fileInfo.Status = FileStatus.Finished;
        }

        public static void UpdateWriteTime(this FileInfo fileInfo, DateTime lastWriteTime) =>
            fileInfo.LastWriteTime = lastWriteTime;

        public static Task ForEachAsync<T>(this IEnumerable<T> source, int dop, Func<T, Task> body)
        {
            return Task.WhenAll(from partition in Partitioner.Create(source).GetPartitions(dop) 
                select Task.Run(async delegate
                {
                    using (partition)
                        while (partition.MoveNext())
                            await body(partition.Current).ConfigureAwait(false);
                }));
        }    
    }
}