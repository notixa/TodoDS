# Repository Guidelines

## Project Structure & Module Organization
This is a WPF desktop app targeting `net9.0-windows`. Source files live at the repo root, with UI definitions in XAML and code-behind in C#.
- Root UI: `App.xaml`, `MainWindow.xaml`, `PetWindow.xaml`, `QuickViewWindow.xaml` and their `.xaml.cs` files.
- `Models/`: domain types such as `TodoItem`.
- `Services/`: app services (storage, autostart, tray icon, helpers), e.g., `TodoStore.cs`, `AutostartService.cs`.
- `bin/` and `obj/`: build outputs; don’t hand-edit or commit generated artifacts.

## Build, Test, and Development Commands
- `dotnet restore` — restore NuGet packages.
- `dotnet build TodoDS.csproj` — compile the WPF app.
- `dotnet run --project TodoDS.csproj` — run locally (starts the UI).
- `dotnet clean` — remove build outputs.

## Coding Style & Naming Conventions
- C# uses 4-space indentation and file-scoped namespaces (`namespace TodoDS.Models;`).
- Types/methods: `PascalCase`; locals/parameters: `camelCase`; private fields use a leading underscore (e.g., `_items`).
- Nullable reference types are enabled; avoid introducing new nullability warnings.

## Testing Guidelines
There is no test project in this repository yet. If you add tests, create a separate test project (for example `TodoDS.Tests`) and run them with `dotnet test`.

## Commit & Pull Request Guidelines
This folder does not appear to contain a Git history. If you initialize one, use short, imperative commit subjects (e.g., “Add tray icon toggle”) and keep commits focused.
For PRs, include a brief summary, linked issues (if any), and screenshots or GIFs for UI changes. List the exact commands you ran to verify behavior.

## Data & Configuration Notes
- User data is stored at `%APPDATA%\TodoDS\todos.json` (`Services/AppPaths.cs`).
- Autostart uses `HKCU\Software\Microsoft\Windows\CurrentVersion\Run` with a `TodoDS` value (`Services/AutostartService.cs`).
Be cautious when changing storage formats or registry integration.
