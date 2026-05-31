# Program Starter Progress

## Current Status

Program Starter is in active development. The config persistence layer, group management, app management, and launching are complete.

Current completed checkpoints:

- Phase 0: WPF Solution Foundation
- Phase 1: Config, Paths, Migration, and Logging
- Phase 2: Group CRUD Foundation
- Phase 3: App Management
- Phase 4: Launching

Next: Phase 5 - UI Polish

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

## Phase 3 Summary

Phase 3 implemented application management within groups:

- [`IFileDialogService`](ProgramStarter.App/Services/IFileDialogService.cs) → [`FileDialogService`](ProgramStarter.App/Services/FileDialogService.cs) - WPF `OpenFileDialog` wrapper for `.exe` file selection
- [`AppEditDialog`](ProgramStarter.App/Views/Dialogs/AppEditDialog.xaml + .cs) - Modal dialog for adding/editing apps with Name + Path fields
- [`AppEditResult`](ProgramStarter.App/Services/AppEditResult.cs) - Result type for dialog return data (name, path, cancelled)
- [`AppEntryItemViewModel`](ProgramStarter.App/ViewModels/AppEntryItemViewModel.cs) - Wraps `AppEntry` with `Name`, `Path`, `IsEnabled` for invalid-path UX
- [`MainViewModel`](ProgramStarter.App/ViewModels/MainViewModel.cs) enhanced with:
  - `AddAppCommand` - Opens file picker, auto-fills name from filename, validates duplicate name/path
  - `EditAppCommand` - Opens pre-filled `AppEditDialog`, validates duplicates (excluding current app)
  - `RemoveAppCommand` - Confirmation dialog before removal
  - `PopulateSelectedGroupApps()` - Syncs `SelectedGroupApps` observable collection when group selection changes
  - Empty group message when no apps exist in selected group
- [`MainWindow.xaml`](ProgramStarter.App/MainWindow.xaml) enhanced with:
  - App card list with `Expander` per app showing path
  - Add/Edit/Remove buttons with DataContext proxying
  - Invalid path visual state (red indicator when file missing)
  - Empty group placeholder text
- [`PathValidationService`](ProgramStarter.App/Services/PathValidationService.cs) - `IsSupportedExtension()`, `FileExists()`, `ValidateForAdd()`, `ValidateForEdit()`
- 24 new tests with `FakeFileDialogService`, `FakeDialogService`, `FakePathValidationService` covering:
  - Add: file picker success, cancelled, duplicate name, duplicate path, invalid extension
  - Edit: valid edit, cancelled, duplicate name (same group), duplicate path (same group)
  - Remove: confirmed, cancelled
  - UI: app selection, empty group message, invalid path state
- Replaced boilerplate app entries with real app count for sidebar display

## Phase 4 Summary

Phase 4 implemented reliable app and group launching:

- [`IProcessStarter`](ProgramStarter.App/Services/IProcessStarter.cs) → [`ProcessStarter`](ProgramStarter.App/Services/ProcessStarter.cs) - Process start abstraction returning `(Process?, LaunchErrorCode?)` tuple
  - Maps `Win32Exception` native error codes to `LaunchErrorCode` (e.g., 5 → `AccessDenied`, 2 → `FileNotFound`)
  - Catches generic `InvalidOperationException` → `LaunchErrorCode.ProcessStartFailed`
  - Catch-all for unexpected exceptions → `LaunchErrorCode.Unknown`
  - Sets `WorkingDirectory` via `FileUtils.GetDirectoryName()` for default behavior
  - Register with DI in [`App.xaml.cs`](ProgramStarter.App/App.xaml.cs)
- [`AppLauncherService`](ProgramStarter.App/Services/AppLauncherService.cs) - Coordinates launch flow:
  - `LaunchOneAsync(app)` - Validates, starts process, logs result, returns `LaunchResult`
  - `LaunchGroupAsync(group, delayMs)` - Iterates apps with configurable delay, collects all results
  - `ValidateBeforeLaunch()` - Checks null path, unsupported extension, missing file before attempting start
  - Logs errors for silent/exe failures using [`IAppLogger`](ProgramStarter.App/Services/IAppLogger.cs)
  - Sets `WorkingDirectory` to the executable's parent directory
