# Program Starter Progress

## Current Status

Program Starter is in active development. The config persistence layer and group management are complete.

Current completed checkpoints:

- Phase 0: WPF Solution Foundation
- Phase 1: Config, Paths, Migration, and Logging
- Phase 2: Group CRUD Foundation

Next: Phase 3 - App Management

## Phase 0 Summary

Phase 0 established the base application structure:

- WPF desktop app project
- xUnit test project
- MVVM folder structure
- Base models (`AppSettings`, `AppGroup`, `AppEntry`, `LaunchResult`, `LaunchErrorCode`)
- Service interfaces (`IConfigService`, `IConfigMigrationService`, `IAppLogger`, `IFileDialogService`, `IPathValidationService`, `IProcessStarter`, `IAppLauncherService`)
- `RelayCommand` and `AsyncRelayCommand`
- Dependency Injection bootstrap (`Microsoft.Extensions.DependencyInjection`)
- `MainWindow` and `MainViewModel`
- Dark shell UI
- Theme resource dictionaries (`Colors.xaml`, `Typography.xaml`, `Cards.xaml`, `Buttons.xaml`, `Layout.xaml`)
- Empty state UI

No real app-launching, config persistence, CRUD, shortcut support, tray support, installer, settings screen, or advanced launch options were implemented.

## Phase 1 Summary

Phase 1 established stable local persistence:

- [`AppPaths`](ProgramStarter.App/Helpers/AppPaths.cs) - `%LocalAppData%/ProgramStarter/` with `SetTestModeRoot`/`ResetRoot` for test isolation
- [`JsonOptions`](ProgramStarter.App/Helpers/JsonOptions.cs) - CamelCase, indented JSON serializer defaults
- [`Constants`](ProgramStarter.App/Helpers/Constants.cs) - Config directory, log directory, schema version, layout constants
- [`IAppLogger`](ProgramStarter.App/Services/IAppLogger.cs) → [`FileAppLogger`](ProgramStarter.App/Services/FileAppLogger.cs) - Lock-based thread-safe logging with rotation (1 MB cap, 5 files)
- [`IConfigMigrationService`](ProgramStarter.App/Services/IConfigMigrationService.cs) → [`ConfigMigrationService`](ProgramStarter.App/Services/ConfigMigrationService.cs) - Schema version check, future schema protection (`UnsupportedSchemaException`), normalization
- [`IConfigService`](ProgramStarter.App/Services/IConfigService.cs) → [`JsonConfigService`](ProgramStarter.App/Services/JsonConfigService.cs) - Load with default creation, corrupted backup, atomic save (temp file + `File.Move`)
- Normalization rules: null groups → empty, blank names → "New Group"/"New App", negative delay → 1000, empty theme → "Dark", orphaned `LastSelectedGroupId` → null
- 14 unit tests covering config lifecycle, schema migration, and normalization
- `InternalsVisibleTo` for test access

## Phase 2 Summary

Phase 2 established the group management foundation:

- [`IDialogService`](ProgramStarter.App/Services/IDialogService.cs) - Interface for testable modal dialogs (`ShowTextInputDialog`, `ShowConfirmDialog`)
- [`WpfDialogService`](ProgramStarter.App/Services/WpfDialogService.cs) - WPF implementation with owner window resolution
- [`TextInputDialog`](ProgramStarter.App/Views/Dialogs/TextInputDialog.xaml) - Modal text input with OK/Cancel, Enter key, whitespace validation
- [`ConfirmDialog`](ProgramStarter.App/Views/Dialogs/ConfirmDialog.xaml) - Modal confirmation with customizable button text
- [`MainViewModel`](ProgramStarter.App/ViewModels/MainViewModel.cs) enhanced with:
  - `AddGroupCommand` - Creates group via dialog, validates duplicate names, selects new group
  - `RenameGroupCommand` - Pre-filled rename dialog, validates duplicates, replaces in collection to refresh UI
  - `DeleteGroupCommand` - Confirmation dialog, removes group, selects next group (or last) after deletion
  - `_isLoading` guard - Suppresses `SaveConfig()` during constructor initialization
  - `_cachedTheme` / `_cachedDelay` - Preserves hand-edited config values in `SaveConfig()`
  - Last selected group restoration on startup via `LastSelectedGroupId`
- [`MainWindow.xaml`](ProgramStarter.App/MainWindow.xaml) converted to sidebar with `ListBox`:
  - `SelectedItem` binding for group selection
  - Inline `DataTemplate` with group name, rename (✏), and delete (✕) buttons
  - `ItemContainerStyle` with hover and selected state triggers
  - `HiddenWhenSelected` style trigger for the "Select a group" prompt
  - Content area shows group name + app count when selected
- [`App.xaml.cs`](ProgramStarter.App/App.xaml.cs) - Registered `IDialogService → WpfDialogService`
- 14 new tests using `FakeDialogService`, `FakeConfigService`, and `FakeLogger` covering:
  - Constructor: last selected restoration, missing ID, empty groups
  - Add: valid name, duplicate name, cancelled
  - Rename: valid name, duplicate name, cancelled
  - Delete: confirmed, cancelled, last group, middle group with next selection
  - Save: Theme/Delay cache preservation

## Verification

All phases were verified with:

```bash
dotnet build  # 0 errors, 0 warnings
dotnet test   # 28/28 passed (14 config + 14 group management)
```

## Phase Completion Checklist

| Phase | Description | Status |
|-------|-------------|--------|
| Phase 0 | WPF Solution Foundation | ✅ Complete |
| Phase 1 | Config, Paths, Migration, and Logging | ✅ Complete |
| Phase 2 | Group CRUD Foundation | ✅ Complete |
| Phase 3 | App Management | ⬜ Not started |
| Phase 4 | Launching | ⬜ Not started |
| Phase 5 | UI Polish | ⬜ Not started |
| Phase 6 | v0.1 Stabilization | ⬜ Not started |
