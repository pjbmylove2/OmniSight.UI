using System;
using System.Collections.Generic;
using System.Threading;

namespace OmniSight.UI.Controls
{
    /// <summary>
    /// 장치 상태 보고 창구. 장치·통신 스레드는 <see cref="Report"/>/<see cref="Heartbeat"/>만 호출하면 되고 UI를 건드리지 않는다.
    /// 표시줄은 <see cref="Version"/>이 바뀔 때만 상태를 읽는다. 모든 메서드는 스레드 안전하다.
    /// </summary>
    public sealed class DeviceStatusRegistry
    {
        public const int HistoryCapacity = 50;

        private sealed class Entry
        {
            public string Id;
            public DeviceLinkState Reported = DeviceLinkState.Disabled;
            public string Message;
            public DateTime? LastResponse;
            public int? Latency;
            public int Errors;
            public DeviceLinkState Effective = DeviceLinkState.Disabled;
            public DateTime EffectiveSince;
            public DateTime? OkSince;
            public readonly Queue<DeviceStatusHistoryItem> History = new Queue<DeviceStatusHistoryItem>();
        }

        private readonly object _sync = new object();
        private readonly Dictionary<string, Entry> _entries = new Dictionary<string, Entry>(StringComparer.Ordinal);
        private readonly List<string> _order = new List<string>();
        private int _version;
        private TimeSpan _warningAfter = TimeSpan.FromSeconds(3);
        private TimeSpan _lostAfter = TimeSpan.FromSeconds(10);
        private TimeSpan _promoteAfter = TimeSpan.FromMilliseconds(500);

        /// <summary>상태가 바뀌거나 보고가 들어올 때마다 증가한다.</summary>
        public int Version => Volatile.Read(ref _version);

        /// <summary>마지막 응답 이후 이 시간이 지나면 경고(기본 3초).</summary>
        public TimeSpan WarningAfter
        {
            get { lock (_sync) { return _warningAfter; } }
            set { Require(value); lock (_sync) { _warningAfter = value; } }
        }

        /// <summary>마지막 응답 이후 이 시간이 지나면 끊김(기본 10초).</summary>
        public TimeSpan LostAfter
        {
            get { lock (_sync) { return _lostAfter; } }
            set { Require(value); lock (_sync) { _lostAfter = value; } }
        }

        /// <summary>경고·끊김에서 정상으로 올라가기 전에 응답이 안정적으로 이어져야 하는 시간(기본 0.5초).</summary>
        public TimeSpan PromoteAfter
        {
            get { lock (_sync) { return _promoteAfter; } }
            set
            {
                if (value < TimeSpan.Zero) { throw new ArgumentOutOfRangeException(nameof(value)); }
                lock (_sync) { _promoteAfter = value; }
            }
        }

        public void Register(string id, DeviceLinkState initial = DeviceLinkState.Disabled)
        {
            lock (_sync)
            {
                Entry entry = GetOrCreate(id);
                entry.Reported = initial;
                Apply(entry, DateTime.UtcNow);
                Bump();
            }
        }

        /// <summary>상태를 보고한다. 정상·경고 보고는 응답으로도 취급한다.</summary>
        public void Report(string id, DeviceLinkState state, string message = null, int? latencyMs = null, DateTime? utcNow = null)
        {
            DateTime now = utcNow ?? DateTime.UtcNow;
            lock (_sync)
            {
                Entry entry = GetOrCreate(id);
                entry.Reported = state;
                entry.Message = message;
                if (latencyMs.HasValue) { entry.Latency = latencyMs; }
                if (state == DeviceLinkState.Connected || state == DeviceLinkState.Warning) { entry.LastResponse = now; }
                Apply(entry, now);
                Bump();
            }
        }

        /// <summary>응답이 왔음을 알린다. 연결 중·끊김으로 보고된 장치는 정상으로 바뀐다.</summary>
        public void Heartbeat(string id, int? latencyMs = null, DateTime? utcNow = null)
        {
            DateTime now = utcNow ?? DateTime.UtcNow;
            lock (_sync)
            {
                Entry entry = GetOrCreate(id);
                entry.LastResponse = now;
                if (latencyMs.HasValue) { entry.Latency = latencyMs; }
                if (entry.Reported == DeviceLinkState.Connecting || entry.Reported == DeviceLinkState.Lost)
                {
                    entry.Reported = DeviceLinkState.Connected;
                    entry.Message = null;
                }
                Apply(entry, now);
                Bump();
            }
        }

