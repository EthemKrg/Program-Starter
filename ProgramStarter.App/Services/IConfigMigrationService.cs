using System.Text.Json;
using ProgramStarter.App.Models;

namespace ProgramStarter.App.Services;

public interface IConfigMigrationService
{
    AppSettings Migrate(JsonDocument rawConfig);
}
