using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace OmniSight.UI.Controls
{
    /// <summary>
    /// 측정값이 허용 범위(하한~상한) 안에 있는지 막대로 보여 준다.
    /// 허용 범위는 막대 가운데 절반이고, 측정값은 세로 표시선으로 그린다.
    /// 범위를 벗어나면 표시선이 오류 색으로 바뀐다.
    /// </summary>
    [ToolboxItem(true)]
    public class RangeIndicator : Control
    {
        // 허용 범위가 막대에서 차지하는 구간(0~1). 범위 밖 값이 보일 여백을 양쪽에 남긴다.
        private const double RangeStart = 0.25;
        private const double RangeSpan = 0.5;
        private const double MinPosition = 0.01;
        private const double MaxPosition = 0.99;

        private UiTheme theme;
        private double position = 0.5;
        private bool failed;

        public RangeIndicator()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw
                | ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
            TabStop = false;
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

        /// <summary>마지막으로 설정한 측정값이 허용 범위를 벗어났는지 여부.</summary>
        [Browsable(false)]
        public bool Failed => failed;

        /// <summary>표시선의 가로 위치(0~1). 검사와 테스트에서 위치를 확인하는 용도다.</summary>
        [Browsable(false)]
        public double Position => position;

        /// <summary>측정값과 허용 범위를 설정한다. 하한이 상한 이상이면 예외를 던진다.</summary>
        public void SetMeasurement(double value, double low, double high)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
                throw new ArgumentOutOfRangeException(nameof(value));
            if (double.IsNaN(low) || double.IsNaN(high) || low >= high)
                throw new ArgumentException("low must be less than high.");

            double newPosition = RangeStart + RangeSpan * (value - low) / (high - low);
            newPosition = Math.Max(MinPosition, Math.Min(MaxPosition, newPosition));
            bool newFailed = value < low || value > high;

            if (newFailed == failed && newPosition.Equals(position))
                return;

            failed = newFailed;
            position = newPosition;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            CardPalette palette = CardPalette.For(theme);
            float scale = DeviceDpi / 96f;
            int trackHeight = Math.Max(2, (int)Math.Round(6 * scale));
            int markWidth = Math.Max(2, (int)Math.Round(3 * scale));
            int trackTop = Math.Max(0, (Height - trackHeight) / 2);
            int rangeLeft = (int)Math.Round(Width * RangeStart);
            int rangeWidth = (int)Math.Round(Width * RangeSpan);
            int markLeft = (int)Math.Round(Width * position) - markWidth / 2;

            using (var track = new SolidBrush(palette.Border))
            using (var range = new SolidBrush(palette.SuccessFill))
            using (var mark = new SolidBrush(failed ? palette.ErrorText : palette.SuccessText))
            {
                e.Graphics.FillRectangle(track, 0, trackTop, Width, trackHeight);
                e.Graphics.FillRectangle(range, rangeLeft, trackTop, rangeWidth, trackHeight);
                e.Graphics.FillRectangle(mark, markLeft, 0, markWidth, Height);
            }
        }
    }
}
