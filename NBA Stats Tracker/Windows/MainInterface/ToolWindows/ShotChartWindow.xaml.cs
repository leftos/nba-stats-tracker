namespace NBA_Stats_Tracker.Windows.MainInterface.ToolWindows
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;
    using System.Drawing.Text;
    using System.IO;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media.Imaging;

    using NBA_Stats_Tracker.Data.Players;

    using Image = System.Drawing.Image;
    using Point = System.Drawing.Point;

    #endregion

    /// <summary>Interaction logic for ShotChartWindow.xaml</summary>
    public partial class ShotChartWindow : Window
    {
        private static readonly Dictionary<int, KeyValuePair<int, int>> XyDict = new Dictionary<int, KeyValuePair<int, int>>
            {
                { 2, new KeyValuePair<int, int>(293, 75) },
                { 3, new KeyValuePair<int, int>(226, 75) },
                { 4, new KeyValuePair<int, int>(293, 128) },
                { 5, new KeyValuePair<int, int>(360, 75) },
                { 6, new KeyValuePair<int, int>(155, 70) },
                { 7, new KeyValuePair<int, int>(186, 168) },
                { 8, new KeyValuePair<int, int>(293, 210) },
                { 9, new KeyValuePair<int, int>(400, 168) },
                { 10, new KeyValuePair<int, int>(431, 70) },
                { 11, new KeyValuePair<int, int>(87, 145) },
                { 12, new KeyValuePair<int, int>(150, 250) },
                { 13, new KeyValuePair<int, int>(293, 297) },
                { 14, new KeyValuePair<int, int>(436, 250) },
                { 15, new KeyValuePair<int, int>(499, 145) },
                { 16, new KeyValuePair<int, int>(32, 122) },
                { 17, new KeyValuePair<int, int>(90, 337) },
                { 18, new KeyValuePair<int, int>(293, 390) },
                { 19, new KeyValuePair<int, int>(496, 337) },
                { 20, new KeyValuePair<int, int>(554, 122) }
            };

        private readonly Dictionary<int, PlayerPBPStats> _pbpsList;

        public ShotChartWindow(bool showHalves = false)
        {
            InitializeComponent();

            var bmi = new BitmapImage();
            bmi.BeginInit();
            bmi.CacheOption = BitmapCacheOption.OnLoad;
            var ms = new MemoryStream();
            Properties.Resources.FloorChart.Save(ms, ImageFormat.Png);
            bmi.StreamSource = ms;
            bmi.EndInit();

            imgShotChart.Source = bmi;

            grdButtons.Visibility = Visibility.Visible;
            LastButtonPressed = "";
            LastHalfSelected = "";

            if (!showHalves)
            {
                rowHalves.Height = new GridLength(0);
            }
        }

        public ShotChartWindow(Dictionary<int, PlayerPBPStats> pbpsList)
            : this()
        {
            _pbpsList = pbpsList;

            grdButtons.Visibility = Visibility.Hidden;

            var list = new List<Image> { Properties.Resources.Default_001 };
            for (var i = 2; i <= 20; i++)
            {
                if (pbpsList[i].FGp >= 0.5)
                {
                    list.Add((Image) Properties.Resources.ResourceManager.GetObject("Red_0" + String.Format("{0:00}", i)));
                }
                else if (pbpsList[i].FGp >= 0.4)
                {
                    list.Add((Image) Properties.Resources.ResourceManager.GetObject("Gray_0" + String.Format("{0:00}", i)));
                }
                else if (pbpsList[i].FGA > 0)
                {
                    list.Add((Image) Properties.Resources.ResourceManager.GetObject("Blue_0" + String.Format("{0:00}", i)));
                }
                else
                {
                    list.Add((Image) Properties.Resources.ResourceManager.GetObject("Default_0" + String.Format("{0:00}", i)));
                }
            }

            Image canvas = new Bitmap(586, 551);
            var frame = new Rectangle(0, 0, 586, 551);
            var g = Graphics.FromImage(canvas);

            list.ForEach(o => g.DrawImage(o, frame, frame, GraphicsUnit.Pixel));

            g.TextRenderingHint = TextRenderingHint.AntiAlias;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            var strFormat = new StringFormat();
            strFormat.Alignment = StringAlignment.Center;
            strFormat.LineAlignment = StringAlignment.Center;

            foreach (var pair in XyDict)
            {
                var s = _pbpsList[pair.Key];
                var fgpString = "";
                if (!Double.IsNaN(s.FGp))
                {
                    if (s.FGM == s.FGA)
                    {
                        fgpString = "1.";
                    }
                    else
                    {
                        fgpString = String.Format("{0:F3}", s.FGp).Substring(1);
                    }
                }

                var gp = new GraphicsPath();
                gp.AddString(
                    String.Format("{0}-{1}\n{2}", s.FGM, s.FGA, fgpString),
                    new FontFamily("Tahoma"),
                    (int) System.Drawing.FontStyle.Bold,
                    16,
                    new Point(pair.Value.Key, pair.Value.Value),
                    strFormat);
                var pen = new Pen(Color.FromArgb(255, 255, 255), 3);
                g.DrawPath(pen, gp);
                var brush = new SolidBrush(Color.FromArgb(0, 0, 0));
                g.FillPath(brush, gp);

                brush.Dispose();
                pen.Dispose();
                gp.Dispose();
            }

            var tempFile = App.AppTempPath + "\\temp.png";
            File.Delete(tempFile);
            canvas.Save(tempFile);

            var bmi = new BitmapImage();
            bmi.BeginInit();
            bmi.CacheOption = BitmapCacheOption.OnLoad;
            bmi.UriSource = new Uri(tempFile, UriKind.Absolute);
            bmi.EndInit();

            imgShotChart.Source = bmi;

            g.Dispose();
            canvas.Dispose();
        }

        public static string LastButtonPressed { get; set; }

        public static string LastHalfSelected { get; set; }

        private void shotButton_Click(object sender, RoutedEventArgs e)
        {
            LastButtonPressed = ((Button) sender).Name;
            LastHalfSelected = rbOffensive.IsChecked == true ? "Offensive" : "Defensive";
            Close();
        }
    }
}