        /// <summary>시간 경과에 따른 자동 강등·승격을 반영한다. 하나라도 바뀌면 true.</summary>
        public bool Evaluate(DateTime? utcNow = null)
        {
            DateTime now = utcNow ?? DateTime.UtcNow;
            bool changed = false;
            lock (_sync)
            {
                foreach (Entry entry in _entries.Values) { changed |= Apply(entry, now); }
                if (changed) { Bump(); }
            }
            return changed;
        }

        public DeviceStatusInfo GetInfo(string id)
        {
            lock (_sync)
            {
                Entry entry;
                if (!_entries.TryGetValue(id, out entry))
                {
                    return new DeviceStatusInfo(id, DeviceLinkState.Disabled, DeviceLinkState.Disabled.ToDetailText(), null, null, 0, DateTime.UtcNow);
                }
                string message = string.IsNullOrEmpty(entry.Message) || entry.Effective != entry.Reported
                    ? entry.Effective.ToDetailText() : entry.Message;
                return new DeviceStatusInfo(entry.Id, entry.Effective, message, entry.LastResponse, entry.Latency, entry.Errors, entry.EffectiveSince);
            }
        }

        /// <summary>최근 상태 변경 이력(오래된 것부터). 호출자에게 복사본을 돌려준다.</summary>
        public IReadOnlyList<DeviceStatusHistoryItem> GetHistory(string id)
        {
            lock (_sync)
            {
                Entry entry;
                return _entries.TryGetValue(id, out entry)
                    ? entry.History.ToArray() : new DeviceStatusHistoryItem[0];
            }
        }

        public IReadOnlyList<string> GetIds()
        {
            lock (_sync) { return _order.ToArray(); }
        }

        private static void Require(TimeSpan value)
        {
            if (value <= TimeSpan.Zero) { throw new ArgumentOutOfRangeException(nameof(value)); }
        }

        private void Bump() { Interlocked.Increment(ref _version); }

        private Entry GetOrCreate(string id)
        {
            if (string.IsNullOrEmpty(id)) { throw new ArgumentException("장치 ID가 비어 있습니다.", nameof(id)); }
            Entry entry;
            if (!_entries.TryGetValue(id, out entry))
            {
                entry = new Entry { Id = id, EffectiveSince = DateTime.UtcNow };
                _entries.Add(id, entry);
                _order.Add(id);
            }
            return entry;
        }

        // 호출자가 _sync를 잡고 있어야 한다.
        private bool Apply(Entry entry, DateTime now)
        {
            DeviceLinkState candidate;
            switch (entry.Reported)
            {
                case DeviceLinkState.Connected:
                case DeviceLinkState.Warning:
                    double age = entry.LastResponse.HasValue ? (now - entry.LastResponse.Value).TotalMilliseconds : 0;
                    if (age >= _lostAfter.TotalMilliseconds) { candidate = DeviceLinkState.Lost; }
                    else if (age >= _warningAfter.TotalMilliseconds || entry.Reported == DeviceLinkState.Warning) { candidate = DeviceLinkState.Warning; }
                    else { candidate = DeviceLinkState.Connected; }
                    break;
                default:
                    candidate = entry.Reported;
                    break;
            }

            bool degraded = entry.Effective == DeviceLinkState.Warning || entry.Effective == DeviceLinkState.Lost;
            if (candidate == DeviceLinkState.Connected && degraded)
            {
                if (!entry.OkSince.HasValue) { entry.OkSince = now; }
                if (now - entry.OkSince.Value < _promoteAfter) { candidate = entry.Effective; }
                else { entry.OkSince = null; }
            }
            else
            {
                entry.OkSince = null;
            }

            if (candidate == entry.Effective) { return false; }

            bool wasError = entry.Effective == DeviceLinkState.Fault || entry.Effective == DeviceLinkState.Lost;
            bool isError = candidate == DeviceLinkState.Fault || candidate == DeviceLinkState.Lost;
            if (isError && !wasError) { entry.Errors++; }
            entry.Effective = candidate;
            entry.EffectiveSince = now;
            string message = string.IsNullOrEmpty(entry.Message) || candidate != entry.Reported ? candidate.ToDetailText() : entry.Message;
            entry.History.Enqueue(new DeviceStatusHistoryItem(now, candidate, message));
            while (entry.History.Count > HistoryCapacity) { entry.History.Dequeue(); }
            return true;
        }
    }
}
