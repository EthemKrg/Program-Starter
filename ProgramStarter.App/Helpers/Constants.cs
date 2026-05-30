namespace ProgramStarter.App.Helpers;

public static class Constants
{
    public const string AppName = "Program Starter";
    public const string ConfigDirectory = "ProgramStarter";
    public const string ConfigFileName = "config.json";
    public const string LogsDirectory = "logs";
    public const string LogFileName = "app.log";

    public const int WindowMinWidth = 900;
    public const int WindowMinHeight = 560;
    public const int SidebarWidth = 220;
    public const int MainContentPadding = 28;
    public const int SectionSpacing = 20;
    public const int CardPadding = 16;
    public const int CardVerticalGap = 12;
    public const int ButtonHeight = 36;
    public const int SmallButtonHeight = 32;

    public const int DefaultDelayMilliseconds = 1000;
    public const int MaxLogSizeBytes = 1_048_576; // 1 MB
    public const int MaxRotatedLogFiles = 5;
    public const int SupportedSchemaVersion = 1;
}
