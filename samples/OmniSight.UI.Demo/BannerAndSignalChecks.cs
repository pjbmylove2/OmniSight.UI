using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using OmniSight.UI.Controls;

namespace OmniSight.UI.Demo
{
    /// <summary>AlertBanner와 SignalRow의 색, 상태 표시, 입력 처리, GDI 핸들 증가를 확인한다.</summary>
    internal static class BannerAndSignalChecks
    {
        [DllImport("user32.dll")]
        private static extern int GetGuiResources(IntPtr process, int kind);

        internal static void RunChecks()
        {
            CheckBanner();
            CheckSignalRow();
            CheckLeaks();
            Console.WriteLine("PASS: alert banner tones, signal row lamp states, null handling, leaks");
        }

        private static void CheckBanner()
        {
            foreach (UiTheme theme in new[] { UiTheme.Dark, UiTheme.Light })
            {
                foreach (BadgeTone tone in new[] { BadgeTone.Info, BadgeTone.Success, BadgeTone.Error, BadgeTone.Warning })
                {
                    using (var banner = new AlertBanner { Width = 500, Height = 56, Theme = theme, Tone = tone, Title = "설정 적용 안내", Text = "검사를 정지한 상태에서 변경해 주세요." })
                    using (var image = new Bitmap(banner.Width, banner.Height))
                    {
                        banner.DrawToBitmap(image, new Rectangle(0, 0, image.Width, image.Height));
                        var colors = BadgeDrawing.Colors(theme, tone);
                        Require(image.GetPixel(banner.Width - 3, banner.Height - 3).ToArgb() == colors.Background.ToArgb(), theme + "/" + tone + " background");
                        Require(ContainsNear(image, colors.Foreground), theme + "/" + tone + " text color");
                    }
                }
            }

            using (var banner = new AlertBanner())
            {
                banner.Text = null;
                banner.Title = null;
                Require(banner.Text.Length == 0 && banner.Title.Length == 0, "null text and title become empty");
            }
        }

        private static void CheckSignalRow()
        {
            foreach (UiTheme theme in new[] { UiTheme.Dark, UiTheme.Light })
            {
                CardPalette palette = CardPalette.For(theme);
                using (var row = new SignalRow { Width = 500, Height = 40, Theme = theme, Channel = "DI 00", SignalName = "검사 트리거" })
                using (var on = new Bitmap(row.Width, row.Height))
                using (var off = new Bitmap(row.Width, row.Height))
                {
                    row.IsOn = true;
                    row.DrawToBitmap(on, new Rectangle(0, 0, on.Width, on.Height));
                    row.IsOn = false;
                    row.DrawToBitmap(off, new Rectangle(0, 0, off.Width, off.Height));

                    Require(on.GetPixel(2, 2).ToArgb() == palette.Input.ToArgb(), theme + " row background");
                    Require(ContainsNear(on, palette.SuccessText), theme + " ON lamp uses the success color");
                    Require(!ContainsNear(off, palette.SuccessText), theme + " OFF has no success color");
                    Require(ContainsNear(off, palette.MutedText), theme + " OFF lamp uses the muted color");
                }
            }

            using (var row = new SignalRow())
            {
                row.Channel = null;
                row.SignalName = null;
                Require(row.Channel.Length == 0 && row.SignalName.Length == 0, "null channel and name become empty");
            }
        }

        private static void CheckLeaks()
        {
            using (var form = new Form { ShowInTaskbar = false, StartPosition = FormStartPosition.Manual, Location = new Point(-32000, -32000) })
            {
                var banner = new AlertBanner { Left = 0, Top = 0, Width = 400, Height = 56, Title = "제목", Text = "본문" };
                var row = new SignalRow { Left = 0, Top = 60, Width = 400, Height = 40, Channel = "DO 00", SignalName = "Ready" };
                form.Controls.Add(banner);
                form.Controls.Add(row);
                form.ClientSize = new Size(400, 110);
                form.Show();

                IntPtr process = System.Diagnostics.Process.GetCurrentProcess().Handle;
                int before = GetGuiResources(process, 0);
                BadgeTone[] tones = { BadgeTone.Info, BadgeTone.Success, BadgeTone.Error, BadgeTone.Warning };
                for (int i = 0; i < 500; i++)
                {
                    UiTheme theme = (i & 1) == 0 ? UiTheme.Dark : UiTheme.Light;
                    banner.Theme = theme;
                    banner.Tone = tones[i % tones.Length];
                    row.Theme = theme;
                    row.IsOn = (i & 2) == 0;
                    using (var image = new Bitmap(form.ClientSize.Width, form.ClientSize.Height))
                        form.DrawToBitmap(image, new Rectangle(0, 0, image.Width, image.Height));
                }

                int after = GetGuiResources(process, 0);
                Require(after - before <= 5, "GDI objects stay flat after 500 cycles (" + before + " -> " + after + ")");
            }
        }

        private static bool ContainsNear(Bitmap image, Color color)
        {
            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    Color pixel = image.GetPixel(x, y);
                    if (Math.Abs(pixel.R - color.R) + Math.Abs(pixel.G - color.G) + Math.Abs(pixel.B - color.B) <= 12)
                        return true;
                }
            }

            return false;
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException("Check failed: " + message);
        }
    }
}
