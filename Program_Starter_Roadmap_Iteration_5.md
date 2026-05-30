# Program Starter - Architecture & Roadmap

**Document Version:** Iteration 5  
**Target Version:** v0.1 Foundation Release  
**Platform:** Windows  
**Primary Goal:** Build a reliable, maintainable, visually clean Windows desktop launcher for starting predefined application groups with one click.  
**Review Status:** Final senior-hardened roadmap with UI polish rules, suitable for AI-agent implementation with strict phase control.

---

## 1. Executive Summary

Program Starter is a Windows-only desktop productivity launcher.

Its first version should allow the user to create launch groups, add applications to those groups, and start all applications in a selected group with one click.

This project must be treated as a small but serious Windows desktop tool, not a temporary script. v0.1 should prioritize a strong foundation, clear architecture, stable persistence, predictable launch behavior, and a clean UI.

The foundation technology should not be changed casually later. The initial architecture must therefore be conservative, Windows-native, testable, and maintainable.

v0.1 is not a process manager, installer, shortcut resolver, tray utility, automation suite, or theme editor. It is a clean launcher foundation.

---

## 2. Final Technology Decision

### Selected Stack

```text
Language: C#
UI Framework: WPF
Runtime: .NET 10 LTS preferred
Architecture Pattern: MVVM
Dependency Injection: Microsoft.Extensions.DependencyInjection
Config Format: JSON
Config Location: %LocalAppData%/ProgramStarter/config.json
Log Location: %LocalAppData%/ProgramStarter/logs/app.log
Target Platform: Windows
Publish Target: win-x64
Publish Mode: Self-contained single-file executable
```

### Runtime Decision

Preferred runtime:

```text
.NET 10 LTS
```

Reason:

```text
The project is starting from a clean foundation. .NET 10 LTS gives a longer support window and avoids building a fresh app on a runtime closer to its support deadline.
```

Fallback option:

```text
.NET 8 LTS is acceptable only if tooling, package compatibility, or local environment constraints make .NET 10 inconvenient.
```

If .NET 8 is used, add this explicit roadmap item:

```text
Before v1.0, review and migrate to .NET 10 LTS if practical.
```

### Why WPF?

WPF with C# is selected because the application is deeply Windows-focused and needs reliable integration with:

- Launching `.exe` applications
- Handling local file paths
- File picker dialogs
- Process start behavior
- Local config persistence
- Future system tray support
- Future Windows startup integration
- Future app icon extraction
- Future shortcut support
- Future run-as-admin support

WPF is mature, stable, well-documented, and suitable for long-term Windows desktop tooling.

### Rejected Technologies

#### Python

Rejected as the project foundation because it is better for quick prototypes than long-term Windows desktop tooling. Packaging, native Windows integration, visual polish, and maintainability would become weaker over time.

#### WinUI 3

Not selected for the foundation version because it introduces more platform/package complexity. It is visually modern but less risk-free than WPF for this small Windows-focused productivity tool.

#### Electron

Rejected because it is too heavy for this use case.

#### Tauri

Rejected for the first version because the project does not need a web UI stack or Rust-based shell. It would add unnecessary setup complexity.

---

## 3. Product Definition

Program Starter lets the user organize applications into named launch groups.

Example groups:

```text
Work
Game Dev
Editing
Gaming
```

Each group contains applications. The user can start one app or start the entire group.

Example:

```text
Work
- Slack
- Unity Hub
- GitHub Desktop
- Chrome
```

The main value is reducing repetitive manual startup clicks.

---

## 4. Core Design Principles

### 4.1 Reliability First

The app must not crash because of:

- Missing config
- Corrupted config
- Unsupported config version
- Missing app path
- App launch failure
- Invalid user input
- Empty groups
- Duplicate clicks
- Access denied errors
- File permission errors

### 4.2 Scope Discipline

v0.1 must not chase advanced features. The first version exists to establish a strong foundation.

### 4.3 Windows-Native Behavior

The app should follow Windows expectations:

- Use LocalAppData for machine-specific config.
- Use native file dialogs.
- Use `ProcessStartInfo` for launching.
- Do not attempt to manage launched processes after starting them.
- Do not require administrator privileges for normal use.

### 4.4 Maintainable MVVM

The app must separate UI, state, services, and models clearly.

### 4.5 Agent-Proof Implementation

The specification must prevent AI coding agents from:

- Overbuilding
- Adding unrelated features
- Creating duplicate systems
- Moving business logic into code-behind
- Saving config on every property setter
- Breaking serialized config structure
- Hiding errors
- Rewriting unrelated files
- Adding placeholder abstractions that are never used

---

## 5. v0.1 Scope

v0.1 is the foundation release.

The goal is:

```text
Create groups.
Add .exe apps.
Save them safely.
Launch one app.
Launch a group.
Handle errors.
Look clean and intentionally designed.
Stay maintainable.
```

### 5.1 Must-Have Features

#### Group Management

- Create group
- Rename group
- Delete group with confirmation
- Select active group
- Prevent duplicate group names, case-insensitive
- Trim and normalize group names
- Save selected group
- Show empty state if no group exists

#### Application Management

- Add application to selected group
- Select `.exe` file using file picker
- Auto-fill app name from file name
- Edit app name
- Edit app path
- Remove app with confirmation
- Validate app path
- Show missing/invalid path state
- Prevent adding unsupported file types
- Prevent duplicate app names inside the same group, case-insensitive
- Prevent duplicate app paths inside the same group in v0.1
- Preserve existing app entries if their paths become missing later

#### Launching

- Launch a single app
- Launch all enabled apps in selected group
- Launch apps in saved order
- Apply default delay between apps
- Prevent duplicate launch execution while a group launch is already running
- Show success/failure summary
- Never crash on launch failure
- Use wording that does not overpromise process success

#### Persistence

- Store config as JSON
- Use camelCase JSON
- Load config on app start
- Do not auto-create a fake `Work` group on first launch
- Show empty first-run state when there are no groups
- Save after committed user actions only
- Store config in `%LocalAppData%/ProgramStarter/config.json`
- Use atomic save behavior
- Backup corrupted config before replacing it
- Do not overwrite unsupported future schema versions
- Normalize loaded data safely

#### UI

- Modern dark WPF interface
- Left sidebar for groups
- Main panel for selected group
- Application cards
- Primary `Start Group` button
- Secondary `Add App` button
- Status area for feedback
- Empty states
- Clear error messages
- Basic dark theme resource dictionaries from Phase 0

#### Logging

- Write technical errors to local log file
- Show clean user-facing error messages
- Store logs under `%LocalAppData%/ProgramStarter/logs/`
- Do not expose raw exception dumps in UI
- Rotate logs with a simple size/retention rule

---

## 6. Explicitly Out of Scope for v0.1

These features must not be implemented in v0.1.

```text
.lnk shortcut support
System tray
Auto-start with Windows
Drag and drop ordering
App icon extraction
Run as administrator
Per-app launch arguments UI
Working directory UI
Chrome URL group management
Folder/file launch entries
Installer
Auto-update
Cloud sync
Plugin system
Multi-user profiles
Advanced animations
Process monitoring after launch
Killing/closing launched apps
Do not start if already running
Theme customization UI
Localization system
```

