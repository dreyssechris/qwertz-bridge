using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace QwertzBridge.App;

/// <summary>Generates the tray icons at runtime (no embedded resources needed for a portable EXE).</summary>
internal static class TrayIcons
{
    /// <summary>Draws a keycap with "&lt;&gt;" glyphs; blue when active, gray when paused.</summary>
    internal static Icon Create(bool active)
    {
        using var bitmap = new Bitmap(16, 16);
        using (var g = Graphics.FromImage(bitmap))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var fill = active ? Color.FromArgb(0, 120, 215) : Color.FromArgb(120, 120, 120);
            using var brush = new SolidBrush(fill);
            using var path = RoundedRect(new Rectangle(1, 1, 14, 14), 3);
            g.FillPath(brush, path);

            using var pen = new Pen(Color.White, 1.8f);
            g.DrawLines(pen, [new PointF(6.5f, 5f), new PointF(4f, 8f), new PointF(6.5f, 11f)]);
            g.DrawLines(pen, [new PointF(9.5f, 5f), new PointF(12f, 8f), new PointF(9.5f, 11f)]);
        }

        var handle = bitmap.GetHicon();
        try
        {
            using var icon = Icon.FromHandle(handle);
            return (Icon)icon.Clone();
        }
        finally
        {
            DestroyIcon(handle);
        }
    }

    private static GraphicsPath RoundedRect(Rectangle bounds, int radius)
    {
        var d = radius * 2;
        var path = new GraphicsPath();
        path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
        path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
        path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
        path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }

    [DllImport("user32.dll")]
    private static extern bool DestroyIcon(IntPtr hIcon);
}
