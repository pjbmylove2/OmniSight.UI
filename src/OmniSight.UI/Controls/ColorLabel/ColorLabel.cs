using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace OmniSight.UI.Controls
{
    public enum BadgeTone { Info, Success, Error, Warning }

    [ToolboxItem(true)]
    [DefaultProperty(nameof(Text))]
    public class ColorLabel : Label
    {
        private UiTheme theme;
        private BadgeTone tone;
        private int cornerRadius = 5;
        private bool emphasized = true;
        private Color badgeBackColor;
        private Color badgeForeColor;
        private Font boldFont;

        public ColorLabel()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            UseMnemonic = false;
            TextAlign = ContentAlignment.MiddleCenter;
            AutoEllipsis = true;
            AutoSize = true;
        }

        [Category("OmniSight"), DefaultValue(UiTheme.Dark)]
        public UiTheme Theme
        {
            get => theme;
            set
            {
                if (theme == value)
                    return;

                CardPalette.For(value);
                theme = value;
                Invalidate();
            }
        }

        [Category("OmniSight"), DefaultValue(BadgeTone.Info)]
        public BadgeTone Tone
        {
            get => tone;
            set
            {
                if (tone == value)
                    return;

                BadgeDrawing.Colors(theme, value);
                tone = value;
                Invalidate();
            }
        }

        [Category("OmniSight"), DefaultValue(5)]
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
                Invalidate();
            }
        }

        [Category("OmniSight"), DefaultValue(true)]
        [Description("Adds bold to the current font style.")]
        public bool Emphasized
        {
            get => emphasized;
            set
            {
                if (emphasized == value)
                    return;

                emphasized = value;
                OnFontChanged(EventArgs.Empty);
            }
        }

        [Category("OmniSight"), DefaultValue(typeof(Color), "")]
        [Description("Solid badge background. Empty uses the selected tone.")]
        public Color BadgeBackColor
        {
            get => badgeBackColor;
            set => SetBadgeColor(ref badgeBackColor, value, nameof(BadgeBackColor));
        }

        [Category("OmniSight"), DefaultValue(typeof(Color), "")]
        [Description("Solid text color. Empty uses the tone color or chooses black or white against a custom background.")]
        public Color BadgeForeColor
        {
            get => badgeForeColor;
            set => SetBadgeColor(ref badgeForeColor, value, nameof(BadgeForeColor));
        }

        [DefaultValue(true)]
        public override bool AutoSize
        {
            get => base.AutoSize;
            set => base.AutoSize = value;
        }

        [DefaultValue(ContentAlignment.MiddleCenter)]
        public override ContentAlignment TextAlign
        {
            get => base.TextAlign;
            set => base.TextAlign = value;
        }

        [DefaultValue(false)]
        public new bool UseMnemonic
        {
            get => base.UseMnemonic;
            set => base.UseMnemonic = value;
        }

        [DefaultValue(true)]
        public new bool AutoEllipsis
        {
            get => base.AutoEllipsis;
            set => base.AutoEllipsis = value;
        }

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

        protected override Padding DefaultPadding => new Padding(9, 4, 9, 4);

        private Padding PaintPadding => Padding == DefaultPadding ? BadgeDrawing.PaddingForDpi(DeviceDpi) : Padding;

        private Font DisplayFont => emphasized ? (boldFont ?? (boldFont = BadgeDrawing.CreateBoldFont(Font)) ?? Font) : Font;

        public override Size GetPreferredSize(Size proposedSize)
        {
            using (Graphics graphics = IsHandleCreated ? CreateGraphics() : null)
            {
                Font font = DisplayFont;
                Size measured = BadgeDrawing.Measure(graphics, Text, font, UseMnemonic);
                int height = Math.Max(measured.Height, (int)Math.Ceiling(font.GetHeight(DeviceDpi)));
                Padding padding = PaintPadding;
                return new Size(measured.Width + padding.Horizontal,
                    Math.Max(BadgeDrawing.Scale(24, DeviceDpi), height + padding.Vertical));
            }
        }

        /// <summary>Draws a centered preset badge into the supplied bounds, using the caller's font.</summary>
        public static void DrawBadge(Graphics graphics, Rectangle bounds, string text, Font font,
            UiTheme theme, BadgeTone tone, int dpi)
        {
            if (graphics == null)
                throw new ArgumentNullException(nameof(graphics));
            if (font == null)
                throw new ArgumentNullException(nameof(font));
            if (dpi <= 0)
                throw new ArgumentOutOfRangeException(nameof(dpi));

            var colors = BadgeDrawing.Colors(theme, tone);
            BadgeDrawing.Draw(graphics, bounds, text, font, colors.Background, colors.Foreground,
                BadgeDrawing.Scale(5, dpi), BadgeDrawing.PaddingForDpi(dpi),
                ContentAlignment.MiddleCenter, ellipsis: true, useMnemonic: false);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            Control parent = Parent;
            while (parent != null && parent.BackColor.A < 255)
                parent = parent.Parent;

            using (var brush = new SolidBrush(parent?.BackColor ?? SystemColors.Control))
                e.Graphics.FillRectangle(brush, e.ClipRectangle);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var colors = BadgeDrawing.Colors(theme, tone);
            Color background = badgeBackColor.IsEmpty ? colors.Background : badgeBackColor;
            Color foreground = !badgeForeColor.IsEmpty ? badgeForeColor
                : badgeBackColor.IsEmpty ? colors.Foreground : BadgeDrawing.ContrastingText(background);

            BadgeDrawing.Draw(e.Graphics, ClientRectangle, Text, DisplayFont, background, foreground,
                (float)(cornerRadius * (double)DeviceDpi / 96), PaintPadding, TextAlign, AutoEllipsis, UseMnemonic);
        }

        protected override void OnFontChanged(EventArgs e)
        {
            ReleaseBoldFont();
            base.OnFontChanged(e);
            Invalidate();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            UpdateAutoSize();
        }

        protected override void OnDpiChangedAfterParent(EventArgs e)
        {
            base.OnDpiChangedAfterParent(e);
            UpdateAutoSize();
            Invalidate();
        }

        protected override void OnParentBackColorChanged(EventArgs e)
        {
            base.OnParentBackColorChanged(e);
            Invalidate();
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            Invalidate();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                ReleaseBoldFont();
            base.Dispose(disposing);
        }

        private void ReleaseBoldFont()
        {
            boldFont?.Dispose();
            boldFont = null;
        }

        private void UpdateAutoSize()
        {
            if (!AutoSize)
                return;

            if (Dock == DockStyle.None)
                Size = GetPreferredSize(Size.Empty);
            Parent?.PerformLayout(this, nameof(PreferredSize));
        }

        private void SetBadgeColor(ref Color field, Color color, string property)
        {
            if (!color.IsEmpty && color.A != 255)
                throw new ArgumentException("Use an opaque color or Color.Empty.", property);
            if (field == color)
                return;

            field = color;
            Invalidate();
        }
    }
}
