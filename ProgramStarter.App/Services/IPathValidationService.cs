using ProgramStarter.App.Models;

namespace ProgramStarter.App.Services;

public interface IPathValidationService
{
    bool IsValidAppPath(string? path);
    bool IsSupportedExtension(string? path);
    bool FileExists(string? path);
    (bool IsValid, string ErrorMessage) ValidateForLaunch(string? path);
    (bool IsValid, string ErrorMessage) ValidateForAdd(string path, string appName, AppGroup group);
}
