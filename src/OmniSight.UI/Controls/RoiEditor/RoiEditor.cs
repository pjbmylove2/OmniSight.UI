using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace OmniSight.UI.Controls
{
    /// <summary>
    /// 영상 위의 검사 영역(ROI) 하나를 끌어서 옮기고 크기를 조절하는 편집기.
    /// ROI는 영상 픽셀 좌표로 다루며, 영상은 컨트롤 안에 비율을 유지해 가운데 맞춤으로 그린다.
    /// 영역은 항상 영상 안에 있고 <see cref="MinimumRoiSize"/>보다 작아지지 않는다.
    /// </summary>
    [ToolboxItem(true)]
    [DefaultEvent(nameof(RoiChanged))]
    public class RoiEditor : Control
    {
        private const int HandleSize = 8;

        private enum Grip { None, Move, TopLeft, Top, TopRight, Right, BottomRight, Bottom, BottomLeft, Left }

        private UiTheme theme;
        private Image image;
        private Size imageSize = new Size(640, 480);
        private Rectangle roi = new Rectangle(0, 0, 100, 100);
        private int minimumRoiSize = 8;
        private Grip dragGrip;
        private Point dragStartImage;
        private Rectangle dragStartRoi;

        public RoiEditor()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            TabStop = false;
        }

        /// <summary>사용자가 끌어서 ROI가 바뀔 때, 또는 코드로 값을 바꿀 때 발생한다.</summary>
        public event EventHandler RoiChanged;

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

        /// <summary>배경으로 보여 줄 영상. 소유권은 호출한 쪽에 있고, 이 컨트롤은 해제하지 않는다.</summary>
        [Browsable(false)]
        public Image Image
        {
            get => image;
            set
            {
                image = value;
                if (value != null)
                    ImageSize = value.Size;
                Invalidate();
            }
        }

        /// <summary>영상 크기(픽셀). ROI 좌표의 범위를 정한다.</summary>
        [Category("OmniSight")]
        public Size ImageSize
        {
            get => imageSize;
            set
            {
                if (value.Width <= 0 || value.Height <= 0)
                    throw new ArgumentOutOfRangeException(nameof(value));
                if (imageSize == value)
                    return;

                imageSize = value;
                roi = Clamp(roi);
                Invalidate();
            }
        }

        /// <summary>검사 영역의 최소 너비·높이(픽셀).</summary>
        [Category("OmniSight"), DefaultValue(8)]
        public int MinimumRoiSize
        {
            get => minimumRoiSize;
            set
            {
                if (value < 1)
                    throw new ArgumentOutOfRangeException(nameof(value));
                minimumRoiSize = value;
                roi = Clamp(roi);
                Invalidate();
            }
        }

        /// <summary>영상 좌표의 검사 영역. 영상 밖이거나 너무 작은 값은 허용 범위로 맞춘다.</summary>
        [Browsable(false)]
        public Rectangle Roi
        {
            get => roi;
            set
            {
                Rectangle next = Clamp(value);
                if (next == roi)
                    return;

                roi = next;
                Invalidate();
                RoiChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>영상이 그려지는 컨트롤 안의 사각형(비율 유지, 가운데 맞춤).</summary>
        public Rectangle ImageBounds
        {
            get
            {
                if (Width <= 0 || Height <= 0)
                    return Rectangle.Empty;

                double scale = Math.Min((double)Width / imageSize.Width, (double)Height / imageSize.Height);
                int width = Math.Max(1, (int)Math.Round(imageSize.Width * scale));
                int height = Math.Max(1, (int)Math.Round(imageSize.Height * scale));
                return new Rectangle((Width - width) / 2, (Height - height) / 2, width, height);
            }
        }

        /// <summary>컨트롤 좌표를 영상 픽셀 좌표로 바꾼다. 영상 밖이면 가장자리로 맞춘다.</summary>
        public Point ToImage(Point control)
        {
            Rectangle bounds = ImageBounds;
            if (bounds.IsEmpty)
                return Point.Empty;

            int x = (int)Math.Round((control.X - bounds.Left) * (double)imageSize.Width / bounds.Width);
            int y = (int)Math.Round((control.Y - bounds.Top) * (double)imageSize.Height / bounds.Height);
            return new Point(Math.Max(0, Math.Min(imageSize.Width, x)), Math.Max(0, Math.Min(imageSize.Height, y)));
        }

        /// <summary>영상 픽셀 사각형을 컨트롤 좌표 사각형으로 바꾼다.</summary>
        public Rectangle ToControl(Rectangle imageRect)
        {
            Rectangle bounds = ImageBounds;
            double sx = (double)bounds.Width / imageSize.Width;
            double sy = (double)bounds.Height / imageSize.Height;
            int left = bounds.Left + (int)Math.Round(imageRect.Left * sx);
            int top = bounds.Top + (int)Math.Round(imageRect.Top * sy);
            int right = bounds.Left + (int)Math.Round(imageRect.Right * sx);
            int bottom = bounds.Top + (int)Math.Round(imageRect.Bottom * sy);
            return new Rectangle(left, top, right - left, bottom - top);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left)
                return;

            dragGrip = HitGrip(e.Location);
            if (dragGrip == Grip.None)
                return;

            Capture = true;
            dragStartImage = ToImage(e.Location);
            dragStartRoi = roi;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (dragGrip == Grip.None)
            {
                Cursor = CursorFor(HitGrip(e.Location));
                return;
            }

            Point now = ToImage(e.Location);
            int dx = now.X - dragStartImage.X;
            int dy = now.Y - dragStartImage.Y;
            Roi = ApplyDrag(dragStartRoi, dragGrip, dx, dy);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (dragGrip != Grip.None)
            {
                dragGrip = Grip.None;
                Capture = false;
            }
        }

        protected override void OnMouseCaptureChanged(EventArgs e)
        {
            base.OnMouseCaptureChanged(e);
            if (!Capture)
                dragGrip = Grip.None;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            CardPalette palette = CardPalette.For(theme);
            Rectangle bounds = ImageBounds;
            using (var back = new SolidBrush(palette.Input))
                e.Graphics.FillRectangle(back, ClientRectangle);

            if (image != null && !bounds.IsEmpty)
                e.Graphics.DrawImage(image, bounds);
            else if (!bounds.IsEmpty)
            {
                using (var empty = new SolidBrush(palette.Surface))
                    e.Graphics.FillRectangle(empty, bounds);
            }

            Rectangle box = ToControl(roi);
            using (var fill = new SolidBrush(Color.FromArgb(40, palette.AccentText)))
            using (var pen = new Pen(palette.AccentText, Math.Max(1f, DeviceDpi / 96f)))
            using (var handle = new SolidBrush(palette.AccentText))
            {
                e.Graphics.FillRectangle(fill, box);
                e.Graphics.DrawRectangle(pen, box);
                foreach (Rectangle h in Handles(box))
                    e.Graphics.FillRectangle(handle, h);
            }
        }

        private Rectangle[] Handles(Rectangle box)
        {
            int size = (int)Math.Round(HandleSize * DeviceDpi / 96f);
            int half = size / 2;
            int cx = box.Left + box.Width / 2;
            int cy = box.Top + box.Height / 2;
            Point[] centers =
            {
                new Point(box.Left, box.Top), new Point(cx, box.Top), new Point(box.Right, box.Top),
                new Point(box.Right, cy), new Point(box.Right, box.Bottom), new Point(cx, box.Bottom),
                new Point(box.Left, box.Bottom), new Point(box.Left, cy)
            };
            var result = new Rectangle[centers.Length];
            for (int i = 0; i < centers.Length; i++)
                result[i] = new Rectangle(centers[i].X - half, centers[i].Y - half, size, size);
            return result;
        }

        private Grip HitGrip(Point location)
        {
            Rectangle box = ToControl(roi);
            Grip[] order =
            {
                Grip.TopLeft, Grip.Top, Grip.TopRight, Grip.Right,
                Grip.BottomRight, Grip.Bottom, Grip.BottomLeft, Grip.Left
            };
            Rectangle[] handles = Handles(box);
            for (int i = 0; i < handles.Length; i++)
            {
                if (handles[i].Contains(location))
                    return order[i];
            }

            return box.Contains(location) ? Grip.Move : Grip.None;
        }

        private static Cursor CursorFor(Grip grip)
        {
            switch (grip)
            {
                case Grip.Move: return Cursors.SizeAll;
                case Grip.TopLeft:
                case Grip.BottomRight: return Cursors.SizeNWSE;
                case Grip.TopRight:
                case Grip.BottomLeft: return Cursors.SizeNESW;
                case Grip.Top:
                case Grip.Bottom: return Cursors.SizeNS;
                case Grip.Left:
                case Grip.Right: return Cursors.SizeWE;
                default: return Cursors.Default;
            }
        }

        private Rectangle ApplyDrag(Rectangle start, Grip grip, int dx, int dy)
        {
            int left = start.Left, top = start.Top, right = start.Right, bottom = start.Bottom;
            switch (grip)
            {
                case Grip.Move:
                    int width = start.Width, height = start.Height;
                    left = Math.Max(0, Math.Min(imageSize.Width - width, start.Left + dx));
                    top = Math.Max(0, Math.Min(imageSize.Height - height, start.Top + dy));
                    return new Rectangle(left, top, width, height);
                case Grip.TopLeft: left += dx; top += dy; break;
                case Grip.Top: top += dy; break;
                case Grip.TopRight: right += dx; top += dy; break;
                case Grip.Right: right += dx; break;
                case Grip.BottomRight: right += dx; bottom += dy; break;
                case Grip.Bottom: bottom += dy; break;
                case Grip.BottomLeft: left += dx; bottom += dy; break;
                case Grip.Left: left += dx; break;
            }

            // 끌지 않는 변은 고정하고, 끄는 변이 반대 변을 넘거나 영상 밖으로 나가지 않게 막는다.
            left = Math.Max(0, Math.Min(left, start.Right - minimumRoiSize));
            top = Math.Max(0, Math.Min(top, start.Bottom - minimumRoiSize));
            right = Math.Min(imageSize.Width, Math.Max(right, start.Left + minimumRoiSize));
            bottom = Math.Min(imageSize.Height, Math.Max(bottom, start.Top + minimumRoiSize));
            return Rectangle.FromLTRB(left, top, right, bottom);
        }

        private Rectangle Clamp(Rectangle value)
        {
            int minSize = Math.Min(minimumRoiSize, Math.Min(imageSize.Width, imageSize.Height));
            int width = Math.Max(minSize, Math.Min(imageSize.Width, value.Width));
            int height = Math.Max(minSize, Math.Min(imageSize.Height, value.Height));
            int left = Math.Max(0, Math.Min(imageSize.Width - width, value.Left));
            int top = Math.Max(0, Math.Min(imageSize.Height - height, value.Top));
            return new Rectangle(left, top, width, height);
        }
    }
}
