using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using OmniSight.UI.Controls;

namespace OmniSight.UI.Demo
{
    /// <summary>--check / --capture 실행 모드. 실패하면 예외를 던져 종료 코드로 알린다.</summary>
    internal static class DemoChecks
    {
        [DllImport("user32.dll")]
        private static extern int GetGuiResources(IntPtr process, int kind);

        public static void RunChecks()
        {
            CheckPalette();
            CheckEdgePixels();
            CheckNoRedundantInvalidate();
            CheckInput();
            CheckDialogResultAndToggle();
            CheckDesignMode();
            CheckLeaks();
            Console.WriteLine("PASS: palette, edge pixels (3 variants x 5 states x 2 themes x 2 sizes), redundant invalidate, input, dialog result and toggle, design mode, leaks");
        }

        public static void RunCapture(string directory)
        {
            Directory.CreateDirectory(directory);
            foreach (bool large in new[] { false, true })
            {
                foreach (UiTheme theme in new[] { UiTheme.Dark, UiTheme.Light })
                {
                    using (var showcase = new ButtonShowcase(theme, large))
                    {
                        string name = "SquareButton-" + (theme == UiTheme.Dark ? "dark" : "light") + (large ? "-2560x1440" : "-1920x1080") + ".png";
                        using (var bitmap = new Bitmap(showcase.Width, showcase.Height))
                        {
                            showcase.DrawToBitmap(bitmap, new Rectangle(Point.Empty, showcase.Size));
                            bitmap.Save(Path.Combine(directory, name), ImageFormat.Png);
                        }
                    }
                }
            }
            Console.WriteLine("captured to " + directory);
        }

        private static SquareButton NewButton(UiTheme theme, SquareButtonVariant variant, SquareButtonState? forced, int height)
        {
            return new SquareButton
            {
                Text = "버튼", Theme = theme, Variant = variant, ForcedState = forced,
                Size = new Size(120, height)
            };
        }

        // VS2022 테마 파일에서 읽은 값 몇 개를 그대로 대조한다.
        private static void CheckPalette()
        {
            var dark = new SquareButtonPalette(UiTheme.Dark);
            var light = new SquareButtonPalette(UiTheme.Light);
            Require(Same(dark.Get(SquareButtonVariant.Default, SquareButtonState.Normal).Back, 0x3d3d3d), "dark Button #3d3d3d");
            Require(Same(dark.Get(SquareButtonVariant.Default, SquareButtonState.Hover).Back, 0x4d4d4d), "dark ButtonHover #4d4d4d");
            Require(Same(dark.Get(SquareButtonVariant.Default, SquareButtonState.Focused).Border, 0x999999), "dark ButtonBorderFocused #999999");
            Require(Same(dark.Get(SquareButtonVariant.Primary, SquareButtonState.Normal).Back, 0x403582), "dark ButtonDefault #403582");
            Require(Same(light.Get(SquareButtonVariant.Default, SquareButtonState.Normal).Back, 0xececf0), "light Button #ececf0");
            Require(Same(light.Get(SquareButtonVariant.Default, SquareButtonState.Hover).Back, 0xc9def5), "light ButtonHover #c9def5");
            Require(Same(light.Get(SquareButtonVariant.Default, SquareButtonState.Pressed).Back, 0x006cbe), "light ButtonPressed #006cbe");
            Require(Same(light.Get(SquareButtonVariant.Primary, SquareButtonState.Normal).Border, 0x3399ff), "light ButtonBorderDefault #3399ff");
        }

