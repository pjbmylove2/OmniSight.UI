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

            var form = new Form { Text = "OmniSight.UI Demo", AutoScroll = true, ClientSize = new Size(900, 360) };
            var dark = new ButtonShowcase(UiTheme.Dark, false) { Dock = DockStyle.Top };
            var light = new ButtonShowcase(UiTheme.Light, false) { Dock = DockStyle.Top };
            form.Controls.Add(light);
            form.Controls.Add(dark);
            Application.Run(form);
            return 0;
        }
    }
}
