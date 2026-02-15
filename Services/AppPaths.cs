using System;
using System.IO;

namespace TodoDS.Services;

public static class AppPaths
{
    public static string DataDirectory =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TodoDS");

    public static string DataFile => Path.Combine(DataDirectory, "todos.json");
}
