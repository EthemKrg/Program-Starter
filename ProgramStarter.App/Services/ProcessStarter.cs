using System.ComponentModel;
using System.Diagnostics;
using ProgramStarter.App.Models;

namespace ProgramStarter.App.Services;

/// <summary>
/// Real implementation of <see cref="IProcessStarter"/> that wraps <see cref="Process.Start"/>.
/// Maps Win32Exception to specific LaunchErrorCode based on NativeErrorCode.
/// </summary>
internal class ProcessStarter : IProcessStarter
{
    public (Process? Process, LaunchErrorCode? Error) Start(ProcessStartInfo startInfo)
    {
        try
        {
            var process = Process.Start(startInfo);
            return (process, null);
        }
        catch (Win32Exception ex)
        {
            // Common Win32 error codes for file-not-found scenarios
            const int ERROR_FILE_NOT_FOUND = 2;
            const int ERROR_PATH_NOT_FOUND = 3;
            const int ERROR_ACCESS_DENIED = 5;

            var errorCode = ex.NativeErrorCode switch
            {
                ERROR_FILE_NOT_FOUND => LaunchErrorCode.FileNotFound,
                ERROR_PATH_NOT_FOUND => LaunchErrorCode.FileNotFound,
                ERROR_ACCESS_DENIED => LaunchErrorCode.AccessDenied,
                _ => LaunchErrorCode.Unknown
            };

            return (null, errorCode);
        }
        catch (InvalidOperationException)
        {
            // Process is already disposed or no file specified
            return (null, LaunchErrorCode.ProcessStartFailed);
        }
    }
}
