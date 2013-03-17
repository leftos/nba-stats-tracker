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

using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;

#endregion

namespace NBA_Stats_Tracker.Windows.MainInterface
{
    /// <summary>
    ///     Contains basic version information and accreditations.
    /// </summary>
    public partial class AboutWindow
    {
        public AboutWindow()
        {
            InitializeComponent();

            lblVersion.Content = "version " + Assembly.GetExecutingAssembly().GetName().Version;

            txbThanks.Text = "I want to thank everyone that took the time to give me suggestions, feedback, and bug reports.\n"
                             + "I also want to thank my family and friends for their support, as well as my professor Mr. "
                             + "Tsakalidis for letting NBA Stats Tracker be the thesis for my Computer Engineering degree.\n"
                             + "Special thanks goes to the NLSC community and specific members which I've named in the Readme.\n"
                             + "\nThanks for all your support, enjoy!";
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnDonate_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("http://users.tellas.gr/~aslan16/donate.html");
        }

        private void btnWebsite_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("http://forums.nba-live.com/viewtopic.php?f=143&t=84110");
        }

        private void btnCheckForUpdates_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.CheckForUpdates(true);
        }

        private void btnLicense_Click(object sender, RoutedEventArgs e)
        {
            string newLicensePath = App.AppTempPath + "license.txt";
            File.Copy("LICENSE", newLicensePath);
            Process.Start(newLicensePath);
        }
    }
}