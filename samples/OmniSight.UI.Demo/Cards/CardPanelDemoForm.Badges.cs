using System.Drawing;
using System.Windows.Forms;
using OmniSight.UI.Controls;

namespace OmniSight.UI.Demo.Cards
{
    internal sealed partial class CardPanelDemoForm
    {
        internal Form CreateBadgeGallery()
        {
            var dialog = new Form
            {
                Text = "컬러 라벨",
                Font = uiFont,
                AutoScaleDimensions = new SizeF(96, 96),
                AutoScaleMode = AutoScaleMode.Dpi,
                ClientSize = new Size(676, 317),
                BackColor = palette.Canvas,
                ForeColor = palette.Text,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                ShowInTaskbar = false,
                StartPosition = FormStartPosition.CenterParent
            };

            var layout = NewTable(2);
            layout.Padding = new Padding(14);
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            CardPanel dark = CreateBadgeExamples(UiTheme.Dark, "다크 테마");
            CardPanel light = CreateBadgeExamples(UiTheme.Light, "라이트 테마");
            dark.Margin = new Padding(0, 0, 6, 0);
            light.Margin = new Padding(6, 0, 0, 0);
            layout.Controls.Add(dark, 0, 0);
            layout.Controls.Add(light, 1, 0);

            Button close = NewButton("닫기");
            close.Dock = DockStyle.None;
            close.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            close.Size = new Size(94, 31);
            close.DialogResult = DialogResult.Cancel;
            StyleButton(close, palette);
            layout.Controls.Add(close, 1, 1);
            dialog.Controls.Add(layout);
            dialog.CancelButton = close;
            return dialog;
        }

        private CardPanel CreateBadgeExamples(UiTheme cardTheme, string title)
        {
            CardPanel card = NewCard(title);
            card.Theme = cardTheme;
            card.ShowHeaderDivider = true;
            CardPalette colors = CardPalette.For(cardTheme);
            var rows = NewTable(1);
            rows.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
            rows.RowStyles.Add(new RowStyle(SizeType.Absolute, 33));
            rows.RowStyles.Add(new RowStyle(SizeType.Absolute, 33));
            rows.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            rows.RowStyles.Add(new RowStyle(SizeType.Absolute, 33));
            rows.RowStyles.Add(new RowStyle(SizeType.Absolute, 33));

            Label presets = NewLabel("상태 프리셋");
            presets.ForeColor = colors.MutedText;
            rows.Controls.Add(presets, 0, 0);
            rows.Controls.Add(BadgeStrip(
                Example("OK", BadgeTone.Success, cardTheme),
                Example("NG", BadgeTone.Error, cardTheme),
                Example("OFF", BadgeTone.Info, cardTheme),
                Example("수동 모드", BadgeTone.Warning, cardTheme)), 0, 1);
            rows.Controls.Add(BadgeStrip(
                Example("종합 NG", BadgeTone.Error, cardTheme),
                Example("MariaDB 연결됨", BadgeTone.Success, cardTheme)), 0, 2);

            Label custom = NewLabel("사용자 지정 배경과 글자색");
            custom.ForeColor = colors.MutedText;
            rows.Controls.Add(custom, 0, 3);
            ColorLabel bright = Example("밝은 배경 · 자동", BadgeTone.Info, cardTheme);
            bright.BadgeBackColor = Color.FromArgb(248, 219, 161);
            ColorLabel dim = Example("어두운 배경 · 자동", BadgeTone.Info, cardTheme);
            dim.BadgeBackColor = Color.FromArgb(62, 40, 88);
            rows.Controls.Add(BadgeStrip(bright, dim), 0, 4);
            ColorLabel manual = Example("글자색 직접 지정", BadgeTone.Info, cardTheme);
            manual.BadgeBackColor = Color.FromArgb(62, 40, 88);
            manual.BadgeForeColor = Color.FromArgb(236, 202, 255);
            manual.Emphasized = false;
            rows.Controls.Add(BadgeStrip(manual), 0, 5);

            card.Controls.Add(rows);
            return card;
        }

        private static FlowLayoutPanel BadgeStrip(params ColorLabel[] badges)
        {
            var strip = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false, Margin = Padding.Empty };
            strip.Controls.AddRange(badges);
            return strip;
        }

        private static ColorLabel Example(string text, BadgeTone tone, UiTheme theme) => new ColorLabel
        {
            Text = text, Tone = tone, Theme = theme, Margin = new Padding(0, 2, 8, 0)
        };
    }
}
