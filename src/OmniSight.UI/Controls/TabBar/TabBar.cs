using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace OmniSight.UI.Controls
{
    /// <summary>
    /// 화면 전환용 가로 탭 바. 선택된 탭은 강조 색으로 채운다.
    /// 마우스 클릭과 좌우 화살표·Home·End 키로 탭을 고른다.
    /// </summary>
    [ToolboxItem(true)]
    [DefaultEvent(nameof(SelectedIndexChanged))]
    public class TabBar : Control
    {
        private readonly List<string> tabs = new List<string>();
        private UiTheme theme;
        private int selectedIndex = -1;
        private int tabWidth = 153;

        public TabBar()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw
                | ControlStyles.Selectable, true);
            TabStop = true;
            Height = 48;
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

        /// <summary>탭 하나의 너비(96 DPI 기준 픽셀).</summary>
        [Category("OmniSight"), DefaultValue(153)]
        public int TabWidth
        {
            get => tabWidth;
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(value));
                if (tabWidth == value)
                    return;

                tabWidth = value;
                Invalidate();
            }
        }

        [Browsable(false)]
        public int Count => tabs.Count;

        /// <summary>선택된 탭 번호. 탭이 없으면 -1이다.</summary>
        [Browsable(false)]
        public int SelectedIndex
        {
            get => selectedIndex;
            set
            {
                if (value < -1 || value >= tabs.Count)
                    throw new ArgumentOutOfRangeException(nameof(value));
                if (selectedIndex == value)
                    return;

                selectedIndex = value;
                Invalidate();
                SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void SetTabs(IEnumerable<string> titles)
        {
            if (titles == null)
                throw new ArgumentNullException(nameof(titles));

            var copy = new List<string>(titles);
            if (copy.Contains(null))
                throw new ArgumentException("titles must not contain null.", nameof(titles));

            tabs.Clear();
            tabs.AddRange(copy);
            int next = tabs.Count == 0 ? -1 : Math.Max(0, Math.Min(selectedIndex, tabs.Count - 1));
            bool changed = next != selectedIndex;
            selectedIndex = next;
            Invalidate();
            if (changed)
                SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>해당 위치의 탭 번호. 탭 밖이면 -1이다.</summary>
        public int HitTest(Point location)
        {
            int width = ScaledTabWidth();
            if (location.Y < 0 || location.Y >= Height || location.X < 0)
                return -1;

            int index = location.X / width;
            return index < tabs.Count ? index : -1;
        }

        protected override bool IsInputKey(Keys keyData)
        {
            switch (keyData & Keys.KeyCode)
            {
                case Keys.Left:
                case Keys.Right:
                case Keys.Home:
                case Keys.End:
                    return true;
            }

            return base.IsInputKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (tabs.Count == 0)
                return;

            int target = selectedIndex;
            switch (e.KeyCode)
            {
                case Keys.Left: target = Math.Max(0, selectedIndex - 1); break;
                case Keys.Right: target = Math.Min(tabs.Count - 1, selectedIndex + 1); break;
                case Keys.Home: target = 0; break;
                case Keys.End: target = tabs.Count - 1; break;
                default: return;
            }

            e.Handled = true;
            SelectedIndex = target;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();
            int index = e.Button == MouseButtons.Left ? HitTest(e.Location) : -1;
            if (index >= 0)
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
            int width = ScaledTabWidth();
            int gap = (int)Math.Round(4 * scale);
            int margin = (int)Math.Round(8 * scale);

            using (var bar = new SolidBrush(palette.Surface))
                e.Graphics.FillRectangle(bar, ClientRectangle);

            for (int i = 0; i < tabs.Count; i++)
            {
                var bounds = new Rectangle(i * width + gap, margin, width - gap * 2, Height - margin * 2);
                bool selected = i == selectedIndex;
                if (selected)
                {
                    using (var fill = new SolidBrush(palette.AccentFill))
                        e.Graphics.FillRectangle(fill, bounds);
                    if (Focused && ShowFocusCues)
                        ControlPaint.DrawFocusRectangle(e.Graphics, Rectangle.Inflate(bounds, -2, -2), palette.AccentText, palette.AccentFill);
                }

                TextRenderer.DrawText(e.Graphics, tabs[i], Font, bounds, selected ? palette.AccentText : palette.Text,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            }
        }

        private int ScaledTabWidth()
        {
            return Math.Max(1, (int)Math.Round(tabWidth * DeviceDpi / 96f));
        }
    }
}