### Important Shortcut Decision

`.lnk` shortcut support is intentionally postponed to v0.2.

Reason:

`.lnk` files can point to many target types:

- `.exe`
- Folder
- Document
- URL
- Microsoft Store app
- Network path
- Missing target
- Target with arguments
- Target requiring admin permission

Supporting `.lnk` properly requires clear shortcut resolution rules. v0.1 should stay stable and predictable by supporting `.exe` only.

---

## 7. High-Level Roadmap

### v0.1 - Foundation Release

Focus:

- WPF app shell
- MVVM structure
- Groups
- `.exe` app entries
- JSON persistence
- App launching
- Error handling
- Minimal logging
- Dark UI foundation

### v0.2 - Shortcut and Quality-of-Life Release

Possible features:

- `.lnk` shortcut support
- Shortcut target resolution
- Drag and drop app ordering
- Per-app delay
- Launch arguments
- Working directory UI
- Better edit dialog

### v0.3 - Windows Integration Release

Possible features:

- Start with Windows
- Minimize to tray
- Close to tray
- Launch selected group on app startup
- Create desktop shortcut for group

### v0.4 - Visual Upgrade Release

Possible features:

- Extract real app icons
- Group icons
- Accent color setting
- Theme customization
- Compact/comfortable layout mode
- Simple transitions

### v0.5 - Advanced Launch Behavior

Possible features:

- Run as administrator
- Do not start if already running
- URL entries
- Folder entries
- Conditional launch rules
- Launch profiles

### v1.0 - Productized Release

Possible features:

- Installer
- App icon
- About page
- Export/import config
- Backup config
- First-run onboarding
- Stable release build

---

## 8. Recommended Solution Structure

The structure should stay clean but not over-fragmented. v0.1 should avoid creating unnecessary views and viewmodels too early.

```text
ProgramStarter/
│
├── ProgramStarter.App/
│   │
│   ├── App.xaml
│   ├── App.xaml.cs
│   ├── MainWindow.xaml
│   ├── MainWindow.xaml.cs
│   │
│   ├── Models/
│   │   ├── AppEntry.cs
│   │   ├── AppGroup.cs
│   │   ├── AppSettings.cs
│   │   ├── LaunchResult.cs
│   │   └── LaunchErrorCode.cs
│   │
│   ├── ViewModels/
│   │   ├── BaseViewModel.cs
│   │   ├── MainViewModel.cs
│   │   ├── GroupItemViewModel.cs
│   │   └── AppEntryItemViewModel.cs
│   │
│   ├── Views/
│   │   └── Dialogs/
│   │       ├── TextInputDialog.xaml
│   │       ├── ConfirmDialog.xaml
│   │       └── AppEditDialog.xaml
│   │
│   ├── Services/
│   │   ├── IConfigService.cs
│   │   ├── JsonConfigService.cs
│   │   ├── IConfigMigrationService.cs
│   │   ├── ConfigMigrationService.cs
│   │   ├── IAppLauncherService.cs
│   │   ├── AppLauncherService.cs
│   │   ├── IProcessStarter.cs
│   │   ├── ProcessStarter.cs
│   │   ├── IFileDialogService.cs
│   │   ├── FileDialogService.cs
│   │   ├── IPathValidationService.cs
│   │   ├── PathValidationService.cs
│   │   ├── IAppLogger.cs
│   │   └── FileAppLogger.cs
│   │
│   ├── Commands/
│   │   ├── RelayCommand.cs
│   │   └── AsyncRelayCommand.cs
│   │
│   ├── Themes/
│   │   ├── Colors.xaml
│   │   ├── Typography.xaml
│   │   ├── Buttons.xaml
│   │   ├── Cards.xaml
│   │   └── Layout.xaml
│   │
│   ├── Assets/
│   │   └── Icons/
│   │
│   └── Helpers/
│       ├── Constants.cs
│       ├── AppPaths.cs
│       └── JsonOptions.cs
│
└── ProgramStarter.Tests/
    │
    ├── Config/
    │   └── JsonConfigServiceTests.cs
    │
    ├── Validation/
    │   └── PathValidationServiceTests.cs
    │
    └── Launching/
        └── AppLauncherServiceTests.cs
```

### Structure Guardrail

Do not create separate views for every small region unless needed.

Allowed in v0.1:

```text
MainWindow with well-organized XAML sections
Small reusable dialogs
Shared styles in resource dictionaries
```

Avoid in v0.1 unless the UI becomes too large:

```text
GroupSidebarView
AppListView
AppCardView
DialogViewModel hierarchy
Large custom control system
```

Reason:

```text
Too much early view fragmentation makes simple CRUD logic harder to review and easier for AI agents to scatter incorrectly.
```

---

## 9. Project Structure Rules

### 9.1 Code-Behind Rules

Code-behind must stay minimal.

Allowed in code-behind:

- View initialization
- Pure UI behavior that cannot cleanly be expressed in XAML
- Window drag behavior
- Dialog close behavior if needed

Forbidden in code-behind:

- Config read/write
- Process launching
- App/group mutation logic
- Validation logic
- Business rules

### 9.2 ViewModel Rules

ViewModels may:

- Own UI state
- Expose observable collections
- Expose commands
- Call services through interfaces
- Format UI-friendly status messages

ViewModels must not:

- Directly instantiate services
- Directly open file dialogs without service abstraction
- Directly call `Process.Start`
- Directly read/write JSON files
- Know concrete storage paths
- Save config from property setters

### 9.3 Service Rules

Services should own system-level operations.

Examples:

- Config persistence
- Config migration
- Process launching
- Process start abstraction
- File dialog interaction
- Path validation
- Logging

### 9.4 Model Rules

Models should be:

- Simple
- Serializable
- Free of UI dependencies
- Stable across versions

Models should not contain:

- WPF references
- Commands
- View state
- Service references

---

## 10. Dependency Injection

The app must use constructor injection.

Recommended package:

```text
Microsoft.Extensions.DependencyInjection
```

### Required DI Rules

- Register services in `App.xaml.cs` or a dedicated bootstrapper.
- ViewModels receive services through constructors.
- Do not instantiate services manually inside ViewModels.
- Do not use global static service access unless there is a strong reason.
- Keep dependencies explicit.

### Example Registration

```csharp
services.AddSingleton<IConfigService, JsonConfigService>();
services.AddSingleton<IConfigMigrationService, ConfigMigrationService>();
services.AddSingleton<IAppLauncherService, AppLauncherService>();
services.AddSingleton<IProcessStarter, ProcessStarter>();
services.AddSingleton<IFileDialogService, FileDialogService>();
services.AddSingleton<IPathValidationService, PathValidationService>();
services.AddSingleton<IAppLogger, FileAppLogger>();

services.AddSingleton<MainViewModel>();
services.AddSingleton<MainWindow>();
```

---

## 11. Core Data Models

### 11.1 AppSettings

```csharp
public class AppSettings
{
    public int SchemaVersion { get; set; } = 1;
    public List<AppGroup> Groups { get; set; } = new();
    public string? LastSelectedGroupId { get; set; }
    public string Theme { get; set; } = "Dark";
    public int DefaultDelayMilliseconds { get; set; } = 1000;
}
```

