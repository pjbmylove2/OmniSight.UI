using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using OmniSight.UI.Controls;

namespace OmniSight.UI.Demo.Cards
{
    internal sealed partial class CardPanelDemoForm
    {
        private static readonly string[] InspectionNames =
        {
            "카메라 1 제품 유무", "카메라 2 제품 유무", "터미널 높이", "터미널 폭", "코일 커팅",
            "터미널 덮개", "정면 AI", "튜브 높이", "홈이탈 AI", "튜브 AI"
        };

        private readonly CardPanel alertsCard = NewCard("장비 오류", "활성 0건");
        private readonly CardPanel inspectionCard = NewCard("검사 항목", "10개");
        private readonly CardPanel queryCard = NewCard("조회 조건", "MariaDB 연결됨 · 샘플");
        private readonly CardPanel resultsCard = NewCard("저장된 검사 결과");
        private readonly TextBox startInput = new TextBox { MaxLength = 10, AccessibleName = "시작일" };
        private readonly TextBox endInput = new TextBox { MaxLength = 10, AccessibleName = "종료일" };
        private readonly ThemedComboBox cameraInput = NewCombo("카메라", "전체", "CAM 1", "CAM 2");
        private readonly ThemedComboBox verdictInput = NewCombo("판정", "전체", "OK", "NG");
        private readonly Label filterNote = NewLabel(string.Empty);
        private readonly Label alertStatus = NewLabel("●  활성 장비 오류 없음");
        private readonly Label resultTitle = NewLabel(string.Empty);
        private readonly Label resultDetail = NewLabel(string.Empty);
        private readonly Label cameraDetail = NewLabel(string.Empty);
        private readonly ColorLabel resultBadge = new ColorLabel { Anchor = AnchorStyles.Right };
        private readonly BufferedGrid grid = new BufferedGrid();
        private readonly List<Button> inspectionButtons = new List<Button>();
        private readonly List<Label> mutedLabels = new List<Label>();
        private readonly Button themeButton = NewButton(string.Empty);
        private readonly Button badgeGalleryButton = NewButton("컬러 라벨");
        private readonly Button queryButton = NewButton("조회");
        private readonly Button resetButton = NewButton("조건 초기화");
        private readonly Button settingsButton = NewButton("검사 사용 설정");
        private TableLayoutPanel resultSummary;

