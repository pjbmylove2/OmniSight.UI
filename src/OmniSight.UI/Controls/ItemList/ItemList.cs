using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace OmniSight.UI.Controls
{
    /// <summary>
    /// 머리글(그룹 이름)과 선택 가능한 항목을 세로로 나열하는 목록. 검사 항목 선택 화면에 쓴다.
    /// 머리글은 선택할 수 없고, 선택된 항목은 강조 색으로 채운다. 스크롤은 없으며 필요한 높이는
    /// <see cref="PreferredHeight"/>로 알 수 있다.
    /// </summary>
    [ToolboxItem(true)]
    [DefaultEvent(nameof(SelectedIndexChanged))]
    public class ItemList : Control
    {
        private readonly List<Entry> entries = new List<Entry>();
        private UiTheme theme;
        private int selectedIndex = -1;

        public ItemList()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw
                | ControlStyles.Selectable, true);
            TabStop = true;
        }

        public event EventHandler SelectedIndexChanged;

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

        /// <summary>머리글을 포함한 전체 줄 수.</summary>
        [Browsable(false)]
        public int Count => entries.Count;

        /// <summary>선택된 줄 번호(머리글 포함 번호). 선택이 없으면 -1이다.</summary>
        [Browsable(false)]
        public int SelectedIndex
        {
            get => selectedIndex;
            set
            {
                if (value < -1 || value >= entries.Count)
                    throw new ArgumentOutOfRangeException(nameof(value));
                if (value >= 0 && entries[value].IsHeader)
                    throw new ArgumentException("A header cannot be selected.", nameof(value));
                if (selectedIndex == value)
                    return;

                selectedIndex = value;
                Invalidate();
                SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>선택된 항목의 글자. 선택이 없으면 null이다.</summary>
        [Browsable(false)]
        public string SelectedText => selectedIndex < 0 ? null : entries[selectedIndex].Text;

        /// <summary>모든 줄이 보이는 데 필요한 높이.</summary>
        [Browsable(false)]
        public int PreferredHeight
        {
            get
            {
                int height = 0;
                foreach (Entry entry in entries)
                    height += RowHeight(entry);
                return height;
            }
        }

        /// <summary>줄을 모두 바꾼다. 같은 글자의 항목이 남아 있으면 선택을 유지한다.</summary>
        public void SetEntries(IEnumerable<string> texts, Func<string, bool> isHeader)
        {
            if (texts == null)
                throw new ArgumentNullException(nameof(texts));
            if (isHeader == null)
                throw new ArgumentNullException(nameof(isHeader));

            var next = new List<Entry>();
            foreach (string text in texts)
            {
                if (text == null)
                    throw new ArgumentException("entries must not contain null.", nameof(texts));
                next.Add(new Entry(text, isHeader(text)));
            }

            string previous = SelectedText;
            entries.Clear();
            entries.AddRange(next);

            int index = -1;
            if (previous != null)
                index = entries.FindIndex(e => !e.IsHeader && e.Text == previous);
            if (index < 0)
                index = entries.FindIndex(e => !e.IsHeader);

            bool changed = index != selectedIndex || previous != (index < 0 ? null : entries[index].Text);
            selectedIndex = index;
            Invalidate();
            if (changed)
                SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>해당 위치의 줄 번호. 줄 밖이면 -1이다.</summary>
        public int HitTest(Point location)
        {
            if (location.X < 0 || location.X >= Width || location.Y < 0)
                return -1;

            int top = 0;
            for (int i = 0; i < entries.Count; i++)
            {
                int bottom = top + RowHeight(entries[i]);
                if (location.Y < bottom)
                    return i;
                top = bottom;
            }

            return -1;
        }

        protected override bool IsInputKey(Keys keyData)
        {
            switch (keyData & Keys.KeyCode)
            {
                case Keys.Up:
                case Keys.Down:
                case Keys.Home:
                case Keys.End:
                    return true;
            }

            return base.IsInputKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            int target;
            switch (e.KeyCode)
            {
                case Keys.Up: target = Neighbor(selectedIndex, -1); break;
                case Keys.Down: target = Neighbor(selectedIndex, 1); break;
                case Keys.Home: target = entries.FindIndex(x => !x.IsHeader); break;
                case Keys.End: target = entries.FindLastIndex(x => !x.IsHeader); break;
                default: return;
            }

            e.Handled = true;
            if (target >= 0)
                SelectedIndex = target;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();
            int index = e.Button == MouseButtons.Left ? HitTest(e.Location) : -1;
            if (index >= 0 && !entries[index].IsHeader)
                SelectedIndex = index;
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

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            CardPalette palette = CardPalette.For(theme);
            float scale = DeviceDpi / 96f;
            int pad = (int)Math.Round(10 * scale);
            int top = 0;

            for (int i = 0; i < entries.Count; i++)
            {
                Entry entry = entries[i];
                int height = RowHeight(entry);
                var bounds = new Rectangle(0, top, Width, height);
                top += height;
                if (bounds.Top >= Height)
                    break;

                if (entry.IsHeader)
                {
                    TextRenderer.DrawText(e.Graphics, entry.Text, Font, Rectangle.Inflate(bounds, -pad, 0), palette.MutedText,
                        TextFormatFlags.Left | TextFormatFlags.Bottom | TextFormatFlags.EndEllipsis);
                    continue;
                }

                var row = Rectangle.Inflate(bounds, 0, -(int)Math.Round(2 * scale));
                bool selected = i == selectedIndex;
                using (var fill = new SolidBrush(selected ? palette.AccentFill : palette.Input))
                    e.Graphics.FillRectangle(fill, row);
                if (selected && Focused && ShowFocusCues)
                    ControlPaint.DrawFocusRectangle(e.Graphics, Rectangle.Inflate(row, -2, -2), palette.AccentText, palette.AccentFill);

                TextRenderer.DrawText(e.Graphics, entry.Text, Font, Rectangle.Inflate(row, -pad, 0),
                    selected ? palette.AccentText : palette.Text,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            }
        }

        private int RowHeight(Entry entry)
        {
            float scale = DeviceDpi / 96f;
            return (int)Math.Round((entry.IsHeader ? 26 : 38) * scale);
        }

        // 머리글을 건너뛰고 방향대로 가장 가까운 항목을 찾는다. 없으면 현재 위치를 돌려준다.
        private int Neighbor(int from, int step)
        {
            for (int i = from + step; i >= 0 && i < entries.Count; i += step)
            {
                if (!entries[i].IsHeader)
                    return i;
            }

            return from;
        }

        private struct Entry
        {
            public Entry(string text, bool isHeader)
            {
                Text = text;
                IsHeader = isHeader;
            }

            public string Text { get; }
            public bool IsHeader { get; }
        }
    }
}
