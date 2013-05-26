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

namespace Updater
{
    #region Using Directives

    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Threading;

    using Microsoft.Win32;

    #endregion

    /// <summary>Interaction logic for App.xaml</summary>
    public partial class App
    {
        private static readonly string AppDocsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                                                     + @"\NBA Stats Tracker\";

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            if (e.Args.Length == 0)
            {
                MessageBox.Show("Updater must be ran with proper path to installer.");
                Environment.Exit(-1);
            }

            Process installerProc = null;
            try
            {
                installerProc = Process.Start(e.Args[0], "/SILENT");
                if (installerProc == null)
                {
                    throw new Exception();
                }
            }
            catch
            {
                MessageBox.Show("Can't start installer.");
                Environment.Exit(0);
            }
            installerProc.WaitForExit();

            var installDir = getRegistrySetting(
                "InstallDir", Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) + @"\NBA Stats Tracker");
            Process.Start(installDir + @"\NBA Stats Tracker.exe");

            Environment.Exit(0);
        }

        private static string getRegistrySetting(string setting, string defaultValue)
        {
            var rk = Registry.CurrentUser;
            var settingValue = defaultValue;
            try
            {
                if (rk == null)
                {
                    throw new Exception();
                }

                rk = rk.OpenSubKey(@"SOFTWARE\Lefteris Aslanoglou\NBA Stats Tracker");
                if (rk != null)
                {
                    settingValue = rk.GetValue(setting, defaultValue).ToString();
                }
            }
            catch
            {
                settingValue = defaultValue;
            }

            return settingValue;
        }

        private void app_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            var exceptionString = e.Exception.ToString();
            var innerExceptionString = e.Exception.InnerException == null
                                           ? "No inner exception information."
                                           : e.Exception.InnerException.Message;
            var versionString = "Version " + Assembly.GetExecutingAssembly().GetName().Version;

            try
            {
                var errorReportPath = AppDocsPath + @"updater_errorlog.txt";
                var f = new StreamWriter(errorReportPath);

                f.WriteLine("Unhandled Exception Error Report for NBA Stats Tracker");
                f.WriteLine(versionString);
                f.WriteLine();
                f.WriteLine("Exception information:");
                f.Write(exceptionString);
                f.WriteLine();
                f.WriteLine();
                f.WriteLine("Inner Exception information:");
                f.Write(innerExceptionString);
                f.Close();

                MessageBox.Show(
                    "NBA Stats Tracker Updater encountered a critical error and will be terminated.\n\n"
                    + "An Error Log has been saved at \n" + errorReportPath,
                    "NBA Stats Tracker Updater Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Process.Start(errorReportPath);
            }
            catch (Exception ex)
            {
                var s = "Can't create errorlog!\nException: " + ex;
                s += ex.InnerException != null ? "\nInner Exception: " + ex.InnerException : "";
                s += "\n\n";
                s += versionString;
                s += "Exception Information:\n" + exceptionString + "\n\n";
                s += "Inner Exception Information:\n" + innerExceptionString;
                MessageBox.Show(s, "NBA Stats Tracker Updater Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            // Prevent default unhandled exception processing
            e.Handled = true;

            Environment.Exit(-1);
        }
    }
}