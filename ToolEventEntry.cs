namespace AgentService
{
    public class ToolEventEntry
    {
        private ToolEventEntry(DateTime timeStamp, string name, string? output, string[]? args)
        {
            TimeStamp = timeStamp;
            Name = name;
            Result = output ?? string.Empty;
            Args = args ?? [];
            IsSuccess = true;
        }

        private ToolEventEntry(DateTime timeStamp, string name, string? errorMessage, Exception? ex, string[]? args)
        {
            FailureMessage = ex?.Message ?? errorMessage ?? "N/A";
            TimeStamp = timeStamp;
            Name = name;
            Args = args ?? [];
            IsSuccess = false;
        }

        public static ToolEventEntry CreateSuccessEntry(DateTime timeStamp, string name, string? output, string[]? args = null) =>
            new ToolEventEntry(timeStamp, name, output, args);
        public static ToolEventEntry CreateFailureEntry(DateTime timeStamp, string name, string? errorMessage = null, Exception? exception = null, string[]? args = null) =>
            new ToolEventEntry(timeStamp, name, errorMessage, exception, args);

        

        public DateTime TimeStamp { get; }
        public bool IsSuccess { get; }
        public string Name { get; } = string.Empty;
        public string FailureMessage { get; } = string.Empty;
        public string[] Args { get; }
        public string Result { get; } = string.Empty;
    }
}