### 11.2 AppGroup

```csharp
public class AppGroup
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "New Group";
    public List<AppEntry> Apps { get; set; } = new();
}
```

### 11.3 AppEntry

```csharp
public class AppEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
}
```

### v0.1 Model Decision

Do not add future fields yet unless actively used.

Not included in v0.1 model:

```text
Arguments
WorkingDirectory
RunAsAdmin
Per-app DelayMilliseconds
IconPath
TargetType
```

Reason:

Unused serialized fields create confusion for AI agents and increase accidental feature creep.

These fields may be added in future versions with schema migration.

### 11.4 LaunchErrorCode

```csharp
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
```

### 11.5 LaunchResult

```csharp
public class LaunchResult
{
    public bool Success { get; set; }
    public string AppName { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public LaunchErrorCode ErrorCode { get; set; } = LaunchErrorCode.None;
    public string UserMessage { get; set; } = string.Empty;
    public string? TechnicalMessage { get; set; }
}
```

---

## 12. JSON and Config Storage

### 12.1 Config Location

The config must be stored in:

```text
%LocalAppData%/ProgramStarter/config.json
```

Use:

```csharp
Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
```

Reason:

Application entries contain machine-specific paths. They should not roam across Windows profiles or machines.

### 12.2 Log Location

Logs must be stored in:

```text
%LocalAppData%/ProgramStarter/logs/app.log
```

### 12.3 JSON Format Rules

The app must use camelCase JSON.

Required serializer options:

```csharp
new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    WriteIndented = true,
    ReadCommentHandling = JsonCommentHandling.Skip,
    AllowTrailingCommas = true
};
```

Recommended helper:

```text
Helpers/JsonOptions.cs
```

Reason:

The serialized config should stay predictable and human-readable.

### 12.4 Example Config

```json
{
  "schemaVersion": 1,
  "groups": [
    {
      "id": "5c57c4bd-c248-46cf-a295-7ce41992d914",
      "name": "Work",
      "apps": [
        {
          "id": "9de2de3d-bad5-4707-86e9-131c163f0f7e",
          "name": "Slack",
          "path": "C:/Program Files/Slack/slack.exe",
          "isEnabled": true
        }
      ]
    }
  ],
  "lastSelectedGroupId": "5c57c4bd-c248-46cf-a295-7ce41992d914",
  "theme": "Dark",
  "defaultDelayMilliseconds": 1000
}
```

---

## 13. Config Service Requirements

### 13.1 Responsibilities

`JsonConfigService` is responsible for:

- Creating app data directory
- Creating logs directory if needed
- Loading config
- Creating default config if missing
- Saving config
- Atomic save behavior
- Backing up corrupted config
- Detecting unsupported schema versions
- Normalizing loaded settings
- Preserving valid existing user data

### 13.2 Missing Config Behavior

If config does not exist:

- Create default settings with an empty group list
- Save config
- Show first-run empty state in UI

Default settings:

```text
SchemaVersion: 1
Groups: []
LastSelectedGroupId: null
Theme: Dark
DefaultDelayMilliseconds: 1000
```

Do not auto-create a `Work` group.

Reason:

The app should let the user define the first group instead of making assumptions.

### 13.3 Corrupted Config Behavior

If config is corrupted:

- Do not crash
- Rename/copy corrupted config to backup
- Create new default config
- Log technical error
- Show user-friendly message

Suggested backup format:

```text
config.corrupted_yyyyMMdd_HHmmss.json
```

### 13.4 Unsupported Future Schema Behavior

If config has a `schemaVersion` greater than the app supports:

- Do not overwrite the config
- Do not attempt unsafe normalization
- Log the issue
- Show a user-friendly message
- Load temporary empty in-memory settings if needed
- Ask the user to update the app or manually back up the config

User-facing message example:

```text
This config was created by a newer version of Program Starter. Your data was not changed.
```

This prevents old builds from destroying newer config files.

### 13.5 Config Migration Service

Include a minimal migration service even if v0.1 only supports schema version 1.

```csharp
public interface IConfigMigrationService
{
    AppSettings Migrate(JsonDocument rawConfig);
}
```

v0.1 behavior:

```text
If schemaVersion is missing, treat as version 1.
If schemaVersion is 1, deserialize and normalize.
If schemaVersion is greater than supported, reject safely and do not save over it.
```

Reason:

v0.2 may add arguments, working directory, delay, shortcuts, target types, or icon metadata. The migration seam should exist before those changes arrive.

### 13.6 Atomic Save Behavior

The app must avoid destroying existing config during save.

Recommended behavior:

```text
1. Serialize settings to JSON in memory.
2. Write JSON to config.tmp.
3. If config.json exists:
       Use File.Replace(config.tmp, config.json, config.bak)
   Else:
       Use File.Move(config.tmp, config.json)
4. Clean up stale temp file if safe.
5. Log save failures.
```

If `File.Replace` is unavailable or fails, fall back carefully without deleting the existing config first.

### 13.7 Auto-Save Rules

Do not save on every property setter.

Do not save on every text change.

Save only after committed user actions:

- Group created
- Group renamed after confirmation
- Group deleted after confirmation
- Selected group changed
- App added after confirmation
- App edited after confirmation
- App removed after confirmation
- App enabled/disabled toggled

Reason:

Saving on every keystroke creates unnecessary disk I/O and increases the chance of broken state being written.

### 13.8 Normalization Rules

When loading config:

- If `Groups` is null, replace with empty list.
- If any group has null `Apps`, replace with empty list.
- If any group has empty `Id`, generate one.
- If any app has empty `Id`, generate one.
- Trim group names.
- Trim app names.
- If group name is empty after trim, rename to `New Group`.
- If app name is empty after trim, infer from file name if possible.
- If `DefaultDelayMilliseconds` is negative, reset to 1000.
- If `Theme` is empty, reset to `Dark`.
- Do not remove app entries just because their paths are missing.
- If `LastSelectedGroupId` no longer exists, set it to null.
- Do not silently merge duplicate groups during normal load unless clearly logged.
- Preserve unknown JSON fields only if the migration system supports it. Otherwise, keep schema minimal.

---

## 14. Name and Duplicate Rules

### 14.1 Name Normalization

Group and app names must be normalized before validation.

Rules:

```text
Trim leading/trailing whitespace.
Reject empty names after trim.
Compare duplicates case-insensitively.
Do not allow names that differ only by spacing or casing.
```

Use:

```csharp
StringComparer.OrdinalIgnoreCase
```

Examples treated as duplicates:

```text
Work
 work 
WORK
```

### 14.2 Duplicate Group Names

Duplicate group names are not allowed.

### 14.3 Duplicate App Names

Duplicate app names inside the same group are not allowed.

The same app name may exist in different groups.

### 14.4 Duplicate App Paths

In v0.1, duplicate app paths inside the same group are not allowed.

Reason:

v0.1 does not support launch arguments or per-app configuration, so duplicate paths are more likely user error than intentional setup.

Future exception:

```text
v0.2+ may allow same executable with different arguments or working directories.
```

---

## 15. Path Validation

### 15.1 Supported File Types in v0.1

Only `.exe` files are supported in v0.1.

Allowed:

```text
.exe
```

Rejected:

