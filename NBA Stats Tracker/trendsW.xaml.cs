using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace NBA_Stats_Tracker
{
    /// <summary>
    /// Interaction logic for trendsW.xaml
    /// </summary>
    public partial class trendsW : Window
    {
        public trendsW(string str, string team1, string team2)
        {
            InitializeComponent();

            BitmapImage bi1 = loadTeamLogo(team1);

            img1.Source = bi1;
            imgUp1.Source = loadTeamLogo("up");

            BitmapImage bi2 = loadTeamLogo(team2);

            img2.Source = bi2;
            imgDown1.Source = loadTeamLogo("down");

            string[] parts = str.Split('$');

            tb1.Text = parts[0];
            tb2.Text = parts[1];
        }

        private static BitmapImage loadTeamLogo(string team)
        {
            var bi1 = new BitmapImage();
            bi1.BeginInit();
            bi1.UriSource = new Uri(MainWindow.AppPath + @"Images\" + team + ".gif");
            bi1.CacheOption = BitmapCacheOption.OnLoad;
            bi1.EndInit();
            return bi1;
        }
    }
}