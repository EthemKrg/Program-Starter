using System.IO;
using ProgramStarter.App.Models;

namespace ProgramStarter.App.Services;

internal class PathValidationService : IPathValidationService
{
    private static readonly string[] SupportedExtensions = { ".exe" };

    public bool IsSupportedExtension(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        var extension = Path.GetExtension(path);
        return SupportedExtensions.Any(ext =>
            string.Equals(extension, ext, StringComparison.OrdinalIgnoreCase));
    }

    public bool FileExists(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        return File.Exists(path);
    }

    public bool IsValidAppPath(string? path)
    {
        return IsSupportedExtension(path) && FileExists(path);
    }

    public (bool IsValid, string ErrorMessage) ValidateForLaunch(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return (false, "Path is empty.");

        if (!IsSupportedExtension(path))
            return (false, "Only .exe files are supported.");

        if (!FileExists(path))
            return (false, "File does not exist.");

        return (true, string.Empty);
    }

    public (bool IsValid, string ErrorMessage) ValidateForAdd(string path, string appName, AppGroup group, AppEntry? excludeApp = null)
    {
        if (string.IsNullOrWhiteSpace(path))
            return (false, "Path is empty.");

        if (!IsSupportedExtension(path))
            return (false, "Only .exe files are supported.");

        if (!FileExists(path))
            return (false, "File does not exist.");

        var trimmedName = appName.Trim();
        if (trimmedName.Length == 0)
            return (false, "App name cannot be empty.");

        if (group.Apps.Any(a =>
            !ReferenceEquals(a, excludeApp) &&
            string.Equals(a.Name, trimmedName, StringComparison.OrdinalIgnoreCase)))
            return (false, $"An app named \"{trimmedName}\" already exists in this group.");

        if (group.Apps.Any(a =>
            !ReferenceEquals(a, excludeApp) &&
            string.Equals(a.Path, path, StringComparison.OrdinalIgnoreCase)))
            return (false, "This app path already exists in this group.");

        return (true, string.Empty);
    }
}
