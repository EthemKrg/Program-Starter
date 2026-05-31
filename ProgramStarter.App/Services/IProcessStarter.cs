using System.Diagnostics;
using ProgramStarter.App.Models;

namespace ProgramStarter.App.Services;

public interface IProcessStarter
{
    (Process? Process, LaunchErrorCode? Error) Start(ProcessStartInfo startInfo);
}
