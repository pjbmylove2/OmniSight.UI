using System;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using OmniSight.UI.Controls;

namespace OmniSight.UI.Demo
{
    /// <summary>ItemList의 머리글 건너뛰기, 선택 유지, 키 입력, 그리기, GDI 핸들 증가를 확인한다.</summary>
    internal static class ItemListChecks
    {
        private static readonly string[] Texts =
        {
            "공통", "카메라 1 제품 유무", "카메라 2 제품 유무",
            "CAM 1 · 터미널 정면", "터미널 높이", "터미널 폭",
            "CAM 2 · 터미널 상부", "홈이탈 AI"
        };

        [DllImport("user32.dll")]
        private static extern int GetGuiResources(IntPtr process, int kind);

        internal static void RunChecks()
        {
            CheckSelection();
            CheckKeysAndHitTest();
            CheckPaint();
            CheckLeaks();
            Console.WriteLine("PASS: item list selection, headers, keys, paint, leaks");
        }

        private static ItemList Create(UiTheme theme)
        {
            var list = new ItemList { Width = 220, Height = 400, Theme = theme };
            list.SetEntries(Texts, IsHeader);
            return list;
        }

        private static bool IsHeader(string text)
        {
            return text == "공통" || text.StartsWith("CAM ");
        }

        private static void CheckSelection()
        {
            using (var list = new ItemList())
            {
                int changes = 0;
                list.SelectedIndexChanged += delegate { changes++; };
                Require(list.SelectedIndex == -1 && list.SelectedText == null, "empty list has no selection");

                list.SetEntries(Texts, IsHeader);
                Require(list.SelectedIndex == 1 && changes == 1, "first item (not the header) is selected");

                list.SelectedIndex = 4;
                list.SelectedIndex = 4;
                Require(list.SelectedText == "터미널 높이" && changes == 2, "same selection raises no extra event");

                list.SetEntries(Texts, IsHeader);
                Require(list.SelectedText == "터미널 높이", "selection survives a refresh with the same items");

                list.SetEntries(new[] { "공통", "다른 항목" }, IsHeader);
                Require(list.SelectedText == "다른 항목", "selection falls back to the first item");

                RequireThrows(() => list.SelectedIndex = 0, "selecting a header is rejected");
                RequireThrows(() => list.SelectedIndex = 9, "out of range selection is rejected");
                RequireThrows(() => list.SetEntries(new string[] { null }, IsHeader), "null text is rejected");
            }
        }

        private static void CheckKeysAndHitTest()
        {
            using (var list = Create(UiTheme.Dark))
            {
                Press(list, Keys.Down);
                Require(list.SelectedIndex == 2, "down moves to the next item");
                Press(list, Keys.Down);
                Require(list.SelectedIndex == 4, "down skips the header");
                Press(list, Keys.End);
                Require(list.SelectedIndex == 7, "End selects the last item");
                Press(list, Keys.Down);
                Require(list.SelectedIndex == 7, "down stops at the last item");
                Press(list, Keys.Home);
                Require(list.SelectedIndex == 1, "Home selects the first item");
                Press(list, Keys.Up);
                Require(list.SelectedIndex == 1, "up stops at the first item");

                Require(list.HitTest(new Point(5, 2)) == 0, "hit test finds the header");
                Require(list.HitTest(new Point(5, list.PreferredHeight + 5)) == -1, "hit test misses below the last row");
                Require(list.HitTest(new Point(-1, 5)) == -1, "hit test misses left of the list");
            }
        }

        private static void CheckPaint()
        {
            foreach (UiTheme theme in new[] { UiTheme.Dark, UiTheme.Light })
            {
                using (var list = Create(theme))
                using (var image = new Bitmap(list.Width, list.Height))
                {
                    CardPalette palette = CardPalette.For(theme);
                    list.SelectedIndex = 1;
                    list.DrawToBitmap(image, new Rectangle(0, 0, list.Width, list.Height));
                    int selectedRowY = 26 + 19;
                    Require(image.GetPixel(list.Width - 6, selectedRowY).ToArgb() == palette.AccentFill.ToArgb(), theme + " selected row fill");
                    int otherRowY = 26 + 38 + 19;
                    Require(image.GetPixel(list.Width - 6, otherRowY).ToArgb() == palette.Input.ToArgb(), theme + " unselected row fill");
                }
            }
        }

        private static void CheckLeaks()
        {
            using (var form = new Form { ShowInTaskbar = false, StartPosition = FormStartPosition.Manual, Location = new Point(-32000, -32000) })
            {
                var list = Create(UiTheme.Dark);
                form.Controls.Add(list);
                form.ClientSize = new Size(220, 400);
                form.Show();

                IntPtr process = System.Diagnostics.Process.GetCurrentProcess().Handle;
                int before = GetGuiResources(process, 0);
                for (int i = 0; i < 500; i++)
                {
                    list.Theme = (i & 1) == 0 ? UiTheme.Dark : UiTheme.Light;
                    list.SelectedIndex = i % 2 == 0 ? 1 : 4;
                    using (var image = new Bitmap(list.Width, list.Height))
                        list.DrawToBitmap(image, new Rectangle(0, 0, list.Width, list.Height));
                }

                int after = GetGuiResources(process, 0);
                Require(after - before <= 5, "GDI objects stay flat after 500 cycles (" + before + " -> " + after + ")");
            }
        }

        private static void Press(ItemList list, Keys key)
        {
            MethodInfo onKeyDown = typeof(ItemList).GetMethod("OnKeyDown", BindingFlags.NonPublic | BindingFlags.Instance);
            onKeyDown.Invoke(list, new object[] { new KeyEventArgs(key) });
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
