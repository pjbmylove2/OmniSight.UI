using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using OmniSight.UI.Controls;

namespace OmniSight.UI.Demo.Cards
{
    internal sealed partial class CardPanelDemoForm : Form
    {
        private const string DateFormat = "yyyy-MM-dd";
        private const string SampleDate = "2026-11-12";

        private readonly Font uiFont = new Font("Segoe UI", 9.5f);
        private readonly Font headingFont = new Font("Segoe UI", 15f, FontStyle.Bold);
        private readonly Font boldFont = new Font("Segoe UI", 9.5f, FontStyle.Bold);
        private readonly InspectionResult[] samples = InspectionResult.Samples();
        private readonly bool[] enabledInspections = Enumerable.Repeat(true, InspectionNames.Length).ToArray();

        private UiTheme theme;
        private CardPalette palette = CardPalette.For(UiTheme.Dark);
        private int selectedInspection = 2;
        private bool invalidDates;
        private bool updatingGrid;

        public CardPanelDemoForm()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            AutoScaleDimensions = new SizeF(96, 96);
            AutoScaleMode = AutoScaleMode.Dpi;
            Text = "검사 모니터 · CardPanel 데모";
            StartPosition = FormStartPosition.CenterScreen;
            Font = uiFont;
            ClientSize = new Size(1280, 780);
            MinimumSize = new Size(1040, 680);

            BuildLayout();
            themeButton.Click += (sender, args) =>
                ApplyTheme(theme == UiTheme.Dark ? UiTheme.Light : UiTheme.Dark);
            badgeGalleryButton.Click += (sender, args) =>
            {
                using (Form gallery = CreateBadgeGallery())
                    gallery.ShowDialog(this);
            };
            queryButton.Click += (sender, args) => RunQuery();
            resetButton.Click += (sender, args) => ResetFilters();
            settingsButton.Click += (sender, args) => ShowInspectionSettings();
            cameraInput.DrawItem += DrawComboItem;
            verdictInput.DrawItem += DrawComboItem;
            grid.CellPainting += PaintVerdictCell;
            grid.DpiChangedAfterParent += (sender, args) => ApplyGridMetrics();
            grid.CurrentCellChanged += (sender, args) =>
            {
                if (!updatingGrid)
                    ShowSelection();
            };

            ApplyGridMetrics();
            ApplyTheme(UiTheme.Dark);
            ResetFilters();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            ApplyGridMetrics();
        }

        protected override void Dispose(bool disposing)
        {
            bool disposeFonts = disposing && !IsDisposed;
            base.Dispose(disposing);
            if (disposeFonts)
            {
                boldFont.Dispose();
                headingFont.Dispose();
                uiFont.Dispose();
            }
        }

        private void ApplyTheme(UiTheme next)
        {
            theme = next;
            palette = CardPalette.For(next);
            SuspendLayout();
            try
            {
                BackColor = palette.Canvas;
                ForeColor = palette.Text;
                foreach (CardPanel card in new[] { alertsCard, inspectionCard, queryCard, resultsCard })
                    card.Theme = next;

                foreach (Label label in mutedLabels)
                    label.ForeColor = palette.MutedText;

                foreach (Control input in new Control[] { startInput, endInput, cameraInput, verdictInput })
                {
                    input.BackColor = palette.Input;
                    input.ForeColor = palette.Text;
                    input.Invalidate();
                }
                foreach (TextBox input in new[] { startInput, endInput })
                {
                    input.Parent.BackColor = palette.Input;
                    input.Parent.Invalidate();
                }
                cameraInput.Palette = verdictInput.Palette = palette;

                alertStatus.BackColor = palette.SuccessFill;
                alertStatus.ForeColor = palette.SuccessText;
                resultBadge.Theme = next;
                resultSummary.BackColor = palette.Input;
                cameraDetail.ForeColor = palette.MutedText;
                filterNote.ForeColor = invalidDates ? palette.ErrorText : palette.MutedText;

                themeButton.Text = theme == UiTheme.Dark ? "라이트 테마" : "다크 테마";
                StyleButton(themeButton, palette);
                StyleButton(badgeGalleryButton, palette);
                StyleButton(resetButton, palette);
                StyleButton(settingsButton, palette);
                StyleButton(queryButton, palette, emphasized: true);
                StyleInspectionItems();
                StyleGrid();
                ShowSelection();
            }
            finally
            {
                ResumeLayout(true);
            }
        }

        private void StyleInspectionItems()
        {
            for (int index = 0; index < inspectionButtons.Count; index++)
            {
                Button button = inspectionButtons[index];
                bool selected = index == selectedInspection;
                button.Text = InspectionNames[index] + (selected ? "  ›" : string.Empty);
                button.Enabled = enabledInspections[index];
                StyleButton(button, palette, selected, border: false);
            }
        }