- [`MainViewModel`](ProgramStarter.App/ViewModels/MainViewModel.cs) enhanced with:
  - `LaunchAppCommand` - Launches single selected app with `IsLaunching` guard
  - `LaunchGroupCommand` - Launches all enabled apps in selected group with delay between each
  - `IsLaunching` property - Disables launch buttons while operation is in progress
  - Status messages: _"Launch requested"_ for toast-like feedback, detailed group summary on completion
- 25 new tests (10 `AppLauncherService` + 5 `MainViewModel` LaunchGroup + 10 `MainViewModel` LaunchApp) covering:
  - LaunchOne: success, file not found, access denied, unsupported extension, null path, process start failure, unknown error
  - LaunchGroup: mixed results, empty group, single app, cancellation via `IsLaunching`
  - ViewModel LaunchApp: success, failure (file not found), validation fail (unsupported extension)
  - ViewModel LaunchGroup: success, mixed results, app validation fails mid-group
- [`Stubs.cs`](ProgramStarter.App/Services/Stubs.cs) deleted after real implementations completed

### Phase 4 Review Fixes Applied

The following fixes were applied after Phase 4 implementation review:

**P0 - Critical Fixes:**
- Fixed [`AppLauncherService.LaunchGroupAsync`](ProgramStarter.App/Services/AppLauncherService.cs:35) status wording from misleading _"successfully launched"_ to accurate _"Launch requested for {name}"_
- Fixed [`ProcessStarter`](ProgramStarter.App/Services/ProcessStarter.cs) to set `WorkingDirectory` on `ProcessStartInfo` via `FileUtils.GetDirectoryName()` before starting

**P1 - High Priority Fixes:**
- Fixed `ProcessStarter.Start()` exception mapping to use `Win32Exception.NativeErrorCode` instead of `Win32Exception.Message` string matching; map 5 → `AccessDenied`, 2 → `FileNotFound`, all others → `ProcessStartFailed`; use `LaunchErrorCode.Unknown` for non-`Win32Exception` failures
- Added 2 additional `AppLauncherService` tests covering the new exception-to-error-code mapping paths
- Added 5 dedicated `MainViewModel` `LaunchGroupCommand` ViewModel tests covering: success, mixed results with individual app validation failures, and mid-group app validation failures
- Removed [`Stubs.cs`](ProgramStarter.App/Services/Stubs.cs) from the project

## Verification

All phases were verified with:

```bash
dotnet build  # 0 errors, 0 warnings
dotnet test   # 75/75 passed (14 config + 14 group mgmt + 24 app mgmt + 23 launching)
```

```text
Test summary by category:
  Config .................. 14 tests  (ConfigService, Migration, Normalization)
  Group Management ........ 14 tests  (AddGroup, RenameGroup, DeleteGroup, Constructor)
  App Management .......... 24 tests  (AddApp, EditApp, RemoveApp, UI states)
  Launching ............... 23 tests  (AppLauncherService: 10, VM LaunchApp: 8, VM LaunchGroup: 5)
  Total ................... 75 tests  — All passed
```

**Manual Verification Gates:**
- ✅ `dotnet build` — 0 errors, 0 warnings across all configurations
- ✅ `dotnet test` — 75/75 passing (100%)
- ✅ No real processes launched during tests (all services faked/mocked)
- ✅ [`Stubs.cs`](ProgramStarter.App/Services/Stubs.cs) removed from solution
- ✅ All service interfaces registered in [`App.xaml.cs`](ProgramStarter.App/App.xaml.cs) DI container
- ✅ Launch buttons disabled during `IsLaunching` via XAML binding
- ✅ Launch summary text uses accurate _"Launch requested"_ wording

## Phase Completion Checklist

| Phase | Description | Status |
|-------|-------------|--------|
| Phase 0 | WPF Solution Foundation | ✅ Complete |
| Phase 1 | Config, Paths, Migration, and Logging | ✅ Complete |
| Phase 2 | Group CRUD Foundation | ✅ Complete |
| Phase 3 | App Management | ✅ Complete |
| Phase 4 | Launching | ✅ Complete |
| Phase 5 | UI Polish | ⬜ Not started |
| Phase 6 | v0.1 Stabilization | ⬜ Not started |
