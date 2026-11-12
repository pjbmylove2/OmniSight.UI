using System.ComponentModel;
using System.Drawing;

namespace OmniSight.UI.Controls
{
    public enum HeaderRightStyle { Text, SuccessBadge }

    public sealed class CardPalette
    {
        private static readonly CardPalette Dark = new CardPalette(
            0x212121, 0x2D2D30, 0x404044, 0x252528, 0x59595F, 0xF0F0F0, 0xB4B4BC,
            0x26374E, 0x88BAFF, 0x233B32, 0x62E0B1, 0x482D2F, 0xFF9294);

        private static readonly CardPalette Light = new CardPalette(
            0xF2F2F4, 0xFFFFFF, 0xD1D1D7, 0xF6F6F8, 0xA3A3AD, 0x242429, 0x565661,
            0xE4EFFD, 0x174D91, 0xDDF3E7, 0x146140, 0xFBE7E8, 0x9C3036);

        private CardPalette(int canvas, int surface, int border, int input, int inputBorder,
            int text, int muted, int accentFill, int accentText, int successFill, int successText,
            int errorFill, int errorText)
        {
            Canvas = FromRgb(canvas);
            Surface = FromRgb(surface);
            Border = FromRgb(border);
            Input = FromRgb(input);
            InputBorder = FromRgb(inputBorder);
            Text = FromRgb(text);
            MutedText = FromRgb(muted);
            AccentFill = FromRgb(accentFill);
            AccentText = FromRgb(accentText);
            SuccessFill = FromRgb(successFill);
            SuccessText = FromRgb(successText);
            ErrorFill = FromRgb(errorFill);
            ErrorText = FromRgb(errorText);
        }

        public Color Canvas { get; }
        public Color Surface { get; }
        public Color Border { get; }
        public Color Input { get; }
        public Color InputBorder { get; }
        public Color Text { get; }
        public Color MutedText { get; }
        public Color AccentFill { get; }
        public Color AccentText { get; }
        public Color SuccessFill { get; }
        public Color SuccessText { get; }
        public Color ErrorFill { get; }
        public Color ErrorText { get; }

        public static CardPalette For(UiTheme theme)
        {
            switch (theme)
            {
                case UiTheme.Dark: return Dark;
                case UiTheme.Light: return Light;
                default: throw new InvalidEnumArgumentException(nameof(theme), (int)theme, typeof(UiTheme));
            }
        }

        private static Color FromRgb(int rgb)
        {
            return Color.FromArgb(unchecked((int)0xFF000000) | rgb);
        }
    }
}