```text
.lnk
.url
.bat
.cmd
.ps1
.folder
any other extension
```

### 15.2 Add App Validation

When adding a new app:

- Path must not be empty.
- File must exist.
- Extension must be `.exe`.
- App name must not be empty after trim.
- Duplicate app names inside the same group must be prevented, case-insensitive.
- Duplicate app paths inside the same group must be prevented, case-insensitive.

### 15.3 Existing App Validation

If an existing app path becomes missing later:

- Do not delete it.
- Show it as invalid.
- Disable launch for that app until fixed.
- Allow user to edit the path.

---

## 16. Launching Rules

### 16.1 v0.1 Launch Behavior

v0.1 only asks Windows to start the selected `.exe`.

It does not:

- Track the launched process
- Kill processes
- Restart processes
- Detect whether the app is already open
- Manage child processes
- Manage app windows

### 16.2 ProcessStartInfo Rules

Use `ProcessStartInfo`.

Recommended:

```csharp
var startInfo = new ProcessStartInfo
{
    FileName = app.Path,
    WorkingDirectory = Path.GetDirectoryName(app.Path) ?? string.Empty,
    UseShellExecute = true
};
```

Working directory UI is out of scope for v0.1. However, setting the working directory to the executable directory by default is acceptable when safe.

### 16.3 Process Start Abstraction

Do not call `Process.Start` directly from ViewModels.

Use:

```csharp
public interface IProcessStarter
{
    Process? Start(ProcessStartInfo startInfo);
}
```

`AppLauncherService` depends on `IProcessStarter`.

Reason:

This makes launch behavior testable without starting real apps during tests.

### 16.4 Launch One App

When launching one app:

- Validate path
- If invalid, return failed `LaunchResult`
- Attempt start through `IProcessStarter`
- Return success/failure
- Log technical failure

### 16.5 Launch Group

When launching a group:

- If `IsLaunching` is true, ignore or reject command.
- Set `IsLaunching = true`.
- Disable launch buttons while group launch is active.
- Launch enabled apps only.
- Launch in saved order.
- Wait `DefaultDelayMilliseconds` between successful/attempted launches.
- Collect all `LaunchResult` objects.
- Set `IsLaunching = false` in a finally block.
- Show summary.

### 16.6 Duplicate Launch Prevention

The app must prevent repeated launch spam.

UI behavior:

```text
Start Group button disabled while launching.
Single app Start buttons disabled while group launch is active.
Status shows: Launching apps...
```

### 16.7 Launch Summary Wording

Do not overpromise that apps are fully running.

Avoid:

```text
Started 4 apps successfully.
```

Prefer:

```text
Launch requested for 4 apps.
```

Partial failure:

```text
Launch requested for 3 apps. 1 failed.
Slack: file path does not exist.
```

Full failure:

```text
Could not launch any apps. Please check the app paths.
```

Reason:

`Process.Start` can succeed even if the launched application later fails or exits immediately.

---

## 17. Logging Requirements

### 17.1 Logger Interface

```csharp
public interface IAppLogger
{
    void Info(string message);
    void Warning(string message);
    void Error(string message, Exception? exception = null);
}
```

### 17.2 Rules

- User-facing messages must stay clean.
- Technical messages must be logged.
- Logs should be plain text.
- v0.1 does not require advanced logging frameworks.
- Avoid logging sensitive user data unnecessarily.
- Log path-related errors enough to debug them.
- Do not show raw stack traces in the UI.

### 17.3 Simple Log Format

```text
[2026-05-31 22:14:33] [ERROR] Failed to launch Slack
System.ComponentModel.Win32Exception: ...
```

### 17.4 Log Rotation

Implement simple log rotation.

Recommended rule:

```text
If app.log exceeds 1 MB, rename it to app_yyyyMMdd_HHmmss.log and start a new app.log.
Keep the latest 5 rotated log files.
Delete older rotated logs.
```

Reason:

A plain log file should not grow forever.

---

## 18. UI Layout and Visual Quality Rules

### 18.1 Main Window Layout

The application should use a simple two-column layout.

Recommended structure:

```text
┌────────────────────────────────────────────────────────────┐
│ Program Starter                                            │
├────────────────┬───────────────────────────────────────────┤
│ Groups         │ Work                                      │
│                │ 4 apps                                    │
│ Work           │                                           │
│ Game Dev       │ [ Start Group ]   [ + Add App ]           │
│ Editing        │                                           │
│ Gaming         │ ┌───────────────────────────────────────┐ │
│                │ │ Slack                                 │ │
│ + Add Group    │ │ C:/Program Files/Slack/slack.exe       │ │
│                │ │ [Start] [Edit] [Remove]               │ │
│                │ └───────────────────────────────────────┘ │
│                │                                           │
│                │ Status: Launch requested for 4 apps.      │
└────────────────┴───────────────────────────────────────────┘
```

Do not add a Settings button, gear button, titlebar customization, theme editor, or navigation pages in v0.1. Settings-style UI is out of scope.

### 18.2 Layout Dimensions

Use consistent spacing instead of default WPF positioning.

Recommended values:

```text
Window minimum size:        900x560
Sidebar width:              220
Main content padding:       28
Section spacing:            20
Card padding:               16
Card vertical gap:          12
Button height:              36-40
Small button height:        30-34
Border radius illusion:     Use simple rounded corners where practical
```

The UI should feel spacious, not cramped. The app is a launcher, so the primary actions must be readable at a glance.

### 18.3 Visual Direction

The interface should be dark, modern, simple, and practical.

Recommended palette:

```text
Background:        #101218
Sidebar:           #151821
Surface:           #181B23
Card:              #222631
Card Hover:        #2B3040
Selected Surface:  #2A2F3D
Accent:            #7C5CFF
Accent Hover:      #8E74FF
Accent Pressed:    #6849E8
Text Primary:      #F2F4F8
Text Secondary:    #9AA3B2
Text Muted:        #687083
Danger:            #FF5C5C
Danger Hover:      #FF7474
Success:           #6EE7B7
Warning:           #F6C85F
Border:            #303544
Invalid Border:    #A94A4A
```

The accent color should be used sparingly. It is for the main action, selected state hints, and focus states. Do not paint the entire UI purple.

### 18.4 Typography

Use a clean Windows-friendly font stack.

Recommended:

```text
Font family: Segoe UI
Main title: 22-24px, SemiBold
Section title: 16-18px, SemiBold
Card title: 15-16px, SemiBold
Body text: 13-14px, Regular
Path text: 12-13px, Regular, muted
Button text: 13-14px, SemiBold
```

Path text should use trimming/ellipsis if it becomes too long. Do not let long paths break the card layout.

### 18.5 Theme Foundation

Phase 0 must create the resource dictionary foundation.

Required:

```text
Colors.xaml
Typography.xaml
Buttons.xaml
Cards.xaml
Layout.xaml
```

Rules:

```text
Do not scatter colors directly across controls.
Do not leave default WPF buttons as final.
Do not introduce third-party UI frameworks in v0.1.
Do not create a full design system beyond what the app actually uses.
```

### 18.6 Button Hierarchy

Buttons must have clear roles.

Required styles:

