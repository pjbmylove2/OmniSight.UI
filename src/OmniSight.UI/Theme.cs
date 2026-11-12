using System.Drawing;

namespace OmniSight.UI
{
    /// <summary>공통 색상/폰트. 컨트롤은 하드코딩 대신 이 값을 사용한다.</summary>
    public static class Theme
    {
        public static Color Primary { get; } = Color.FromArgb(0, 120, 215);
        public static Color OnPrimary { get; } = Color.White;
        public static Font DefaultFont { get; } = new Font("Segoe UI", 9F);
    }
}
