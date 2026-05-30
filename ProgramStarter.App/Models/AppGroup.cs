namespace ProgramStarter.App.Models;

public class AppGroup
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "New Group";
    public List<AppEntry> Apps { get; set; } = new();
}
