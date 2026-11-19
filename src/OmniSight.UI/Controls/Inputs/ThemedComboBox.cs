using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace OmniSight.UI.Controls
{
    /// <summary>다크·라이트 테마 색으로 항목을 직접 그리는 드롭다운 목록.</summary>
    [ToolboxItem(true)]
    public class ThemedComboBox : ComboBox
    {
        private UiTheme theme;

        public ThemedComboBox()
        {
            DrawMode = DrawMode.OwnerDrawFixed;
            DropDownStyle = ComboBoxStyle.DropDownList;
            ItemHeight = 29;
            FlatStyle = FlatStyle.Flat;
            ApplyTheme();
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
                ApplyTheme();
                Invalidate();
            }
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            CardPalette palette = CardPalette.For(theme);
            bool selected = (e.State & DrawItemState.Selected) != 0 && (e.State & DrawItemState.ComboBoxEdit) == 0;
            Color back = selected ? palette.AccentFill : palette.Input;
            Color fore = selected ? palette.AccentText : palette.Text;

            using (var fill = new SolidBrush(back))
                e.Graphics.FillRectangle(fill, e.Bounds);

            if (e.Index < 0)
                return;

            int pad = (int)Math.Round(6 * DeviceDpi / 96f);
            var textBounds = new Rectangle(e.Bounds.X + pad, e.Bounds.Y, Math.Max(0, e.Bounds.Width - pad), e.Bounds.Height);
            TextRenderer.DrawText(e.Graphics, GetItemText(Items[e.Index]), Font, textBounds, fore,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }

        private void ApplyTheme()
        {
            CardPalette palette = CardPalette.For(theme);
            BackColor = palette.Input;
            ForeColor = palette.Text;
        }
    }
}
