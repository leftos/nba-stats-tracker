#region Copyright Notice

// Created by Lefteris Aslanoglou, (c) 2011-2013
// 
// Initial development until v1.0 done as part of the implementation of thesis
// "Application Development for Basketball Statistical Analysis in Natural Language"
// under the supervision of Prof. Athanasios Tsakalidis & MSc Alexandros Georgiou
// 
// All rights reserved. Unless specifically stated otherwise, the code in this file should 
// not be reproduced, edited and/or republished without explicit permission from the 
// author.

#endregion

#region Using Directives

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Win32;

#endregion

namespace Updater
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            if (e.Args.Length == 0)
            {
                MessageBox.Show("Updater must be ran with proper path to installer.");
                Current.Shutdown();
            }

            Process installerProc = null;
            try
            {
                installerProc = Process.Start(e.Args[0], "/SILENT");
                if (installerProc == null)
                    throw new Exception();
            }
            catch
            {
                MessageBox.Show("Can't start installer.");
                Environment.Exit(0);
            }
            installerProc.WaitForExit();

            string installDir = GetRegistrySetting("InstallDir",
                                                   Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) +
                                                   @"\NBA Stats Tracker");
            Process.Start(installDir + @"\NBA Stats Tracker.exe");

            Environment.Exit(0);
        }

        private static string GetRegistrySetting(string setting, string defaultValue)
        {
            RegistryKey rk = Registry.CurrentUser;
            string settingValue = defaultValue;
            try
            {
                if (rk == null)
                    throw new Exception();

                rk = rk.OpenSubKey(@"SOFTWARE\Lefteris Aslanoglou\NBA Stats Tracker");
                if (rk != null)
                    settingValue = rk.GetValue(setting, defaultValue).ToString();
            }
            catch
            {
                settingValue = defaultValue;
            }

            return settingValue;
        }

        private static readonly string AppDocsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) +
                                                    @"\NBA Stats Tracker\";

        private void app_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            var updErrorLog = AppDocsPath + @"updater_errorlog.txt";
            try
            {
                var f = new StreamWriter(updErrorLog);

                f.WriteLine("Unhandled Exception Error Report for NBA Stats Tracker");
                f.WriteLine("Version " + Assembly.GetExecutingAssembly().GetName().Version);
                f.WriteLine();
                f.Write(e.Exception.ToString());
                f.WriteLine();
                f.WriteLine();
                f.Write(e.Exception.InnerException == null ? "None" : e.Exception.InnerException.Message);
                f.WriteLine();
                f.WriteLine();
                f.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Can't create errorlog!\n\n" + ex + "\n\n" + ex.InnerException);
            }

            MessageBox.Show("NBA Stats Tracker encountered a critical error and will be terminated.\n\nAn Error Log has been saved at " +
                            AppDocsPath + @"updater_errorlog.txt");

            Process.Start(updErrorLog);

            // Prevent default unhandled exception processing
            e.Handled = true;

            Environment.Exit(-1);
        }
    }
}