        private void BuildLayout()
        {
            SuspendLayout();
            try
            {
                var root = NewTable(1);
                root.Padding = new Padding(14, 6, 14, 14);
                root.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
                root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
                var topBar = NewTable(3);
                topBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                topBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 116));
                topBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 116));
                var title = NewLabel("검사 모니터");
                title.Font = headingFont;
                topBar.Controls.Add(title, 0, 0);
                themeButton.Margin = new Padding(0, 4, 0, 7);
                badgeGalleryButton.Margin = new Padding(0, 4, 8, 7);
                topBar.Controls.Add(badgeGalleryButton, 1, 0);
                topBar.Controls.Add(themeButton, 2, 0);
                root.Controls.Add(topBar, 0, 0);

                var dashboard = NewTable(2);
                dashboard.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 244));
                dashboard.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                dashboard.Controls.Add(NewColumn(alertsCard, inspectionCard, 188, new Padding(0, 0, 6, 0)), 0, 0);
                dashboard.Controls.Add(NewColumn(queryCard, resultsCard, 166, new Padding(6, 0, 0, 0)), 1, 0);
                root.Controls.Add(dashboard, 0, 1);
                Controls.Add(root);

                BuildAlerts();
                BuildInspectionList();
                BuildQuery();
                BuildResults();
                AcceptButton = queryButton;
            }
            finally
            {
                ResumeLayout(true);
            }
        }

        private void BuildAlerts()
        {
            var alerts = NewTable(1);
            alerts.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            alerts.RowStyles.Add(new RowStyle(SizeType.Absolute, 27));
            alerts.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
            alerts.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));

            alertStatus.Padding = new Padding(8, 0, 6, 0);
            alertStatus.Font = boldFont;
            alerts.Controls.Add(alertStatus, 0, 0);
            alerts.Controls.Add(NewMutedLabel("최근 장비 알림"), 0, 1);
            alerts.Controls.Add(NewLabel("14:18:52  검사 시작"), 0, 2);
            alerts.Controls.Add(NewLabel("14:18:49  카메라 연결 완료"), 0, 3);
            alertsCard.Controls.Add(alerts);
        }

        private void BuildInspectionList()
        {
            inspectionCard.ShowHeaderDivider = true;
            var body = NewTable(1);
            body.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            body.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            var viewport = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Margin = Padding.Empty };
            var list = NewTable(1);
            list.Dock = DockStyle.Top;
            list.AutoSize = true;
            list.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            int row = 0;
            for (int index = 0; index < InspectionNames.Length; index++)
            {
                if (index == 0 || index == 2 || index == 8)
                {
                    string group = index == 0 ? "공통" : index == 2 ? "CAM 1 · 터미널 정면" : "CAM 2 · 터미널 상부";
                    list.RowStyles.Add(new RowStyle(SizeType.Absolute, 25));
                    Label label = NewMutedLabel(group);
                    label.Font = boldFont;
                    list.Controls.Add(label, 0, row++);
                }

                Button button = NewButton(InspectionNames[index]);
                button.TextAlign = ContentAlignment.MiddleLeft;
                button.Padding = new Padding(8, 0, 6, 0);
                button.Margin = new Padding(0, 0, 0, 4);
                int itemIndex = index;
                button.Click += (sender, args) =>
                {
                    selectedInspection = itemIndex;
                    StyleInspectionItems();
                };
                inspectionButtons.Add(button);
                list.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
                list.Controls.Add(button, 0, row++);
            }
            viewport.Controls.Add(list);
            settingsButton.Margin = new Padding(0, 6, 0, 0);
            body.Controls.Add(viewport, 0, 0);
            body.Controls.Add(settingsButton, 0, 1);
            inspectionCard.Controls.Add(body);
        }

        private void BuildQuery()
        {
            queryCard.HeaderRightStyle = HeaderRightStyle.SuccessBadge;
            var body = NewTable(6);
            foreach (int width in new[] { 21, 21, 15, 15, 12, 16 })
                body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, width));

            body.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
            body.RowStyles.Add(new RowStyle(SizeType.Absolute, 33));
            body.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            string[] names = { "시작일", "종료일", "카메라", "판정" };
            Control[] editors = { startInput, endInput, cameraInput, verdictInput };
            for (int index = 0; index < editors.Length; index++)
            {
                body.Controls.Add(NewMutedLabel(names[index]), index, 0);
                Control editor = editors[index];
                if (editor is TextBox input)
                    editor = WrapDateInput(input);
                editor.Margin = new Padding(0, 1, 9, 1);
                editor.Dock = DockStyle.Fill;
                body.Controls.Add(editor, index, 1);
            }
            queryButton.Margin = new Padding(0, 1, 8, 1);
            resetButton.Margin = new Padding(0, 1, 0, 1);
            body.Controls.Add(queryButton, 4, 1);
            body.Controls.Add(resetButton, 5, 1);
            body.Controls.Add(filterNote, 0, 2);
            body.SetColumnSpan(filterNote, 6);
            queryCard.Controls.Add(body);
        }

        private void BuildResults()
        {
            var body = NewTable(1);
            body.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            body.RowStyles.Add(new RowStyle(SizeType.Absolute, 84));

            grid.Dock = DockStyle.Fill;
            grid.Margin = new Padding(0, 0, 0, 8);
            grid.ReadOnly = true;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.AllowUserToResizeRows = false;
            grid.AllowUserToResizeColumns = false;
            grid.AllowUserToOrderColumns = false;
            grid.RowHeadersVisible = false;
            grid.MultiSelect = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.BorderStyle = BorderStyle.None;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.None;
            grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grid.EnableHeadersVisualStyles = false;
            grid.AccessibleName = "검사 결과 표";
            AddColumn("time", "시각", 95);
            AddColumn("cam1", "CAM 1", 65);
            AddColumn("cam2", "CAM 2", 65);
            AddColumn("height", "터미널 높이 좌/우", 130);
            AddColumn("width", "터미널 폭", 88);
            AddColumn("coil", "잔 코일 길이 좌/우", 136);
            AddColumn("twist", "들뜸량 좌/우", 115);
            AddColumn("verdict", "종합", 65);
            body.Controls.Add(grid, 0, 0);

            resultSummary = NewTable(2);
            resultSummary.Padding = new Padding(10, 5, 10, 5);
            resultSummary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            resultSummary.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            resultSummary.RowStyles.Add(new RowStyle(SizeType.Percent, 36));
            resultSummary.RowStyles.Add(new RowStyle(SizeType.Percent, 34));
            resultSummary.RowStyles.Add(new RowStyle(SizeType.Percent, 30));
            resultTitle.Font = boldFont;
            resultDetail.Font = boldFont;
            resultBadge.Font = boldFont;
            resultBadge.Margin = new Padding(3, 0, 0, 0);
            resultSummary.Controls.Add(resultTitle, 0, 0);
            resultSummary.Controls.Add(resultBadge, 1, 0);
            resultSummary.Controls.Add(resultDetail, 0, 1);
            resultSummary.SetColumnSpan(resultDetail, 2);
            resultSummary.Controls.Add(cameraDetail, 0, 2);
            resultSummary.SetColumnSpan(cameraDetail, 2);
            body.Controls.Add(resultSummary, 0, 1);
            resultsCard.Controls.Add(body);
        }

        private void AddColumn(string name, string header, int weight)
        {
            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = name,
                HeaderText = header,
                FillWeight = weight,
                MinimumWidth = 44,
                SortMode = DataGridViewColumnSortMode.NotSortable
            });
        }

        private Label NewMutedLabel(string text)
        {
            Label label = NewLabel(text);
            mutedLabels.Add(label);
            return label;
        }

        private static Label NewLabel(string text) => new Label
        {
            Text = text, AutoEllipsis = true, Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft, Margin = Padding.Empty, UseMnemonic = false
        };

        private static Button NewButton(string text) => new Button
        {
            Text = text, Dock = DockStyle.Fill, Margin = Padding.Empty, FlatStyle = FlatStyle.Flat,
            UseVisualStyleBackColor = false, UseMnemonic = false, AutoEllipsis = true
        };

        private Panel WrapDateInput(TextBox input)
        {
            var editor = new Panel { Padding = new Padding(8, 6, 8, 0) };
            input.BorderStyle = BorderStyle.None;
            input.Dock = DockStyle.Top;
            editor.Controls.Add(input);
            editor.Paint += (sender, args) =>
            {
                if (editor.Width < 1 || editor.Height < 1)
                    return;

                using (var pen = new Pen(input.Focused ? palette.AccentText : palette.InputBorder))
                    args.Graphics.DrawRectangle(pen, 0, 0, editor.Width - 1, editor.Height - 1);
            };
            input.Enter += (sender, args) => editor.Invalidate();
            input.Leave += (sender, args) => editor.Invalidate();
            editor.Click += (sender, args) => input.Focus();
            return editor;
        }

        private static ThemedComboBox NewCombo(string name, params string[] choices)
        {
            var combo = new ThemedComboBox
            {
                AccessibleName = name, DropDownStyle = ComboBoxStyle.DropDownList,
                DrawMode = DrawMode.OwnerDrawFixed
            };
            combo.Items.AddRange(choices);
            combo.SelectedIndex = 0;
            return combo;
        }

        private static TableLayoutPanel NewTable(int columns)
        {
            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = columns, Margin = Padding.Empty
            };
            if (columns == 1)
                table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            return table;
        }

        private static CardPanel NewCard(string title, string status = "") => new CardPanel
        {
            HeaderLeftText = title, HeaderRightText = status, Dock = DockStyle.Fill, Margin = Padding.Empty
        };

        private static TableLayoutPanel NewColumn(CardPanel first, CardPanel second, int height, Padding margin)
        {
            var column = NewTable(1);
            column.Margin = margin;
            column.RowStyles.Add(new RowStyle(SizeType.Absolute, height));
            column.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            first.Margin = new Padding(0, 0, 0, 12);
            column.Controls.Add(first, 0, 0);
            column.Controls.Add(second, 0, 1);
            return column;
        }

        private sealed class BufferedGrid : DataGridView
        {
            public BufferedGrid() => DoubleBuffered = true;
        }

        private sealed class ThemedComboBox : ComboBox
        {
            public ThemedComboBox()
            {
                SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                    | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            }

            public CardPalette Palette { get; set; } = CardPalette.For(UiTheme.Dark);

            protected override void OnPaint(PaintEventArgs e)
            {
                Rectangle bounds = ClientRectangle;
                if (bounds.Width < 2 || bounds.Height < 2)
                    return;

                int arrowWidth = (int)Math.Round(25 * DeviceDpi / 96d);
                int inset = (int)Math.Round(8 * DeviceDpi / 96d);
                using (var fill = new SolidBrush(Palette.Input))
                using (var border = new Pen(Focused ? Palette.AccentText : Palette.InputBorder))
                using (var arrow = new Pen(Palette.MutedText))
                {
                    e.Graphics.FillRectangle(fill, bounds);
                    e.Graphics.DrawRectangle(border, 0, 0, bounds.Width - 1, bounds.Height - 1);
                    int x = bounds.Right - arrowWidth / 2;
                    int y = bounds.Height / 2;
                    int size = Math.Max(2, (int)Math.Round(3 * DeviceDpi / 96d));
                    e.Graphics.DrawLine(arrow, x - size, y - 1, x, y + size - 1);
                    e.Graphics.DrawLine(arrow, x, y + size - 1, x + size, y - 1);
                }
                var text = new Rectangle(inset, 0, Math.Max(0, bounds.Width - arrowWidth - inset), bounds.Height);
                TextRenderer.DrawText(e.Graphics, GetItemText(SelectedItem), Font, text, Palette.Text,
                    TextFormatFlags.SingleLine | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis
                    | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                base.OnPaint(e);
            }

            protected override void OnSelectedIndexChanged(EventArgs e)
            {
                base.OnSelectedIndexChanged(e);
                Invalidate();
            }

            protected override void OnGotFocus(EventArgs e)
            {
                base.OnGotFocus(e);
                Invalidate();
            }

            protected override void OnLostFocus(EventArgs e)
            {
                base.OnLostFocus(e);
                Invalidate();
            }
        }
    }
}
