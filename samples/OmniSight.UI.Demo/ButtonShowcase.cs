using System;
using System.Drawing;
using System.Windows.Forms;
using OmniSight.UI.Controls;

namespace OmniSight.UI.Demo
{
    /// <summary>한 테마의 3변형 × 5상태 버튼 표.</summary>
    internal sealed class ButtonShowcase : Panel
    {
        private static readonly string[] StateNames = { "보통", "마우스 올림", "누름", "키보드 포커스", "사용 불가" };
        private static readonly string[] VariantNames = { "기본", "기본 동작", "위험" };

        public ButtonShowcase(UiTheme theme, bool large)
        {
            var palette = new SquareButtonPalette(theme);
            Font font = large ? new Font("Segoe UI", 11F) : new Font("Segoe UI", 9F);
            int buttonHeight = large ? 40 : 32;
            int buttonWidth = large ? 150 : 120;
            int labelWidth = large ? 110 : 90;
            int gap = large ? 12 : 10;
            int top = large ? 56 : 46;

            BackColor = palette.WindowBack;
            Width = 24 + labelWidth + StateNames.Length * (buttonWidth + gap) + 24;
            Height = top + 22 + VariantNames.Length * (buttonHeight + gap) + 12;

            Color text = theme == UiTheme.Dark ? Color.FromArgb(0xfa, 0xfa, 0xfa) : Color.FromArgb(0x1e, 0x1e, 0x1e);
            Color muted = theme == UiTheme.Dark ? Color.FromArgb(0xa0, 0xa0, 0xa5) : Color.FromArgb(0x5c, 0x5c, 0x5c);
            var title = new Label
            {
                Text = theme == UiTheme.Dark ? "다크 (VS2022 Dark)" : "라이트 (VS2022 Light)",
                ForeColor = text, BackColor = palette.WindowBack,
                Font = new Font("Segoe UI", large ? 12F : 10F, FontStyle.Bold),
                AutoSize = true, Location = new Point(24, 14)
            };
            Controls.Add(title);

            for (int s = 0; s < StateNames.Length; s++)
            {
                Controls.Add(new Label
                {
                    Text = StateNames[s], ForeColor = muted, BackColor = palette.WindowBack, Font = font,
                    AutoSize = false, TextAlign = ContentAlignment.MiddleCenter,
                    Bounds = new Rectangle(24 + labelWidth + s * (buttonWidth + gap), top - 2, buttonWidth, 20)
                });
            }

            for (int v = 0; v < VariantNames.Length; v++)
            {
                int y = top + 22 + v * (buttonHeight + gap);
                Controls.Add(new Label
                {
                    Text = VariantNames[v], ForeColor = text, BackColor = palette.WindowBack, Font = font,
                    AutoSize = false, TextAlign = ContentAlignment.MiddleLeft,
                    Bounds = new Rectangle(24, y, labelWidth, buttonHeight)
                });
                for (int s = 0; s < StateNames.Length; s++)
                {
                    Controls.Add(new SquareButton
                    {
                        Text = "버튼", Theme = theme, Variant = (SquareButtonVariant)v, Font = font,
                        ForcedState = (SquareButtonState)s,
                        Bounds = new Rectangle(24 + labelWidth + s * (buttonWidth + gap), y, buttonWidth, buttonHeight)
                    });
                }
            }
            Disposed += delegate { font.Dispose(); title.Font.Dispose(); };
        }
    }
}
