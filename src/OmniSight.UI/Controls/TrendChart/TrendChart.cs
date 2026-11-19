using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace OmniSight.UI.Controls
{
    /// <summary>
    /// 측정값 추세 선 그래프. 선은 최대 두 개이고, 허용 범위는 세로 가운데 절반 띠로 그린다.
    /// 값이 범위를 벗어나도 그래프 밖으로 나가지 않도록 위아래 여백에서 잘린다.
    /// </summary>
    [ToolboxItem(true)]
    public class TrendChart : Control
    {
        // 허용 범위가 세로에서 차지하는 구간. 위 값이 상한, 아래 값이 하한이다.
        private const double RangeTop = 0.25;
        private const double RangeBottom = 0.75;
        private const double MinY = 0.04;
        private const double MaxY = 0.96;

        private UiTheme theme;
        private PointF[] first;
        private PointF[] second;

        public TrendChart()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
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

        /// <summary>첫 번째 선의 점 개수. 선이 없으면 0이다.</summary>
        [Browsable(false)]
        public int FirstCount => first == null ? 0 : first.Length;

        /// <summary>두 번째 선의 점 개수. 선이 없으면 0이다.</summary>
        [Browsable(false)]
        public int SecondCount => second == null ? 0 : second.Length;

        /// <summary>
        /// 선 데이터를 바꾼다. <paramref name="min"/>은 하한, <paramref name="max"/>는 상한이다.
        /// 점이 2개 미만인 선은 그리지 않는다. <paramref name="secondSamples"/>는 null일 수 있다.
        /// </summary>
        public void SetSeries(double[] firstSamples, double[] secondSamples, double min, double max)
        {
            if (double.IsNaN(min) || double.IsNaN(max) || min >= max)
                throw new ArgumentException("min must be less than max.");

            first = Project(firstSamples, min, max);
            second = Project(secondSamples, min, max);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            CardPalette palette = CardPalette.For(theme);
            Graphics g = e.Graphics;
            using (var background = new SolidBrush(palette.Input))
            using (var band = new SolidBrush(palette.SuccessFill))
            using (var edge = new Pen(Color.FromArgb(96, palette.SuccessText)))
            {
                g.FillRectangle(background, ClientRectangle);
                int top = (int)Math.Round(Height * RangeTop);
                int bottom = (int)Math.Round(Height * RangeBottom);
                g.FillRectangle(band, 0, top, Width, bottom - top);
                g.DrawLine(edge, 0, top, Width, top);
                g.DrawLine(edge, 0, bottom, Width, bottom);
            }

            g.SmoothingMode = SmoothingMode.AntiAlias;
            DrawSeries(g, first, palette.AccentText);
            DrawSeries(g, second, palette.SuccessText);
        }

        private static PointF[] Project(double[] samples, double min, double max)
        {
            if (samples == null || samples.Length < 2)
                return null;

            var points = new PointF[samples.Length];
            for (int i = 0; i < samples.Length; i++)
            {
                double value = samples[i];
                if (double.IsNaN(value) || double.IsInfinity(value))
                    throw new ArgumentOutOfRangeException(nameof(samples), "samples must be finite.");

                double y = RangeBottom - (RangeBottom - RangeTop) * (value - min) / (max - min);
                points[i] = new PointF((float)i / (samples.Length - 1), (float)Math.Max(MinY, Math.Min(MaxY, y)));
            }

            return points;
        }

        private void DrawSeries(Graphics g, PointF[] points, Color color)
        {
            if (points == null)
                return;

            float width = Math.Max(1f, 1.5f * DeviceDpi / 96f);
            using (var pen = new Pen(color, width))
            {
                var path = new PointF[points.Length];
                for (int i = 0; i < points.Length; i++)
                    path[i] = new PointF(points[i].X * (Width - 1), points[i].Y * Height);
                g.DrawLines(pen, path);
            }
        }
    }
}
