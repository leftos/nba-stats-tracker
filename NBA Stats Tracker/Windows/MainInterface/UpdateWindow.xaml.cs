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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Windows;
using LeftosCommonLibrary.CommonDialogs;

#endregion

namespace NBA_Stats_Tracker.Windows.MainInterface
{
    /// <summary>
    ///     Interaction logic for UpdateWindow.xaml
    /// </summary>
    public partial class UpdateWindow : Window
    {
        private readonly string _changelogURL;
        private readonly string _downloadURL;
        private readonly string _installerURL;
        private readonly string _supportURL;

        private UpdateWindow()
        {
            InitializeComponent();
        }

        public UpdateWindow(string curVersion, string newVersion, string message, string installerURL, string downloadURL,
                            string supportURL, string changelogURL) : this()
        {
            txbCurrentVersion.Text = txbCurrentVersion.Text + " " + curVersion;
            txbLatestVersion.Text = txbLatestVersion.Text + " " + newVersion;
            txbMessage.Text = message;

            _installerURL = installerURL;
            _downloadURL = downloadURL;
            _supportURL = supportURL;
            _changelogURL = changelogURL;
        }

        private void btnVisitDownload_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(_downloadURL);
        }

        private void btnVisitSupport_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(_supportURL);
        }

        private void btnViewChangelog_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var webClient = new WebClient();
                string updateUri = _changelogURL;
                webClient.DownloadFileCompleted += OnChangelogDownloadCompleted;
                webClient.DownloadFileAsync(new Uri(updateUri), App.AppTempPath + "changelog.txt");
            }
            catch (Exception ex)
            {
                MessageBox.Show("The changelog couldn't be downloaded at this time. Please try again later.\n\n" + ex.Message);
            }
        }

        private void OnChangelogDownloadCompleted(object sender, AsyncCompletedEventArgs asyncCompletedEventArgs)
        {
            List<string> lines = File.ReadAllLines(App.AppTempPath + "changelog.txt").ToList();
            lines.Add("");
            var cmw = new CopyableMessageWindow(lines.Aggregate((l1, l2) => l1 + "\n" + l2), "NBA Stats Tracker - What's New",
                                                TextAlignment.Left);
            cmw.ShowDialog();
        }

        private void btnInstallNow_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string localInstallerPath = App.AppTempPath + "Setup.exe";
                var pw = new ProgressWindow("Please wait while the installer is being downloaded...\n" + _installerURL);
                pw.Show();
                var webClient = new WebClient();
                webClient.DownloadProgressChanged += delegate(object o, DownloadProgressChangedEventArgs args)
                    {
                        pw.SetProgressBarValue(args.ProgressPercentage);
                    };
                webClient.DownloadFileCompleted += delegate
                    {
                        pw.CanClose = true;
                        pw.Close();
                        if (
                            MessageBox.Show(
                                "NBA Stats Tracker will now close to install the latest version and then restart.\n\nAre you sure you want to continue?",
                                "NBA Stats Tracker", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes) !=
                            MessageBoxResult.Yes)
                        {
                            return;
                        }

                        string newUpdaterPath = App.AppTempPath + "\\Updater.exe";
                        try
                        {
                            File.Copy(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Updater.exe", newUpdaterPath,
                                      true);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Couldn't run the Updater. " + ex.Message);
                            return;
                        }
                        Process.Start(newUpdaterPath, "\"" + localInstallerPath + "\"");
                        Environment.Exit(0);
                    };
                webClient.DownloadFileAsync(new Uri(_installerURL), localInstallerPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("The changelog couldn't be downloaded at this time. Please try again later.\n\n" + ex.Message);
            }
        }

        private void btnRemindMeLater_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnDisableNotifications_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.MWInstance.mnuOptionsCheckForUpdates.IsChecked = false;
            MainWindow.MWInstance.SetCheckForUpdatesRegistrySetting();
        }
    }
}