using System.Windows.Forms;
using OmniSight.UI.Controls;

namespace OmniSight.UI.Demo.Cards
{
    internal sealed class CardShowcase : TableLayoutPanel
    {
        internal CardShowcase()
        {
            Dock = DockStyle.Top;
            Height = 132;
            ColumnCount = 2;
            RowCount = 1;
            ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Controls.Add(CreatePreview(UiTheme.Dark, "다크 테마"), 0, 0);
            Controls.Add(CreatePreview(UiTheme.Light, "라이트 테마"), 1, 0);
        }

        private static Panel CreatePreview(UiTheme theme, string title)
        {
            var preview = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = Padding.Empty,
                Padding = new Padding(12),
                BackColor = CardPalette.For(theme).Canvas
            };
            var card = new CardPanel
            {
                Dock = DockStyle.Fill,
                Theme = theme,
                HeaderLeftText = "검사 상태",
                HeaderRightText = title,
                ShowHeaderDivider = true
            };
            var badges = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 7, 0, 0),
                Margin = Padding.Empty
            };
            badges.Controls.AddRange(new Control[]
            {
                new ColorLabel { Text = "OK", Tone = BadgeTone.Success, Theme = theme },
                new ColorLabel { Text = "NG", Tone = BadgeTone.Error, Theme = theme },
                new ColorLabel { Text = "수동 모드", Tone = BadgeTone.Warning, Theme = theme },
                new ColorLabel { Text = "대기", Tone = BadgeTone.Info, Theme = theme }
            });
            card.Controls.Add(badges);
            preview.Controls.Add(card);
            return preview;
        }
    }
}
