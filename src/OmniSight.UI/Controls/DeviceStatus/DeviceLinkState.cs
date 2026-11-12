using System;

namespace OmniSight.UI.Controls
{
    /// <summary>장치 연결 상태 6단계. 색뿐 아니라 램프 모양도 서로 다르다.</summary>
    public enum DeviceLinkState
    {
        /// <summary>사용 안 함(빈 원).</summary>
        Disabled = 0,
        /// <summary>연결 시도 중(회전 호).</summary>
        Connecting = 1,
        /// <summary>정상(초록 채움).</summary>
        Connected = 2,
        /// <summary>경고, 응답 지연(노랑 채움).</summary>
        Warning = 3,
        /// <summary>장치 오류(빨강 채움).</summary>
        Fault = 4,
        /// <summary>끊김, 응답 없음(빨강 이중 링).</summary>
        Lost = 5
    }

    /// <summary><see cref="DeviceLinkState"/> 표시 문구.</summary>
    public static class DeviceLinkStateText
    {
        /// <summary>짧은 상태 이름.</summary>
        public static string ToDisplayText(this DeviceLinkState state)
        {
            switch (state)
            {
                case DeviceLinkState.Disabled: return "미사용";
                case DeviceLinkState.Connecting: return "연결 중";
                case DeviceLinkState.Connected: return "정상";
                case DeviceLinkState.Warning: return "경고";
                case DeviceLinkState.Fault: return "오류";
                case DeviceLinkState.Lost: return "끊김";
                default: return state.ToString();
            }
        }

        /// <summary>메시지를 지정하지 않았을 때 쓰는 기본 설명.</summary>
        public static string ToDetailText(this DeviceLinkState state)
        {
            switch (state)
            {
                case DeviceLinkState.Disabled: return "사용 안 함";
                case DeviceLinkState.Connecting: return "연결 시도 중";
                case DeviceLinkState.Connected: return "연결됨";
                case DeviceLinkState.Warning: return "응답 지연";
                case DeviceLinkState.Fault: return "장치 오류";
                case DeviceLinkState.Lost: return "응답 없음";
                default: return state.ToString();
            }
        }
    }
}