        private static void StyleButton(Button button, CardPalette colors, bool emphasized = false,
            bool border = true)
        {
            button.BackColor = emphasized ? colors.AccentFill : colors.Input;
            button.ForeColor = emphasized ? colors.AccentText : colors.Text;
            button.FlatAppearance.BorderSize = border ? 1 : 0;
            button.FlatAppearance.BorderColor = emphasized ? colors.AccentText : colors.InputBorder;
            button.FlatAppearance.MouseOverBackColor = colors.AccentFill;
            button.FlatAppearance.MouseDownBackColor = colors.Surface;
        }

        private void StyleGrid()
        {
            grid.BackgroundColor = palette.Surface;
            grid.DefaultCellStyle.BackColor = palette.Surface;
            grid.DefaultCellStyle.ForeColor = palette.Text;
            grid.DefaultCellStyle.SelectionBackColor = palette.AccentFill;
            grid.DefaultCellStyle.SelectionForeColor = palette.Text;
            grid.AlternatingRowsDefaultCellStyle.BackColor = palette.Input;
            grid.ColumnHeadersDefaultCellStyle.BackColor = palette.Input;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = palette.MutedText;
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = palette.Input;
            grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = palette.MutedText;
            grid.ColumnHeadersDefaultCellStyle.Font = boldFont;
            grid.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;
            grid.Invalidate();
        }

        private void ApplyGridMetrics()
        {
            int dpi = grid.DeviceDpi;
            int rowHeight = (int)Math.Round(43 * dpi / 96d);
            int inset = (int)Math.Round(9 * dpi / 96d);
            grid.RowTemplate.Height = rowHeight;
            grid.ColumnHeadersHeight = (int)Math.Round(34 * dpi / 96d);
            grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(inset, 0, 0, 0);
            grid.DefaultCellStyle.Padding = new Padding(inset, 0, 0, 0);
            int cameraWidth = (int)Math.Round(58 * dpi / 96d);
            grid.Columns["cam1"].MinimumWidth = cameraWidth;
            grid.Columns["cam2"].MinimumWidth = cameraWidth;
            foreach (DataGridViewRow row in grid.Rows)
                row.Height = rowHeight;

            int comboHeight = (int)Math.Ceiling(Font.GetHeight(DeviceDpi)) + (int)Math.Round(9 * DeviceDpi / 96d);
            cameraInput.ItemHeight = comboHeight;
            verdictInput.ItemHeight = comboHeight;
        }

