using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json;

namespace AgentService
{
    public record Patch(string contentToBeReplaced, string replacementContent);

    public class Tools
    {
        private static ILogger logger = LoggerFactory.Create(cfg => cfg.AddConsole()).CreateLogger<Tools>();

        //[Description("""
        //    Creates a new file or overwrites an existing file with the provided text. 
        //    Parameters: 
        //    - 'filePath' is the target path including file name/extension (e.g., 'docs/notes.txt'); 
        //    - 'content' is the text string to write inside the file (optional, defaults to empty file). Returns a status message.
        //    """)]
        //public static async Task<string> CreateFile(string filePath, string? content = null)
        //{
        //    logger.LogInformation($"TOOL_INVOCATION: {nameof(CreateFile)}");
        //    try
        //    {
        //        await File.WriteAllTextAsync(filePath, content);

        //        var eventEntry = ToolEventEntry.CreateSuccessEntry(
        //            timeStamp: DateTime.Now,
        //            name: nameof(CreateFile),
        //            output: $"File created at: {filePath}",
        //            args: [$"{nameof(filePath)}: {filePath}", $"{nameof(content)}: {content ?? "N/A"}"]);

        //        await ToolEventManager.WriteToolEventLog(eventEntry);

        //        return $"File: {Path.GetFileName(filePath)} was successfully created.";
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.LogError(ex, $"Error occurred while creating file: {filePath}");

        //        var eventEntry = ToolEventEntry.CreateFailureEntry(
        //            exception: ex, 
        //            timeStamp: DateTime.Now, 
        //            name: nameof(CreateFile),
        //            args: [$"{nameof(filePath)}: {filePath}", $"{nameof(content)}: {content ?? "N/A"}"]);

        //        await ToolEventManager.WriteToolEventLog(eventEntry);

        //        return $"The operation failed with the following error: {ex.Message}, Inner Exception: {ex.InnerException?.Message ?? "N/A"}";
        //    }
        //}

        //[Description("""
        //    Replaces target text blocks within a file with new content.
    
        //    Usage Guidelines:
        //    - `contentToBeReplaced` must match the target text EXACTLY, including indentation, spaces, and newline characters.
        //    - Provide enough unique context lines in `contentToBeReplaced` to ensure a single, unambiguous match.
        //    - Keep patches minimal and localized—only include lines that are changing or required for precise matching.
        //    """)]
        //public static async Task<string> PatchContent(string path, List<Patch> patches)
        //{
        //    var missingPatches = new List<Patch>();
        //    logger.LogInformation($"TOOL_INVOCATION: {nameof(PatchContent)}.");
        //    try
        //    {
        //        if (!Path.Exists(path))
        //        {
        //            logger.LogInformation($"File not found at path '{path}'.");

        //            var eventEntry = ToolEventEntry.CreateFailureEntry(
        //                timeStamp: DateTime.Now,
        //                name: nameof(PatchContent),
        //                errorMessage: $"File not found at path '{path}'",
        //                args: [$"{nameof(path)}: {path}", $"{nameof(patches)}: {JsonSerializer.Serialize(patches)}"]);

        //            await ToolEventManager.WriteToolEventLog(eventEntry);
        //            return $"The provided file path doesn't exist. Please check '{path}'.";
        //        }

        //        var content = await File.ReadAllTextAsync(path);

        //        foreach (var patch in patches)
        //        {
        //            if (!content.Contains(patch.contentToBeReplaced))
        //            {
        //                missingPatches.Add(patch);
        //                continue;
        //            }

        //            content = content.Replace(patch.contentToBeReplaced, patch.replacementContent);
        //        }
                
        //        await File.WriteAllTextAsync(path, content);
        //    }
        //    catch (Exception ex)
        //    {
        //        var eventEntry = ToolEventEntry.CreateFailureEntry(
        //            timeStamp: DateTime.Now,
        //            name: nameof(PatchContent),
        //            errorMessage: $"Failed to patch file at path '{path}'",
        //            exception: ex,
        //            args: [$"{nameof(path)}: {path}", $"{nameof(patches)}: {JsonSerializer.Serialize(patches)}"]);

        //        await ToolEventManager.WriteToolEventLog(eventEntry);
        //        return $"The operation failed with the following error: {ex.Message}, Inner Exception: {ex.InnerException?.Message ?? "N/A"}";
        //    }

        //    ToolEventEntry eventEntrySuccess;

        //    if (missingPatches.Any())
        //    {
        //        eventEntrySuccess = ToolEventEntry.CreateFailureEntry(
        //            timeStamp: DateTime.Now,
        //            name: nameof(PatchContent),
        //            errorMessage: $"Some patches could not be applied to the file at path '{path}'",
        //            args: [$"{nameof(path)}: {path}", $"{nameof(patches)}: {JsonSerializer.Serialize(patches)}", $"MissingPatches: {JsonSerializer.Serialize(missingPatches)}"]);