```text
Primary Button:    Start Group
Secondary Button:  Add App, Add Group
Neutral Button:    Edit
Danger Button:     Remove/Delete confirmation actions
Ghost Button:      Small non-primary actions if needed
```

Rules:

```text
Only one visually dominant primary button should exist in the main content area.
Danger actions must not look like normal actions.
Disabled buttons must look disabled, not broken.
Hover and pressed states should be visible but subtle.
```

### 18.7 Sidebar Behavior

The sidebar should be functional and readable.

Rules:

```text
Show Groups title at the top.
Show group list below it.
Selected group must have a clear selected state.
Group item hover state should be visible.
Add Group should stay at the bottom or clearly separated from the list.
Do not use checkboxes for group selection.
Do not show raw IDs.
Do not add group icons in v0.1 unless using a simple generic placeholder.
```

### 18.8 App Card Behavior

Each app should be displayed as a card.

Card content:

```text
App name
App path
Invalid/missing path label if needed
Enabled/disabled state if supported visually
Start, Edit, Remove actions
```

Rules:

```text
App name must be the strongest text inside the card.
Path must be secondary and ellipsized.
Invalid path state should be visible without being visually aggressive.
Remove should not be the most prominent action.
Start should be easy to find.
Cards should not look like spreadsheet rows.
```

### 18.9 Empty States

Empty states are part of the product, not placeholders.

#### No Groups

```text
No groups yet.
Create your first launch group.

[ + Add Group ]
```

#### Empty Group

```text
No apps in this group yet.
Add your first app to start building this launch group.

[ + Add App ]
```

Rules:

```text
Empty states should be centered or clearly placed in the main panel.
Use muted text for explanation.
Use one clear call-to-action.
Do not show debug-looking placeholder text.
```

### 18.10 Invalid Path State

If an app path is missing or invalid, the app should remain visible.

Recommended user-facing text:

```text
This app path no longer exists.
Edit the app and select a valid .exe file.
```

Visual rules:

```text
Show a warning or invalid label inside the card.
Disable Start for that app.
Keep Edit available.
Do not delete or hide the entry automatically.
```

### 18.11 Launching State

During group launch:

```text
Start Group button disabled.
Single app Start buttons disabled.
Status shows: Launching apps...
Cursor can remain normal; no full-screen loading overlay.
```

Do not add complex progress bars in v0.1. A simple status text is enough.

### 18.12 Dialog Visual Rules

Dialogs should be simple and consistent.

Required dialogs:

```text
TextInputDialog
ConfirmDialog
AppEditDialog
```

Rules:

```text
Dialog title should be clear.
Primary action should be on the right.
Cancel should be available.
Validation errors should appear inside the dialog, not as raw exception popups.
Delete confirmation should use a danger-colored confirmation button.
Do not create separate complex pages for these flows in v0.1.
```

### 18.13 UI Quality Bar

The UI is acceptable for v0.1 only if:

```text
[ ] It does not look like an untouched default WPF app.
[ ] The primary action is obvious within 2 seconds.
[ ] Empty states explain the next action.
[ ] Long paths do not destroy layout.
[ ] Missing paths are visible but not catastrophic-looking.
[ ] Hover, selected, disabled, and danger states are visually distinct.
[ ] Colors and spacing are centralized in resource dictionaries.
[ ] There is no theme customization UI in v0.1.
[ ] There are no advanced animations in v0.1.
```

### 18.14 UI Implementation Guardrail

The app should feel polished, but implementation must stay simple.

Allowed:

```text
Resource dictionaries
Simple ControlTemplates for buttons/cards if needed
Rounded card borders
Hover/selected/disabled visual states
Basic placeholder icon shape
```

Avoid:

```text
Custom window chrome
Animated page transitions
Third-party UI libraries
Complex icon extraction
Full settings screen
Theme editor
Over-fragmented custom controls
```

Reason:

```text
v0.1 needs a clean launcher, not a UI framework experiment.
```

## 19. UX Rules

### General

- Primary action should be obvious.
- Empty states should explain what to do next.
- Errors should be readable.
- Destructive actions should ask for confirmation.
- App should not feel like a raw debug tool.
- Avoid excessive modal dialogs.
- Use status messages for normal feedback.

### Delete Group

Deleting a group must show confirmation:

```text
Delete "Work" group?
This will remove all apps inside this group from Program Starter.
```

### Delete App

Deleting an app must show confirmation in v0.1:

```text
Remove "Slack" from this group?
This will not uninstall the application from your computer.
```

Reason:

v0.1 does not include Undo. Confirmation is safer than accidental deletion.

### Add App Flow

```text
1. User clicks + Add App.
2. File picker opens.
3. User selects .exe file.
4. App name auto-fills from file name.
5. User confirms or edits name.
6. App is added to selected group.
7. Config auto-saves after confirm.
```

### Edit App Flow

```text
1. User clicks Edit.
2. Dialog opens with current name and path.
3. User can change name or select a new .exe path.
4. Validation runs.
5. User confirms.
6. App updates.
7. Config auto-saves after confirm.
```

---

## 20. Non-Functional Requirements

### Reliability

```text
App must not crash on bad config.
App must not crash on launch failure.
App must not overwrite unsupported future config versions.
No unhandled exception should close the app during normal user actions.
```

### Performance

```text
App startup should feel instant for normal config sizes.
Config load should not introduce visible delay.
UI must remain responsive during group launch.
Group launch must use async command flow.
```

### Permissions

```text
App should work without administrator privileges.
App should not request elevation in v0.1.
```

### Connectivity

```text
App should not require internet.
No cloud sync or telemetry in v0.1.
```

### Build and Publish

```text
App should be publishable as win-x64 self-contained single-file executable.
Writable data must stay outside the application directory.
Config and logs must never be written next to the executable.
Embedded resources and XAML dictionaries must be validated in published build.
```

---

## 21. Testing Strategy

v0.1 should include a small test project for service-level tests.

UI automation tests are not required.

### 21.1 Required Test Areas

#### Config Tests

- Load missing config creates default empty config.
- Load corrupted config creates backup and default config.
- Unsupported future schema version is not overwritten.
- Missing schema version is treated as version 1.
- Save writes valid camelCase JSON.
- Atomic save does not destroy previous config if temp save fails.
- Null groups are normalized.
- Null app lists are normalized.
- Missing IDs are generated.
- LastSelectedGroupId is cleared if missing.
- DefaultDelayMilliseconds is normalized if invalid.

#### Validation Tests

- Empty path is invalid.
- Missing file is invalid.
- Unsupported extension is invalid.
- `.exe` file is valid.
- `.lnk` is rejected in v0.1.
- Duplicate group name is rejected.
- Duplicate app name inside same group is rejected.
- Duplicate app path inside same group is rejected.
- Names differing only by whitespace/casing are rejected as duplicates.

#### Launch Tests

- Invalid path returns failed result.
- Unsupported extension returns failed result.
- Launch exceptions are mapped to `LaunchResult`.
- Technical errors are logged.
- `IProcessStarter` is used instead of launching real apps.
- Group launch returns mixed success/failure results.
- `IsLaunching` guard prevents duplicate launch requests.

### 21.2 Testing Rule

Do not test real production apps like Slack, Unity Hub, Steam, Chrome, or Discord.

Use controlled test paths and fake/mocked process behavior.

