using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace OmniSight.UI.Controls
{
    /// <summary>
    /// 설정 화면의 안내·경고 띠. 제목(선택)과 본문을 한 줄씩 보여 주고, <see cref="BadgeTone"/>으로 색을 정한다.
    /// 긴 글은 말줄임표로 줄인다. 여러 줄 본문이 필요하면 높이를 늘리고 줄바꿈이 있는 글을 넣는다.
    /// </summary>
    [ToolboxItem(true)]
    [DefaultProperty(nameof(Text))]
    public class AlertBanner : Control
    {
        private UiTheme theme;
        private BadgeTone tone = BadgeTone.Info;
        private string title = string.Empty;

        public AlertBanner()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            TabStop = false;
            Height = 48;
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

        /// <summary>본문 위에 굵게 보여 줄 제목. 비어 있으면 본문만 그린다.</summary>
        [Category("OmniSight"), DefaultValue("")]
        public string Title
        {
            get => title;
            set
            {
                string next = value ?? string.Empty;
                if (title == next)
                    return;

                title = next;
                Invalidate();
            }
        }

        [Browsable(true), DefaultValue("")]
        public override string Text
        {
            get => base.Text;
            set => base.Text = value ?? string.Empty;
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var colors = BadgeDrawing.Colors(theme, tone);
            float scale = DeviceDpi / 96f;
            int padX = (int)Math.Round(14 * scale);
            int padY = (int)Math.Round(8 * scale);

            using (var fill = new SolidBrush(colors.Background))
                e.Graphics.FillRectangle(fill, ClientRectangle);

            var content = new Rectangle(padX, padY, Math.Max(0, Width - padX * 2), Math.Max(0, Height - padY * 2));
            const TextFormatFlags flags = TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix;

            if (title.Length == 0)
            {
                TextRenderer.DrawText(e.Graphics, Text, Font, content, colors.Foreground, flags | TextFormatFlags.VerticalCenter);
                return;
            }

            using (var bold = new Font(Font, FontStyle.Bold))
            {
                int titleHeight = TextRenderer.MeasureText(e.Graphics, title, bold, Size.Empty, TextFormatFlags.NoPrefix).Height;
                var titleBounds = new Rectangle(content.Left, content.Top, content.Width, Math.Min(titleHeight, content.Height));
                TextRenderer.DrawText(e.Graphics, title, bold, titleBounds, colors.Foreground, flags | TextFormatFlags.Top);

                int gap = (int)Math.Round(6 * scale);
                int bodyTop = titleBounds.Bottom + gap;
                var bodyBounds = new Rectangle(content.Left, bodyTop, content.Width, Math.Max(0, content.Bottom - bodyTop));
                TextRenderer.DrawText(e.Graphics, Text, Font, bodyBounds, colors.Foreground, flags | TextFormatFlags.Top);
            }
        }
    }
}
