using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using OmniSight.UI.Controls;

namespace OmniSight.UI.Demo
{
    /// <summary>RangeIndicator의 위치 계산, 판정, 입력 검증, GDI 핸들 증가를 확인한다.</summary>
    internal static class RangeIndicatorChecks
    {
        [DllImport("user32.dll")]
        private static extern int GetGuiResources(IntPtr process, int kind);

        internal static void RunChecks()
        {
            CheckPositionAndVerdict();
            CheckInvalidInput();
            CheckPaintAndLeaks();
            Console.WriteLine("PASS: range indicator position, verdict, input validation, paint, leaks");
        }

        private static void CheckPositionAndVerdict()
        {
            using (var indicator = new RangeIndicator())
            {
                indicator.SetMeasurement(12.20, 12.20, 12.60);
                Require(Near(indicator.Position, 0.25) && !indicator.Failed, "lower limit sits at the range start and passes");

                indicator.SetMeasurement(12.60, 12.20, 12.60);
                Require(Near(indicator.Position, 0.75) && !indicator.Failed, "upper limit sits at the range end and passes");

                indicator.SetMeasurement(12.40, 12.20, 12.60);
                Require(Near(indicator.Position, 0.50) && !indicator.Failed, "middle value is centered");

                indicator.SetMeasurement(12.68, 12.20, 12.60);
                Require(indicator.Failed && indicator.Position > 0.75, "over the upper limit fails and stays right of the range");

                indicator.SetMeasurement(0.0, 12.20, 12.60);
                Require(indicator.Failed && Near(indicator.Position, 0.01), "far below the range is clamped to the left edge");

                indicator.SetMeasurement(99.0, 12.20, 12.60);
                Require(indicator.Failed && Near(indicator.Position, 0.99), "far above the range is clamped to the right edge");
            }
        }

        private static void CheckInvalidInput()
        {
            using (var indicator = new RangeIndicator())
            {
                indicator.SetMeasurement(12.40, 12.20, 12.60);
                RequireThrows(() => indicator.SetMeasurement(1, 5, 5), "equal limits are rejected");
                RequireThrows(() => indicator.SetMeasurement(1, 6, 5), "reversed limits are rejected");
                RequireThrows(() => indicator.SetMeasurement(double.NaN, 1, 2), "NaN value is rejected");
                RequireThrows(() => indicator.SetMeasurement(1, double.NaN, 2), "NaN limit is rejected");
                Require(Near(indicator.Position, 0.50) && !indicator.Failed, "rejected input keeps the previous state");
            }
        }

        private static void CheckPaintAndLeaks()
        {
            using (var form = new Form { ShowInTaskbar = false, StartPosition = FormStartPosition.Manual, Location = new Point(-32000, -32000) })
            {
                var indicator = new RangeIndicator { Left = 0, Top = 0, Width = 240, Height = 16 };
                form.Controls.Add(indicator);
                form.Show();

                IntPtr process = System.Diagnostics.Process.GetCurrentProcess().Handle;
                int gdiBefore = GetGuiResources(process, 0);
                for (int i = 0; i < 2000; i++)
                {
                    indicator.Theme = (i & 1) == 0 ? UiTheme.Dark : UiTheme.Light;
                    indicator.SetMeasurement(12.0 + (i % 100) / 100.0, 12.20, 12.60);
                    using (var image = new Bitmap(indicator.Width, indicator.Height))
                        indicator.DrawToBitmap(image, new Rectangle(0, 0, indicator.Width, indicator.Height));
                }

                int gdiAfter = GetGuiResources(process, 0);
                Require(gdiAfter - gdiBefore <= 5, "GDI objects stay flat after 2000 theme/value updates (" + gdiBefore + " -> " + gdiAfter + ")");
            }
        }

        private static bool Near(double a, double b)
        {
            return Math.Abs(a - b) < 1e-9;
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
