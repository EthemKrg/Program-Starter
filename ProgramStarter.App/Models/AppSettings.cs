namespace ProgramStarter.App.Models;

public class AppSettings
{
    public int SchemaVersion { get; set; } = 1;
    public List<AppGroup> Groups { get; set; } = new();
    public string? LastSelectedGroupId { get; set; }
    public string Theme { get; set; } = "Dark";
    public int DefaultDelayMilliseconds { get; set; } = 1000;
}
