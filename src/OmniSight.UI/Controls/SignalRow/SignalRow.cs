using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace OmniSight.UI.Controls
{
    /// <summary>
    /// 디지털 입출력 신호 한 줄. 채널 이름(예: "DI 00"), 신호 이름, 점등 램프와 ON/OFF 글자를 가로로 보여 준다.
    /// 값을 바꾸는 기능은 없고 상태 표시만 한다. 출력 강제 같은 조작은 이 컨트롤이 하지 않는다.
    /// </summary>
    [ToolboxItem(true)]
    [DefaultProperty(nameof(SignalName))]
    public class SignalRow : Control
    {
        private UiTheme theme;
        private string channel = string.Empty;
        private string signalName = string.Empty;
        private bool isOn;

        public SignalRow()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            TabStop = false;
            Height = 40;
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

        /// <summary>채널 이름. 예: "DI 00".</summary>
        [Category("OmniSight"), DefaultValue("")]
        public string Channel
        {
            get => channel;
            set
            {
                string next = value ?? string.Empty;
                if (channel == next)
                    return;

                channel = next;
                Invalidate();
            }
        }

        /// <summary>신호 이름. 예: "검사 트리거".</summary>
        [Category("OmniSight"), DefaultValue("")]
        public string SignalName
        {
            get => signalName;
            set
            {
                string next = value ?? string.Empty;
                if (signalName == next)
                    return;

                signalName = next;
                Invalidate();
            }
        }

        /// <summary>신호가 켜져 있는지 여부.</summary>
        [Category("OmniSight"), DefaultValue(false)]
        public bool IsOn
        {
            get => isOn;
            set
            {
                if (isOn == value)
                    return;

                isOn = value;
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            CardPalette palette = CardPalette.For(theme);
            float scale = DeviceDpi / 96f;
            int pad = (int)Math.Round(12 * scale);
            Color stateColor = isOn ? palette.SuccessText : palette.MutedText;

            using (var back = new SolidBrush(palette.Input))
                e.Graphics.FillRectangle(back, ClientRectangle);

            const TextFormatFlags flags = TextFormatFlags.Left | TextFormatFlags.VerticalCenter
                | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix;
            int nameLeft = (int)Math.Round(Width * 0.30);
            int stateLeft = (int)Math.Round(Width * 0.70);

            TextRenderer.DrawText(e.Graphics, channel, Font,
                new Rectangle(pad, 0, Math.Max(0, nameLeft - pad), Height), palette.MutedText, flags);
            TextRenderer.DrawText(e.Graphics, signalName, Font,
                new Rectangle(nameLeft, 0, Math.Max(0, stateLeft - nameLeft - pad), Height), palette.Text, flags);

            int lamp = (int)Math.Round(10 * scale);
            var lampBounds = new Rectangle(stateLeft, (Height - lamp) / 2, lamp, lamp);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (var lampBrush = new SolidBrush(stateColor))
                e.Graphics.FillEllipse(lampBrush, lampBounds);

            int textLeft = lampBounds.Right + (int)Math.Round(6 * scale);
            using (var bold = new Font(Font, FontStyle.Bold))
            {
                TextRenderer.DrawText(e.Graphics, isOn ? "ON" : "OFF", bold,
                    new Rectangle(textLeft, 0, Math.Max(0, Width - textLeft - pad), Height), stateColor, flags);
            }
        }
    }
}
