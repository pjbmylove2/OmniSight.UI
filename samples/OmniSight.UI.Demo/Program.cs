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
                    ButtonChecks.RunChecks();
                    return 0;
                }
                if (args.Length > 1 && args[0] == "--capture")
                {
                    DemoChecks.RunCapture(args[1]);
                    ButtonChecks.RunCapture(args[1]);
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
        private readonly DemoSection _dark = new DemoSection(UiTheme.Dark, false);
        private readonly DemoSection _light = new DemoSection(UiTheme.Light, false);
        private int _step;

        public DemoForm()
        {
            Text = "OmniSight.UI Demo";
            AutoScroll = true;
            ClientSize = new Size(1100, 700);

            var buttonsDark = new ButtonShowcase(UiTheme.Dark, false) { Dock = DockStyle.Top };
            var buttonsLight = new ButtonShowcase(UiTheme.Light, false) { Dock = DockStyle.Top };
            var next = new SquareButton { Text = "상태 순환", Left = 24, Top = 14, Width = 140, Height = 36, Variant = SquareButtonVariant.Primary };
            next.Click += delegate { StepStates(); };
            var tools = new Panel { Dock = DockStyle.Top, Height = 64 };
            tools.Controls.Add(next);

            // Dock=Top은 나중에 추가한 것이 위로 오므로 아래쪽부터 추가한다.
            Controls.Add(buttonsLight);
            Controls.Add(buttonsDark);
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
