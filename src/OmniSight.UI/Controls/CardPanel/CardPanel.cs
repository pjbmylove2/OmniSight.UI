using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace OmniSight.UI.Controls
{
    [ToolboxItem(true)]
    [DefaultProperty(nameof(HeaderLeftText))]
    public class CardPanel : Panel
    {
        private const TextFormatFlags HeaderTextFlags = TextFormatFlags.SingleLine
            | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix
            | TextFormatFlags.NoPadding | TextFormatFlags.PreserveGraphicsClipping;

        private UiTheme theme;
        private int cornerRadius = 9;
        private bool showHeaderDivider;
        private HeaderRightStyle headerRightStyle;
        private string leftText = string.Empty;
        private string centerText = string.Empty;
        private string rightText = string.Empty;

        public CardPanel()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            ApplyPalette(CardPalette.For(theme));
        }

        [Category("OmniSight"), DefaultValue(UiTheme.Dark)]
        public UiTheme Theme
        {
            get => theme;
            set
            {
                if (theme == value)
                    return;

                CardPalette palette = CardPalette.For(value);
                theme = value;
                ApplyPalette(palette);
                Invalidate();
            }
        }

        [Category("OmniSight"), DefaultValue(9)]
        [Description("Corner radius in logical pixels at 96 DPI.")]
        public int CornerRadius
        {
            get => cornerRadius;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value));
                if (cornerRadius == value)
                    return;

                cornerRadius = value;
                PerformLayout();
                Invalidate();
            }
        }

        [Category("OmniSight"), DefaultValue(false)]
        public bool ShowHeaderDivider
        {
            get => showHeaderDivider;
            set
            {
                if (showHeaderDivider == value)
                    return;

                showHeaderDivider = value;
                Invalidate();
            }
        }

        [Category("OmniSight"), DefaultValue(HeaderRightStyle.Text)]
        public HeaderRightStyle HeaderRightStyle
        {
            get => headerRightStyle;
            set
            {
                if (value != HeaderRightStyle.Text && value != HeaderRightStyle.SuccessBadge)
                    throw new InvalidEnumArgumentException(nameof(value), (int)value, typeof(HeaderRightStyle));
                if (headerRightStyle == value)
                    return;

                headerRightStyle = value;
                Invalidate();
            }
        }

        [Category("OmniSight"), DefaultValue("")]
        public string HeaderLeftText
        {
            get => leftText;
            set => SetHeaderText(ref leftText, value);
        }

        [Category("OmniSight"), DefaultValue("")]
        public string HeaderCenterText
        {
            get => centerText;
            set => SetHeaderText(ref centerText, value);
        }

        [Category("OmniSight"), DefaultValue("")]
        public string HeaderRightText
        {
            get => rightText;
            set => SetHeaderText(ref rightText, value);
        }

        protected override Size DefaultSize => new Size(250, 170);
        protected override Padding DefaultPadding => new Padding(14);

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override Color BackColor
        {
            get => base.BackColor;
            set => base.BackColor = value;
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override Color ForeColor
        {
            get => base.ForeColor;
            set => base.ForeColor = value;
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new BorderStyle BorderStyle
        {
            get => BorderStyle.None;
        }

        public override Rectangle DisplayRectangle
        {
            get
            {
                Rectangle area = base.DisplayRectangle;
                Padding extra = ExtraInsets;
                int top = extra.Top + HeaderHeight;
                return new Rectangle(area.X + extra.Left, area.Y + top,
                    Math.Max(0, area.Width - extra.Horizontal),
                    Math.Max(0, area.Height - top - extra.Bottom));
            }
        }

        private bool HasHeader => !string.IsNullOrEmpty(leftText)
            || !string.IsNullOrEmpty(centerText) || !string.IsNullOrEmpty(rightText);

        private int HeaderHeight
        {
            get
            {
                if (!HasHeader)
                    return 0;

                int dpi = DeviceDpi;
                int textHeight = (int)Math.Ceiling(Font.GetHeight(dpi) + 2f * dpi / 72f);
                return Math.Max(Scale(32), textHeight + Scale(10));
            }
        }

        private float Radius => (float)Math.Min(cornerRadius * (double)DeviceDpi / 96,
            Math.Min(ClientSize.Width, ClientSize.Height) / 2f);

        private Padding ExtraInsets
        {
            get
            {
                int inset = (int)Math.Ceiling(Radius * 0.30f) + Scale(2);
                return new Padding(Math.Max(0, inset - Padding.Left), Math.Max(0, inset - Padding.Top),
                    Math.Max(0, inset - Padding.Right), Math.Max(0, inset - Padding.Bottom));
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            using (var brush = new SolidBrush(Parent?.BackColor ?? SystemColors.Control))
                e.Graphics.FillRectangle(brush, e.ClipRectangle);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            float borderWidth = Math.Max(1, DeviceDpi / 96f);
            if (ClientSize.Width > borderWidth && ClientSize.Height > borderWidth)
            {
                CardPalette palette = CardPalette.For(theme);
                float half = borderWidth / 2f;
                var bounds = new RectangleF(half, half, ClientSize.Width - borderWidth,
                    ClientSize.Height - borderWidth);

                using (GraphicsPath path = BadgeDrawing.RoundedPath(bounds, Math.Max(0, Radius - half)))
                using (var fill = new SolidBrush(BackColor))
                using (var border = new Pen(palette.Border, borderWidth))
                {
                    SmoothingMode original = e.Graphics.SmoothingMode;
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    e.Graphics.FillPath(fill, path);
                    e.Graphics.DrawPath(border, path);
                    e.Graphics.SmoothingMode = original;
                }

                if (HasHeader)
                    PaintHeader(e.Graphics, palette);
            }
            base.OnPaint(e);
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            PerformLayout();
            Invalidate();
        }

        protected override void OnDpiChangedAfterParent(EventArgs e)
        {
            base.OnDpiChangedAfterParent(e);
            PerformLayout();
            Invalidate();
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            Invalidate();
        }

        protected override void OnParentBackColorChanged(EventArgs e)
        {
            base.OnParentBackColorChanged(e);
            Invalidate();
        }

        protected override AccessibleObject CreateAccessibilityInstance() => new CardAccessibleObject(this);

        private void ApplyPalette(CardPalette palette)
        {
            BackColor = palette.Surface;
            ForeColor = palette.Text;
        }

        private void SetHeaderText(ref string field, string value)
        {
            string text = string.IsNullOrWhiteSpace(value) ? string.Empty : value;
            if (field == text)
                return;

            bool hadHeader = HasHeader;
            field = text;
            if (hadHeader != HasHeader)
                PerformLayout();
            Invalidate();
        }

        private void PaintHeader(Graphics graphics, CardPalette palette)
        {
            Padding extra = ExtraInsets;
            int left = Padding.Left + extra.Left;
            int right = Padding.Right + extra.Right;
            if (centerText.Length > 0)
                left = right = Math.Max(left, right);

            int top = Padding.Top + extra.Top;
            int height = HeaderHeight;
            int width = ClientSize.Width - left - right;
            if (width <= 0 || ClientSize.Height < top + height)
                return;

            var row = new Rectangle(left, top, width, height);
            int count = (leftText.Length > 0 ? 1 : 0) + (centerText.Length > 0 ? 1 : 0)
                + (rightText.Length > 0 ? 1 : 0);
            Rectangle leftSlot = row;
            Rectangle centerSlot = row;
            Rectangle rightSlot = row;
            if (count > 1)
            {
                int gap = Math.Min(Scale(8), width / 10);
                int side = centerText.Length > 0 ? width / 3 : (int)(width * 0.6);
                leftSlot.Width = side - gap;
                if (centerText.Length > 0)
                    centerSlot = new Rectangle(left + side, top, width - 2 * side, height);
                int rightStart = centerText.Length > 0 ? width - side : side;
                rightSlot = new Rectangle(left + rightStart + gap, top, width - rightStart - gap, height);
            }

            if (leftText.Length > 0 && leftSlot.Width > 0)
            {
                using (var titleFont = new Font(Font.FontFamily, Font.SizeInPoints + 2f,
                    Font.Style | FontStyle.Bold, GraphicsUnit.Point))
                {
                    DrawText(graphics, leftText, titleFont, leftSlot, ForeColor, TextFormatFlags.Left);
                }
            }

            DrawText(graphics, centerText, Font, centerSlot, ForeColor, TextFormatFlags.HorizontalCenter);
            if (headerRightStyle == HeaderRightStyle.SuccessBadge)
                PaintBadge(graphics, rightSlot);
            else
                DrawText(graphics, rightText, Font, rightSlot, palette.MutedText, TextFormatFlags.Right);

            if (showHeaderDivider)
            {
                using (var pen = new Pen(palette.Border))
                    graphics.DrawLine(pen, row.Left, row.Bottom - 1, row.Right - 1, row.Bottom - 1);
            }
        }

        private void PaintBadge(Graphics graphics, Rectangle slot)
        {
            Padding padding = BadgeDrawing.PaddingForDpi(DeviceDpi);
            if (rightText.Length == 0 || slot.Width <= padding.Horizontal)
                return;

            using (Font bold = BadgeDrawing.CreateBoldFont(Font))
            {
                Font font = bold ?? Font;
                Size text = BadgeDrawing.Measure(graphics, rightText, font, false);
                int width = Math.Min(slot.Width, text.Width + padding.Horizontal);
                int height = Math.Min(slot.Height, Math.Max(Scale(24), text.Height + padding.Vertical));
                var bounds = new Rectangle(slot.Right - width, slot.Top + (slot.Height - height) / 2, width, height);
                ColorLabel.DrawBadge(graphics, bounds, rightText, font, theme, BadgeTone.Success, DeviceDpi);
            }
        }

        private static void DrawText(Graphics graphics, string text, Font font, Rectangle bounds,
            Color color, TextFormatFlags alignment)
        {
            if (text.Length == 0 || bounds.Width <= 0 || bounds.Height <= 0)
                return;

            GraphicsState state = graphics.Save();
            try
            {
                graphics.SetClip(bounds, CombineMode.Intersect);
                TextRenderer.DrawText(graphics, text, font, bounds, color, HeaderTextFlags | alignment);
            }
            finally
            {
                graphics.Restore(state);
            }
        }

        private int Scale(int logical) => (int)Math.Round(logical * DeviceDpi / 96d);

        private sealed class CardAccessibleObject : ControlAccessibleObject
        {
            private readonly CardPanel card;

            public CardAccessibleObject(CardPanel card) : base(card) => this.card = card;

            public override string Name
            {
                get => card.AccessibleName ?? string.Join(" ", new[]
                {
                    card.HeaderLeftText, card.HeaderCenterText, card.HeaderRightText
                }).Trim();
                set => card.AccessibleName = value;
            }
        }
    }
}
