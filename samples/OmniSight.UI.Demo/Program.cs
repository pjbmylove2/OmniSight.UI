using System;
using System.Drawing;
using System.Windows.Forms;
using OmniSight.UI.Controls;

namespace OmniSight.UI.Demo
{
    internal static class Program
    {
        [STAThread]
        private static int Main(string[] args)
        {
            Application.EnableVisualStyles();
            try
            {
                if (args.Length > 0 && args[0] == "--check")
                {
                    DemoChecks.RunChecks();
                    return 0;
                }
                if (args.Length > 1 && args[0] == "--capture")
                {
                    DemoChecks.RunCapture(args[1]);
                    return 0;
                }
            }
            catch (Exception error)
            {
                Console.Error.WriteLine(error);
                return 1;
            }

            Application.Run(new DemoForm());
            return 0;
        }
    }

    internal sealed class DemoForm : Form
    {
        private static readonly DeviceLinkState[] Cycle =
        {
            DeviceLinkState.Disabled, DeviceLinkState.Connecting, DeviceLinkState.Connected,
            DeviceLinkState.Warning, DeviceLinkState.Fault, DeviceLinkState.Lost
        };
        private readonly DemoSection _dark = new DemoSection(DeviceStatusTheme.Dark, false);
        private readonly DemoSection _light = new DemoSection(DeviceStatusTheme.Light, false);
        private int _step;

        public DemoForm()
        {
            Text = "OmniSight.UI Demo";
            ClientSize = new Size(1100, 330);
            var next = new ThemedButton { Text = "상태 순환", Left = 24, Top = 14, Width = 140, Height = 36 };
            next.Click += delegate { StepStates(); };
            var tools = new Panel { Dock = DockStyle.Top, Height = 64 };
            tools.Controls.Add(next);
            Controls.Add(_light);
            Controls.Add(_dark);
            Controls.Add(tools);
        }

        private void StepStates()
        {
            _step++;
            foreach (DemoSection section in new[] { _dark, _light })
            {
                for (int i = 0; i < DemoSection.DeviceIds.Length; i++)
                {
                    section.Strip.Registry.Report(DemoSection.DeviceIds[i], Cycle[(_step + i) % Cycle.Length], null, 100 + i * 30);
                }
                section.Strip.RefreshStatus();
            }
        }
    }
}
