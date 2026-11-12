using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace OmniSight.UI.Controls
{
    /// <summary>
    /// 램프 + 장치 이름의 각진 칩. 보통은 <see cref="DeviceStatusStrip"/>이 만들고 관리한다.
    /// 더블 버퍼, 전체 영역을 직접 칠하는 불투명 배경, 캐시한 브러시·펜을 쓰고 값이 바뀔 때만 다시 그린다.
    /// </summary>
    [ToolboxItem(false)]
    [DesignerCategory("Code")]
    public sealed class DeviceStatusChip : Control
    {
        private const TextFormatFlags TextFlags = TextFormatFlags.Left | TextFormatFlags.VerticalCenter |
            TextFormatFlags.NoPadding | TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis;

        private string _deviceName = string.Empty;
        private DeviceLinkState _state;
        private bool _selected;
        private bool _hover;
        private int _angle = 270;
        private DeviceStatusPalette _palette = new DeviceStatusPalette(UiTheme.Dark);
        private LampResources _resources;
        private readonly SolidBrush _back = new SolidBrush(Color.Black);
        private readonly Pen _border = new Pen(Color.Black);

        public DeviceStatusChip()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
            TabStop = true;
            Cursor = Cursors.Hand;
            AccessibleRole = AccessibleRole.PushButton;
            _resources = new LampResources(_palette);
            UpdateAccessibility();
        }

        /// <summary>장치 식별자(레지스트리 키).</summary>
        [Category("OmniSight")]
        [DefaultValue("")]
        public string DeviceId { get; set; } = string.Empty;

        [Category("OmniSight")]
        [DefaultValue("")]
        public string DeviceName
        {
            get { return _deviceName; }
            set
            {
                value = value ?? string.Empty;
                if (_deviceName == value) { return; }
                _deviceName = value;
                UpdateAccessibility();
                Invalidate();
            }
        }

        [Category("OmniSight")]
        [DefaultValue(DeviceLinkState.Disabled)]
        public DeviceLinkState State
        {
            get { return _state; }
            set
            {
                if (_state == value) { return; }
                _state = value;
                UpdateAccessibility();
                Invalidate();
            }
        }

        /// <summary>상세 팝업이 열려 있는 칩.</summary>
        [Category("OmniSight")]
        [DefaultValue(false)]
        public bool Selected
        {
            get { return _selected; }
            set
            {
                if (_selected == value) { return; }
                _selected = value;
                Invalidate();
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DeviceStatusPalette Palette
        {
            get { return _palette; }
            set
            {
                _palette = value ?? throw new ArgumentNullException(nameof(value));
                ApplyPalette();
            }
        }

        /// <summary>팔레트 색을 바꾼 뒤 다시 읽는다.</summary>
        public void ApplyPalette()
        {
            LampResources old = _resources;
            _resources = new LampResources(_palette);
            if (old != null) { old.Dispose(); }
            Invalidate();
        }

        internal string ToolTipText;

        internal int LampDiameter => Math.Max(7, (int)(Height * 0.32f));

        private int PadX => Math.Max(8, (int)(Height * 0.33f));

        private Rectangle LampBounds
        {
            get
            {
                int d = LampDiameter;
                return new Rectangle(PadX, (Height - d) / 2, d, d);
            }
        }

        /// <summary>연결 중 회전 호를 돌린다. 램프 사각형만 다시 그린다.</summary>
        internal void Advance(int degrees)
        {
            if (_state != DeviceLinkState.Connecting) { return; }
            _angle = (_angle + degrees) % 360;
            Rectangle lamp = LampBounds;
            lamp.Inflate(1, 1);
            Invalidate(lamp);
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            Size text = TextRenderer.MeasureText(_deviceName, Font, new Size(int.MaxValue, int.MaxValue),
                TextFormatFlags.NoPadding | TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix);
            int width = PadX + LampDiameter + Math.Max(6, LampDiameter) + text.Width + PadX;
            return new Size(width, Height > 0 ? Height : 28);
        }

        protected override Size DefaultSize => new Size(96, 28);

        private Color BorderColor
        {
            get
            {
                if (_selected) { return _palette.SelectedBorder; }
                if (_hover || Focused) { return _palette.HoverBorder; }
                if (_state == DeviceLinkState.Fault || _state == DeviceLinkState.Lost) { return _palette.Error; }
                return _palette.Edge;
            }
        }

        private Color BackFill
        {
            get { return _selected ? _palette.Selected : _hover ? _palette.Hover : _palette.Surface; }
        }

        // 부모 색이 비치지 않도록 전체 영역을 직접 채우고 1px 테두리를 안쪽에 그린다.
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            Color back = BackFill;
            if (_back.Color != back) { _back.Color = back; }
            Color border = BorderColor;
            if (_border.Color != border) { _border.Color = border; }
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.None;
            g.FillRectangle(_back, 0, 0, Width, Height);
            if (Width > 1 && Height > 1) { g.DrawRectangle(_border, 0, 0, Width - 1, Height - 1); }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Rectangle lamp = LampBounds;
            LampPainter.Draw(e.Graphics, lamp, _state, _angle, _resources);
            int textLeft = lamp.Right + Math.Max(6, lamp.Width);
            var textBounds = new Rectangle(textLeft, 0, Math.Max(0, Width - textLeft - PadX / 2), Height);
            TextRenderer.DrawText(e.Graphics, _deviceName, Font, textBounds, _palette.Text, _back.Color, TextFlags);
            base.OnPaint(e);
        }

        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnGotFocus(EventArgs e) { Invalidate(); base.OnGotFocus(e); }
        protected override void OnLostFocus(EventArgs e) { Invalidate(); base.OnLostFocus(e); }
        protected override void OnFontChanged(EventArgs e) { base.OnFontChanged(e); Invalidate(); }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space)
            {
                e.Handled = true;
                OnClick(EventArgs.Empty);
            }
            base.OnKeyDown(e);
        }

        private void UpdateAccessibility()
        {
            AccessibleName = _deviceName + ", " + _state.ToDisplayText();
            AccessibleDescription = "장치 연결 상태: " + _state.ToDisplayText();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_resources != null) { _resources.Dispose(); _resources = null; }
                _back.Dispose();
                _border.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
