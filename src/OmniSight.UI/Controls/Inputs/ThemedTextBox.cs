using System.ComponentModel;
using System.Windows.Forms;

namespace OmniSight.UI.Controls
{
    /// <summary>다크·라이트 테마 색을 적용하는 한 줄 입력 상자. 높이는 직접 지정할 수 있다.</summary>
    [ToolboxItem(true)]
    public class ThemedTextBox : TextBox
    {
        private UiTheme theme;

        public ThemedTextBox()
        {
            AutoSize = false;
            BorderStyle = BorderStyle.FixedSingle;
            ApplyTheme();
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
                ApplyTheme();
            }
        }

        private void ApplyTheme()
        {
            CardPalette palette = CardPalette.For(theme);
            BackColor = palette.Input;
            ForeColor = palette.Text;
        }
    }
}
