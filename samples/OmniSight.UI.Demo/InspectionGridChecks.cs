using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using OmniSight.UI.Controls;

namespace OmniSight.UI.Demo
{
    /// <summary>InspectionGrid의 테마 색, 판정 배지 그리기, 입력 검증, GDI 핸들 증가를 확인한다.</summary>
    internal static class InspectionGridChecks
    {
        [DllImport("user32.dll")]
        private static extern int GetGuiResources(IntPtr process, int kind);

        internal static void RunChecks()
        {
            CheckThemeColors();
            CheckVerdictBadge();
            CheckInvalidInput();
            CheckLeaks();
            Console.WriteLine("PASS: inspection grid theme colors, verdict badges, input validation, leaks");
        }

        private static void CheckThemeColors()
        {
            using (var grid = new InspectionGrid())
            {
                foreach (UiTheme theme in new[] { UiTheme.Dark, UiTheme.Light })
                {
                    grid.Theme = theme;
                    CardPalette palette = CardPalette.For(theme);
                    Require(grid.DefaultCellStyle.BackColor.ToArgb() == palette.Surface.ToArgb(), theme + " cell background");
                    Require(grid.DefaultCellStyle.ForeColor.ToArgb() == palette.Text.ToArgb(), theme + " cell text");
                    Require(grid.ColumnHeadersDefaultCellStyle.ForeColor.ToArgb() == palette.MutedText.ToArgb(), theme + " header text");
                    Require(grid.AlternatingRowsDefaultCellStyle.BackColor.ToArgb() == palette.Input.ToArgb(), theme + " alternate row");
                }
            }
        }

        private static void CheckVerdictBadge()
        {
            foreach (UiTheme theme in new[] { UiTheme.Dark, UiTheme.Light })
            {
                using (var form = new Form { ShowInTaskbar = false, StartPosition = FormStartPosition.Manual, Location = new Point(-32000, -32000) })
                {
                    var grid = CreateGrid(theme);
                    form.Controls.Add(grid);
                    form.ClientSize = new Size(320, 140);
                    form.Show();

                    var okColors = BadgeDrawing.Colors(theme, BadgeTone.Success);
                    var ngColors = BadgeDrawing.Colors(theme, BadgeTone.Error);
                    using (var image = new Bitmap(grid.Width, grid.Height))
                    {
                        grid.DrawToBitmap(image, new Rectangle(0, 0, grid.Width, grid.Height));
                        Require(Contains(image, okColors.Background), theme + " OK badge background is drawn");
                        Require(Contains(image, ngColors.Background), theme + " NG badge background is drawn");
                    }

                    grid.VerdictColumnIndex = 0;
                    Require(grid.VerdictColumnIndex == 0, "verdict column can be changed");
                }
            }
        }

        private static void CheckInvalidInput()
        {
            using (var grid = new InspectionGrid())
            {
                try
                {
                    grid.VerdictColumnIndex = -2;
                }
                catch (ArgumentOutOfRangeException)
                {
                    Require(grid.VerdictColumnIndex == -1, "rejected index keeps the previous value");
                    return;
                }

                throw new InvalidOperationException("Check failed: index below -1 is rejected");
            }
        }

        private static void CheckLeaks()
        {
            using (var form = new Form { ShowInTaskbar = false, StartPosition = FormStartPosition.Manual, Location = new Point(-32000, -32000) })
            {
                var grid = CreateGrid(UiTheme.Dark);
                form.Controls.Add(grid);
                form.ClientSize = new Size(320, 140);
                form.Show();

                IntPtr process = System.Diagnostics.Process.GetCurrentProcess().Handle;
                int before = GetGuiResources(process, 0);
                for (int i = 0; i < 500; i++)
                {
                    grid.Theme = (i & 1) == 0 ? UiTheme.Dark : UiTheme.Light;
                    using (var image = new Bitmap(grid.Width, grid.Height))
                        grid.DrawToBitmap(image, new Rectangle(0, 0, grid.Width, grid.Height));
                }

                int after = GetGuiResources(process, 0);
                Require(after - before <= 5, "GDI objects stay flat after 500 theme/paint cycles (" + before + " -> " + after + ")");
            }
        }

        private static InspectionGrid CreateGrid(UiTheme theme)
        {
            var grid = new InspectionGrid { Dock = DockStyle.Fill, Theme = theme };
            grid.Columns.Add("time", "시각");
            grid.Columns.Add("height", "높이");
            grid.Columns.Add("verdict", "판정");
            grid.Rows.Add("14:32:17", "12.34", "NG");
            grid.Rows.Add("14:32:15", "12.36", "OK");
            grid.Rows.Add("14:32:13", "12.33", "OK");
            return grid;
        }

        private static bool Contains(Bitmap image, Color color)
        {
            int target = color.ToArgb();
            for (int y = 0; y < image.Height; y++)
                for (int x = 0; x < image.Width; x++)
                    if (image.GetPixel(x, y).ToArgb() == target)
                        return true;
            return false;
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException("Check failed: " + message);
        }
    }
}
