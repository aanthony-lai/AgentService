using System;
using System.Collections.Generic;
using System.Text;

namespace AgentService
{
    public interface ILogReader
    {
        Task<string> ReadAsync(CancellationToken stoppingToken);
    }

    public class LogReader(IConfiguration config) : ILogReader
    {
        public async Task<string> ReadAsync(CancellationToken stoppingToken) =>
            await File.ReadAllTextAsync(config["LogSource"]!, stoppingToken);
    }
}
