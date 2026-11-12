using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace OmniSight.UI.Controls
{
    /// <summary>램프를 그릴 때 쓰는 브러시·펜 묶음. 팔레트가 바뀔 때만 다시 만들고 소유자가 Dispose한다.</summary>
    internal sealed class LampResources : IDisposable
    {
        public readonly SolidBrush Ok, Warn, Error;
        public readonly Pen Muted, Track, Accent, ErrorRing;

        public LampResources(DeviceStatusPalette palette)
        {
            Ok = new SolidBrush(palette.Ok);
            Warn = new SolidBrush(palette.WarnLamp);
            Error = new SolidBrush(palette.Error);
            Muted = new Pen(palette.Muted, 1.5f);
            Track = new Pen(palette.Track, 2f);
            Accent = new Pen(palette.Accent, 2f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
            ErrorRing = new Pen(palette.Error, 1.5f);
        }

        public void Dispose()
        {
            Ok.Dispose(); Warn.Dispose(); Error.Dispose();
            Muted.Dispose(); Track.Dispose(); Accent.Dispose(); ErrorRing.Dispose();
        }
    }

    internal static class LampPainter
    {
        public static void Draw(Graphics g, Rectangle bounds, DeviceLinkState state, int angle, LampResources res)
        {
            SmoothingMode previous = g.SmoothingMode;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var ring = new RectangleF(bounds.X + 1f, bounds.Y + 1f, bounds.Width - 2f, bounds.Height - 2f);
            switch (state)
            {
                case DeviceLinkState.Disabled:
                    g.DrawEllipse(res.Muted, ring);
                    break;
                case DeviceLinkState.Connecting:
                    g.DrawEllipse(res.Track, ring);
                    g.DrawArc(res.Accent, ring, angle, 100f);
                    break;
                case DeviceLinkState.Connected:
                    g.FillEllipse(res.Ok, bounds);
                    break;
                case DeviceLinkState.Warning:
                    g.FillEllipse(res.Warn, bounds);
                    break;
                case DeviceLinkState.Fault:
                    g.FillEllipse(res.Error, bounds);
                    break;
                case DeviceLinkState.Lost:
                    g.DrawEllipse(res.ErrorRing, new RectangleF(bounds.X + .75f, bounds.Y + .75f, bounds.Width - 1.5f, bounds.Height - 1.5f));
                    g.FillEllipse(res.Error, new RectangleF(bounds.X + 3f, bounds.Y + 3f, bounds.Width - 6f, bounds.Height - 6f));
                    break;
            }
            g.SmoothingMode = previous;
        }
    }

    /// <summary>팝업 머리글의 램프. 칩과 같은 램프 그리기를 쓴다.</summary>
    internal sealed class DeviceStatusLamp : Control
    {
        private LampResources _resources;
        private DeviceLinkState _state;
        private Color _back;

        public DeviceStatusLamp()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.Selectable, false);
            TabStop = false;
        }

        public void Apply(DeviceStatusPalette palette, DeviceLinkState state)
        {
            bool paletteChanged = _resources == null || _back != palette.Popup;
            if (paletteChanged)
            {
                if (_resources != null) { _resources.Dispose(); }
                _resources = new LampResources(palette);
                _back = palette.Popup;
                BackColor = palette.Popup;
            }
            if (paletteChanged || _state != state)
            {
                _state = state;
                Invalidate();
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            using (var brush = new SolidBrush(_back)) { e.Graphics.FillRectangle(brush, ClientRectangle); }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (_resources == null) { return; }
            int d = Math.Min(Width, Height);
            LampPainter.Draw(e.Graphics, new Rectangle((Width - d) / 2, (Height - d) / 2, d, d), _state, 270, _resources);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _resources != null) { _resources.Dispose(); _resources = null; }
            base.Dispose(disposing);
        }
    }
}
