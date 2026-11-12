using System;
using System.Drawing;
using System.Windows.Forms;
using OmniSight.UI.Controls;

namespace OmniSight.UI.Demo
{
    /// <summary>한 테마의 헤더 막대(제목 + 장치 상태 표시줄)와 6단계 상태 범례를 보여 주는 데모 영역.</summary>
    internal sealed class DemoSection : Panel
    {
        public static readonly string[] DeviceIds = { "cam1", "cam2", "io", "light", "db" };
        private static readonly string[] DeviceNames = { "CAM 1", "CAM 2", "I/O 보드", "조명", "MariaDB" };
        private static readonly DeviceLinkState[] InitialStates =
        {
            DeviceLinkState.Connected, DeviceLinkState.Warning, DeviceLinkState.Connected,
            DeviceLinkState.Connecting, DeviceLinkState.Fault
        };

        private readonly Panel _bar = new Panel();
        private readonly Label _title = new Label();
        private readonly Label _legendCaption = new Label();
        private readonly Panel _legendBar = new Panel();

        public DemoSection(DeviceStatusTheme theme, bool large)
        {
            Theme = theme;
            var palette = new DeviceStatusPalette(theme);
            Font font = large ? new Font("Segoe UI", 11F) : new Font("Segoe UI", 9F);
            int chipHeight = large ? 36 : 28;
            int barHeight = large ? 72 : 56;

            Strip = new DeviceStatusStrip { Theme = theme, Font = font, ChipHeight = chipHeight, Name = "headerStrip" };
            Legend = new DeviceStatusStrip { Theme = theme, Font = font, ChipHeight = chipHeight, ShowDetailPopup = false, Name = "legendStrip" };

            BackColor = palette.Shell;
            Dock = DockStyle.Top;
            Height = barHeight + chipHeight + 56;

            _bar.Dock = DockStyle.Top;
            _bar.Height = barHeight;
            _bar.BackColor = palette.Shell;
            _title.Text = theme == DeviceStatusTheme.Dark ? "솔레노이드 밸브 검사기 · 다크 테마" : "솔레노이드 밸브 검사기 · 라이트 테마";
            _title.ForeColor = palette.Text;
            _title.Font = new Font("Segoe UI", large ? 14F : 12F, FontStyle.Bold);
            _title.AutoSize = true;
            _title.Location = new Point(24, (barHeight - _title.PreferredHeight) / 2);
            _bar.Controls.Add(_title);
            _bar.Controls.Add(Strip);

            _legendCaption.Text = "6단계 상태 (미사용 · 연결 중 · 정상 · 경고 · 오류 · 끊김)";
            _legendCaption.ForeColor = palette.Muted;
            _legendCaption.Font = font;
            _legendCaption.AutoSize = true;
            _legendCaption.Location = new Point(24, barHeight + 12);

            _legendBar.BackColor = palette.Shell;
            _legendBar.Bounds = new Rectangle(24, barHeight + 12 + _legendCaption.PreferredHeight + 8, 1000, chipHeight);
            _legendBar.Controls.Add(Legend);

            Controls.Add(_legendBar);
            Controls.Add(_legendCaption);
            Controls.Add(_bar);

            foreach (string name in new[] { "미사용", "연결 중", "정상", "경고", "오류", "끊김" })
            {
                Legend.AddDevice(name, name);
            }
            Legend.Registry.Report("미사용", DeviceLinkState.Disabled);
            Legend.Registry.Report("연결 중", DeviceLinkState.Connecting);
            Legend.Registry.Report("정상", DeviceLinkState.Connected);
            Legend.Registry.Report("경고", DeviceLinkState.Warning);
            Legend.Registry.Report("오류", DeviceLinkState.Fault);
            Legend.Registry.Report("끊김", DeviceLinkState.Lost);
            Legend.RefreshStatus();

            for (int i = 0; i < DeviceIds.Length; i++) { Strip.AddDevice(DeviceIds[i], DeviceNames[i]); }
            for (int i = 0; i < DeviceIds.Length; i++) { Strip.Registry.Report(DeviceIds[i], InitialStates[i], null, 120 + i * 40); }
            Strip.RefreshStatus();

            PlaceStrip();
            _bar.Resize += delegate { PlaceStrip(); };
            Strip.SizeChanged += delegate { PlaceStrip(); };
            Disposed += delegate { font.Dispose(); };
        }

        public DeviceStatusTheme Theme { get; }
        public DeviceStatusStrip Strip { get; }
        public DeviceStatusStrip Legend { get; }

        // 헤더 막대 오른쪽 끝에 세로 가운데로 놓는다.
        private void PlaceStrip()
        {
            Strip.Location = new Point(Math.Max(0, _bar.ClientSize.Width - Strip.Width - 24), (_bar.Height - Strip.Height) / 2);
        }
    }
}
