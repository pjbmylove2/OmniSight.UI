using System.Drawing;

namespace OmniSight.UI.Controls
{
    public enum DeviceStatusTheme
    {
        Dark = 0,
        Light = 1
    }

    /// <summary>
    /// 장치 상태 컨트롤의 색. 테마별 기본값으로 만들고, 필요하면 색을 바꾼 뒤
    /// <see cref="DeviceStatusStrip.RefreshPalette"/>를 호출한다.
    /// </summary>
    public sealed class DeviceStatusPalette
    {
        public DeviceStatusPalette(DeviceStatusTheme theme)
        {
            Theme = theme;
            if (theme == DeviceStatusTheme.Light)
            {
                Shell = Rgb(0xffffff); Surface = Rgb(0xf5f6f8); Edge = Rgb(0xc3cad5);
                Text = Rgb(0x202631); Muted = Rgb(0x626d7d); Accent = Rgb(0x215ba8); AccentBack = Rgb(0xe6effc);
                Ok = Rgb(0x08764f); Warn = Rgb(0x8a570a); WarnLamp = Rgb(0xd68a00); Error = Rgb(0xbc3434);
                Track = Rgb(0xd7dfe8); Popup = Rgb(0xffffff);
            }
            else
            {
                Shell = Rgb(0x1e1e1e); Surface = Rgb(0x252528); Edge = Rgb(0x404044);
                Text = Rgb(0xececec); Muted = Rgb(0xa0a0a5); Accent = Rgb(0x88baff); AccentBack = Rgb(0x26374e);
                Ok = Rgb(0x63d9aa); Warn = Rgb(0xffd08a); WarnLamp = Rgb(0xffd08a); Error = Rgb(0xf58a88);
                Track = Rgb(0x3b3c42); Popup = Rgb(0x2d2d30);
            }
        }

        public DeviceStatusTheme Theme { get; }
        /// <summary>헤더 등 바깥 배경.</summary>
        public Color Shell { get; set; }
        /// <summary>칩 바탕.</summary>
        public Color Surface { get; set; }
        public Color Edge { get; set; }
        public Color Text { get; set; }
        public Color Muted { get; set; }
        public Color Accent { get; set; }
        public Color AccentBack { get; set; }
        public Color Ok { get; set; }
        /// <summary>경고 글자색.</summary>
        public Color Warn { get; set; }
        /// <summary>경고 램프색. 라이트 테마에서 눈에 띄도록 글자색과 따로 둔다.</summary>
        public Color WarnLamp { get; set; }
        public Color Error { get; set; }
        public Color Track { get; set; }
        public Color Popup { get; set; }

        private static Color Rgb(int rgb)
        {
            return Color.FromArgb(255, (rgb >> 16) & 0xff, (rgb >> 8) & 0xff, rgb & 0xff);
        }
    }
}
