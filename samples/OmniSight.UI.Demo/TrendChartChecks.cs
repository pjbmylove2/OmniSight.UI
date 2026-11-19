using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using OmniSight.UI.Controls;

namespace OmniSight.UI.Demo
{
    /// <summary>TrendChart의 입력 검증, 선 그리기, 테마, GDI 핸들 증가를 확인한다.</summary>
    internal static class TrendChartChecks
    {
        [DllImport("user32.dll")]
        private static extern int GetGuiResources(IntPtr process, int kind);

        internal static void RunChecks()
        {
            CheckSeriesInput();
            CheckPaint();
            CheckLeaks();
            Console.WriteLine("PASS: trend chart series input, paint (2 themes), leaks");
        }

        private static void CheckSeriesInput()
        {
            using (var chart = new TrendChart())
            {
                chart.SetSeries(new[] { 12.3, 12.4, 12.5 }, new[] { 12.2, 12.6 }, 12.2, 12.6);
                Require(chart.FirstCount == 3 && chart.SecondCount == 2, "both series are kept");

                chart.SetSeries(new[] { 12.3 }, null, 12.2, 12.6);
                Require(chart.FirstCount == 0 && chart.SecondCount == 0, "fewer than two points draws nothing");

                RequireThrows(() => chart.SetSeries(new[] { 1.0, 2.0 }, null, 5, 5), "equal limits are rejected");
                RequireThrows(() => chart.SetSeries(new[] { 1.0, double.NaN }, null, 0, 1), "NaN sample is rejected");
            }
        }

        private static void CheckPaint()
        {
            foreach (UiTheme theme in new[] { UiTheme.Dark, UiTheme.Light })
            {
                using (var chart = new TrendChart { Width = 240, Height = 80, Theme = theme })
                {
                    chart.SetSeries(new[] { 12.2, 12.4, 12.6, 12.4 }, null, 12.2, 12.6);
                    CardPalette palette = CardPalette.For(theme);
                    using (var image = new Bitmap(chart.Width, chart.Height))
                    {
                        chart.DrawToBitmap(image, new Rectangle(0, 0, chart.Width, chart.Height));
                        Require(image.GetPixel(2, 2).ToArgb() == palette.Input.ToArgb(), theme + " background");
                        Require(image.GetPixel(2, chart.Height / 2).ToArgb() == palette.SuccessFill.ToArgb()
                            || Contains(image, palette.AccentText), theme + " range band or series is drawn");
                        Require(Contains(image, palette.AccentText), theme + " first series color");
                    }
                }
            }
        }

        private static void CheckLeaks()
        {
            using (var form = new Form { ShowInTaskbar = false, StartPosition = FormStartPosition.Manual, Location = new Point(-32000, -32000) })
            {
                var chart = new TrendChart { Left = 0, Top = 0, Width = 240, Height = 80 };
                form.Controls.Add(chart);
                form.Show();

                IntPtr process = System.Diagnostics.Process.GetCurrentProcess().Handle;
                int before = GetGuiResources(process, 0);
                for (int i = 0; i < 1000; i++)
                {
                    chart.Theme = (i & 1) == 0 ? UiTheme.Dark : UiTheme.Light;
                    chart.SetSeries(new[] { 12.2, 12.3 + (i % 10) / 100.0, 12.5 }, new[] { 12.4, 12.3, 12.6 }, 12.2, 12.6);
                    using (var image = new Bitmap(chart.Width, chart.Height))
                        chart.DrawToBitmap(image, new Rectangle(0, 0, chart.Width, chart.Height));
                }

                int after = GetGuiResources(process, 0);
                Require(after - before <= 5, "GDI objects stay flat after 1000 updates (" + before + " -> " + after + ")");
            }
        }

        private static bool Contains(Bitmap image, Color color)
        {
            // 선은 안티앨리어싱되므로 색이 가까운 픽셀이 있으면 그려진 것으로 본다.
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

        private static void RequireThrows(Action action, string message)
        {
            try
            {
                action();
            }
            catch (ArgumentException)
            {
                return;
            }

            throw new InvalidOperationException("Check failed: " + message);
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException("Check failed: " + message);
        }
    }
}
