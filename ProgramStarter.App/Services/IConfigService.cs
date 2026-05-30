using ProgramStarter.App.Models;

namespace ProgramStarter.App.Services;

public interface IConfigService
{
    AppSettings Load();
    void Save(AppSettings settings);
}
