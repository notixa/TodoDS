using System;
using System.Diagnostics;
using Microsoft.Win32;

namespace TodoDS.Services;

public static class AutostartService
{
    private const string AppName = "TodoDS";
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";

    public static bool IsEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, false);
        return key?.GetValue(AppName) is string value && !string.IsNullOrWhiteSpace(value);
    }

    public static void SetEnabled(bool enabled)
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, true);
        if (key == null)
        {
            return;
        }

        if (!enabled)
        {
            key.DeleteValue(AppName, false);
            return;
        }

        var executable = Environment.ProcessPath ?? Process.GetCurrentProcess().MainModule?.FileName;
        if (string.IsNullOrWhiteSpace(executable))
        {
            return;
        }

        var command = $"\"{executable}\"";
        key.SetValue(AppName, command);
    }
}
