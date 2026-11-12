using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using OmniSight.UI.Controls.Vision;

namespace OmniSight.UI.Demo
{
    /// <summary>
    /// ImageViewer 데모: 모의 카메라 스레드가 영상을 만들어 라이브 뷰어에 넣고,
    /// 같은 영상에 불량 박스와 OK/NG를 붙여 결과 뷰어에 넣는다. OpenCV 없이 원시 버퍼만 쓴다.
    /// </summary>
    internal sealed unsafe class ViewerDemoForm : Form
    {
        private const int Width0 = 2048;
        private const int Height0 = 1536;

        private readonly ImageViewer _live = new ImageViewer(new ImageViewerOptions { BackColor = Color.FromArgb(30, 30, 30) });
        private readonly ImageViewer _result = new ImageViewer(new ImageViewerOptions { BackColor = Color.FromArgb(30, 30, 30), InspectionMode = true });
        private readonly Label _status = new Label { Dock = DockStyle.Bottom, Height = 28, ForeColor = Color.White, BackColor = Color.FromArgb(45, 45, 45), TextAlign = ContentAlignment.MiddleLeft };
        private readonly System.Windows.Forms.Timer _statusTimer = new System.Windows.Forms.Timer { Interval = 500 };
        private Thread _camera;
        private volatile bool _stop;

        public ViewerDemoForm()
        {
            Text = "OmniSight.UI ImageViewer Demo";
            ClientSize = new Size(1200, 520);
            BackColor = Color.FromArgb(45, 45, 45);

            var split = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 600 };
            _live.Dock = DockStyle.Fill;
            _result.Dock = DockStyle.Fill;
            split.Panel1.Controls.Add(_live);
            split.Panel2.Controls.Add(_result);
            Controls.Add(split);
            Controls.Add(_status);

            _live.PixelHover += (s, e) => _status.Text = e.IsInsideImage ? "(" + e.X + ", " + e.Y + ") = " + e.Value : "";
            _statusTimer.Tick += delegate
            {
                ViewerStatistics s = _live.Statistics;
                _status.Text = "라이브 받음 " + s.Received + " / 표시 " + s.Displayed + " / 건너뜀 " + s.Skipped
                    + "  (복사 " + s.AverageCopyMs.ToString("0.00") + "ms, 그리기 " + s.AveragePaintMs.ToString("0.00") + "ms)";
            };

            Load += delegate
            {
                _statusTimer.Start();
                _camera = new Thread(CameraLoop) { IsBackground = true, Name = "MockCamera" };
                _camera.Start();
            };
            FormClosing += delegate
            {
                _stop = true;
                _statusTimer.Stop();
                if (_camera != null)
                {
                    _camera.Join(2000);
                }
            };
        }

        /// <summary>카메라 스레드: 30fps로 영상을 만들어 SetImage. 어느 스레드에서 불러도 된다.</summary>
        private void CameraLoop()
        {
            int stride = Width0;
            IntPtr buffer = Marshal.AllocHGlobal(new IntPtr((long)stride * Height0));
            try
            {
                for (int frame = 0; !_stop; frame++)
                {
                    Fill((byte*)buffer, stride, frame);
                    _live.SetImage(buffer, Width0, Height0, stride, ViewerPixelFormat.Mono8);

                    bool ng = frame % 10 >= 7;
                    var overlays = new List<OverlayItem>
                    {
                        OverlayItem.Label(ng ? "NG" : "OK", ng ? Color.Red : Color.Lime, ContentAlignment.TopLeft)
                    };
                    if (ng)
                    {
                        overlays.Add(OverlayItem.Rect(new RectangleF(400 + frame % 7 * 40, 500, 220, 160), Color.Red, 3F));
                        overlays.Add(OverlayItem.Text(new PointF(400 + frame % 7 * 40, 480), "불량", Color.Yellow));
                    }
                    _result.SetImage(buffer, Width0, Height0, stride, ViewerPixelFormat.Mono8, overlays);

                    Thread.Sleep(33);   // SetImage 복사 후 바로 버퍼를 다음 프레임에 재사용해도 안전하다.
                }
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        private static void Fill(byte* p, int stride, int frame)
        {
            for (int y = 0; y < Height0; y++)
            {
                byte* row = p + (long)y * stride;
                for (int x = 0; x < Width0; x++)
                {
                    row[x] = (byte)(((x + frame * 4) >> 3) ^ (y >> 3));
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _statusTimer.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
