using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NBA_Stats_Tracker.Windows.MainInterface.ToolWindows
{
    using System.Drawing;
    using System.Drawing.Text;
    using System.IO;

    using NBA_Stats_Tracker.Data.Players;

    /// <summary>
    /// Interaction logic for ShotChartWindow.xaml
    /// </summary>
    public partial class ShotChartWindow : Window
    {
        private readonly Dictionary<int, PlayerPBPStats> _pbpsList;

        private static readonly Dictionary<int, KeyValuePair<int, int>> XyDict = new Dictionary<int, KeyValuePair<int, int>>
            {
                { 2, new KeyValuePair<int, int>(239, 30) },
                {3, new KeyValuePair<int, int>(318, 30)},
                {4, new KeyValuePair<int, int>(239, 122)},
                {5, new KeyValuePair<int, int>(160, 30)},
                {6, new KeyValuePair<int, int>(399, 30)},
                {7, new KeyValuePair<int, int>(340, 173)},
                {8, new KeyValuePair<int, int>(239, 200)},
                {9, new KeyValuePair<int, int>(138, 173)},
                {10, new KeyValuePair<int, int>(79, 30)},
                {11, new KeyValuePair<int, int>(456, 150)},
                {12, new KeyValuePair<int, int>(408, 250)},
                {13, new KeyValuePair<int, int>(239, 300)},
                {14, new KeyValuePair<int, int>(70, 250)},
                { 15, new KeyValuePair<int, int>(22, 150) }
            };

        public ShotChartWindow()
        {
            InitializeComponent();
        }

        public ShotChartWindow(Dictionary<int, PlayerPBPStats> pbpsList)
            : this()
        {
            _pbpsList = pbpsList;

            

            var list = new List<Image> { Properties.Resources.Default_001 };
            for (int i = 2; i <= 15; i++)
            {
                if (pbpsList[i].FGp >= 0.5)
                {
                    list.Add((Image)Properties.Resources.ResourceManager.GetObject("Red_0" + String.Format("{0:00}", i)));
                }
                else if (pbpsList[i].FGp >= 0.4)
                {
                    list.Add((Image)Properties.Resources.ResourceManager.GetObject("Grey_0" + String.Format("{0:00}", i)));
                }
                else if (pbpsList[i].FGA > 0)
                {
                    list.Add((Image)Properties.Resources.ResourceManager.GetObject("Blue_0" + String.Format("{0:00}", i)));
                }
                else
                {
                    list.Add((Image)Properties.Resources.ResourceManager.GetObject("Default_0" + String.Format("{0:00}", i)));
                }
            }

            Image canvas = new Bitmap(478, 397);
            var frame = new Rectangle(0, 0, 478, 397);
            var g = Graphics.FromImage(canvas);

            list.ForEach(o => g.DrawImage(o, frame, frame, GraphicsUnit.Pixel));

            g.TextRenderingHint = TextRenderingHint.AntiAlias;
            var strFormat = new StringFormat();
            strFormat.Alignment = StringAlignment.Center;
            strFormat.LineAlignment = StringAlignment.Center;

            foreach (var pair in XyDict)
            {
                var s = _pbpsList[pair.Key];
                string fgpString = "";
                if (!double.IsNaN(s.FGp))
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
                g.DrawString(
                    String.Format("{0}-{1}\n{2}", s.FGM, s.FGA, fgpString),
                    new Font("Tahoma", 11),
                    Brushes.White,
                    pair.Value.Key,
                    pair.Value.Value,
                    strFormat);
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
    }
}