        private static void CheckEdgePixels()
        {
            foreach (UiTheme theme in new[] { UiTheme.Dark, UiTheme.Light })
            {
                var palette = new SquareButtonPalette(theme);
                foreach (int height in new[] { 32, 40 })
                {
                    foreach (SquareButtonVariant variant in Enum.GetValues(typeof(SquareButtonVariant)))
                    {
                        foreach (SquareButtonState state in Enum.GetValues(typeof(SquareButtonState)))
                        {
                            // 부모를 눈에 띄는 마젠타로 두어, 가장자리에 부모 색이 비치는지 확인한다.
                            using (var parent = new Panel { BackColor = Color.Magenta, Size = new Size(160, height + 20) })
                            using (SquareButton button = NewButton(theme, variant, state, height))
                            {
                                button.Location = new Point(10, 10);
                                parent.Controls.Add(button);
                                using (var bitmap = new Bitmap(button.Width, button.Height))
                                {
                                    button.DrawToBitmap(bitmap, new Rectangle(Point.Empty, button.Size));
                                    Color border = palette.Get(variant, state).Border;
                                    foreach (Point corner in Corners(button.Size))
                                    {
                                        Color actual = bitmap.GetPixel(corner.X, corner.Y);
                                        Require(Same(actual, border), $"edge {theme}/{variant}/{state}/{height} at {corner}: {actual} expected {border}");
                                    }
                                    Color back = palette.Get(variant, state).Back;
                                    Color inner = bitmap.GetPixel(4, button.Height / 2);
                                    Require(Same(inner, back), $"inner {theme}/{variant}/{state}/{height}: {inner} expected {back}");
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void CheckNoRedundantInvalidate()
        {
            // 화면 밖 창은 WM_PAINT를 받지 못하므로 거의 투명하게 화면 안에 잠깐 띄워 Paint 횟수를 센다.
            using (var host = new Form { ShowInTaskbar = false, StartPosition = FormStartPosition.Manual, Location = new Point(0, 0), Opacity = 0.01, Size = new Size(240, 120) })
            {
                SquareButton button = NewButton(UiTheme.Dark, SquareButtonVariant.Default, null, 32);
                host.Controls.Add(button);
                host.Show();
                Application.DoEvents();
                button.Update();
                int paints = 0;
                button.Paint += delegate { paints++; };
                for (int i = 0; i < 100; i++)
                {
                    button.Variant = SquareButtonVariant.Default;
                    button.Theme = UiTheme.Dark;
                    button.TextAlign = ContentAlignment.MiddleCenter;
                    button.ForcedState = null;
                    button.Checked = false;
                }
                button.Update();
                Require(paints == 0, "same values must not repaint (" + paints + " paints)");
                button.Variant = SquareButtonVariant.Danger;
                button.Update();
                Require(paints == 1, "one change must paint exactly once (" + paints + " paints)");
            }
        }

        private static void CheckInput()
        {
            using (var host = new Form { ShowInTaskbar = false, StartPosition = FormStartPosition.Manual, Location = new Point(-20000, 0) })
            {
                SquareButton button = NewButton(UiTheme.Dark, SquareButtonVariant.Default, null, 32);
                host.Controls.Add(button);
                host.Show();
                Application.DoEvents();
                int clicks = 0;
                button.Click += delegate { clicks++; };
                button.PerformClick();
                Require(clicks == 1, "PerformClick raises Click once");
                button.Enabled = false;
                button.PerformClick();
                Require(clicks == 1, "disabled button does not click");
                Require(button.VisualState == SquareButtonState.Disabled, "disabled visual state");
                button.Enabled = true;
                Require(button.VisualState == SquareButtonState.Normal, "enabled returns to normal");
                button.ForcedState = SquareButtonState.Hover;
                Require(button.VisualState == SquareButtonState.Hover, "forced state wins");
                button.ForcedState = null;
            }
        }

        private static void CheckDialogResultAndToggle()
        {
            using (var form = new Form { ShowInTaskbar = false, StartPosition = FormStartPosition.Manual, Location = new Point(-20000, 0) })
            {
                SquareButton ok = NewButton(UiTheme.Dark, SquareButtonVariant.Primary, null, 32);
                ok.DialogResult = DialogResult.OK;
                form.Controls.Add(ok);
                form.AcceptButton = ok;
                form.Show();
                Application.DoEvents();
                Require(form.AcceptButton == ok, "AcceptButton accepts IButtonControl");
                ok.PerformClick();
                Require(form.DialogResult == DialogResult.OK, "DialogResult is propagated to the form");
            }
            using (var form = new Form { ShowInTaskbar = false, StartPosition = FormStartPosition.Manual, Location = new Point(-20000, 0) })
            {
                SquareButton toggle = NewButton(UiTheme.Light, SquareButtonVariant.Default, null, 32);
                toggle.IsToggle = true;
                form.Controls.Add(toggle);
                form.Show();
                Application.DoEvents();
                int changes = 0;
                toggle.CheckedChanged += delegate { changes++; };
                toggle.PerformClick();
                Require(toggle.Checked && changes == 1, "toggle turns on");
                Require(toggle.VisualState == SquareButtonState.Pressed, "checked toggle looks pressed");
                toggle.PerformClick();
                Require(!toggle.Checked && changes == 2, "toggle turns off");
            }
        }

        private static void CheckDesignMode()
        {
            using (var surface = new DesignSurface())
            {
                surface.BeginLoad(typeof(Form));
                var host = (IDesignerHost)surface.GetService(typeof(IDesignerHost));
                var form = (Form)host.RootComponent;
                form.ClientSize = new Size(400, 120);
                var button = (SquareButton)host.CreateComponent(typeof(SquareButton));
                form.Controls.Add(button);
                button.Location = new Point(10, 10);
                button.Text = "버튼";
                ((Control)surface.View).CreateControl();
                form.CreateControl();
                button.CreateControl();
                Application.DoEvents();
                Require(button.Site != null && button.Site.DesignMode, "button is sited in design mode");
                using (var bitmap = new Bitmap(button.Width, button.Height))
                {
                    button.DrawToBitmap(bitmap, new Rectangle(Point.Empty, button.Size));
                    var palette = new SquareButtonPalette(UiTheme.Dark);
                    Require(Same(bitmap.GetPixel(button.Width / 2, 3), palette.Get(SquareButtonVariant.Default, SquareButtonState.Normal).Back),
                        "design-mode render uses the theme color");
                }
            }
        }

        private static void CheckLeaks()
        {
            using (var host = new Form { ShowInTaskbar = false, StartPosition = FormStartPosition.Manual, Location = new Point(-20000, 0), Width = 400, Height = 200 })
            {
                SquareButton button = NewButton(UiTheme.Dark, SquareButtonVariant.Default, null, 32);
                host.Controls.Add(button);
                host.Show();
                Application.DoEvents();
                var random = new Random(3);
                Action iterate = () =>
                {
                    for (int i = 0; i < 1000; i++)
                    {
                        button.Theme = (UiTheme)random.Next(2);
                        button.Variant = (SquareButtonVariant)random.Next(3);
                        button.ForcedState = (SquareButtonState)random.Next(5);
                        button.Update();
                    }
                };
                for (int warm = 0; warm < 3; warm++) { iterate(); }
                GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
                using (Process process = Process.GetCurrentProcess())
                {
                    int gdiBefore = GetGuiResources(process.Handle, 0);
                    int userBefore = GetGuiResources(process.Handle, 1);
                    long memoryBefore = GC.GetTotalMemory(true);
                    for (int round = 0; round < 10; round++) { iterate(); }
                    GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
                    int gdiAfter = GetGuiResources(process.Handle, 0);
                    int userAfter = GetGuiResources(process.Handle, 1);
                    long memoryAfter = GC.GetTotalMemory(true);
                    Require(gdiAfter <= gdiBefore + 8, $"GDI handles {gdiBefore} -> {gdiAfter}");
                    Require(userAfter <= userBefore + 8, $"USER handles {userBefore} -> {userAfter}");
                    Require(memoryAfter - memoryBefore < 8L * 1024 * 1024, $"managed memory {memoryBefore} -> {memoryAfter}");
                    Console.WriteLine($"leak check: GDI {gdiBefore}->{gdiAfter}, USER {userBefore}->{userAfter}, memory {memoryBefore / 1024}KB->{memoryAfter / 1024}KB after 10,000 state changes");
                }
            }
        }

        private static Point[] Corners(Size size)
        {
            return new[] { new Point(0, 0), new Point(size.Width - 1, 0), new Point(0, size.Height - 1), new Point(size.Width - 1, size.Height - 1) };
        }

        private static bool Same(Color a, Color b) => a.R == b.R && a.G == b.G && a.B == b.B;
        private static bool Same(Color a, int rgb) => a.R == ((rgb >> 16) & 0xff) && a.G == ((rgb >> 8) & 0xff) && a.B == (rgb & 0xff);

        private static void Require(bool condition, string message)
        {
            if (!condition) { throw new InvalidOperationException("Failed: " + message); }
        }
    }
}
