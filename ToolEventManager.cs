using System.Text.Json;

namespace AgentService
{
    public static class ToolEventManager
    {
        private static readonly string _toolEventLogPath = @"C:/Test/ToolInvocationEvents";
        private static readonly string _toolEventFile = Path.Combine(_toolEventLogPath, $"ToolEvents_{DateTime.Now:yyyyMMdd_HHmmss}.json");

        public static async Task WriteToolEventLog(ToolEventEntry eventEntry)
        {
            if (!Directory.Exists(_toolEventLogPath))
                Directory.CreateDirectory(_toolEventLogPath);

            var json = JsonSerializer.Serialize(eventEntry);
            await File.AppendAllTextAsync(_toolEventFile, json + Environment.NewLine);
        }
    }
}
