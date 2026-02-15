using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace TodoDS.Services;

public static class TrayIconFactory
{
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool DestroyIcon(IntPtr hIcon);

    public static Icon Create()
    {
        const int size = 64;
        using var bitmap = new Bitmap(size, size);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.Clear(Color.FromArgb(30, 30, 30));

        using var cardBrush = new SolidBrush(Color.FromArgb(255, 218, 102));
        using var darkBrush = new SolidBrush(Color.FromArgb(43, 43, 43));
        using var path = RoundedRect(new RectangleF(6, 6, size - 12, size - 12), 12);
        graphics.FillPath(cardBrush, path);
        graphics.FillRectangle(darkBrush, 16, 20, 32, 5);
        graphics.FillRectangle(darkBrush, 16, 32, 24, 5);
        graphics.FillRectangle(darkBrush, 16, 44, 20, 5);

        var hIcon = bitmap.GetHicon();
        try
        {
            return (Icon)Icon.FromHandle(hIcon).Clone();
        }
        finally
        {
            DestroyIcon(hIcon);
        }
    }

    private static GraphicsPath RoundedRect(RectangleF bounds, float radius)
    {
        var path = new GraphicsPath();
        var diameter = radius * 2;
        var arc = new RectangleF(bounds.Location, new SizeF(diameter, diameter));

        path.AddArc(arc, 180, 90);
        arc.X = bounds.Right - diameter;
        path.AddArc(arc, 270, 90);
        arc.Y = bounds.Bottom - diameter;
        path.AddArc(arc, 0, 90);
        arc.X = bounds.Left;
        path.AddArc(arc, 90, 90);
        path.CloseFigure();
        return path;
    }
}
