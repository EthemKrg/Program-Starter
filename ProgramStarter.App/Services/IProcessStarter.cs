using System.Diagnostics;

namespace ProgramStarter.App.Services;

public interface IProcessStarter
{
    Process? Start(ProcessStartInfo startInfo);
}
