namespace ProgramStarter.App.Models;

public enum LaunchErrorCode
{
    None,
    EmptyPath,
    FileNotFound,
    UnsupportedFileType,
    AccessDenied,
    ProcessStartFailed,
    Unknown
}
