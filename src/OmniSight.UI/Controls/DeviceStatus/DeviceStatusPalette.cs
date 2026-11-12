using System.Drawing;

namespace OmniSight.UI.Controls
{
    /// <summary>
    /// 장치 상태 컨트롤의 색. 기본값은 Visual Studio 2022의 다크·라이트 테마 정의
    /// (Theme.Dark.pkgdef, Theme.Light.pkgdef)에서 읽은 값이다. 색을 바꾼 뒤에는
    /// <see cref="DeviceStatusStrip.RefreshPalette"/>를 호출한다.
    /// </summary>
    public sealed class DeviceStatusPalette
    {
        public DeviceStatusPalette(UiTheme theme)
        {
            Theme = theme;
            if (theme == UiTheme.Light)
            {
                Shell = Rgb(0xf5f5f5);          // Environment.ToolWindowBackground
                Surface = Rgb(0xececf0);        // CommonControls.Button
                Edge = Rgb(0xacacac);           // CommonControls.ButtonBorder
                Hover = Rgb(0xc9def5);          // CommonControls.ButtonHover
                HoverBorder = Rgb(0x006cbe);    // CommonControls.ButtonBorderHover
                Selected = Rgb(0xc9def5);       // CommonControls.ButtonFocused
                SelectedBorder = Rgb(0x2d2d2d); // CommonControls.ButtonBorderFocused
                Text = Rgb(0x1e1e1e);           // CommonControls.Button(글자)
                Muted = Rgb(0x717171);          // Environment.ToolTipHintText
                Accent = Rgb(0x006cbe);         // ThemedDialog.Hyperlink
                Ok = Rgb(0x2e7d32);             // CommonDocument.StatusBannerSuccessBorder
                Warn = Rgb(0x9d5d00);           // CommonDocument.StatusBannerWarningBorder
                WarnLamp = Rgb(0x9d5d00);
                Error = Rgb(0xd53230);          // CommonDocument.StatusBannerErrorBorder
                Track = Rgb(0xd6d8dc);          // Diagnostics.ToolWindowSplitBarBackground
                Popup = Rgb(0xfbfbfb);          // ThemedDialog.WindowPanel
                PopupList = Rgb(0xeeeef2);      // Environment.ToolWindowContentGrid
                PopupBorder = Rgb(0xcccedb);    // Environment.ToolWindowBorder
            }
            else
            {
                Shell = Rgb(0x1f1f1f);
                Surface = Rgb(0x3d3d3d);
                Edge = Rgb(0x3d3d3d);
                Hover = Rgb(0x4d4d4d);
                HoverBorder = Rgb(0x4d4d4d);
                Selected = Rgb(0x333333);
                SelectedBorder = Rgb(0x999999);
                Text = Rgb(0xfafafa);
                Muted = Rgb(0xb2b2b2);          // Environment.ToolWindowTabText
                Accent = Rgb(0x83beeb);
                Ok = Rgb(0x6ccb5f);
                Warn = Rgb(0xfce100);
                WarnLamp = Rgb(0xfce100);
                Error = Rgb(0xff99a4);
                Track = Rgb(0x424242);
                Popup = Rgb(0x242424);
                PopupList = Rgb(0x2e2e2e);
                PopupBorder = Rgb(0x3d3d3d);
            }
        }

        public UiTheme Theme { get; }
        /// <summary>헤더 등 바깥 배경.</summary>
        public Color Shell { get; set; }
        /// <summary>칩 바탕(보통).</summary>
        public Color Surface { get; set; }
        /// <summary>칩 테두리(보통).</summary>
        public Color Edge { get; set; }
        /// <summary>마우스를 올렸을 때 칩 바탕.</summary>
        public Color Hover { get; set; }
        public Color HoverBorder { get; set; }
        /// <summary>상세 팝업이 열려 있는 칩 바탕.</summary>
        public Color Selected { get; set; }
        public Color SelectedBorder { get; set; }
        public Color Text { get; set; }
        public Color Muted { get; set; }
        /// <summary>연결 중 회전 호와 링크 색.</summary>
        public Color Accent { get; set; }
        public Color Ok { get; set; }
        /// <summary>경고 글자색.</summary>
        public Color Warn { get; set; }
        /// <summary>경고 램프색.</summary>
        public Color WarnLamp { get; set; }
        public Color Error { get; set; }
        /// <summary>연결 중 회전 호의 바탕 고리.</summary>
        public Color Track { get; set; }
        public Color Popup { get; set; }
        /// <summary>팝업의 이력 목록 바탕.</summary>
        public Color PopupList { get; set; }
        public Color PopupBorder { get; set; }

        private static Color Rgb(int rgb)
        {
            return Color.FromArgb(255, (rgb >> 16) & 0xff, (rgb >> 8) & 0xff, rgb & 0xff);
        }
    }
}
