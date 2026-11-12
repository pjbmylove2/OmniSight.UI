using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
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
            CheckChipRendering();
            CheckNoRedundantInvalidate();
            CheckRegistryDemotion();
            CheckThreadSafety();
            CheckDesignMode();
            CheckLeaks();
            Console.WriteLine("PASS: chip rendering (6 states x 2 themes), redundant invalidate, registry demotion, thread safety, design mode, leaks");
        }

        public static void RunCapture(string directory)
        {
            Directory.CreateDirectory(directory);
            foreach (bool large in new[] { false, true })
            {
                using (var host = new Panel { Size = new Size(large ? 2560 : 1920, 0) })
                {
                    var dark = new DemoSection(DeviceStatusTheme.Dark, large);
                    var light = new DemoSection(DeviceStatusTheme.Light, large);
                    host.Controls.Add(light);
                    host.Controls.Add(dark);
                    host.Height = dark.Height + light.Height;
                    Capture(host, Path.Combine(directory, large ? "DeviceStatusStrip-2560x1440.png" : "DeviceStatusStrip-1920x1080.png"));
                    if (!large)
                    {
                        PrimeHistory(dark.Strip);
                        PrimeHistory(light.Strip);
                        CapturePopup(dark.Strip, Path.Combine(directory, "DeviceStatusPopup-dark.png"));
                        CapturePopup(light.Strip, Path.Combine(directory, "DeviceStatusPopup-light.png"));
                    }
                }
            }
            Console.WriteLine("captured to " + directory);
        }

        // 팝업 캡처용으로 CAM 2에 상태 변경 이력을 남긴다(시각을 직접 지정해 결과가 일정하다).
        private static void PrimeHistory(DeviceStatusStrip strip)
        {
            DateTime t = DateTime.UtcNow.AddSeconds(-40);
            strip.Registry.Report("cam2", DeviceLinkState.Connecting, null, null, t);
            strip.Registry.Report("cam2", DeviceLinkState.Connected, null, 180, t.AddSeconds(8));
            strip.Registry.Report("cam2", DeviceLinkState.Lost, "응답 없음", null, t.AddSeconds(20));
            strip.Registry.Report("cam2", DeviceLinkState.Connecting, null, null, t.AddSeconds(26));
            strip.Registry.Report("cam2", DeviceLinkState.Connected, null, 240, t.AddSeconds(30));
            strip.Registry.Report("cam2", DeviceLinkState.Warning, "응답 지연", 420, DateTime.UtcNow.AddSeconds(-2));
            strip.RefreshStatus();
        }

        private static void CapturePopup(DeviceStatusStrip strip, string path)
        {
            strip.ShowDetail("cam2");
            Application.DoEvents();
            DeviceStatusPopup popup = Application.OpenForms.OfType<DeviceStatusPopup>().First();
            using (var bitmap = new Bitmap(popup.Width, popup.Height))
            {
                popup.DrawToBitmap(bitmap, new Rectangle(Point.Empty, popup.Size));
                bitmap.Save(path, ImageFormat.Png);
            }
            strip.CloseDetail();
            Application.DoEvents();
        }

        private static void Capture(Control control, string path)
        {
            using (var bitmap = new Bitmap(control.Width, control.Height))
            {
                control.DrawToBitmap(bitmap, new Rectangle(Point.Empty, control.Size));
                bitmap.Save(path, ImageFormat.Png);
            }
        }

        private static DeviceStatusChip NewChip(DeviceStatusTheme theme, DeviceLinkState state, int height)
        {
            var chip = new DeviceStatusChip
            {
                DeviceName = "CAM 1",
                Palette = new DeviceStatusPalette(theme),
                State = state,
                Height = height
            };
            chip.Width = chip.GetPreferredSize(Size.Empty).Width;
            return chip;
        }

        private static void CheckChipRendering()
        {
            foreach (DeviceStatusTheme theme in new[] { DeviceStatusTheme.Dark, DeviceStatusTheme.Light })
            {
                var palette = new DeviceStatusPalette(theme);
                foreach (int height in new[] { 28, 36 })
                {
                    foreach (DeviceLinkState state in Enum.GetValues(typeof(DeviceLinkState)))
                    {
                        using (DeviceStatusChip chip = NewChip(theme, state, height))
                        using (var bitmap = new Bitmap(chip.Width, chip.Height))
                        {
                            chip.DrawToBitmap(bitmap, new Rectangle(Point.Empty, chip.Size));
                            Color border = state == DeviceLinkState.Fault || state == DeviceLinkState.Lost ? palette.Error : palette.Edge;
                            foreach (Point corner in new[] { new Point(0, 0), new Point(chip.Width - 1, 0), new Point(0, chip.Height - 1), new Point(chip.Width - 1, chip.Height - 1) })
                            {
                                Require(Same(bitmap.GetPixel(corner.X, corner.Y), border),
                                    $"chip corner {theme}/{state}/{height} at {corner}: {bitmap.GetPixel(corner.X, corner.Y)}");
                            }

                            int d = Math.Max(7, (int)(height * 0.32f));
                            int cx = Math.Max(8, (int)(height * 0.33f)) + d / 2;
                            Color center = bitmap.GetPixel(cx, height / 2);
                            Color expected;
                            switch (state)
                            {
                                case DeviceLinkState.Connected: expected = palette.Ok; break;
                                case DeviceLinkState.Warning: expected = palette.WarnLamp; break;
                                case DeviceLinkState.Fault:
                                case DeviceLinkState.Lost: expected = palette.Error; break;
                                default: expected = palette.Surface; break;
                            }
                            Require(Same(center, expected), $"lamp center {theme}/{state}/{height}: {center} expected {expected}");
                        }
                    }
                }
            }
        }

        private static void CheckNoRedundantInvalidate()
        {
            // 화면 밖의 창은 WM_PAINT를 받지 못하므로, 거의 투명하게 화면 안에 잠깐 띄워서 Paint 횟수를 센다.
            using (var host = new Form { ShowInTaskbar = false, StartPosition = FormStartPosition.Manual, Location = new Point(0, 0), Opacity = 0.01, Size = new Size(200, 100) })
            {
                DeviceStatusChip chip = NewChip(DeviceStatusTheme.Dark, DeviceLinkState.Connected, 28);
                host.Controls.Add(chip);
                host.Show();
                Application.DoEvents();
                chip.Update();
                int paints = 0;
                chip.Paint += delegate { paints++; };
                for (int i = 0; i < 100; i++) { chip.State = DeviceLinkState.Connected; chip.Selected = false; chip.DeviceName = "CAM 1"; }
                chip.Update();
                Require(paints == 0, "same values must not repaint (" + paints + " paints)");
                chip.State = DeviceLinkState.Fault;
                chip.Update();
                Require(paints == 1, "one state change must paint exactly once (" + paints + " paints)");
            }
        }

        private static void CheckRegistryDemotion()
        {
            var registry = new DeviceStatusRegistry();
            DateTime t = new DateTime(2026, 11, 12, 9, 0, 0, DateTimeKind.Utc);
            registry.Report("a", DeviceLinkState.Connected, null, 100, t);
            registry.Evaluate(t.AddSeconds(1));
            Require(registry.GetInfo("a").State == DeviceLinkState.Connected, "fresh response stays connected");
            registry.Evaluate(t.AddSeconds(4));
            Require(registry.GetInfo("a").State == DeviceLinkState.Warning, "3s without response becomes warning");
            registry.Evaluate(t.AddSeconds(11));
            Require(registry.GetInfo("a").State == DeviceLinkState.Lost, "10s without response becomes lost");
            Require(registry.GetInfo("a").ErrorCount == 1, "lost counts as one error");
            registry.Heartbeat("a", 90, t.AddSeconds(12));
            registry.Evaluate(t.AddSeconds(12.1));
            Require(registry.GetInfo("a").State == DeviceLinkState.Lost, "recovery is held back for the promote time");
            registry.Heartbeat("a", 90, t.AddSeconds(12.6));
            registry.Evaluate(t.AddSeconds(12.7));
            Require(registry.GetInfo("a").State == DeviceLinkState.Connected, "stable responses promote to connected");
            Require(registry.GetHistory("a").Count == 4, "history keeps Connected, Warning, Lost, Connected (" + registry.GetHistory("a").Count + ")");
            for (int i = 0; i < 200; i++)
            {
                registry.Report("b", i % 2 == 0 ? DeviceLinkState.Fault : DeviceLinkState.Connected, null, null, t.AddSeconds(100 + i));
                registry.Evaluate(t.AddSeconds(100 + i));
            }
            Require(registry.GetHistory("b").Count <= DeviceStatusRegistry.HistoryCapacity, "history is capped");
        }

        private static void CheckThreadSafety()
        {
            using (var host = new Panel())
            {
                var strip = new DeviceStatusStrip();
                host.Controls.Add(strip);
                foreach (string id in DemoSection.DeviceIds) { strip.AddDevice(id, id); }
                var failures = new System.Collections.Concurrent.ConcurrentQueue<Exception>();
                Task[] writers = Enumerable.Range(0, 4).Select(n => Task.Run(() =>
                {
                    var random = new Random(n);
                    try
                    {
                        for (int i = 0; i < 5000; i++)
                        {
                            string id = DemoSection.DeviceIds[random.Next(DemoSection.DeviceIds.Length)];
                            if (i % 3 == 0) { strip.Registry.Heartbeat(id, random.Next(500)); }
                            else { strip.Registry.Report(id, (DeviceLinkState)random.Next(6), "t", random.Next(500)); }
                        }
                    }
                    catch (Exception error) { failures.Enqueue(error); }
                })).ToArray();
                while (!Task.WaitAll(writers, 5)) { strip.RefreshStatus(); }
                strip.RefreshStatus();
                Exception first;
                Require(!failures.TryDequeue(out first), "concurrent Report/Heartbeat: " + first);
                strip.Dispose();
            }
        }

        private static void CheckDesignMode()
        {
            using (var surface = new DesignSurface())
            {
                surface.BeginLoad(typeof(Form));
                var host = (IDesignerHost)surface.GetService(typeof(IDesignerHost));
                var form = (Form)host.RootComponent;
                form.ClientSize = new Size(900, 120);
                var strip = (DeviceStatusStrip)host.CreateComponent(typeof(DeviceStatusStrip));
                form.Controls.Add(strip);
                strip.Location = new Point(10, 10);
                var view = (Control)surface.View;
                view.CreateControl();
                form.CreateControl();
                strip.CreateControl();
                Application.DoEvents();
                Require(strip.Site != null && strip.Site.DesignMode, "strip is sited in design mode");
                Require(strip.Controls.Count == 6, "design mode shows 6 sample chips (" + strip.Controls.Count + ")");
                Require(strip.Width > 200 && strip.Height == strip.ChipHeight, "design-mode strip sizes itself to the samples");
                using (var bitmap = new Bitmap(strip.Width, strip.Height))
                {
                    strip.DrawToBitmap(bitmap, new Rectangle(Point.Empty, strip.Size));
                    int distinct = 0;
                    Color first = bitmap.GetPixel(strip.Width / 2, strip.Height / 2);
                    for (int x = 0; x < bitmap.Width; x += 3)
                    {
                        if (!Same(bitmap.GetPixel(x, bitmap.Height / 2), first)) { distinct++; }
                    }
                    Require(distinct > 10, "design-mode render is not blank");
                }
            }
        }

        private static void CheckLeaks()
        {
            using (var host = new Form { ShowInTaskbar = false, StartPosition = FormStartPosition.Manual, Location = new Point(-20000, 0), Width = 1200, Height = 200 })
            {
                var strip = new DeviceStatusStrip { Location = new Point(10, 10) };
                host.Controls.Add(strip);
                foreach (string id in DemoSection.DeviceIds) { strip.AddDevice(id, id); }
                host.Show();
                Application.DoEvents();
                var random = new Random(7);
                Action iterate = () =>
                {
                    for (int i = 0; i < 500; i++)
                    {
                        string id = DemoSection.DeviceIds[random.Next(DemoSection.DeviceIds.Length)];
                        strip.Registry.Report(id, (DeviceLinkState)random.Next(6), null, random.Next(500));
                        if (i % 5 == 0) { strip.Theme = strip.Theme == DeviceStatusTheme.Dark ? DeviceStatusTheme.Light : DeviceStatusTheme.Dark; }
                        strip.RefreshStatus();
                        host.Update();
                        if (i % 50 == 0)
                        {
                            strip.ShowDetail(DemoSection.DeviceIds[random.Next(DemoSection.DeviceIds.Length)]);
                            Application.DoEvents();
                            strip.CloseDetail();
                            Application.DoEvents();
                        }
                    }
                };
                for (int warm = 0; warm < 3; warm++) { iterate(); }
                GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
                using (Process process = Process.GetCurrentProcess())
                {
                    int gdiBefore = GetGuiResources(process.Handle, 0);
                    int userBefore = GetGuiResources(process.Handle, 1);
                    long memoryBefore = GC.GetTotalMemory(true);
                    for (int round = 0; round < 40; round++) { iterate(); }
                    GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
                    int gdiAfter = GetGuiResources(process.Handle, 0);
                    int userAfter = GetGuiResources(process.Handle, 1);
                    long memoryAfter = GC.GetTotalMemory(true);
                    Require(gdiAfter <= gdiBefore + 8, $"GDI handles {gdiBefore} -> {gdiAfter}");
                    Require(userAfter <= userBefore + 8, $"USER handles {userBefore} -> {userAfter}");
                    Require(memoryAfter - memoryBefore < 8L * 1024 * 1024, $"managed memory {memoryBefore} -> {memoryAfter}");
                    Console.WriteLine($"leak check: GDI {gdiBefore}->{gdiAfter}, USER {userBefore}->{userAfter}, memory {memoryBefore / 1024}KB->{memoryAfter / 1024}KB after 20,000 updates and 400 popups");
                }
            }
        }

        private static bool Same(Color a, Color b) => a.R == b.R && a.G == b.G && a.B == b.B;

        private static void Require(bool condition, string message)
        {
            if (!condition) { throw new InvalidOperationException("Failed: " + message); }
        }
    }
}
