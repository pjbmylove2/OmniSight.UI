using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace OmniSight.UI.Controls
{
    /// <summary>장치 상태 상세 팝업. <see cref="DeviceStatusStrip"/>이 칩을 클릭하면 연다.</summary>
    [DesignerCategory("Form")]
    public sealed partial class DeviceStatusPopup : Form
    {
        private const int MaxShownHistory = 5;
        private readonly string _deviceId;
        private DeviceStatusPalette _palette;
        private List<DeviceStatusHistoryItem> _history = new List<DeviceStatusHistoryItem>();
        private long _historyKey = -1;
        private SolidBrush _itemBack = new SolidBrush(Color.Black);

        public DeviceStatusPopup() : this(new DeviceStatusPalette(DeviceStatusTheme.Dark), string.Empty, string.Empty)
        {
        }

        public DeviceStatusPopup(DeviceStatusPalette palette, string deviceId, string deviceName)
        {
            InitializeComponent();
            _deviceId = deviceId ?? string.Empty;
            lblTitle.Text = deviceName ?? string.Empty;
            _palette = palette ?? throw new ArgumentNullException(nameof(palette));
            ApplyPalette(palette);
        }

        public string DeviceId => _deviceId;

        /// <summary>재연결 버튼을 눌렀을 때. 실제 재연결은 장치 쪽에서 처리한다.</summary>
        public event EventHandler ReconnectRequested;

        public void ApplyPalette(DeviceStatusPalette palette)
        {
            _palette = palette ?? throw new ArgumentNullException(nameof(palette));
            BackColor = palette.Popup;
            ForeColor = palette.Text;
            lblTitle.BackColor = palette.Popup; lblTitle.ForeColor = palette.Text;
            lblState.BackColor = palette.Popup; lblState.ForeColor = palette.Muted;
            line.BackColor = palette.Edge;
            foreach (Label key in new[] { lblLinkKey, lblLastKey, lblLatencyKey, lblErrorsKey, lblHistoryTitle })
            {
                key.BackColor = palette.Popup; key.ForeColor = palette.Muted;
            }
            foreach (Label value in new[] { lblLink, lblLast, lblLatency, lblErrors })
            {
                value.BackColor = palette.Popup; value.ForeColor = palette.Text;
            }
            lstHistory.BackColor = palette.Surface;
            lstHistory.ForeColor = palette.Text;
            btnReconnect.Apply(palette);
            Invalidate(true);
        }

        /// <summary>표시 내용을 갱신한다. 값이 같으면 컨트롤을 건드리지 않는다.</summary>
        public void UpdateInfo(DeviceStatusInfo info, IReadOnlyList<DeviceStatusHistoryItem> history)
        {
            if (info == null) { throw new ArgumentNullException(nameof(info)); }
            lamp.Apply(_palette, info.State);
            SetText(lblState, info.State.ToDisplayText());
            lblState.ForeColor = StateColor(info.State);
            SetText(lblLink, info.Message ?? string.Empty);
            SetText(lblLast, FormatLast(info.LastResponseUtc));
            SetText(lblLatency, info.LatencyMs.HasValue ? info.LatencyMs.Value + " ms" : "-");
            SetText(lblErrors, info.ErrorCount + "회");
            if (history != null) { UpdateHistory(history); }
        }

        private Color StateColor(DeviceLinkState state)
        {
            switch (state)
            {
                case DeviceLinkState.Connected: return _palette.Ok;
                case DeviceLinkState.Warning: return _palette.Warn;
                case DeviceLinkState.Fault:
                case DeviceLinkState.Lost: return _palette.Error;
                case DeviceLinkState.Connecting: return _palette.Accent;
                default: return _palette.Muted;
            }
        }

        private static string FormatLast(DateTime? lastUtc)
        {
            if (!lastUtc.HasValue) { return "-"; }
            double seconds = Math.Max(0, (DateTime.UtcNow - lastUtc.Value).TotalSeconds);
            return lastUtc.Value.ToLocalTime().ToString("HH:mm:ss") + " (" + (int)seconds + "초 전)";
        }

        private static void SetText(Label label, string text)
        {
            if (label.Text != text) { label.Text = text; }
        }

        private void UpdateHistory(IReadOnlyList<DeviceStatusHistoryItem> history)
        {
            long key = history.Count == 0 ? 0 : history[history.Count - 1].TimeUtc.Ticks * 31 + history.Count;
            if (key == _historyKey) { return; }
            _historyKey = key;
            _history = new List<DeviceStatusHistoryItem>(history);
            lstHistory.BeginUpdate();
            lstHistory.Items.Clear();
            for (int i = _history.Count - 1; i >= 0 && lstHistory.Items.Count < MaxShownHistory; i--) { lstHistory.Items.Add(_history[i]); }
            lstHistory.EndUpdate();
        }

        private void HistoryDrawItem(object sender, DrawItemEventArgs e)
        {
            if (_itemBack.Color != _palette.Surface) { _itemBack.Color = _palette.Surface; }
            e.Graphics.FillRectangle(_itemBack, e.Bounds);
            if (e.Index < 0 || e.Index >= lstHistory.Items.Count) { return; }
            var item = (DeviceStatusHistoryItem)lstHistory.Items[e.Index];
            string text = item.TimeUtc.ToLocalTime().ToString("HH:mm:ss") + "  " + item.State.ToDisplayText() + "  " + item.Message;
            TextRenderer.DrawText(e.Graphics, text, Font,
                new Rectangle(e.Bounds.X + 8, e.Bounds.Y, e.Bounds.Width - 8, e.Bounds.Height),
                StateColor(item.State), _palette.Surface,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
        }

        private void ReconnectClick(object sender, EventArgs e)
        {
            EventHandler handler = ReconnectRequested;
            if (handler != null) { handler(this, EventArgs.Empty); }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (var pen = new Pen(_palette.Edge))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            }
        }

        protected override void OnDeactivate(EventArgs e)
        {
            base.OnDeactivate(e);
            if (!IsDisposed && !Disposing && Visible) { Close(); }
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (keyData == Keys.Escape) { Close(); return true; }
            return base.ProcessDialogKey(keyData);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null) { components.Dispose(); }
                if (_itemBack != null) { _itemBack.Dispose(); _itemBack = null; }
            }
            base.Dispose(disposing);
        }
    }
}
