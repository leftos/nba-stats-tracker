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
using System.Windows;
using Microsoft.Win32;

#endregion

namespace Updater
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            if (e.Args.Length == 0)
            {
                MessageBox.Show("Updater must be ran with proper path to installer.");
                Current.Shutdown();
            }

            Process installerProc;
            try
            {
                installerProc = Process.Start(e.Args[0], "/SILENT");
                if (installerProc == null)
                    throw new Exception();
            }
            catch
            {
                MessageBox.Show("Can't start installer.");
                Current.Shutdown();
                return;
            }
            installerProc.WaitForExit();

            string installDir = GetRegistrySetting("InstallDir",
                                                   Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) +
                                                   @"\NBA Stats Tracker");
            Process.Start(installDir + @"\NBA Stats Tracker.exe");

            Current.Shutdown();
        }

        public static string GetRegistrySetting(string setting, string defaultValue)
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
    }
}