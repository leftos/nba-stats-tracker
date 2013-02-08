#region Copyright Notice

//    Copyright 2011-2013 Eleftherios Aslanoglou
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

#endregion

#region Using Directives

using System;
using System.Windows.Media.Imaging;

#endregion

namespace NBA_Stats_Tracker.Windows
{
    /// <summary>
    ///     OBSOLETE:
    ///     Used to show trends in teams' performances between two different snapshots of the same league.
    /// </summary>
    public partial class TrendsWindow
    {
        public TrendsWindow(string str, string team1, string team2)
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