---

## 22. Phase Plan

## Phase 0 - Foundation Setup and Theme Base

Goal: Create the project foundation.

Tasks:

- Create WPF project with .NET 10 LTS preferred.
- Create test project.
- Add solution structure.
- Add MVVM folders.
- Add base models.
- Add service interfaces.
- Add DI setup.
- Add `RelayCommand`.
- Add `AsyncRelayCommand`.
- Add base theme resource dictionaries.
- Create main window shell.
- Create first-run empty state placeholder.

Deliverable:

```text
App opens with empty shell UI.
DI works.
MainViewModel is resolved through DI.
Theme dictionaries exist.
No business logic exists in code-behind.
```

---

## Phase 1 - Config, Paths, Migration, and Logging

Goal: Create stable local persistence.

Tasks:

- Implement `AppPaths`.
- Use `%LocalAppData%/ProgramStarter/`.
- Implement `JsonOptions`.
- Implement `IAppLogger`.
- Implement `FileAppLogger`.
- Implement simple log rotation.
- Implement `IConfigMigrationService`.
- Implement `ConfigMigrationService`.
- Implement `JsonConfigService`.
- Create default empty config.
- Load existing config.
- Save config.
- Add atomic save behavior.
- Add corrupted config backup.
- Add unsupported future schema protection.
- Add config normalization.
- Add config unit tests.

Deliverable:

```text
App can safely load and save config.
Broken config does not crash app.
Unsupported future config is not overwritten.
Errors are logged.
```

---

## Phase 2 - Group Management

Goal: User can manage groups.

Tasks:

- Display groups in sidebar.
- Add group.
- Rename group.
- Delete group with confirmation.
- Select active group.
- Prevent duplicate group names.
- Trim and normalize group names.
- Save changes only after committed actions.
- Restore last selected group on startup.
- Add group tests where logic is service-level or ViewModel-level.

Deliverable:

```text
User can create, rename, delete, and select groups.
```

---

## Phase 3 - App Management

Goal: User can manage `.exe` app entries.

Tasks:

- Implement file picker for `.exe` only.
- Add app to selected group.
- Auto-fill app name from file name.
- Edit app name.
- Edit app path.
- Remove app with confirmation.
- Validate app path.
- Prevent unsupported file types.
- Prevent duplicate app names inside group.
- Prevent duplicate app paths inside group.
- Trim and normalize app names.
- Show invalid path state.
- Save changes only after committed actions.
- Add validation tests.

Deliverable:

```text
User can build a group of valid .exe apps.
Missing existing app paths are shown clearly.
```

---

## Phase 4 - Launching

Goal: User can launch apps and groups reliably.

Tasks:

- Implement `IProcessStarter`.
- Implement `ProcessStarter`.
- Implement `AppLauncherService`.
- Launch single app.
- Launch selected group.
- Set default working directory to executable directory when safe.
- Add default delay between apps.
- Add `IsLaunching` guard.
- Disable launch buttons while launching.
- Collect launch results.
- Show accurate launch request summary.
- Log failures.
- Add launch tests with fake process behavior.

Deliverable:

```text
User can start a full work setup with one click.
Launch failures do not crash the app.
Tests do not launch real apps.
```

---

## Phase 5 - UI Polish

Goal: Make the app feel clean, intentional, and usable without expanding scope.

Tasks:

- Apply dark theme from resource dictionaries.
- Style sidebar with clear selected and hover states.
- Style primary, secondary, neutral, danger, and disabled buttons.
- Style app cards with readable hierarchy.
- Add hover states.
- Add selected group state.
- Add empty states with clear calls-to-action.
- Add invalid path visual state.
- Add status area.
- Improve spacing using shared layout resources.
- Improve typography using shared typography resources.
- Add simple placeholder app icon only; do not extract real app icons in v0.1.
- Ensure long app paths are trimmed/ellipsized and do not break layout.
- Ensure delete/destructive actions are visually distinct.
- Validate published single-file build renders resources correctly.

Deliverable:

```text
App feels like a small polished launcher, not a default WPF window.
The UI is clean and intentional, but still simple enough to maintain.
```

---

## Phase 6 - v0.1 Stabilization

Goal: Prepare first stable release.

Tasks:

- Test missing config.
- Test corrupted config.
- Test unsupported future config.
- Test empty config.
- Test missing app path.
- Test unsupported file type.
- Test duplicate group/app names.
- Test duplicate app paths.
- Test launch failure.
- Test double-click launch prevention.
- Test save/load cycle.
- Test published single-file executable.
- Confirm config/logs are not written next to executable.
- Remove unused code.
- Review architecture.
- Prepare publish profile.
- Publish win-x64 self-contained single-file build.

Deliverable:

```text
Stable Program Starter v0.1 build.
```

---

## 23. Definition of Done for v0.1

v0.1 is complete only when all of the following are true:

```text
[ ] App uses C# WPF.
[ ] App targets .NET 10 LTS preferred, or .NET 8 LTS with migration note.
[ ] App follows MVVM.
[ ] Dependency injection is set up.
[ ] Config is stored in LocalAppData.
[ ] Config uses camelCase JSON.
[ ] Config loads safely.
[ ] Config saves atomically.
[ ] Corrupted config is backed up.
[ ] Unsupported future config is not overwritten.
[ ] Config migration seam exists.
[ ] Logging exists.
[ ] Log rotation exists.
[ ] User can create groups.
[ ] User can rename groups.
[ ] User can delete groups with confirmation.
[ ] Duplicate group names are prevented.
[ ] Group names are trimmed and normalized.
[ ] User can add .exe apps.
[ ] Unsupported file types are rejected.
[ ] User can edit app name/path.
[ ] User can remove apps with confirmation.
[ ] Duplicate app names are prevented inside the same group.
[ ] Duplicate app paths are prevented inside the same group.
[ ] App names are trimmed and normalized.
[ ] Missing app paths are shown clearly.
[ ] User can launch one app.
[ ] User can launch selected group.
[ ] IProcessStarter abstraction exists.
[ ] Duplicate launch spam is prevented.
[ ] Launch failures do not crash the app.
[ ] Launch summary wording is accurate.
[ ] UI has a clean dark theme.
[ ] Theme resource dictionaries exist.
[ ] Code-behind does not contain business logic.
[ ] Service-level tests exist.
[ ] Tests do not launch real production apps.
[ ] App can be published as win-x64 self-contained single-file executable.
[ ] Config and logs are written outside the executable directory.
```

---

## 24. Implementation Rules for AI Agents

These rules are mandatory when using AI coding agents.

### 24.1 Scope Rules

- Do not implement features outside the current phase.
- Do not add `.lnk` support in v0.1.
- Do not add tray support in v0.1.
- Do not add startup integration in v0.1.
- Do not add app icon extraction in v0.1.
- Do not add run-as-admin support in v0.1.
- Do not add installer work in v0.1.
- Do not add auto-update in v0.1.
- Do not add URL launching in v0.1.
- Do not add drag and drop ordering in v0.1.

### 24.2 Architecture Rules

- Follow MVVM.
- Keep business logic out of code-behind.
- Use constructor injection.
- Do not instantiate services directly inside ViewModels.
- Keep services focused.
- Keep models simple and serializable.
- Do not create God classes.
- Do not duplicate systems.
- Do not create fake placeholder services that will be hard to replace later.