        //        await ToolEventManager.WriteToolEventLog(eventEntrySuccess);

        //        return $"""
        //        The following patches couldn't not be patched: 
        //        {missingPatches.Select(p => $"Old value: {p.contentToBeReplaced} - New value: {p.replacementContent}")}
        //        """;
        //    }

        //    eventEntrySuccess = ToolEventEntry.CreateSuccessEntry(
        //        timeStamp: DateTime.Now,
        //        name: nameof(PatchContent),
        //        output: $"All patches were successfully applied to the file at path '{path}'",
        //        args: [$"{nameof(path)}: {path}", $"{nameof(patches)}: {JsonSerializer.Serialize(patches)}"]);

        //    await ToolEventManager.WriteToolEventLog(eventEntrySuccess);
        //    return "All patches were successfully updated.";
        //}

        [Description("Reads the text content of a file given its local file path.")]
        public static async Task<string> ReadFile([Description("The absolute or relative file path to read.")] string filePath)
        {
            logger.LogInformation($"TOOL_INVOCATION: {nameof(ReadFile)}");
            if (!File.Exists(filePath))
            {
                var eventEntry = ToolEventEntry.CreateFailureEntry(
                    timeStamp: DateTime.Now,
                    name: nameof(ReadFile),
                    errorMessage: $"File not found at path '{filePath}'",
                    args: [$"{nameof(filePath)}: {filePath}"]);
                
                await ToolEventManager.WriteToolEventLog(eventEntry);
                return $"Error: File not found at path '{filePath}'.";
            }

            try
            {
                var eventEntry = ToolEventEntry.CreateSuccessEntry(
                    timeStamp: DateTime.Now,
                    name: nameof(ReadFile),
                    output: $"Successfully read file at path '{filePath}'",
                    args: [$"{nameof(filePath)}: {filePath}"]);

                await ToolEventManager.WriteToolEventLog(eventEntry);
                return File.ReadAllText(filePath);
            }
            catch (Exception ex)
            {
                var eventEntry = ToolEventEntry.CreateFailureEntry(
                    timeStamp: DateTime.Now,
                    name: nameof(ReadFile),
                    errorMessage: $"Failed to read file at path '{filePath}'",
                    exception: ex,
                    args: [$"{nameof(filePath)}: {filePath}"]);

                await ToolEventManager.WriteToolEventLog(eventEntry);   
                return $"Error reading file: {ex.Message}";
            }
        }

        [Description("""
            LAST RESORT ONLY: Use this tool ONLY if no other specialized tool supports the operation you are trying to perform.
            Executes a single command-line executable with an array of arguments.
            Each call runs exactly one command.
            Chaining commands (e.g., using &&, |, ;, or sending multiple commands at once) is NOT supported and will fail.
            """)]
        public static async Task<string> ExecuteShellCommand(
        [Description("The executable name or full path only (e.g. 'git', 'dotnet', 'cmd.exe')")] string command,
        [Description("Single set of command-line arguments for the executable.")] string[] args)
        {
            logger.LogInformation($"TOOL_INVOCATION: '{command} {string.Join(" ", args)}'");
            var startInfo = new ProcessStartInfo
            {
                FileName = command,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = Environment.CurrentDirectory
            };

            foreach (var arg in args)
                startInfo.ArgumentList.Add(arg);

            var process = new Process { StartInfo = startInfo };

            try
            {
                process.Start();

                Task<string> outputTask = process.StandardOutput.ReadToEndAsync();
                Task<string> errorTask = process.StandardError.ReadToEndAsync();

                await process.WaitForExitAsync();

                string output = await outputTask;
                string error = await errorTask;

                if (!string.IsNullOrWhiteSpace(output))
                    return JsonSerializer.Serialize(output);

                var result = !string.IsNullOrWhiteSpace(error) ? $"Error: {error}" : "Command executed with no output.";
                
                var eventEntry = ToolEventEntry.CreateSuccessEntry(
                    timeStamp: DateTime.Now,
                    name: nameof(ExecuteShellCommand),
                    output: result,
                    args: [$"{nameof(command)}: {command}", $"{nameof(args)}: {JsonSerializer.Serialize(args)}"]); 
                
                await ToolEventManager.WriteToolEventLog(eventEntry);
                return JsonSerializer.Serialize(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while executing shell command");

                var eventEntry = ToolEventEntry.CreateFailureEntry(
                    timeStamp: DateTime.Now,
                    name: nameof(ExecuteShellCommand),
                    errorMessage: $"Failed to execute shell command '{command}'",
                    exception: ex,
                    args: [$"{nameof(command)}: {command}", $"{nameof(args)}: {JsonSerializer.Serialize(args)}"]);

                await ToolEventManager.WriteToolEventLog(eventEntry);
                return JsonSerializer.Serialize($"Error: {ex.Message}. Please review the command.");
            }
        }
    }
}
