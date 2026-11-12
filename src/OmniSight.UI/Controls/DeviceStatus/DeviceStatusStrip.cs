using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace OmniSight.UI.Controls
{
    /// <summary>장치 칩 클릭 이벤트 인수.</summary>
    public sealed class DeviceStatusEventArgs : EventArgs
    {
        public DeviceStatusEventArgs(string deviceId, string deviceName, DeviceStatusInfo info)
        {
            DeviceId = deviceId;
            DeviceName = deviceName;
            Info = info;
        }

        public string DeviceId { get; }
        public string DeviceName { get; }
        public DeviceStatusInfo Info { get; }
    }

    /// <summary>
    /// 헤더에 놓는 장치 연결 상태 표시줄. 장치마다 램프 + 이름 칩 하나를 그리고, 클릭하면 상세 팝업을 연다.
    /// 장치 쪽은 <see cref="Registry"/>에 보고만 하면 되고, 타이머·갱신은 이 컨트롤이 처리한다.
    /// </summary>
    [ToolboxItem(true)]
    [DesignerCategory("Code")]
    [DefaultEvent("DeviceClicked")]
    [Description("장치 연결 상태를 램프와 이름 칩으로 표시하는 표시줄")]
    public class DeviceStatusStrip : Control
    {
        private const int IdleInterval = 250;
        private const int AnimationInterval = 80;

        private readonly List<DeviceStatusChip> _chips = new List<DeviceStatusChip>();
        private DeviceStatusRegistry _registry = new DeviceStatusRegistry();
        private DeviceStatusTheme _theme = DeviceStatusTheme.Dark;
        private DeviceStatusPalette _palette = new DeviceStatusPalette(DeviceStatusTheme.Dark);
        private int _chipHeight = 28;
        private int _chipSpacing = 8;
        private int _seenVersion = -1;
        private bool _showDetailPopup = true;
        private bool _sampleMode;
        private bool _fitting;
        private Timer _timer;
        private ToolTip _toolTip;
        private DeviceStatusPopup _popup;
        private string _popupDeviceId;

        public DeviceStatusStrip()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
            AutoSize = true;
        }

        /// <summary>칩을 눌렀을 때(상세 팝업을 열기 전).</summary>
        [Category("OmniSight")]
        public event EventHandler<DeviceStatusEventArgs> DeviceClicked;

        /// <summary>팝업의 재연결 버튼을 눌렀을 때. 실제 재연결은 장치 쪽에서 처리한다.</summary>
        [Category("OmniSight")]
        public event EventHandler<DeviceStatusEventArgs> ReconnectRequested;

        [Category("OmniSight")]
        [DefaultValue(DeviceStatusTheme.Dark)]
        [Description("다크·라이트 색 프리셋")]
        public DeviceStatusTheme Theme
        {
            get { return _theme; }
            set
            {
                if (_theme == value && _palette.Theme == value) { return; }
                _theme = value;
                Palette = new DeviceStatusPalette(value);
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DeviceStatusPalette Palette
        {
            get { return _palette; }
            set
            {
                _palette = value ?? throw new ArgumentNullException(nameof(value));
                RefreshPalette();
            }
        }

        /// <summary>팔레트 색을 직접 바꾼 뒤 호출하면 칩과 열린 팝업이 다시 칠해진다.</summary>
        public void RefreshPalette()
        {
            foreach (DeviceStatusChip chip in _chips) { chip.Palette = _palette; }
            if (_popup != null && !_popup.IsDisposed) { _popup.ApplyPalette(_palette); }
            Invalidate();
        }

        [Category("OmniSight")]
        [DefaultValue(28)]
        [Description("칩 높이(px). 램프 지름과 좌우 여백이 높이에 비례한다.")]
        public int ChipHeight
        {
            get { return _chipHeight; }
            set
            {
                if (value < 16 || value > 96) { throw new ArgumentOutOfRangeException(nameof(value), "16~96 사이여야 합니다."); }
                if (_chipHeight == value) { return; }
                _chipHeight = value;
                FitToContent();
            }
        }

        [Category("OmniSight")]
        [DefaultValue(8)]
        [Description("칩 사이 간격(px)")]
        public int ChipSpacing
        {
            get { return _chipSpacing; }
            set
            {
                if (value < 0 || value > 64) { throw new ArgumentOutOfRangeException(nameof(value)); }
                if (_chipSpacing == value) { return; }
                _chipSpacing = value;
                FitToContent();
            }
        }

        [Category("OmniSight")]
        [DefaultValue(true)]
        [Description("칩을 누르면 상세 팝업을 연다.")]
        public bool ShowDetailPopup
        {
            get { return _showDetailPopup; }
            set { _showDetailPopup = value; if (!value) { CloseDetail(); } }
        }

        [Browsable(true)]
        [DefaultValue(true)]
        public override bool AutoSize
        {
            get { return base.AutoSize; }
            set { base.AutoSize = value; if (value) { FitToContent(); } }
        }

        /// <summary>장치 상태 보고 창구. 다른 곳과 공유하려면 지정한다.</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DeviceStatusRegistry Registry
        {
            get { return _registry; }
            set
            {
                _registry = value ?? throw new ArgumentNullException(nameof(value));
                _seenVersion = -1;
                foreach (DeviceStatusChip chip in _chips) { _registry.Register(chip.DeviceId); }
                RefreshStatus();
            }
        }

        [Browsable(false)]
        public int DeviceCount => _sampleMode ? 0 : _chips.Count;

        protected override Size DefaultSize => new Size(300, 28);

        private bool IsDesignTime
        {
            get
            {
                return DesignMode || (Site != null && Site.DesignMode) ||
                    LicenseManager.UsageMode == LicenseUsageMode.Designtime;
            }
        }

        /// <summary>장치 칩을 추가한다. 초기 상태는 미사용이다.</summary>
        public void AddDevice(string id, string name)
        {
            if (string.IsNullOrEmpty(id)) { throw new ArgumentException("장치 ID가 비어 있습니다.", nameof(id)); }
            if (_sampleMode) { ClearChips(); _sampleMode = false; }
            if (FindChip(id) != null) { throw new ArgumentException("이미 추가된 장치입니다: " + id, nameof(id)); }
            _registry.Register(id);
            DeviceStatusChip chip = CreateChip(id, name);
            _chips.Add(chip);
            Controls.Add(chip);
            FitToContent();
            RefreshStatus();
        }

        public bool RemoveDevice(string id)
        {
            DeviceStatusChip chip = FindChip(id);
            if (chip == null) { return false; }
            if (_popupDeviceId == id) { CloseDetail(); }
            RemoveChip(chip);
            FitToContent();
            return true;
        }

        public void ClearDevices()
        {
            CloseDetail();
            ClearChips();
            _sampleMode = false;
            FitToContent();
        }

        /// <summary>자동 강등·승격을 반영하고 바뀐 상태를 칩에 적용한다. UI 스레드에서 호출한다.</summary>
        public void RefreshStatus()
        {
            if (InvokeRequired && IsHandleCreated)
            {
                BeginInvoke(new Action(RefreshStatus));
                return;
            }
            _registry.Evaluate();
            bool connecting = false;
            int version = _registry.Version;
            bool changed = version != _seenVersion;
            _seenVersion = version;
            foreach (DeviceStatusChip chip in _chips)
            {
                if (changed) { ApplyInfo(chip, _registry.GetInfo(chip.DeviceId)); }
                if (chip.State == DeviceLinkState.Connecting)
                {
                    connecting = true;
                    chip.Advance(30);
                }
            }
            if (_popup != null && !_popup.IsDisposed)
            {
                _popup.UpdateInfo(_registry.GetInfo(_popupDeviceId), _registry.GetHistory(_popupDeviceId));
            }
            if (_timer != null)
            {
                int interval = connecting ? AnimationInterval : IdleInterval;
                if (_timer.Interval != interval) { _timer.Interval = interval; }
            }
        }

        /// <summary>해당 장치의 상세 팝업을 연다. 이미 열려 있으면 닫고 다시 연다.</summary>
        public void ShowDetail(string id)
        {
            DeviceStatusChip chip = FindChip(id);
            if (chip == null) { throw new ArgumentException("알 수 없는 장치입니다: " + id, nameof(id)); }
            CloseDetail();
            var popup = new DeviceStatusPopup(_palette, id, chip.DeviceName);
            popup.ReconnectRequested += PopupReconnectRequested;
            popup.FormClosed += PopupFormClosed;
            popup.UpdateInfo(_registry.GetInfo(id), _registry.GetHistory(id));
            _popup = popup;
            _popupDeviceId = id;
            chip.Selected = true;

            Point location = chip.IsHandleCreated ? chip.PointToScreen(new Point(0, chip.Height + 2)) : Point.Empty;
            Rectangle area = Screen.FromControl(this).WorkingArea;
            location.X = Math.Max(area.Left, Math.Min(location.X, area.Right - popup.Width));
            location.Y = Math.Max(area.Top, Math.Min(location.Y, area.Bottom - popup.Height));
            popup.Location = location;
            popup.Show(FindForm());
        }

        public void CloseDetail()
        {
            DeviceStatusPopup popup = _popup;
            if (popup != null && !popup.IsDisposed) { popup.Close(); }
        }

        private void PopupFormClosed(object sender, FormClosedEventArgs e)
        {
            var popup = (DeviceStatusPopup)sender;
            popup.ReconnectRequested -= PopupReconnectRequested;
            popup.FormClosed -= PopupFormClosed;
            DeviceStatusChip chip = _popupDeviceId == null ? null : FindChip(_popupDeviceId);
            if (chip != null) { chip.Selected = false; }
            if (ReferenceEquals(_popup, popup)) { _popup = null; _popupDeviceId = null; }
        }

        private void PopupReconnectRequested(object sender, EventArgs e)
        {
            DeviceStatusChip chip = _popupDeviceId == null ? null : FindChip(_popupDeviceId);
            EventHandler<DeviceStatusEventArgs> handler = ReconnectRequested;
            if (chip != null && handler != null)
            {
                handler(this, new DeviceStatusEventArgs(chip.DeviceId, chip.DeviceName, _registry.GetInfo(chip.DeviceId)));
            }
        }

        private DeviceStatusChip CreateChip(string id, string name)
        {
            var chip = new DeviceStatusChip { DeviceId = id, DeviceName = name ?? id, Height = _chipHeight, Palette = _palette };
            chip.Click += ChipClick;
            return chip;
        }

        private void ChipClick(object sender, EventArgs e)
        {
            var chip = (DeviceStatusChip)sender;
            if (_sampleMode) { return; }
            DeviceStatusInfo info = _registry.GetInfo(chip.DeviceId);
            EventHandler<DeviceStatusEventArgs> handler = DeviceClicked;
            if (handler != null) { handler(this, new DeviceStatusEventArgs(chip.DeviceId, chip.DeviceName, info)); }
            if (_showDetailPopup) { ShowDetail(chip.DeviceId); }
        }

        private void ApplyInfo(DeviceStatusChip chip, DeviceStatusInfo info)
        {
            chip.State = info.State;
            string tip = chip.DeviceName + " · " + info.State.ToDisplayText() + " · " + info.Message;
            if (chip.ToolTipText != tip)
            {
                chip.ToolTipText = tip;
                if (_toolTip == null) { _toolTip = new ToolTip(); }
                _toolTip.SetToolTip(chip, tip);
            }
        }

        private DeviceStatusChip FindChip(string id)
        {
            foreach (DeviceStatusChip chip in _chips)
            {
                if (chip.DeviceId == id) { return chip; }
            }
            return null;
        }

        private void RemoveChip(DeviceStatusChip chip)
        {
            chip.Click -= ChipClick;
            if (_toolTip != null) { _toolTip.SetToolTip(chip, null); }
            _chips.Remove(chip);
            Controls.Remove(chip);
            chip.Dispose();
        }

        private void ClearChips()
        {
            foreach (DeviceStatusChip chip in _chips.ToArray()) { RemoveChip(chip); }
        }

        // 디자이너에서는 장치가 없어도 모양과 크기를 확인할 수 있게 상태별 샘플 칩을 보여 준다.
        private void EnsureDesignSamples()
        {
            if (_chips.Count > 0) { return; }
            _sampleMode = true;
            var samples = new[]
            {
                new KeyValuePair<string, DeviceLinkState>("정상", DeviceLinkState.Connected),
                new KeyValuePair<string, DeviceLinkState>("경고", DeviceLinkState.Warning),
                new KeyValuePair<string, DeviceLinkState>("오류", DeviceLinkState.Fault),
                new KeyValuePair<string, DeviceLinkState>("끊김", DeviceLinkState.Lost),
                new KeyValuePair<string, DeviceLinkState>("연결 중", DeviceLinkState.Connecting),
                new KeyValuePair<string, DeviceLinkState>("미사용", DeviceLinkState.Disabled)
            };
            foreach (KeyValuePair<string, DeviceLinkState> sample in samples)
            {
                DeviceStatusChip chip = CreateChip("sample:" + sample.Key, sample.Key);
                chip.State = sample.Value;
                _chips.Add(chip);
                Controls.Add(chip);
            }
            FitToContent();
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            int width = 0;
            foreach (DeviceStatusChip chip in _chips)
            {
                if (width > 0) { width += _chipSpacing; }
                width += MeasureChip(chip);
            }
            return new Size(Math.Max(width, 1), _chipHeight);
        }

        private int MeasureChip(DeviceStatusChip chip)
        {
            chip.Height = _chipHeight;
            return chip.GetPreferredSize(Size.Empty).Width;
        }

        private void FitToContent()
        {
            if (_fitting) { return; }
            _fitting = true;
            try
            {
                LayoutChips();
                if (AutoSize)
                {
                    Size preferred = GetPreferredSize(Size.Empty);
                    if (Size != preferred) { Size = preferred; }
                }
            }
            finally { _fitting = false; }
        }

        private void LayoutChips()
        {
            int x = 0;
            SuspendLayout();
            foreach (DeviceStatusChip chip in _chips)
            {
                int width = MeasureChip(chip);
                chip.SetBounds(x, 0, width, _chipHeight);
                x += width + _chipSpacing;
            }
            ResumeLayout(false);
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);
            if (!_fitting) { FitToContent(); }
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            foreach (DeviceStatusChip chip in _chips) { chip.Font = Font; }
            FitToContent();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (IsDesignTime)
            {
                EnsureDesignSamples();
                return;
            }
            EnsureTimer();
            _timer.Start();
            RefreshStatus();
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            if (_timer != null) { _timer.Stop(); }
            base.OnHandleDestroyed(e);
        }

        private void EnsureTimer()
        {
            if (_timer != null) { return; }
            _timer = new Timer { Interval = IdleInterval };
            _timer.Tick += TimerTick;
        }

        private void TimerTick(object sender, EventArgs e)
        {
            RefreshStatus();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                CloseDetail();
                if (_timer != null)
                {
                    _timer.Stop();
                    _timer.Tick -= TimerTick;
                    _timer.Dispose();
                    _timer = null;
                }
                foreach (DeviceStatusChip chip in _chips) { chip.Click -= ChipClick; }
                _chips.Clear();
                if (_toolTip != null) { _toolTip.Dispose(); _toolTip = null; }
            }
            base.Dispose(disposing);
        }
    }
}
