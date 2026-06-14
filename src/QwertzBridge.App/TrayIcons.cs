using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace QwertzBridge.App;

// Draws the tray icon at runtime: a rounded squircle with white "<>" chevrons and a
// soft top sheen. Indigo when active, gray when paused. No embedded resources needed.
internal static class TrayIcons
{
    internal static Icon Create(bool active)
    {
        using var bitmap = new Bitmap(32, 32);
        using (var g = Graphics.FromImage(bitmap))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            Draw(g, 32, active);
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

    private static void Draw(Graphics g, float s, bool active)
    {
        var pad = s * 0.07f;
        var rect = new RectangleF(pad, pad, s - 2 * pad, s - 2 * pad);
        using var body = Squircle(rect, s * 0.24f);

        var (c1, c2) = active
            ? (Color.FromArgb(99, 102, 241), Color.FromArgb(147, 51, 234))
            : (Color.FromArgb(156, 163, 175), Color.FromArgb(107, 114, 128));
        using (var fill = new LinearGradientBrush(rect, c1, c2, 50f))
            g.FillPath(fill, body);

        // Soft sheen across the top half.
        g.SetClip(body);
        var sheenRect = new RectangleF(pad, pad, s - 2 * pad, s * 0.52f);
        using (var sheen = new LinearGradientBrush(sheenRect,
            Color.FromArgb(active ? 75 : 45, 255, 255, 255), Color.FromArgb(0, 255, 255, 255), 90f))
            g.FillRectangle(sheen, sheenRect);
        g.ResetClip();

        var cx = s / 2f;
        var cy = s / 2f;
        var hh = s * 0.17f;
        var gap = s * 0.085f;
        var depth = s * 0.19f;
        using var pen = new Pen(Color.White, s * 0.088f)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round,
            LineJoin = LineJoin.Round,
        };
        g.DrawLines(pen, [new PointF(cx - gap, cy - hh), new PointF(cx - gap - depth, cy), new PointF(cx - gap, cy + hh)]);
        g.DrawLines(pen, [new PointF(cx + gap, cy - hh), new PointF(cx + gap + depth, cy), new PointF(cx + gap, cy + hh)]);
    }

    private static GraphicsPath Squircle(RectangleF r, float radius)
    {
        var d = radius * 2;
        var path = new GraphicsPath();
        path.AddArc(r.X, r.Y, d, d, 180, 90);
        path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
        path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
        path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }

    [DllImport("user32.dll")]
    private static extern bool DestroyIcon(IntPtr hIcon);
}
