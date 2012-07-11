#region Copyright Notice

// Created by Lefteris Aslanoglou, (c) 2011-2012
// 
// Implementation of thesis
// "Application Development for Basketball Statistical Analysis in Natural Language"
// under the supervision of Prof. Athanasios Tsakalidis & MSc Alexandros Georgiou,
// Computer Engineering & Informatics Department, University of Patras, Greece.
// 
// All rights reserved. Unless specifically stated otherwise, the code in this file should 
// not be reproduced, edited and/or republished without explicit permission from the 
// author.

#endregion

#region Using Directives

using System.Diagnostics;
using System.Reflection;
using System.Windows;

#endregion

namespace NBA_Stats_Tracker.Windows
{
    /// <summary>
    /// Interaction logic for aboutW.xaml
    /// </summary>
    public partial class AboutWindow
    {
        public AboutWindow()
        {
            InitializeComponent();

            lblVersion.Content = "version " + Assembly.GetExecutingAssembly().GetName().Version;

            txbThanks.Text =
                "I want to thank everyone that took the time to give me suggestions, feedback, and bug reports.\n" +
                "I also want to thank my family and friends for their support, as well as my professor Mr. " +
                "Tsakalidis for letting NBA Stats Tracker be the thesis for my Computer Engineering degree.\n" +
                "Special thanks goes to the NLSC community and specific members which I've named in the Readme.\n" +
                "\nThanks for all your support, enjoy!";
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnDonate_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("http://students.ceid.upatras.gr/~aslanoglou/donate.html");
        }

        private void btnWebsite_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("http://forums.nba-live.com/viewtopic.php?f=143&t=84110");
        }

        private void btnCheckForUpdates_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.CheckForUpdates(true);
        }
    }
}