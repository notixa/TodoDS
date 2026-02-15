using System;
using System.Globalization;

namespace TodoDS.Services;

public static class DateTimeHelper
{
    private static readonly string[] Formats =
    {
        "yyyy-MM-dd HH:mm",
        "yyyy/MM/dd HH:mm",
        "yyyy-MM-dd",
        "yyyy/MM/dd",
    };

    public static bool TryParseDueTime(string input, out DateTime? dueTime)
    {
        dueTime = null;
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        if (!DateTime.TryParseExact(
                input.Trim(),
                Formats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AllowWhiteSpaces,
                out var parsed))
        {
            return false;
        }

        if (input.Trim().Length <= 10)
        {
            parsed = parsed.Date.AddHours(9);
        }

        dueTime = parsed;
        return true;
    }
}
