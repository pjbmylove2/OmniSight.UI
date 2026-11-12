using System;

namespace OmniSight.UI.Controls
{
    /// <summary>장치 상태 스냅샷. 불변이라 스레드 사이에서 안전하게 전달할 수 있다.</summary>
    public sealed class DeviceStatusInfo
    {
        internal DeviceStatusInfo(string id, DeviceLinkState state, string message, DateTime? lastResponseUtc,
            int? latencyMs, int errorCount, DateTime changedUtc)
        {
            Id = id;
            State = state;
            Message = message;
            LastResponseUtc = lastResponseUtc;
            LatencyMs = latencyMs;
            ErrorCount = errorCount;
            ChangedUtc = changedUtc;
        }

        public string Id { get; }
        public DeviceLinkState State { get; }
        /// <summary>상세 설명. 보고된 메시지가 없으면 상태별 기본 문구.</summary>
        public string Message { get; }
        public DateTime? LastResponseUtc { get; }
        public int? LatencyMs { get; }
        /// <summary>오류·끊김으로 들어간 횟수.</summary>
        public int ErrorCount { get; }
        /// <summary>현재 상태로 바뀐 시각(UTC).</summary>
        public DateTime ChangedUtc { get; }
    }

    /// <summary>상태 변경 이력 한 건.</summary>
    public sealed class DeviceStatusHistoryItem
    {
        internal DeviceStatusHistoryItem(DateTime timeUtc, DeviceLinkState state, string message)
        {
            TimeUtc = timeUtc;
            State = state;
            Message = message;
        }

        public DateTime TimeUtc { get; }
        public DeviceLinkState State { get; }
        public string Message { get; }
    }
}
