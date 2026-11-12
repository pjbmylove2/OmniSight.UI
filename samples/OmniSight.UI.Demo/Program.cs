using System;
using System.Windows.Forms;
using OmniSight.UI.Controls;

namespace OmniSight.UI.Demo
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            var form = new Form { Text = "OmniSight.UI Demo", Width = 400, Height = 200 };
            form.Controls.Add(new ThemedButton { Text = "ThemedButton", Left = 20, Top = 20, Width = 140, Height = 36 });
            Application.Run(form);
        }
    }
}