### 24.3 Code Change Rules

- Do not rewrite unrelated files.
- Keep changes small and reviewable.
- Explain what changed and why.
- Do not silently change config schema.
- Do not break existing saved config without migration.
- Do not overwrite unsupported future config versions.
- Do not introduce large dependencies without approval.
- Do not introduce CommunityToolkit.Mvvm unless explicitly approved.
- Do not introduce MaterialDesign, MahApps, or other third-party UI frameworks in v0.1.
- Do not hide exceptions without logging.
- Do not patch symptoms while leaving root issues.
- Enable nullable reference types.
- Enable implicit usings.
- Treat warnings seriously.

### 24.4 Persistence Rules

- Do not save config on every property setter.
- Do not save config on every text change.
- Save only after committed user actions.
- Use atomic save behavior.
- Use camelCase JSON.
- Keep schema minimal.

### 24.5 UI Rules

- Do not leave default WPF styling as final.
- Do not use excessive animations.
- Do not make destructive actions silent.
- Do not show raw exceptions to users.
- Do not create confusing duplicate names.
- Keep common colors/styles in resource dictionaries.

### 24.6 Reporting Rules After Each AI Implementation

After implementation, the AI agent must report:

```text
- Files created
- Files modified
- Features implemented
- Features intentionally not implemented
- Assumptions made
- How to run the app
- How to run tests
- Known risks or TODOs
```

---

## 25. First Implementation Prompt for AI Agent

```text
We are building Program Starter, a Windows-only desktop launcher app.

Use:
- C#
- WPF
- .NET 10 LTS preferred
- MVVM
- Microsoft.Extensions.DependencyInjection
- JSON config
- LocalAppData storage

This is v0.1 foundation work.

Do not implement:
- .lnk shortcut support
- system tray
- Windows startup integration
- app icon extraction
- run as admin
- installer
- auto-update
- URL launching
- drag and drop ordering
- working directory UI
- launch arguments UI

Implement Phase 0 only:
- Create clean solution/project structure.
- Add WPF project.
- Add test project.
- Add folders: Models, ViewModels, Views, Services, Commands, Themes, Assets, Helpers.
- Add models: AppSettings, AppGroup, AppEntry, LaunchResult, LaunchErrorCode.
- Add service interfaces: IConfigService, IConfigMigrationService, IAppLauncherService, IProcessStarter, IFileDialogService, IPathValidationService, IAppLogger.
- Add RelayCommand and AsyncRelayCommand.
- Add DI bootstrap using Microsoft.Extensions.DependencyInjection.
- Create MainWindow and MainViewModel.
- MainWindow should resolve MainViewModel through DI.
- Add basic dark theme resource dictionaries: Colors.xaml, Typography.xaml, Buttons.xaml, Cards.xaml, Layout.xaml.
- Create a simple shell UI with left group sidebar placeholder and main content placeholder.
- Add no-groups empty state placeholder.
- Use the UI rules from section 18: dark palette, spacing resources, typography resources, non-default button/card styles, and no Settings/theme/custom-window scope creep.

Rules:
- Follow MVVM.
- No business logic in code-behind.
- Do not create a one-file prototype.
- Keep changes small and reviewable.
- Do not add features outside Phase 0.
- Do not add third-party UI frameworks.
- Enable nullable reference types.
- Enable implicit usings.
- Explain the created structure after implementation.

After implementation, report:
- Files created
- Files modified
- Features implemented
- Features intentionally not implemented
- Assumptions made
- How to run the app
- How to run tests
- Known risks or TODOs
```

---

## 26. Review Checklist for Each AI Implementation Step

### Architecture

```text
[ ] Does it follow MVVM?
[ ] Is code-behind minimal?
[ ] Are services separated from UI?
[ ] Are services injected through constructors?
[ ] Are models simple and serializable?
[ ] Is there any God class?
[ ] Did it avoid fake abstractions?
```

### Scope

```text
[ ] Did it stay within the current phase?
[ ] Did it add forbidden v0.1 features?
[ ] Did it rewrite unrelated files?
[ ] Did it introduce unnecessary dependencies?
```

### Persistence

```text
[ ] Is config stored in LocalAppData?
[ ] Is JSON camelCase?
[ ] Is save behavior safe?
[ ] Is corrupted config handled?
[ ] Is unsupported future config protected?
[ ] Is config schema stable?
[ ] Does auto-save happen only after committed actions?
```

### Launching

```text
[ ] Are paths validated before launch?
[ ] Are launch failures returned as results?
[ ] Are technical errors logged?
[ ] Is duplicate launch spam prevented?
[ ] Does launching use IProcessStarter?
[ ] Do tests avoid launching real apps?
```

### UX / UI

```text
[ ] Are empty states clear?
[ ] Are errors readable?
[ ] Is primary action obvious?
[ ] Does the app avoid default WPF styling as final?
[ ] Are sidebar selected/hover states clear?
[ ] Are app cards readable and not spreadsheet-like?
[ ] Are long paths trimmed/ellipsized correctly?
[ ] Are invalid paths visible without deleting user data?
[ ] Are disabled, hover, selected, and danger states distinct?
[ ] Are destructive actions confirmed?
[ ] Is launch wording accurate instead of overpromising?
[ ] Did the implementation avoid settings/theme/custom-window scope creep?
```

### Tests

```text
[ ] Are service-level tests added where appropriate?
[ ] Do tests avoid launching real production apps?
[ ] Are corrupted/missing config cases covered?
[ ] Is unsupported future schema covered?
[ ] Are duplicate name/path cases covered?
```

---

## 27. Recommended AI Agent Workflow

Use AI agents in small phase-based passes. Do not ask one agent to build the whole app in one giant prompt.

### 27.1 Recommended Roles

```text
Codex / planning agent:
- Break the current phase into a short implementation plan.
- Identify risky files before code changes.
- Review whether the implementation stayed inside scope.

DeepSeek / builder agent:
- Implement only the approved current phase.
- Keep changes small.
- Report files created/modified and assumptions.

Separate reviewer agent:
- Review the actual code after implementation.
- Check architecture, scope, persistence, launch behavior, UI quality, and tests.
- Do not rewrite immediately; first report issues.
```

### 27.2 Best Workflow Per Phase

```text
1. Give the current document to the planning/review agent.
2. Ask for a phase-specific plan only.
3. Review the plan manually.
4. Give the approved phase prompt to DeepSeek.
5. Let DeepSeek implement only that phase.
6. Run the app/tests locally.
7. Give the diff or changed files to a reviewer agent.
8. Fix only the concrete issues found.
9. Commit.
10. Move to the next phase.
```

Do not let the builder agent perform broad refactors between phases unless the reviewer identifies a concrete reason.

### 27.3 Commit Strategy

Recommended commits:

```text
phase-0-foundation
phase-1-config-logging
phase-2-groups
phase-3-app-management
phase-4-launching
phase-5-ui-polish
phase-6-stabilization
```

Each commit should build. Avoid giant unreviewable commits.

### 27.4 DeepSeek Builder Prompt Template

Use this structure for every implementation pass:

```text
You are the builder agent for Program Starter.

Read and follow Program_Starter_Roadmap_Iteration_5.md.

Current task:
Implement Phase X only: [phase name].

Hard rules:
- Do not implement features outside Phase X.
- Do not add .lnk support, tray, startup integration, icon extraction, installer, URL launching, run-as-admin, drag/drop ordering, theme editor, or settings screen.
- Follow MVVM.
- Keep business logic out of code-behind.
- Use constructor injection.
- Do not instantiate services inside ViewModels.
- Do not rewrite unrelated files.
- Do not introduce third-party UI frameworks.
- Do not silently change config schema.
- Do not hide exceptions without logging.
- Keep changes small and reviewable.

Before coding:
1. Summarize the exact files you expect to create or modify.
2. Mention any assumptions.
3. Mention anything from the phase you are intentionally not doing.

After coding:
Report:
- Files created
- Files modified
- Features implemented
- Features intentionally not implemented
- Assumptions made
- How to run the app
- How to run tests
- Known risks or TODOs
```

### 27.5 DeepSeek UI Polish Prompt

Use this only for Phase 5, after the core app works.

```text
You are improving the UI polish for Program Starter Phase 5 only.

Read section 18 of Program_Starter_Roadmap_Iteration_5.md carefully.

Goal:
Make the existing WPF app feel like a clean, modern, dark Windows launcher without expanding product scope.

Implement only:
- Resource dictionary based colors, typography, buttons, cards, and layout spacing.
- Clean two-column layout.
- Sidebar selected/hover states.
- App card styling.
- Primary/secondary/neutral/danger/disabled button styles.
- Empty states.
- Invalid path visual state.
- Status area polish.
- Long path trimming/ellipsis.

Do not implement:
- Settings screen
- Theme editor
- Custom window chrome
- App icon extraction
- Advanced animations
- Third-party UI framework
- New navigation system
- Major view fragmentation

Quality bar:
- It must not look like untouched default WPF.
- The Start Group button must be visually dominant.
- Remove/Delete must be visually dangerous but not dominant.
- Empty states must explain the next action.
- Long paths must not break layout.
- Styling must be centralized in resource dictionaries.

After implementation, report exactly what changed and include any XAML files touched.
```

### 27.6 Reviewer Agent Prompt

Use after each DeepSeek implementation.

```text
You are the senior reviewer for Program Starter.

Review the implementation against Program_Starter_Roadmap_Iteration_5.md.

Do not rewrite the code yet.
First produce a review report.

Check:
- Did it stay inside the current phase?
- Did it add forbidden v0.1 features?
- Is MVVM respected?
- Is code-behind minimal?
- Are services injected instead of manually created?
- Are models simple and serializable?
- Is config schema stable?
- Are persistence rules safe?
- Are launch rules testable and using IProcessStarter?
- Are exceptions logged but not dumped into UI?
- Are tests meaningful and not launching real apps?
- Does UI avoid default WPF styling as final?
- Did it introduce unnecessary dependencies or God classes?

Output format:
1. Verdict: Pass / Pass with fixes / Fail
2. Critical issues
3. Important issues
4. Minor issues
5. Scope creep found
6. Suggested minimal fixes
7. Files that need attention
```

### 27.7 Manual Gate Before Moving Phase

Before moving to the next phase, manually confirm:

```text
[ ] App builds.
[ ] Tests pass if tests exist for this phase.
[ ] No forbidden feature was added.
[ ] No unrelated files were rewritten.
[ ] Current phase deliverable works.
[ ] Reviewer did not find critical architecture problems.
[ ] Commit is clean and understandable.
```

## 28. Senior Risk Register

### Risk 1 - Shortcut Complexity

`.lnk` support looks simple but can point to many target types.

Mitigation:

```text
v0.1 supports .exe only.
```

### Risk 2 - Config Data Loss

Bad save logic can destroy user config.

Mitigation:

```text
Use temp file + File.Replace where possible.
Keep backup.
Log save failures.
Do not overwrite unsupported future config.
```

### Risk 3 - Service Instantiation Inside ViewModels

Agents may create services directly in ViewModels.

Mitigation:

```text
Require constructor injection and DI registration.
```

### Risk 4 - Silent Failures

Clean UI messages can accidentally hide technical errors.

Mitigation:

```text
Add local file logging.
```

### Risk 5 - Launch Spam

Double-clicking Start Group can launch duplicates.

Mitigation:

```text
Use IsLaunching guard.
Disable launch buttons during launch.
```

### Risk 6 - Feature Creep

Launcher can become process manager, installer, shortcut resolver, and theme editor too early.

Mitigation:

```text
Strict phase-based roadmap.
Hard v0.1 out-of-scope list.
```

### Risk 7 - Roaming Config Issues

Using Roaming AppData can move machine-specific paths across devices.

Mitigation:

```text
Use LocalAppData.
```

### Risk 8 - Over-Fragmented MVVM

Agents may split every small UI area into separate files and create unnecessary complexity.

Mitigation:

```text
Use simple MainWindow-centered UI for v0.1 unless the file becomes hard to maintain.
Avoid early custom-control architecture.
```

### Risk 9 - Auto-Save Overuse

Agents may save config from property setters or every text change.

Mitigation:

```text
Save only after committed commands.
```

### Risk 10 - Misleading Launch Status

The app may claim apps started successfully even when Windows only accepted the launch request.

Mitigation:

```text
Use launch request wording.
```

---

## 29. Iteration 5 Senior Change Summary

Iteration 5 keeps the Iteration 4 foundation and adds final UI quality and agent workflow hardening:

```text
1. Keep the Iteration 4 technical foundation unchanged.
2. Strengthen UI section with concrete layout, spacing, typography, button hierarchy, and card rules.
3. Remove Settings/gear ambiguity from the v0.1 wireframe because settings UI is out of scope.
4. Add explicit UI quality bar so the app does not ship as default WPF.
5. Add long-path trimming/ellipsis requirement.
6. Add sidebar, app card, empty state, invalid path, dialog, and launching state rules.
7. Keep UI polish simple: no third-party UI frameworks, custom chrome, theme editor, advanced animations, or icon extraction.
8. Expand Phase 5 with practical UI polish tasks.
9. Add recommended AI agent workflow for Codex/DeepSeek/reviewer usage.
10. Add DeepSeek builder prompt template.
11. Add dedicated DeepSeek UI polish prompt.
12. Add reviewer agent prompt and manual phase gate checklist.
```

---

## 30. Final Direction

Program Starter should be built as a small, reliable Windows desktop tool.

The v0.1 goal is not maximum features.

The v0.1 goal is:

```text
A clean WPF foundation.
A safe JSON config system.
A migration-ready persistence layer.
Reliable .exe launch requests.
Clear user feedback.
A dark, usable UI.
Maintainable architecture.
Small but useful service-level tests.
```

Final foundation:

```text
C# + WPF + .NET 10 LTS preferred + MVVM + DI + JSON + LocalAppData
```

Do not weaken this foundation for short-term speed.

The correct product instinct is:

```text
Small scope.
Strong foundation.
No magic.
No feature soup.
No config roulette.
```

Build the launcher like a compact garage tool: simple shape, reliable steel, no decorative engine hanging out of the hood.
