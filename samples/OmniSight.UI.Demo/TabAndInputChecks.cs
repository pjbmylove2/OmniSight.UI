using System;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using OmniSight.UI.Controls;

namespace OmniSight.UI.Demo
{
    /// <summary>TabBar와 ThemedTextBox, ThemedComboBox의 선택·키 입력·테마·GDI 핸들 증가를 확인한다.</summary>
    internal static class TabAndInputChecks
    {
        [DllImport("user32.dll")]
        private static extern int GetGuiResources(IntPtr process, int kind);

        internal static void RunChecks()
        {
            CheckTabSelection();
            CheckTabKeys();
            CheckTabPaint();
            CheckInputThemes();
            CheckLeaks();
            Console.WriteLine("PASS: tab bar selection, keys, paint, themed inputs, leaks");
        }

        private static void CheckTabSelection()
        {
            using (var tabs = new TabBar { Width = 800 })
            {
                int changes = 0;
                tabs.SelectedIndexChanged += delegate { changes++; };

                Require(tabs.SelectedIndex == -1 && tabs.Count == 0, "empty bar has no selection");
                tabs.SetTabs(new[] { "검사 운전", "수동 점검", "검사 설정", "검사 이력", "시스템 설정" });
                Require(tabs.Count == 5 && tabs.SelectedIndex == 0 && changes == 1, "first tab is selected when tabs appear");

                tabs.SelectedIndex = 2;
                tabs.SelectedIndex = 2;
                Require(tabs.SelectedIndex == 2 && changes == 2, "same selection raises no extra event");

                int width = (int)Math.Round(tabs.TabWidth * tabs.DeviceDpi / 96f);
                Require(tabs.HitTest(new Point(width * 3 + 5, 10)) == 3, "hit test finds the fourth tab");
                Require(tabs.HitTest(new Point(width * 5 + 5, 10)) == -1, "hit test misses beyond the last tab");
                Require(tabs.HitTest(new Point(-1, 10)) == -1, "hit test misses left of the bar");

                tabs.SetTabs(new[] { "A", "B" });
                Require(tabs.SelectedIndex == 1, "selection is clamped when tabs shrink");
                tabs.SetTabs(new string[0]);
                Require(tabs.SelectedIndex == -1, "selection clears when tabs are removed");

                RequireThrows(() => tabs.SelectedIndex = 3, "out of range selection is rejected");
                RequireThrows(() => tabs.SetTabs(new string[] { null }), "null title is rejected");
                RequireThrows(() => tabs.TabWidth = 0, "zero tab width is rejected");
            }
        }

        private static void CheckTabKeys()
        {
            using (var tabs = new TabBar())
            {
                tabs.SetTabs(new[] { "A", "B", "C" });
                Press(tabs, Keys.Right);
                Press(tabs, Keys.Right);
                Press(tabs, Keys.Right);
                Require(tabs.SelectedIndex == 2, "right arrow stops at the last tab");
                Press(tabs, Keys.Home);
                Require(tabs.SelectedIndex == 0, "Home selects the first tab");
                Press(tabs, Keys.Left);
                Require(tabs.SelectedIndex == 0, "left arrow stops at the first tab");
                Press(tabs, Keys.End);
                Require(tabs.SelectedIndex == 2, "End selects the last tab");
            }
        }

        private static void CheckTabPaint()
        {
            foreach (UiTheme theme in new[] { UiTheme.Dark, UiTheme.Light })
            {
                using (var tabs = new TabBar { Width = 800, Theme = theme })
                {
                    tabs.SetTabs(new[] { "검사 운전", "수동 점검" });
                    CardPalette palette = CardPalette.For(theme);
                    using (var image = new Bitmap(tabs.Width, tabs.Height))
                    {
                        tabs.DrawToBitmap(image, new Rectangle(0, 0, tabs.Width, tabs.Height));
                        Require(image.GetPixel(700, 2).ToArgb() == palette.Surface.ToArgb(), theme + " bar background");
                        Require(image.GetPixel(30, tabs.Height / 2 - 12).ToArgb() == palette.AccentFill.ToArgb()
                            || image.GetPixel(10, tabs.Height / 2).ToArgb() == palette.AccentFill.ToArgb(), theme + " selected tab fill");
                    }
                }
            }
        }

        private static void CheckInputThemes()
        {
            using (var box = new ThemedTextBox())
            using (var combo = new ThemedComboBox())
            {
                foreach (UiTheme theme in new[] { UiTheme.Dark, UiTheme.Light })
                {
                    box.Theme = theme;
                    combo.Theme = theme;
                    CardPalette palette = CardPalette.For(theme);
                    Require(box.BackColor.ToArgb() == palette.Input.ToArgb() && box.ForeColor.ToArgb() == palette.Text.ToArgb(), theme + " text box colors");
                    Require(combo.BackColor.ToArgb() == palette.Input.ToArgb() && combo.ForeColor.ToArgb() == palette.Text.ToArgb(), theme + " combo colors");
                }

                Require(combo.DrawMode == DrawMode.OwnerDrawFixed && combo.DropDownStyle == ComboBoxStyle.DropDownList, "combo box defaults");
            }
        }

        private static void CheckLeaks()
        {
            using (var form = new Form { ShowInTaskbar = false, StartPosition = FormStartPosition.Manual, Location = new Point(-32000, -32000) })
            {
                var tabs = new TabBar { Left = 0, Top = 0, Width = 600 };
                var combo = new ThemedComboBox { Left = 0, Top = 60, Width = 160 };
                combo.Items.AddRange(new object[] { "우측 터미널", "좌측 터미널" });
                var box = new ThemedTextBox { Left = 180, Top = 60, Width = 120 };
                form.Controls.Add(tabs);
                form.Controls.Add(combo);
                form.Controls.Add(box);
                tabs.SetTabs(new[] { "A", "B", "C" });
                form.Show();

                IntPtr process = System.Diagnostics.Process.GetCurrentProcess().Handle;
                int before = GetGuiResources(process, 0);
                for (int i = 0; i < 500; i++)
                {
                    UiTheme theme = (i & 1) == 0 ? UiTheme.Dark : UiTheme.Light;
                    tabs.Theme = theme;
                    combo.Theme = theme;
                    box.Theme = theme;
                    tabs.SelectedIndex = i % 3;
                    using (var image = new Bitmap(form.ClientSize.Width, form.ClientSize.Height))
                        form.DrawToBitmap(image, new Rectangle(0, 0, image.Width, image.Height));
                }

                int after = GetGuiResources(process, 0);
                Require(after - before <= 5, "GDI objects stay flat after 500 cycles (" + before + " -> " + after + ")");
            }
        }

        private static void Press(TabBar tabs, Keys key)
        {
            MethodInfo onKeyDown = typeof(TabBar).GetMethod("OnKeyDown", BindingFlags.NonPublic | BindingFlags.Instance);
            onKeyDown.Invoke(tabs, new object[] { new KeyEventArgs(key) });
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
