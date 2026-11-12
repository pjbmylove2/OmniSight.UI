using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace OmniSight.UI.Controls
{
    /// <summary>파이프라인 검증용 최소 샘플 컨트롤. Theme 색상을 사용하는 플랫 버튼.</summary>
    [ToolboxItem(true)]
    [DesignerCategory("Code")]
    public class ThemedButton : Button
    {
        public ThemedButton()
        {
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            BackColor = Theme.Primary;
            ForeColor = Theme.OnPrimary;
            Font = Theme.DefaultFont;
            Cursor = Cursors.Hand;
        }
    }
}
