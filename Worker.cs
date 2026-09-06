using Google.Apis.Auth.OAuth2;
using Google.GenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;
using System.Text.Json;

namespace AgentService
{
    public class Worker : BackgroundService
    {
        private const string ReportFilePath = @"C:/Test/Reports";
        private const int MaxRetries = 3;
        
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _config;
        private readonly ILogReader _logReader;
        private readonly string _reportFile;
        private IChatClient _client = null!;

        public Worker(ILogger<Worker> logger, IConfiguration config, ILogReader logReader)
        {
            _logger = logger;
            _config = config;
            _logReader = logReader;
            _reportFile = Path.Combine(ReportFilePath, $"LogAnalysisReport_{DateTime.Now:yyyyMMdd_HHmmss}.json");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Initialize();

            var agent = await CreateAgentAsync();

            while (!stoppingToken.IsCancellationRequested)
            {
                var session = await agent.CreateSessionAsync();

                _logger.LogInformation("Log analyzer agent is running.");
                _logger.LogInformation("Fetching log entries ...");

                var logs = await _logReader.ReadAsync(stoppingToken);

                _logger.LogInformation($"Fetched {logs.Length} characters of log data.");
                _logger.LogInformation("Proceeding with analyzing logs ...");

                List<ReportEntry>? newEntries = null;
                int currentAttempt = 0;

                string promptMessage = $"Analyze the following logs and provide a summary of any errors or warnings:\n {logs}";
                while (currentAttempt < MaxRetries && newEntries == null)
                {
                    currentAttempt++;

                    var response = await agent.RunAsync(
                        message: promptMessage,
                        session: session,
                        options: new() { ResponseFormat = ChatResponseFormat.ForJsonSchema(typeof(Report)) });

                    try
                    {
                        newEntries = JsonSerializer.Deserialize<Report>(response.Text)!.Entries;
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogWarning(ex, $"Failed to parse agent response on attempt {currentAttempt} of {MaxRetries}.");

                        if (currentAttempt < MaxRetries)
                            promptMessage = "The parsing of the report failed, please review the format of your response and correct it.";
                        else
                            _logger.LogError("Max retries reached. Unable to parse agent response for this iteration.");
                    }
                }

                if (newEntries is null || newEntries.Count is 0)
                {
                    _logger.LogInformation("No new entries to add. The application will cool down for 30 minutes before next iteration.");
                    await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
                    continue;
                }

                Report existingReport = new();
                var existingJson = await File.ReadAllTextAsync(_reportFile, stoppingToken);
                try
                {
                    existingReport = JsonSerializer.Deserialize<Report>(existingJson) ?? new Report();
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, $"Could not deserialize existing report file due to {ex.Message}. Overwriting with new entries.");
                }

                existingReport.Entries.AddRange(newEntries);

                var updatedJsonReport = JsonSerializer.Serialize(existingReport, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(_reportFile, updatedJsonReport, stoppingToken);

                _logger.LogInformation("Analysis complete. Cooling down for 30 minutes before next iteration.");
                await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
            }
        }

        private async Task Initialize()
        {
            CreateDirectoriesAndFiles();
            var provider = _config["Provider"];

            if (string.IsNullOrEmpty(provider))
                throw new InvalidOperationException("Provider is not set in the configuration.");

            var model = provider.Equals("OpenAI", StringComparison.OrdinalIgnoreCase) 
                ? _config["OPENAI_MODEL"] 
                : _config["VERTEX_AI_MODEL"];

            var apiKey = provider.Equals("OpenAI", StringComparison.OrdinalIgnoreCase)
                ? Environment.GetEnvironmentVariable("OPENAI_API_KEY")
                : string.Empty;

            if (provider.Equals("OpenAI", StringComparison.OrdinalIgnoreCase) && string.IsNullOrEmpty(apiKey))
                throw new InvalidOperationException("OPENAI_API_KEY is required for OpenAI provider.");

            _client = _config["Provider"] switch
            {
                "OpenAI" => new OpenAIClient(new ApiKeyCredential(apiKey!)).GetChatClient(model).AsIChatClient(),
                "Google" => new Client(
                    vertexAI: true, 
                    credential: await GoogleCredential.GetApplicationDefaultAsync(), 
                    project: _config["GoogleCloud:Project"], 
                    location: _config["GoogleCloud:Location"]).AsIChatClient(model),
                _ => throw new InvalidOperationException("Unsupported provider.")
            };
        }

        private void CreateDirectoriesAndFiles()
        {
            if (!Directory.Exists(ReportFilePath))
                Directory.CreateDirectory(ReportFilePath);

            if (!File.Exists(_reportFile))
                File.Create(_reportFile).Dispose();
        }

        private async Task<AIAgent> CreateAgentAsync()
        {
            var instructions = await File.ReadAllTextAsync(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Agent", "Instructions", "logAnalyzerInstructions.md"));
            instructions = instructions.Replace("{{REPORT_PATH}}", _reportFile);

            ChatClientAgentOptions options = new()
            {
                Name = "LogAnalyzerAgent",
                ChatOptions = new()
                {
                    Instructions = instructions,
                    Tools = 
                    [
                        //AIFunctionFactory.Create(Tools.PatchContent), 
                        //AIFunctionFactory.Create(Tools.CreateFile), 
                        AIFunctionFactory.Create(Tools.ReadFile),
                        AIFunctionFactory.Create(Tools.ExecuteShellCommand)
                    ]
                }
            };

            return _client.AsBuilder().Build().AsAIAgent(options);
        }
    }

    public enum LogLevel
    {
        Info,
        Warning,
        Error
    }

    public class LogEntry
    {
        public LogEntry(DateTime timeStamp, LogLevel level, string message, string source)
        {
            this.timeStamp = timeStamp;
            this.level = level;
            this.message = message;
            this.source = source;
        }

        private readonly DateTime timeStamp;
        private readonly LogLevel level;
        private readonly string message;
        private readonly string source;

        public string ToLogString() => $"[{timeStamp}] [{level}] {message} (Source: {source})";
        public override string ToString() => ToLogString();
    }
}