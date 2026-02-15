# TodoDS

TodoDS is a lightweight Windows desktop todo app built with WPF and .NET 9. It focuses on quick access from the system tray, an always-on-top list window, and hierarchical lists.

## Features
- Tree-based lists and todos with nesting
- Due time and completion toggle
- System tray quick view and settings access
- Autostart toggle and tray menu actions
- Always-on-top window for the current list or all todos
- Local JSON storage at `%APPDATA%\TodoDS\todos.json`

## Tech Stack
- .NET 9 (net9.0-windows)
- WPF
- Hardcodet.Wpf.TaskbarNotification

## Build and Run
```bash
dotnet restore
dotnet build TodoDS.csproj
dotnet run --project TodoDS.csproj
```

## Data and Autostart
- Data file: `%APPDATA%\TodoDS\todos.json`
- Autostart registry key: `HKCU\Software\Microsoft\Windows\CurrentVersion\Run` (value `TodoDS`)

## Screenshots
- TODO

## License
MIT
