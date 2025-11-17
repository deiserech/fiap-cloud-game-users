
using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FiapCloudGames.Api
{
    public class ResourceLoggingService : BackgroundService
    {
        private readonly ILogger<ResourceLoggingService> _logger;

        public ResourceLoggingService(ILogger<ResourceLoggingService> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var process = Process.GetCurrentProcess();
            var lastCpuTime = process.TotalProcessorTime;
            var lastTime = DateTime.UtcNow;
            int processorCount = Environment.ProcessorCount;

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                process.Refresh();
                var memoryMb = process.WorkingSet64 / (1024 * 1024);
                var cpuTime = process.TotalProcessorTime.TotalSeconds;

                // Calcular CPU usage (%)
                var now = DateTime.UtcNow;
                var cpuUsedMs = (process.TotalProcessorTime - lastCpuTime).TotalMilliseconds;
                var elapsedMs = (now - lastTime).TotalMilliseconds;
                var cpuUsage = (elapsedMs > 0) ? (cpuUsedMs / (elapsedMs * processorCount)) * 100.0 : 0.0;

                double? memUsagePct = null;
                double? memLimitMb = null;
                try
                {
                    var memLimitStr = File.ReadAllText("/sys/fs/cgroup/memory/memory.limit_in_bytes");
                    if (long.TryParse(memLimitStr, out var memLimitBytes) && memLimitBytes > 0 && memLimitBytes < long.MaxValue)
                    {
                        memLimitMb = memLimitBytes / (1024.0 * 1024.0);
                        memUsagePct = (memLimitMb > 0) ? (memoryMb / memLimitMb) * 100.0 : null;
                    }
                }
                catch { }

                lastCpuTime = process.TotalProcessorTime;
                lastTime = now;
                var logMsg = $"ResourceUsage | cpu_time_s={cpuTime} | cpu_usage_pct={cpuUsage} | memory_mb={memoryMb}";
                if (memLimitMb.HasValue)
                    logMsg += $" | memory_limit_mb={memLimitMb}";
                if (memUsagePct.HasValue)
                    logMsg += $" | mem_usage_pct={memUsagePct}";
                _logger.LogInformation(logMsg);
            }
        }
    }
}
