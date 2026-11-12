using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace OmniSight.UI.Controls
{
    internal static class BadgeDrawing
    {
        private const TextFormatFlags TextFlags = TextFormatFlags.SingleLine | TextFormatFlags.NoPadding;

        internal static (Color Background, Color Foreground) Colors(UiTheme theme, BadgeTone tone)
        {
            CardPalette palette = CardPalette.For(theme);
            switch (tone)
            {
                case BadgeTone.Success:
                    return (palette.SuccessFill, theme == UiTheme.Dark ? Rgb(0x63D9AA) : palette.SuccessText);
                case BadgeTone.Error:
                    return (palette.ErrorFill, theme == UiTheme.Dark ? Rgb(0xF58A88) : palette.ErrorText);
                case BadgeTone.Info:
                    return (palette.AccentFill, palette.AccentText);
                case BadgeTone.Warning:
                    return theme == UiTheme.Dark ? (Rgb(0x443728), Rgb(0xFFD08A)) : (Rgb(0xFFF1DA), Rgb(0x75480C));
                default:
                    throw new InvalidEnumArgumentException(nameof(tone), (int)tone, typeof(BadgeTone));
            }
        }

        internal static Color ContrastingText(Color background)
        {
            double luminance = 0.2126 * Linear(background.R) + 0.7152 * Linear(background.G)
                + 0.0722 * Linear(background.B);
            return (luminance + 0.05) / 0.05 >= 1.05 / (luminance + 0.05) ? Color.Black : Color.White;
        }

        internal static int Scale(int value, int dpi) => (int)Math.Round(value * (double)dpi / 96);

        internal static Padding PaddingForDpi(int dpi) => new Padding(Scale(9, dpi), Scale(4, dpi),
            Scale(9, dpi), Scale(4, dpi));

        internal static Font CreateBoldFont(Font font)
        {
            FontStyle style = font.Style | FontStyle.Bold;
            return font.Bold || !font.FontFamily.IsStyleAvailable(style) ? null : new Font(font, style);
        }

        internal static Size Measure(Graphics graphics, string text, Font font, bool useMnemonic)
        {
            TextFormatFlags flags = TextFlags | (useMnemonic ? 0 : TextFormatFlags.NoPrefix);
            string line = SingleLine(text);
            return graphics == null
                ? TextRenderer.MeasureText(line, font, Size.Empty, flags)
                : TextRenderer.MeasureText(graphics, line, font, Size.Empty, flags);
        }

        internal static void Draw(Graphics graphics, Rectangle bounds, string text, Font font,
            Color background, Color foreground, float radius, Padding padding, ContentAlignment alignment,
            bool ellipsis, bool useMnemonic)
        {
            if (bounds.Width <= 0 || bounds.Height <= 0)
                return;

            GraphicsState state = graphics.Save();
            try
            {
                graphics.SetClip(bounds, CombineMode.Intersect);
                SmoothingMode previous = graphics.SmoothingMode;
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath shape = RoundedPath(bounds, radius))
                using (var brush = new SolidBrush(background))
                    graphics.FillPath(brush, shape);
                graphics.SmoothingMode = previous;

                var content = new Rectangle(bounds.X + padding.Left, bounds.Y + padding.Top,
                    bounds.Width - padding.Horizontal, bounds.Height - padding.Vertical);
                if (string.IsNullOrEmpty(text) || content.Width <= 0 || content.Height <= 0)
                    return;

                graphics.SetClip(content, CombineMode.Intersect);
                TextFormatFlags flags = TextFlags | TextFormatFlags.PreserveGraphicsClipping | Align(alignment);
                if (ellipsis)
                    flags |= TextFormatFlags.EndEllipsis;
                if (!useMnemonic)
                    flags |= TextFormatFlags.NoPrefix;
                TextRenderer.DrawText(graphics, SingleLine(text), font, content, foreground, flags);
            }
            finally
            {
                graphics.Restore(state);
            }
        }

        internal static GraphicsPath RoundedPath(RectangleF bounds, float radius)
        {
            var path = new GraphicsPath();
            float diameter = Math.Min(radius * 2, Math.Min(bounds.Width, bounds.Height));
            if (diameter <= 0)
            {
                path.AddRectangle(bounds);
                return path;
            }

            path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.Left, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }

        private static string SingleLine(string text) => (text ?? string.Empty).Replace("\r\n", " ")
            .Replace('\r', ' ').Replace('\n', ' ');

        private static TextFormatFlags Align(ContentAlignment alignment)
        {
            switch (alignment)
            {
                case ContentAlignment.TopCenter: return TextFormatFlags.HorizontalCenter;
                case ContentAlignment.TopRight: return TextFormatFlags.Right;
                case ContentAlignment.MiddleLeft: return TextFormatFlags.VerticalCenter;
                case ContentAlignment.MiddleRight: return TextFormatFlags.VerticalCenter | TextFormatFlags.Right;
                case ContentAlignment.MiddleCenter: return TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter;
                case ContentAlignment.BottomLeft: return TextFormatFlags.Bottom;
                case ContentAlignment.BottomCenter: return TextFormatFlags.Bottom | TextFormatFlags.HorizontalCenter;
                case ContentAlignment.BottomRight: return TextFormatFlags.Bottom | TextFormatFlags.Right;
                default: return TextFormatFlags.Left;
            }
        }

        private static double Linear(byte component)
        {
            double value = component / 255d;
            return value <= 0.04045 ? value / 12.92 : Math.Pow((value + 0.055) / 1.055, 2.4);
        }

        private static Color Rgb(int rgb) => Color.FromArgb(unchecked((int)0xFF000000) | rgb);
    }
}
