using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace ExportFromFTP
{

    public static class ExportStrategyFactory
    {
        public static IExportStrategy CreateExportStrategySemaphore(
            IServiceProvider serviceProvider,
            ILogger<ExportWorker> logger,
            Func<(string, DateTime), IFtpService, Task> ProcessFile)
        {
            return new ExportStrategySemaphore(serviceProvider, logger, ProcessFile);
        }

        public static IExportStrategy CreateExportStrategyPartition(
            IServiceProvider serviceProvider,
            ILogger<ExportWorker> logger,
            Func<(string, DateTime), IFtpService, Task> ProcessFile)
        {
            return new ExportStrategyPartition(serviceProvider, logger, ProcessFile);
        }

        public static IExportStrategy CreateExportStrategyPartitionParallel(
            IServiceProvider serviceProvider,
            ILogger<ExportWorker> logger,
            Func<(string, DateTime), IFtpService, Task> ProcessFile)
        {
            return new ExportStrategyPartitionParallel(serviceProvider, logger, ProcessFile);
        }

        public static IExportStrategy CreateExportStrategyActionBlock(
            IServiceProvider serviceProvider,
            ILogger<ExportWorker> logger,
            Func<(string, DateTime), IFtpService, Task> ProcessFile)
        {
            return new ExportStrategyActionBlock(serviceProvider, logger, ProcessFile);
        }
    }

    public abstract class ExportStrategy : IExportStrategy
    {

        protected readonly ILogger<ExportWorker> _logger;
        protected readonly IServiceProvider _serviceProvider;
        protected readonly Func<(string, DateTime), IFtpService, Task> _ProcessFile;

        public ExportStrategy(
            IServiceProvider serviceProvider,
            ILogger<ExportWorker> logger,
            Func<(string, DateTime), IFtpService, Task> ProcessFile)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _ProcessFile = ProcessFile;
        }

        public abstract Task ExecuteExportAsync(IEnumerable<(string, DateTime)> source, int dop);
    }

    public class ExportStrategySemaphore : ExportStrategy
    {
        public ExportStrategySemaphore(
            IServiceProvider serviceProvider,
            ILogger<ExportWorker> logger,
            Func<(string, DateTime), IFtpService, Task> ProcessFile) : base(serviceProvider, logger, ProcessFile) {}

        public override async Task ExecuteExportAsync(IEnumerable<(string, DateTime)> source, int dop)
        {
            var ftpServiceScopeStack = new ConcurrentStack<IServiceScope>();
            var ftpServiceStack = new ConcurrentStack<IFtpService>();

            for (int i = 1; i <= dop; i++)
            {
                var scope = _serviceProvider.CreateScope();
                var ftpService = scope.ServiceProvider.GetRequiredService<IFtpService>();
                ftpServiceScopeStack.Push(scope);
                ftpServiceStack.Push(ftpService);
            }
            
            SemaphoreSlim semaphore = new SemaphoreSlim(dop);
            await Task.WhenAll(from item in source   
                select Task.Run(async delegate
                {
                    await semaphore.WaitAsync();
                    IFtpService? ftpService = null;
                    try {
                        if (ftpServiceStack.TryPop(out ftpService))
                            await _ProcessFile(item, ftpService).ConfigureAwait(false);
                    }
                    finally
                    {
                        if (ftpService != null)
                            ftpServiceStack.Push(ftpService);
                        semaphore.Release();
                    }
                }));

            IServiceScope? scope1 = null;
            while (!ftpServiceScopeStack.IsEmpty)
            {
                if (ftpServiceScopeStack.TryPop(out scope1))
                    scope1.Dispose();
            }
        }
    }

    public class ExportStrategyPartition : ExportStrategy
    {
        public ExportStrategyPartition(
            IServiceProvider serviceProvider,
            ILogger<ExportWorker> logger,
            Func<(string, DateTime), IFtpService, Task> ProcessFile) : base(serviceProvider, logger, ProcessFile) {}

        public override async Task ExecuteExportAsync(IEnumerable<(string, DateTime)> source, int dop)
        {
            await Task.WhenAll(
                from partition in Partitioner.Create(source).GetPartitions(dop) 
                    select Task.Run(async delegate
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var ftpService = scope.ServiceProvider.GetRequiredService<IFtpService>();
                        using (partition)
                            while (partition.MoveNext())
                                await _ProcessFile(partition.Current, ftpService).ConfigureAwait(false);
                    }));
        }
    }

    public class ExportStrategyPartitionParallel : ExportStrategy
    {
        public ExportStrategyPartitionParallel(
            IServiceProvider serviceProvider,
            ILogger<ExportWorker> logger,
            Func<(string, DateTime), IFtpService, Task> ProcessFile) : base(serviceProvider, logger, ProcessFile) {}

        public override async Task ExecuteExportAsync(IEnumerable<(string, DateTime)> source, int dop)
        {
            async Task AwaitPartition(IEnumerator<(string, DateTime)> partition)
            {
                using var scope = _serviceProvider.CreateScope();
                var ftpService = scope.ServiceProvider.GetRequiredService<IFtpService>();
                using (partition)
                    while (partition.MoveNext())
                        await _ProcessFile(partition.Current, ftpService).ConfigureAwait(false);
            }

            await Task.WhenAll(
                Partitioner.Create(source).GetPartitions(dop).AsParallel().Select(p => AwaitPartition(p)));
        }
    }

    public class ExportStrategyActionBlock : ExportStrategy
    {
        public ExportStrategyActionBlock(
            IServiceProvider serviceProvider,
            ILogger<ExportWorker> logger,
            Func<(string, DateTime), IFtpService, Task> ProcessFile) : base(serviceProvider, logger, ProcessFile) {}

        public override async Task ExecuteExportAsync(IEnumerable<(string, DateTime)> source, int dop)
        {
            var ftpServiceScopeStack = new ConcurrentStack<IServiceScope>();
            var ftpServiceStack = new ConcurrentStack<IFtpService>();

            for (int i = 1; i <= dop; i++)
            {
                var scope = _serviceProvider.CreateScope();
                var ftpService = scope.ServiceProvider.GetRequiredService<IFtpService>();
                ftpServiceScopeStack.Push(scope);
                ftpServiceStack.Push(ftpService);
            }

            var options = new ExecutionDataflowBlockOptions {MaxDegreeOfParallelism = dop};
            var block = new ActionBlock<(string , DateTime)>(PreProcessFile, options);

            foreach (var item in source)
                await block.SendAsync(item);
            block.Complete();
            await block.Completion;

            IServiceScope? scope1 = null;
            while (!ftpServiceScopeStack.IsEmpty)
            {
                if (ftpServiceScopeStack.TryPop(out scope1))
                    scope1.Dispose();
            }

            async Task PreProcessFile((string , DateTime) item)
            {
                IFtpService? ftpService = null;
                try {
                    if (ftpServiceStack.TryPop(out ftpService))
                        await _ProcessFile(item, ftpService).ConfigureAwait(false);
                }
                finally
                {
                    if (ftpService != null)
                        ftpServiceStack.Push(ftpService);
                }   
            }

        }
    }
}