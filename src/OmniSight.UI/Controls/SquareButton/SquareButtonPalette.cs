using System;
using System.Drawing;

namespace OmniSight.UI.Controls
{
    /// <summary>한 상태의 바탕·테두리·글자 색.</summary>
    public struct SquareButtonColors
    {
        public SquareButtonColors(Color back, Color border, Color text)
        {
            Back = back;
            Border = border;
            Text = text;
        }

        public Color Back { get; set; }
        public Color Border { get; set; }
        public Color Text { get; set; }
    }

    /// <summary>
    /// 변형 × 상태별 색. 기본값은 Visual Studio 2022 테마 정의(Theme.Dark.pkgdef, Theme.Light.pkgdef의
    /// CommonControls 범주)에서 읽은 값이다. 위험 변형과 다크의 기본 동작 올림·누름 색만 직접 정했다.
    /// </summary>
    public sealed class SquareButtonPalette
    {
        private const int Variants = 3;
        private const int States = 5;
        private readonly SquareButtonColors[,] _colors = new SquareButtonColors[Variants, States];

        public SquareButtonPalette(UiTheme theme)
        {
            Theme = theme;
            if (theme == UiTheme.Light) { FillLight(); } else { FillDark(); }
        }

        public UiTheme Theme { get; }

        /// <summary>창 바탕색(데모·문서용).</summary>
        public Color WindowBack { get; private set; }

        public SquareButtonColors Get(SquareButtonVariant variant, SquareButtonState state)
        {
            return _colors[(int)variant, (int)state];
        }

        public void Set(SquareButtonVariant variant, SquareButtonState state, SquareButtonColors colors)
        {
            _colors[(int)variant, (int)state] = colors;
        }

        private void Fill(SquareButtonVariant variant, SquareButtonState state, int back, int border, int text)
        {
            _colors[(int)variant, (int)state] = new SquareButtonColors(Rgb(back), Rgb(border), Rgb(text));
        }

        private void FillDark()
        {
            WindowBack = Rgb(0x1f1f1f);
            // Default: Button / ButtonHover / ButtonPressed / ButtonFocused / ButtonDisabled
            Fill(SquareButtonVariant.Default, SquareButtonState.Normal, 0x3d3d3d, 0x3d3d3d, 0xfafafa);
            Fill(SquareButtonVariant.Default, SquareButtonState.Hover, 0x4d4d4d, 0x4d4d4d, 0xfafafa);
            Fill(SquareButtonVariant.Default, SquareButtonState.Pressed, 0x333333, 0x333333, 0xfafafa);
            Fill(SquareButtonVariant.Default, SquareButtonState.Focused, 0x333333, 0x999999, 0xfafafa);
            Fill(SquareButtonVariant.Default, SquareButtonState.Disabled, 0x333333, 0x3d3d3d, 0x5c5c5c);
            // Primary: ButtonDefault. 올림·누름은 직접 정한 값(#403582를 밝게·어둡게).
            Fill(SquareButtonVariant.Primary, SquareButtonState.Normal, 0x403582, 0x403582, 0xfafafa);
            Fill(SquareButtonVariant.Primary, SquareButtonState.Hover, 0x4d4096, 0x4d4096, 0xfafafa);
            Fill(SquareButtonVariant.Primary, SquareButtonState.Pressed, 0x342a6b, 0x342a6b, 0xfafafa);
            Fill(SquareButtonVariant.Primary, SquareButtonState.Focused, 0x403582, 0x999999, 0xfafafa);
            Fill(SquareButtonVariant.Primary, SquareButtonState.Disabled, 0x333333, 0x3d3d3d, 0x5c5c5c);
            // Danger: VS 테마에 없는 변형. 글자·테두리만 오류색.
            Fill(SquareButtonVariant.Danger, SquareButtonState.Normal, 0x3d3d3d, 0xf48771, 0xf48771);
            Fill(SquareButtonVariant.Danger, SquareButtonState.Hover, 0x4d4d4d, 0xf48771, 0xf48771);
            Fill(SquareButtonVariant.Danger, SquareButtonState.Pressed, 0x333333, 0xf48771, 0xf48771);
            Fill(SquareButtonVariant.Danger, SquareButtonState.Focused, 0x333333, 0x999999, 0xf48771);
            Fill(SquareButtonVariant.Danger, SquareButtonState.Disabled, 0x333333, 0x3d3d3d, 0x5c5c5c);
        }

        private void FillLight()
        {
            WindowBack = Rgb(0xf5f5f5);
            Fill(SquareButtonVariant.Default, SquareButtonState.Normal, 0xececf0, 0xacacac, 0x1e1e1e);
            Fill(SquareButtonVariant.Default, SquareButtonState.Hover, 0xc9def5, 0x006cbe, 0x1e1e1e);
            Fill(SquareButtonVariant.Default, SquareButtonState.Pressed, 0x006cbe, 0x006cbe, 0xffffff);
            Fill(SquareButtonVariant.Default, SquareButtonState.Focused, 0xc9def5, 0x2d2d2d, 0x1e1e1e);
            Fill(SquareButtonVariant.Default, SquareButtonState.Disabled, 0xf5f5f5, 0xcccedb, 0xa2a4a5);
            // Primary: ButtonDefault(바탕은 일반과 같고 테두리만 #3399ff)
            Fill(SquareButtonVariant.Primary, SquareButtonState.Normal, 0xececf0, 0x3399ff, 0x1e1e1e);
            Fill(SquareButtonVariant.Primary, SquareButtonState.Hover, 0xc9def5, 0x006cbe, 0x1e1e1e);
            Fill(SquareButtonVariant.Primary, SquareButtonState.Pressed, 0x006cbe, 0x006cbe, 0xffffff);
            Fill(SquareButtonVariant.Primary, SquareButtonState.Focused, 0xc9def5, 0x2d2d2d, 0x1e1e1e);
            Fill(SquareButtonVariant.Primary, SquareButtonState.Disabled, 0xf5f5f5, 0xcccedb, 0xa2a4a5);
            // Danger: VS 테마에 없는 변형. 글자·테두리만 오류색.
            Fill(SquareButtonVariant.Danger, SquareButtonState.Normal, 0xececf0, 0xe51400, 0xc21000);
            Fill(SquareButtonVariant.Danger, SquareButtonState.Hover, 0xc9def5, 0xe51400, 0xc21000);
            Fill(SquareButtonVariant.Danger, SquareButtonState.Pressed, 0x006cbe, 0x006cbe, 0xffffff);
            Fill(SquareButtonVariant.Danger, SquareButtonState.Focused, 0xc9def5, 0x2d2d2d, 0xc21000);
            Fill(SquareButtonVariant.Danger, SquareButtonState.Disabled, 0xf5f5f5, 0xcccedb, 0xa2a4a5);
        }

        private static Color Rgb(int rgb)
        {
            return Color.FromArgb(255, (rgb >> 16) & 0xff, (rgb >> 8) & 0xff, rgb & 0xff);
        }
    }
}
