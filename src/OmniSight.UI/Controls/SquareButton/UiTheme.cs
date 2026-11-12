namespace OmniSight.UI.Controls
{
    /// <summary>Visual Studio 2022의 다크·라이트 테마.</summary>
    public enum UiTheme
    {
        Dark = 0,
        Light = 1
    }

    /// <summary><see cref="SquareButton"/>의 색 구성.</summary>
    public enum SquareButtonVariant
    {
        /// <summary>일반 버튼.</summary>
        Default = 0,
        /// <summary>기본 동작 버튼(VS의 ButtonDefault).</summary>
        Primary = 1,
        /// <summary>중지·삭제·초기화처럼 위험한 동작.</summary>
        Danger = 2
    }

    /// <summary>버튼이 가질 수 있는 시각 상태.</summary>
    public enum SquareButtonState
    {
        Normal = 0,
        Hover = 1,
        Pressed = 2,
        Focused = 3,
        Disabled = 4
    }
}
