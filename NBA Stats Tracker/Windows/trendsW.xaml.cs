#region Copyright Notice

// Created by Lefteris Aslanoglou, (c) 2011-2012
// 
// Implementation of thesis
// "Application Development for Basketball Statistical Analysis in Natural Language"
// under the supervision of Prof. Athanasios Tsakalidis & MSc Alexandros Georgiou
// 
// All rights reserved. Unless specifically stated otherwise, the code in this file should 
// not be reproduced, edited and/or republished without explicit permission from the 
// author.

#endregion

#region Using Directives

using System;
using System.Windows;
using System.Windows.Media.Imaging;

#endregion

namespace NBA_Stats_Tracker.Windows
{
    /// <summary>
    /// Interaction logic for trendsW.xaml
    /// </summary>
    public partial class trendsW
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