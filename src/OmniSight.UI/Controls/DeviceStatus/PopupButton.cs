using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace OmniSight.UI.Controls
{
    /// <summary>팝업용 각진 버튼. 전체 영역을 직접 채워 시스템 색이 가장자리에 비치지 않는다.</summary>
    [ToolboxItem(false)]
    [DesignerCategory("Code")]
    internal sealed class PopupButton : Control
    {
        private bool _hover;
        private bool _pressed;
        private DeviceStatusPalette _palette = new DeviceStatusPalette(DeviceStatusTheme.Dark);
        private readonly SolidBrush _back = new SolidBrush(Color.Black);
        private readonly Pen _border = new Pen(Color.Black);

        public PopupButton()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
            Cursor = Cursors.Hand;
            AccessibleRole = AccessibleRole.PushButton;
        }

        public void Apply(DeviceStatusPalette palette)
        {
            _palette = palette;
            Invalidate();
        }

        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; _pressed = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs e) { _pressed = e.Button == MouseButtons.Left; Invalidate(); base.OnMouseDown(e); }
        protected override void OnMouseUp(MouseEventArgs e) { _pressed = false; Invalidate(); base.OnMouseUp(e); }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space) { e.Handled = true; OnClick(EventArgs.Empty); }
            base.OnKeyDown(e);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            Color back = _pressed ? _palette.Track : _hover ? _palette.AccentBack : _palette.Surface;
            if (_back.Color != back) { _back.Color = back; }
            Color border = _hover || Focused ? _palette.Accent : _palette.Edge;
            if (_border.Color != border) { _border.Color = border; }
            e.Graphics.FillRectangle(_back, 0, 0, Width, Height);
            if (Width > 1 && Height > 1) { e.Graphics.DrawRectangle(_border, 0, 0, Width - 1, Height - 1); }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            TextRenderer.DrawText(e.Graphics, Text, Font, ClientRectangle, _palette.Text, _back.Color,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        }

        protected override void OnGotFocus(EventArgs e) { Invalidate(); base.OnGotFocus(e); }
        protected override void OnLostFocus(EventArgs e) { Invalidate(); base.OnLostFocus(e); }

        protected override void Dispose(bool disposing)
        {
            if (disposing) { _back.Dispose(); _border.Dispose(); }
            base.Dispose(disposing);
        }
    }
}