        private void DrawComboItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0)
                return;

            var combo = (ComboBox)sender;
            bool inEditor = (e.State & DrawItemState.ComboBoxEdit) != 0;
            bool selected = !inEditor && (e.State & DrawItemState.Selected) != 0;
            using (var brush = new SolidBrush(selected ? palette.AccentFill : palette.Input))
                e.Graphics.FillRectangle(brush, e.Bounds);

            Rectangle bounds = e.Bounds;
            bounds.Inflate(-3, 0);
            TextRenderer.DrawText(e.Graphics, combo.GetItemText(combo.Items[e.Index]), e.Font, bounds,
                selected ? palette.AccentText : palette.Text, TextFormatFlags.VerticalCenter
                    | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            if ((e.State & DrawItemState.Focus) != 0)
                e.DrawFocusRectangle();
        }

        private void PaintVerdictCell(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != grid.Columns["verdict"].Index)
                return;

            var text = e.FormattedValue as string;
            if (text != "OK" && text != "NG")
                return;

            int height = Math.Min(e.CellBounds.Height, (int)Math.Round(24 * grid.DeviceDpi / 96d));
            int width = Math.Min((int)Math.Round(40 * grid.DeviceDpi / 96d),
                e.CellBounds.Width - e.CellStyle.Padding.Left);
            var badge = new Rectangle(e.CellBounds.Left + e.CellStyle.Padding.Left,
                e.CellBounds.Top + (e.CellBounds.Height - height) / 2, width, height);
            e.PaintBackground(e.ClipBounds, true);

            GraphicsState state = e.Graphics.Save();
            try
            {
                e.Graphics.SetClip(Rectangle.Intersect(e.ClipBounds, e.CellBounds), CombineMode.Intersect);
                ColorLabel.DrawBadge(e.Graphics, badge, text, boldFont, theme,
                    text == "OK" ? BadgeTone.Success : BadgeTone.Error, grid.DeviceDpi);
            }
            finally
            {
                e.Graphics.Restore(state);
            }
            e.Paint(e.ClipBounds, DataGridViewPaintParts.Focus);
            e.Handled = true;
        }

        private void ResetFilters()
        {
            startInput.Text = SampleDate;
            endInput.Text = SampleDate;
            cameraInput.SelectedIndex = 0;
            verdictInput.SelectedIndex = 0;
            RunQuery();
        }

        private void RunQuery()
        {
            bool validStart = DateTime.TryParseExact(startInput.Text.Trim(), DateFormat,
                CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime start);
            bool validEnd = DateTime.TryParseExact(endInput.Text.Trim(), DateFormat,
                CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime end);
            invalidDates = !validStart || !validEnd || end < start;
            filterNote.ForeColor = invalidDates ? palette.ErrorText : palette.MutedText;
            if (invalidDates)
            {
                filterNote.Text = !validStart || !validEnd
                    ? "날짜를 yyyy-MM-dd 형식으로 입력하세요."
                    : "종료일은 시작일과 같거나 이후여야 합니다.";
                return;
            }

            filterNote.Text = string.Format(CultureInfo.InvariantCulture,
                "조회 범위 · {0:yyyy-MM-dd} 00:00:00 ~ {1:yyyy-MM-dd} 23:59:59", start, end);

            updatingGrid = true;
            grid.SuspendLayout();
            try
            {
                grid.Rows.Clear();
                foreach (InspectionResult sample in samples)
                {
                    if (!sample.Matches(start, end, cameraInput.SelectedIndex, verdictInput.SelectedIndex))
                        continue;

                    int index = grid.Rows.Add(sample.Time.ToString("HH:mm:ss", CultureInfo.InvariantCulture),
                        InspectionResult.VerdictText(sample.Cam1), InspectionResult.VerdictText(sample.Cam2),
                        sample.Height, sample.Width, sample.Coil, sample.Twist,
                        InspectionResult.VerdictText(sample.Passed));
                    grid.Rows[index].Tag = sample;
                }
                if (grid.Rows.Count > 0)
                    grid.CurrentCell = grid[0, 0];
            }
            finally
            {
                grid.ResumeLayout(true);
                updatingGrid = false;
            }

            resultsCard.HeaderRightText = $"{grid.Rows.Count}건 · 최신순 · 계측값 mm";
            ShowSelection();
        }

        private void ShowSelection()
        {
            var sample = grid.CurrentRow?.Tag as InspectionResult;
            resultBadge.Visible = sample != null;
            if (sample == null)
            {
                resultTitle.Text = "조회 결과 없음";
                resultDetail.Text = "현재 조건에 해당하는 샘플 기록이 없습니다.";
                resultDetail.ForeColor = palette.MutedText;
                cameraDetail.Text = string.Empty;
                return;
            }

            resultTitle.Text = "선택 결과 · " + sample.Time.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
            resultDetail.Text = sample.Defect ?? "검사 기준 충족";
            resultDetail.ForeColor = sample.Passed ? palette.SuccessText : palette.ErrorText;
            cameraDetail.Text = $"CAM 1 · 정면 {InspectionResult.VerdictText(sample.Cam1)}"
                + $"     CAM 2 · 상부 {InspectionResult.VerdictText(sample.Cam2)}";
            resultBadge.Text = "종합 " + InspectionResult.VerdictText(sample.Passed);
            resultBadge.Tone = sample.Passed ? BadgeTone.Success : BadgeTone.Error;
        }

        private void ShowInspectionSettings()
        {
            using (var dialog = new Form
            {
                Text = "검사 사용 설정", ClientSize = new Size(300, 320), Font = uiFont,
                BackColor = palette.Canvas, ForeColor = palette.Text, AutoScaleMode = AutoScaleMode.Dpi,
                FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false, MinimizeBox = false,
                ShowInTaskbar = false, StartPosition = FormStartPosition.CenterParent, Padding = new Padding(12)
            })
            {
                var options = new CheckedListBox
                {
                    Dock = DockStyle.Fill, CheckOnClick = true, BackColor = palette.Input, ForeColor = palette.Text
                };
                for (int index = 0; index < InspectionNames.Length; index++)
                    options.Items.Add(InspectionNames[index], enabledInspections[index]);

                Button apply = NewButton("적용");
                apply.Dock = DockStyle.Bottom;
                apply.Height = 32;
                apply.DialogResult = DialogResult.OK;
                StyleButton(apply, palette, emphasized: true);
                dialog.Controls.Add(options);
                dialog.Controls.Add(apply);
                dialog.AcceptButton = apply;
                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;

                for (int index = 0; index < enabledInspections.Length; index++)
                    enabledInspections[index] = options.GetItemChecked(index);
            }
            if (selectedInspection >= 0 && !enabledInspections[selectedInspection])
                selectedInspection = Array.IndexOf(enabledInspections, true);
            inspectionCard.HeaderRightText = $"{enabledInspections.Count(enabled => enabled)}개";
            StyleInspectionItems();
        }
    }
}
