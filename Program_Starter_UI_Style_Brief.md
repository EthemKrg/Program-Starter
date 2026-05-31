# Program Starter UI Style Brief v1

## Goal

Create a premium but calm Windows launcher UI. It should feel like a clean developer/productivity tool, not a hobby prototype.

## Visual Direction

- Dark
- Minimal
- Clean spacing
- Subtle borders
- Soft cards
- One restrained accent color
- No visual noise

## Color Palette

```text
Background: #0D1117
Sidebar:    #111827
Card:       #161B22
Border:     #2A3441
Text:       #E5E7EB
Muted Text: #8B949E
Accent:     #7C8CFF
Danger:     #EF4444
```

Use the accent color only for:

- Primary buttons
- Selected group state
- Focus borders
- Small active indicators

Do not overuse the accent color.

## Layout Rules

```text
Sidebar width: 240px
Content padding: 32px
Card radius: 12px
Button radius: 8px
Spacing scale: 8 / 12 / 16 / 24 / 32
```

## Sidebar

- Top area shows the app name: **Program Starter**
- Group list below
- Selected group uses a subtle accent border and dark card background
- Add Group button stays at the bottom as a small ghost button
- Avoid icon clutter

## Main Content

When a group is selected:

```text
Header:
Group Name
Small subtitle: "3 apps in this group"

Right side:
+ Add App button
```

App cards should show:

```text
App name
Executable path as muted small text
Run button
Small edit/delete actions
```

## Empty State

Use a simple empty state:

```text
Title: No groups yet
Text: Create your first launch group.
Button: Create Group
```

If an icon is used, keep it simple and line-based.

## Buttons

```text
Primary: accent background
Secondary: transparent with border
Ghost: text-only or very subtle hover background
Danger: red, only for destructive confirmation
```

Disabled buttons should be muted but still readable.

## Dialogs

Add, rename, and delete dialogs should be:

- Small modal windows
- Card-like dark background
- Clear title
- Single input when needed
- Cancel + Confirm actions
- Destructive actions use danger styling

## Do Not Add

- Custom title bar
- Heavy blur/glass effects
- Heavy animation system
- Third-party UI framework
- Theme editor
- Settings screen
- Decorative UI noise

## Phase 5 Polish Targets

Only polish these areas:

- Spacing
- Sidebar visual hierarchy
- Card styles
- Button styles
- Input styles
- Empty state
- Dialog styling
- Hover, focus, disabled states

## Agent Direction

```text
Use a premium calm dark desktop UI style inspired by high-end minimal product design: clean spacing, subtle borders, strong typography hierarchy, soft cards, one restrained accent color, and no visual clutter. Do not copy any specific designer or product.
```
