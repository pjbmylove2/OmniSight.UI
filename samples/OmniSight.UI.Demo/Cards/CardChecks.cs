using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using OmniSight.UI.Controls;

namespace OmniSight.UI.Demo.Cards
{
    internal static class CardChecks
    {
        internal static void RunChecks()
        {
            CheckMainShowcase();

            using (var form = new Form { BackColor = Color.Fuchsia })
            using (var titleFont = new Font("Segoe UI", 18, FontStyle.Bold))
            {
                var card = new CardPanel
                {
                    Bounds = new Rectangle(10, 10, 330, 160),
                    HeaderLeftText = "검사 결과",
                    HeaderCenterText = "라인 A",
                    HeaderRightText = "연결됨",
                    HeaderRightStyle = HeaderRightStyle.SuccessBadge
                };
                var content = new Panel { Dock = DockStyle.Fill };
                var badge = new ColorLabel { Text = "OK", Font = titleFont, Tone = BadgeTone.Success };
                form.Controls.Add(card);
                card.Controls.Add(content);
                content.Controls.Add(badge);
                ShowOffscreen(form);

                Require(content.Bounds == card.DisplayRectangle && content.Top > card.Padding.Top,
                    "card reserves the header above docked children");
                card.HeaderLeftText = card.HeaderCenterText = card.HeaderRightText = string.Empty;
                Require(content.Top == card.Padding.Top, "empty header restores the body space");

                int shortWidth = badge.Width;
                badge.Text = "MariaDB 연결됨";
                Require(badge.Width > shortWidth, "badge grows to fit its text");
                badge.Text = "MM";
                foreach (UiTheme theme in new[] { UiTheme.Dark, UiTheme.Light })
                {
                    badge.Theme = theme;
                    foreach (BadgeTone tone in new[] { BadgeTone.Success, BadgeTone.Error, BadgeTone.Info, BadgeTone.Warning })
                    {
                        badge.Tone = tone;
                        var colors = BadgeDrawing.Colors(theme, tone);
                        using (Bitmap image = Capture(badge))
                        {
                            Require(image.GetPixel(0, 0).ToArgb() == content.BackColor.ToArgb(), "rounded corner blends with parent");
                            Require(image.GetPixel(image.Width / 2, 1).ToArgb() == colors.Background.ToArgb(), "preset background");
                            Require(ContainsColor(image, colors.Foreground), "preset foreground");
                        }
                    }
                }

                badge.BadgeBackColor = Color.Beige;
                using (Bitmap image = Capture(badge))
                    Require(ContainsColor(image, Color.Black), "light custom fill chooses black");
                badge.BadgeBackColor = Color.MidnightBlue;
                using (Bitmap image = Capture(badge))
                    Require(ContainsColor(image, Color.White), "dark custom fill chooses white");
                badge.BadgeForeColor = Color.Gold;
                using (Bitmap image = Capture(badge))
                    Require(ContainsColor(image, Color.Gold), "explicit foreground has priority");
            }
            Console.WriteLine("PASS: CardPanel docking and empty header, ColorLabel themes, auto sizing and custom contrast");
        }

        private static void CheckMainShowcase()
        {
            using (var form = new DemoForm())
            {
                ShowOffscreen(form);
                CardShowcase showcase = null;
                foreach (Control control in form.Controls)
                    if (control is CardShowcase cards)
                        showcase = cards;

                Require(showcase != null && showcase.Visible && form.ClientRectangle.Contains(showcase.Bounds),
                    "the main demo shows the card showcase without scrolling");
                for (int index = 0; index < 2; index++)
                {
                    UiTheme theme = index == 0 ? UiTheme.Dark : UiTheme.Light;
                    var card = (CardPanel)showcase.GetControlFromPosition(index, 0).Controls[0];
                    Require(card.Theme == theme, "each preview card uses its assigned theme");
                    var badges = card.Controls[0];
                    Require(badges.Controls.Count == 4, "each preview shows four badge presets");
                    foreach (ColorLabel badge in badges.Controls)
                    {
                        Require(badge.Theme == theme && badge.Visible && badges.ClientRectangle.Contains(badge.Bounds),
                            "each preview badge is fully visible and matches its card theme");
                    }
                }
            }
        }

        internal static void RunCapture(string directory)
        {
            Directory.CreateDirectory(directory);
            using (var form = new CardPanelDemoForm())
            {
                ShowOffscreen(form);
                Save(form, Path.Combine(directory, "CardPanel-dashboard.png"));
                using (Form gallery = form.CreateBadgeGallery())
                {
                    ShowOffscreen(gallery);
                    Save(gallery, Path.Combine(directory, "ColorLabel-gallery.png"));
                }
            }
        }

        private static void ShowOffscreen(Form form)
        {
            form.StartPosition = FormStartPosition.Manual;
            form.Location = new Point(-3000, 0);
            form.ShowInTaskbar = false;
            form.Show();
            Application.DoEvents();
        }

        private static Bitmap Capture(Control control)
        {
            var image = new Bitmap(control.Width, control.Height);
            control.DrawToBitmap(image, new Rectangle(Point.Empty, image.Size));
            return image;
        }

        private static void Save(Control control, string path)
        {
            using (Bitmap image = Capture(control))
                image.Save(path, ImageFormat.Png);
        }

        private static bool ContainsColor(Bitmap image, Color color)
        {
            for (int y = 0; y < image.Height; y++)
                for (int x = 0; x < image.Width; x++)
                    if (image.GetPixel(x, y).ToArgb() == color.ToArgb())
                        return true;
            return false;
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException(message);
        }
    }
}
