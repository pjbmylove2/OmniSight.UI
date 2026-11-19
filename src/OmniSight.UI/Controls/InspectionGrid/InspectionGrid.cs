using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace OmniSight.UI.Controls
{
    /// <summary>
    /// 검사 결과 표. 다크·라이트 테마 색을 적용하고, 판정 열의 "OK"/"NG" 문자열을 배지로 그린다.
    /// 기본 판정 열은 마지막 열이다.
    /// </summary>
    [ToolboxItem(true)]
    public class InspectionGrid : DataGridView
    {
        private const string OkText = "OK";
        private const string NgText = "NG";

        private UiTheme theme;
        private int verdictColumnIndex = -1;

        public InspectionGrid()
        {
            DoubleBuffered = true;
            AllowUserToAddRows = false;
            AllowUserToDeleteRows = false;
            AllowUserToResizeRows = false;
            ReadOnly = true;
            RowHeadersVisible = false;
            BorderStyle = BorderStyle.None;
            CellBorderStyle = DataGridViewCellBorderStyle.None;
            ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            EnableHeadersVisualStyles = false;
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

        /// <summary>판정 배지를 그릴 열 번호. -1이면 마지막 열이다.</summary>
        [Category("OmniSight"), DefaultValue(-1)]
        public int VerdictColumnIndex
        {
            get => verdictColumnIndex;
            set
            {
                if (value < -1)
                    throw new ArgumentOutOfRangeException(nameof(value));
                if (verdictColumnIndex == value)
                    return;

                verdictColumnIndex = value;
                Invalidate();
            }
        }

        protected override void OnCellPainting(DataGridViewCellPaintingEventArgs e)
        {
            int verdictColumn = verdictColumnIndex < 0 ? ColumnCount - 1 : verdictColumnIndex;
            string verdict = e.Value as string;
            if (e.RowIndex >= 0 && e.ColumnIndex == verdictColumn && (verdict == OkText || verdict == NgText))
            {
                e.PaintBackground(e.ClipBounds, true);
                float scale = DeviceDpi / 96f;
                int width = (int)Math.Round(36 * scale);
                int height = (int)Math.Round(24 * scale);
                int left = e.CellBounds.Left + (int)Math.Round(7 * scale);
                int top = e.CellBounds.Top + (e.CellBounds.Height - height) / 2;
                ColorLabel.DrawBadge(e.Graphics, new Rectangle(left, top, width, height), verdict, Font,
                    theme, verdict == OkText ? BadgeTone.Success : BadgeTone.Error, DeviceDpi);
                e.Handled = true;
                return;
            }

            base.OnCellPainting(e);
        }

        private void ApplyTheme()
        {
            CardPalette palette = CardPalette.For(theme);
            BackgroundColor = palette.Surface;
            GridColor = palette.Border;
            DefaultCellStyle.BackColor = palette.Surface;
            DefaultCellStyle.ForeColor = palette.Text;
            DefaultCellStyle.SelectionBackColor = palette.AccentFill;
            DefaultCellStyle.SelectionForeColor = palette.Text;
            DefaultCellStyle.Padding = new Padding(7, 0, 0, 0);
            AlternatingRowsDefaultCellStyle.BackColor = palette.Input;
            ColumnHeadersDefaultCellStyle.BackColor = palette.Input;
            ColumnHeadersDefaultCellStyle.ForeColor = palette.MutedText;
            ColumnHeadersDefaultCellStyle.SelectionBackColor = palette.Input;
            ColumnHeadersDefaultCellStyle.SelectionForeColor = palette.MutedText;
        }
    }
}
