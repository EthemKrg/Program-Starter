namespace ProgramStarter.App.Models;

public class LaunchResult
{
    public bool Success { get; set; }
    public string AppName { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public LaunchErrorCode ErrorCode { get; set; } = LaunchErrorCode.None;
    public string UserMessage { get; set; } = string.Empty;
    public string? TechnicalMessage { get; set; }
}
