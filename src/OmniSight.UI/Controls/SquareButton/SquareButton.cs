using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace OmniSight.UI.Controls
{
    /// <summary>
    /// 각진 사각형 버튼. 색은 Visual Studio 2022의 다크·라이트 테마 값을 쓴다.
    /// Win32 Button을 상속하지 않고 전체 영역을 정수 좌표로 직접 칠하므로 클릭·hover·포커스 때
    /// 가장자리에 시스템 색이나 흰 선이 비치지 않는다. 더블 버퍼이며 상태가 바뀔 때만 다시 그린다.
    /// </summary>
    [ToolboxItem(true)]
    [DesignerCategory("Code")]
    [DefaultEvent("Click")]
    [DefaultProperty("Text")]
    [Description("VS2022 다크·라이트 테마 색을 쓰는 각진 버튼")]
    public class SquareButton : Control, IButtonControl
    {
        private const TextFormatFlags TextFlags = TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis |
            TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding;

        private UiTheme _theme = UiTheme.Dark;
        private SquareButtonPalette _palette = new SquareButtonPalette(UiTheme.Dark);
        private SquareButtonVariant _variant = SquareButtonVariant.Default;
        private ContentAlignment _textAlign = ContentAlignment.MiddleCenter;
        private Image _image;
        private bool _isToggle;
        private bool _checked;
        private bool _hover;
        private bool _pressed;
        private bool _keyPressed;
        private bool _isDefault;
        private SquareButtonState? _forcedState;
        private DialogResult _dialogResult = DialogResult.None;
        private readonly SolidBrush _back = new SolidBrush(Color.Black);
        private readonly Pen _border = new Pen(Color.Black);

        public SquareButton()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.Selectable |
                ControlStyles.StandardClick | ControlStyles.StandardDoubleClick, true);
            SetStyle(ControlStyles.SupportsTransparentBackColor, false);
            TabStop = true;
            Cursor = Cursors.Default;
            AccessibleRole = AccessibleRole.PushButton;
            Font = SystemFonts.MessageBoxFont;
            Size = DefaultSize;
        }

        protected override Size DefaultSize => new Size(100, 32);

        /// <summary>색 변형: 일반, 기본 동작, 위험.</summary>
        [Category("OmniSight")]
        [DefaultValue(SquareButtonVariant.Default)]
        [Description("일반, 기본 동작(Primary), 위험(Danger) 중 색 변형")]
        public SquareButtonVariant Variant
        {
            get { return _variant; }
            set { if (_variant != value) { _variant = value; Invalidate(); } }
        }

        /// <summary>VS2022 다크 또는 라이트 색.</summary>
        [Category("OmniSight")]
        [DefaultValue(UiTheme.Dark)]
        [Description("VS2022 다크 또는 라이트 테마 색")]
        public UiTheme Theme
        {
            get { return _theme; }
            set
            {
                if (_theme == value && _palette.Theme == value) { return; }
                _theme = value;
                _palette = new SquareButtonPalette(value);
                Invalidate();
            }
        }

        /// <summary>색을 직접 지정할 때 쓴다. 값을 바꾼 뒤에는 <see cref="RefreshPalette"/>를 호출한다.</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public SquareButtonPalette Palette
        {
            get { return _palette; }
            set
            {
                _palette = value ?? throw new ArgumentNullException(nameof(value));
                _theme = value.Theme;
                Invalidate();
            }
        }

        public void RefreshPalette() { Invalidate(); }

        /// <summary>true면 누를 때마다 <see cref="Checked"/>가 바뀐다.</summary>
        [Category("OmniSight")]
        [DefaultValue(false)]
        [Description("누를 때마다 켜짐·꺼짐이 바뀌는 토글 버튼")]
        public bool IsToggle
        {
            get { return _isToggle; }
            set { if (_isToggle != value) { _isToggle = value; if (!value) { Checked = false; } } }
        }

        /// <summary>토글 버튼의 켜짐 상태. 켜져 있으면 눌린 색으로 보인다.</summary>
        [Category("OmniSight")]
        [DefaultValue(false)]
        public bool Checked
        {
            get { return _checked; }
            set
            {
                if (_checked == value) { return; }
                _checked = value;
                Invalidate();
                CheckedChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        [Category("OmniSight")]
        public event EventHandler CheckedChanged;

        /// <summary>글자 앞에 그리는 아이콘.</summary>
        [Category("OmniSight")]
        [DefaultValue(null)]
        public Image Image
        {
            get { return _image; }
            set { if (!ReferenceEquals(_image, value)) { _image = value; Invalidate(); } }
        }

        [Category("OmniSight")]
        [DefaultValue(ContentAlignment.MiddleCenter)]
        [Description("글자 가로 정렬. 왼쪽·가운데만 지원한다.")]
        public ContentAlignment TextAlign
        {
            get { return _textAlign; }
            set { if (_textAlign != value) { _textAlign = value; Invalidate(); } }
        }

        [Category("Behavior")]
        [DefaultValue(DialogResult.None)]
        public DialogResult DialogResult
        {
            get { return _dialogResult; }
            set { _dialogResult = value; }
        }

        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public override string Text
        {
            get { return base.Text; }
            set { base.Text = value; }
        }

        [DefaultValue(typeof(Color), "")]
        public override Color BackColor
        {
            get { return base.BackColor; }
            set { base.BackColor = value; }
        }

        /// <summary>현재 화면에 쓰이는 시각 상태.</summary>
        [Browsable(false)]
        public SquareButtonState VisualState
        {
            get
            {
                if (_forcedState.HasValue) { return _forcedState.Value; }
                if (!Enabled) { return SquareButtonState.Disabled; }
                if (_pressed || _keyPressed || _checked) { return SquareButtonState.Pressed; }
                if (_hover) { return SquareButtonState.Hover; }
                if (Focused && ShowFocusCues) { return SquareButtonState.Focused; }
                return SquareButtonState.Normal;
            }
        }

        /// <summary>
        /// 입력과 상관없이 시각 상태를 고정한다(문서 캡처·화면 확인용). null이면 실제 입력에 따른다.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public SquareButtonState? ForcedState
        {
            get { return _forcedState; }
            set { if (_forcedState != value) { _forcedState = value; Invalidate(); } }
        }

        public void NotifyDefault(bool value)
        {
            _isDefault = value;
        }

        public void PerformClick()
        {
            if (Enabled && Visible && CanSelect) { OnClick(EventArgs.Empty); }
        }

        protected override void OnClick(EventArgs e)
        {
            if (_isToggle) { Checked = !_checked; }
            Form form = FindForm();
            if (form != null && _dialogResult != DialogResult.None) { form.DialogResult = _dialogResult; }
            base.OnClick(e);
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            Size text = string.IsNullOrEmpty(Text) ? Size.Empty
                : TextRenderer.MeasureText(Text, Font, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPadding | TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix);
            int image = _image == null ? 0 : _image.Width + (text.Width > 0 ? 8 : 0);
            int width = Padding.Horizontal + 24 + image + text.Width;
            int height = Math.Max(text.Height + 12, _image == null ? 0 : _image.Height + 12);
            return new Size(Math.Max(width, 48), Math.Max(height, Height > 0 ? Height : 32));
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            SquareButtonColors c = _palette.Get(_variant, VisualState);
            if (_back.Color != c.Back) { _back.Color = c.Back; }
            if (_border.Color != c.Border) { _border.Color = c.Border; }
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.None;
            g.PixelOffsetMode = PixelOffsetMode.None;
            g.FillRectangle(_back, 0, 0, Width, Height);
            if (Width > 1 && Height > 1) { g.DrawRectangle(_border, 0, 0, Width - 1, Height - 1); }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            SquareButtonColors c = _palette.Get(_variant, VisualState);
            Graphics g = e.Graphics;
            int left = Padding.Left + 12;
            int right = Width - Padding.Right - 12;
            if (_image != null)
            {
                g.DrawImageUnscaled(_image, left, (Height - _image.Height) / 2);
                left += _image.Width + 8;
            }

            var bounds = new Rectangle(left, 0, Math.Max(0, right - left), Height);
            TextFormatFlags align = (_textAlign & (ContentAlignment.TopLeft | ContentAlignment.MiddleLeft | ContentAlignment.BottomLeft)) != 0
                ? TextFormatFlags.Left : TextFormatFlags.HorizontalCenter;
            TextRenderer.DrawText(g, Text, Font, bounds, c.Text, c.Back, TextFlags | align);
            base.OnPaint(e);
        }

        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _pressed = true;
                Focus();
                Invalidate();
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_pressed)
            {
                bool inside = ClientRectangle.Contains(e.Location);
                if (inside != _hover) { _hover = inside; Invalidate(); }
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (_pressed) { _pressed = false; Invalidate(); }
            base.OnMouseUp(e);
        }

        protected override void OnLostFocus(EventArgs e)
        {
            _pressed = false;
            _keyPressed = false;
            Invalidate();
            base.OnLostFocus(e);
        }

        protected override void OnGotFocus(EventArgs e) { Invalidate(); base.OnGotFocus(e); }

        protected override bool IsInputKey(Keys keyData)
        {
            return keyData == Keys.Space || base.IsInputKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space && !_keyPressed)
            {
                _keyPressed = true;
                Invalidate();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Enter && !_isDefault)
            {
                e.Handled = true;
                PerformClick();
            }
            base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space && _keyPressed)
            {
                _keyPressed = false;
                Invalidate();
                e.Handled = true;
                PerformClick();
            }
            base.OnKeyUp(e);
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            _hover = false;
            _pressed = false;
            _keyPressed = false;
            base.OnEnabledChanged(e);
            Invalidate();
        }

        protected override void OnTextChanged(EventArgs e) { base.OnTextChanged(e); Invalidate(); }
        protected override void OnFontChanged(EventArgs e) { base.OnFontChanged(e); Invalidate(); }
        protected override void OnPaddingChanged(EventArgs e) { base.OnPaddingChanged(e); Invalidate(); }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _back.Dispose();
                _border.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
