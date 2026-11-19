using System;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using OmniSight.UI.Controls;

namespace OmniSight.UI.Demo
{
    /// <summary>RoiEditor의 좌표 변환, 이동·크기 조절 제한, 이벤트, 그리기, GDI 핸들 증가를 확인한다.</summary>
    internal static class RoiEditorChecks
    {
        [DllImport("user32.dll")]
        private static extern int GetGuiResources(IntPtr process, int kind);

        internal static void RunChecks()
        {
            CheckClampAndEvents();
            CheckCoordinates();
            CheckDrag();
            CheckPaintAndLeaks();
            Console.WriteLine("PASS: roi editor clamp, events, coordinates, drag limits, paint, leaks");
        }

        private static RoiEditor Create()
        {
            return new RoiEditor { Width = 640, Height = 480, ImageSize = new Size(640, 480), MinimumRoiSize = 10 };
        }

        private static void CheckClampAndEvents()
        {
            using (var editor = Create())
            {
                int events = 0;
                editor.RoiChanged += delegate { events++; };

                editor.Roi = new Rectangle(225, 42, 61, 57);
                Require(editor.Roi == new Rectangle(225, 42, 61, 57) && events == 1, "valid roi is kept and raises the event");
                editor.Roi = new Rectangle(225, 42, 61, 57);
                Require(events == 1, "same roi raises no event");

                editor.Roi = new Rectangle(-50, -50, 100, 100);
                Require(editor.Roi.Left == 0 && editor.Roi.Top == 0, "roi is moved inside the image");
                editor.Roi = new Rectangle(600, 450, 100, 100);
                Require(editor.Roi.Right == 640 && editor.Roi.Bottom == 480, "roi at the far corner is pulled back inside");
                editor.Roi = new Rectangle(10, 10, 1, 1);
                Require(editor.Roi.Width == 10 && editor.Roi.Height == 10, "tiny roi is enlarged to the minimum");
                editor.Roi = new Rectangle(0, 0, 5000, 5000);
                Require(editor.Roi.Width == 640 && editor.Roi.Height == 480, "huge roi is limited to the image");

                editor.Roi = new Rectangle(500, 400, 100, 60);
                editor.ImageSize = new Size(300, 200);
                Require(editor.Roi.Right <= 300 && editor.Roi.Bottom <= 200, "shrinking the image pulls the roi inside");

                RequireThrows(() => editor.ImageSize = new Size(0, 10), "zero image size is rejected");
                RequireThrows(() => editor.MinimumRoiSize = 0, "zero minimum size is rejected");
            }
        }

        private static void CheckCoordinates()
        {
            using (var editor = new RoiEditor { Width = 800, Height = 400, ImageSize = new Size(400, 200) })
            {
                Rectangle bounds = editor.ImageBounds;
                Require(bounds == new Rectangle(0, 0, 800, 400), "same aspect ratio fills the control");
                Require(editor.ToImage(new Point(400, 200)) == new Point(200, 100), "center maps to the image center");

                editor.ImageSize = new Size(400, 400);
                bounds = editor.ImageBounds;
                Require(bounds.Width == 400 && bounds.Height == 400 && bounds.Left == 200, "square image is centered with side margins");
                Require(editor.ToImage(new Point(0, 0)) == new Point(0, 0), "point outside the image is clamped to the edge");

                Rectangle onScreen = editor.ToControl(new Rectangle(100, 100, 100, 100));
                Require(onScreen == new Rectangle(300, 100, 100, 100), "image rectangle maps to the control");
            }
        }

        private static void CheckDrag()
        {
            using (var editor = Create())
            {
                editor.Roi = new Rectangle(100, 100, 200, 100);

                Drag(editor, new Point(200, 150), new Point(250, 170));
                Require(editor.Roi == new Rectangle(150, 120, 200, 100), "dragging inside moves the roi");

                Drag(editor, new Point(250, 170), new Point(5000, 5000));
                Require(editor.Roi.Right == 640 && editor.Roi.Bottom == 480 && editor.Roi.Size == new Size(200, 100), "move stops at the image edge without resizing");

                editor.Roi = new Rectangle(100, 100, 200, 100);
                Drag(editor, new Point(300, 200), new Point(350, 230));
                Require(editor.Roi == new Rectangle(100, 100, 250, 130), "bottom-right corner grows the roi");

                Drag(editor, new Point(350, 230), new Point(0, 0));
                Require(editor.Roi.Left == 100 && editor.Roi.Top == 100 && editor.Roi.Width == 10 && editor.Roi.Height == 10,
                    "corner cannot cross the opposite corner and keeps the minimum size");

                editor.Roi = new Rectangle(100, 100, 200, 100);
                Drag(editor, new Point(100, 150), new Point(-300, 150));
                Require(editor.Roi.Left == 0 && editor.Roi.Right == 300 && editor.Roi.Top == 100 && editor.Roi.Height == 100,
                    "left edge stops at the image edge and leaves other edges alone");

                Rectangle before = editor.Roi;
                Drag(editor, new Point(600, 20), new Point(620, 40));
                Require(editor.Roi == before, "dragging outside the roi changes nothing");
            }
        }

        private static void CheckPaintAndLeaks()
        {
            foreach (UiTheme theme in new[] { UiTheme.Dark, UiTheme.Light })
            {
                using (var editor = Create())
                using (var image = new Bitmap(editor.Width, editor.Height))
                {
                    editor.Theme = theme;
                    editor.Roi = new Rectangle(100, 100, 200, 100);
                    editor.DrawToBitmap(image, new Rectangle(0, 0, image.Width, image.Height));
                    CardPalette palette = CardPalette.For(theme);
                    Color edge = image.GetPixel(100, 150);
                    Require(Math.Abs(edge.R - palette.AccentText.R) + Math.Abs(edge.G - palette.AccentText.G) + Math.Abs(edge.B - palette.AccentText.B) <= 12,
                        theme + " roi border is drawn in the accent color");
                }
            }

            using (var form = new Form { ShowInTaskbar = false, StartPosition = FormStartPosition.Manual, Location = new Point(-32000, -32000) })
            {
                var editor = Create();
                form.Controls.Add(editor);
                form.ClientSize = editor.Size;
                form.Show();

                IntPtr process = System.Diagnostics.Process.GetCurrentProcess().Handle;
                int before = GetGuiResources(process, 0);
                for (int i = 0; i < 500; i++)
                {
                    editor.Theme = (i & 1) == 0 ? UiTheme.Dark : UiTheme.Light;
                    editor.Roi = new Rectangle(50 + i % 100, 50, 120, 90);
                    using (var image = new Bitmap(editor.Width, editor.Height))
                        editor.DrawToBitmap(image, new Rectangle(0, 0, image.Width, image.Height));
                }

                int after = GetGuiResources(process, 0);
                Require(after - before <= 5, "GDI objects stay flat after 500 cycles (" + before + " -> " + after + ")");
            }
        }

        private static void Drag(RoiEditor editor, Point from, Point to)
        {
            Invoke(editor, "OnMouseDown", new MouseEventArgs(MouseButtons.Left, 1, from.X, from.Y, 0));
            Invoke(editor, "OnMouseMove", new MouseEventArgs(MouseButtons.Left, 0, to.X, to.Y, 0));
            Invoke(editor, "OnMouseUp", new MouseEventArgs(MouseButtons.Left, 1, to.X, to.Y, 0));
        }

        private static void Invoke(RoiEditor editor, string method, MouseEventArgs args)
        {
            typeof(RoiEditor).GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance).Invoke(editor, new object[] { args });
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
