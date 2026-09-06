using System.Text.Json.Serialization;

public class Report
{
    [JsonPropertyName("entries")]
    public List<ReportEntry> Entries { get; set; } = new();
}

public class ReportEntry
{
    [JsonPropertyName("timestamp")]
    public DateTimeOffset Timestamp { get; set; }

    [JsonPropertyName("severity")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public LogSeverity Severity { get; set; }

    [JsonPropertyName("service")]
    public string Service { get; set; } = string.Empty;

    [JsonPropertyName("summary")]
    public string Summary { get; set; } = string.Empty;

    [JsonPropertyName("root_cause_hypothesis")]
    public string RootCauseHypothesis { get; set; } = string.Empty;

    [JsonPropertyName("recommended_action")]
    public RecommendedAction RecommendedAction { get; set; } = new();
}

public class RecommendedAction
{
    [JsonPropertyName("action_type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ActionType ActionType { get; set; }

    [JsonPropertyName("target_channel")]
    public string TargetChannel { get; set; } = string.Empty;

    [JsonPropertyName("payload")]
    public ActionPayload Payload { get; set; } = new();
}

public class ActionPayload
{
    [JsonPropertyName("details")]
    public string Details { get; set; } = string.Empty;
}

public enum LogSeverity
{
    CRITICAL,
    ERROR,
    WARN,
    INFO,
    DEBUG
}

public enum ActionType
{
    IMMEDIATE_ESCALATION,
    ALERT_AND_TICKET,
    MONITOR,
    LOG_ONLY
}
