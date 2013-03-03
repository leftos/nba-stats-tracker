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
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using LeftosCommonLibrary;

#endregion

namespace NBA_Stats_Tracker
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        internal const string AppName = "NBA Stats Tracker";
        public const string AppRegistryKey = @"SOFTWARE\Lefteris Aslanoglou\NBA Stats Tracker";
        public static bool RealNBAOnly;

        public static readonly string AppDocsPath = NBA_Stats_Tracker.Windows.MainInterface.MainWindow.AppDocsPath;
        public static string SavesPath = NBA_Stats_Tracker.Windows.MainInterface.MainWindow.SavesPath;
        public static readonly string AppTempPath = NBA_Stats_Tracker.Windows.MainInterface.MainWindow.AppTempPath;

        /// <summary>
        ///     Handles the DispatcherUnhandledException event of the App control.
        ///     Makes sure that any unhandled exceptions produce an error report that includes a stack trace.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="DispatcherUnhandledExceptionEventArgs" /> instance containing the event data.
        /// </param>
        private void app_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            string exceptionString = e.Exception.ToString();
            string innerExceptionString = e.Exception.InnerException == null
                                              ? "No inner exception information."
                                              : e.Exception.InnerException.Message;
            string versionString = "Version " + Assembly.GetExecutingAssembly().GetName().Version;

            try
            {
                string errorReportPath = AppDocsPath + @"errorlog_unh.txt";
                var f = new StreamWriter(errorReportPath);

                f.WriteLine(string.Format("Unhandled Exception Error Report for {0}", AppName));
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
                    AppName + " encountered a critical error and will be terminated.\n\n" + "An Error Log has been saved at \n" +
                    errorReportPath, AppName + " Error", MessageBoxButton.OK, MessageBoxImage.Error);

                Process.Start(errorReportPath);
            }
            catch (Exception ex)
            {
                string s = "Can't create errorlog!\nException: " + ex;
                s += ex.InnerException != null ? "\nInner Exception: " + ex.InnerException : "";
                s += "\n\n";
                s += versionString;
                s += "Exception Information:\n" + exceptionString + "\n\n";
                s += "Inner Exception Information:\n" + innerExceptionString;
                MessageBox.Show(s, AppName + " Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            // Prevent default unhandled exception processing
            e.Handled = true;

            Environment.Exit(-1);
        }

        /// <summary>
        ///     Forces a critical error to happen and produces an error-report which includes the stack trace.
        /// </summary>
        /// <param name="e">The e.</param>
        /// <param name="additional">The additional.</param>
        public static void ErrorReport(Exception e, string additional = "")
        {
            string exceptionString = e.ToString();
            string innerExceptionString = e.InnerException == null ? "No inner exception information." : e.InnerException.Message;
            string versionString = "Version " + Assembly.GetExecutingAssembly().GetName().Version;

            try
            {
                string errorReportPath = AppDocsPath + @"errorlog.txt";
                var f = new StreamWriter(errorReportPath);

                f.WriteLine("Forced Exception Error Report for " + AppName);
                f.WriteLine(versionString);
                f.WriteLine("Developer information: " + additional);
                f.WriteLine();
                f.WriteLine("Exception information:");
                f.Write(exceptionString);
                f.WriteLine();
                f.WriteLine();
                f.WriteLine("Inner Exception information:");
                f.Write(innerExceptionString);
                f.Close();

                MessageBox.Show(
                    AppName + " encountered a critical error and will be terminated.\n\n" + "An Error Log has been saved at \n" +
                    errorReportPath, AppName + " Error", MessageBoxButton.OK, MessageBoxImage.Error);

                Process.Start(errorReportPath);
            }
            catch (Exception ex)
            {
                string s = "Can't create errorlog!\nException: " + ex;
                s += ex.InnerException != null ? "\nInner Exception: " + ex.InnerException : "";
                s += "\n\n";
                s += versionString;
                s += "Exception Information:\n" + exceptionString + "\n\n";
                s += "Inner Exception Information:\n" + innerExceptionString;
                MessageBox.Show(s, AppName + " Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            Environment.Exit(-1);
        }

        /// <summary>
        ///     Raises the <see cref="E:System.Windows.Application.Startup" /> event.
        ///     Checks if the program is called with the -RealNBAOnly argument, which makes the program download the latest NBA stats and exit.
        /// </summary>
        /// <param name="e">
        ///     A <see cref="T:System.Windows.StartupEventArgs" /> that contains the event data.
        /// </param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            if (!Directory.Exists(AppDocsPath))
            {
                try
                {
                    Directory.CreateDirectory(AppDocsPath);
                }
                catch (Exception)
                {
                    Debug.WriteLine("DocsPath couldn't be created.");
                }
            }

            try
            {
                File.Delete(AppDocsPath + @"\tracelog.txt");
            }
            catch
            {
                Debug.WriteLine("Couldn't delete previous trace file, if any.");
            }

            Tools.AppName = AppName;
            Tools.AppRegistryKey = AppRegistryKey;

            Trace.Listeners.Clear();

            var twtl = new TextWriterTraceListener(AppDocsPath + @"\tracelog.txt")
                {
                    Name = "TextLogger",
                    TraceOutputOptions = TraceOptions.ThreadId | TraceOptions.DateTime
                };

            var ctl = new ConsoleTraceListener(false) {TraceOutputOptions = TraceOptions.DateTime};

            Trace.Listeners.Add(twtl);
            Trace.Listeners.Add(ctl);
            Trace.AutoFlush = true;

            if (e.Args.Length > 0)
            {
                if (e.Args[0] == "-RealNBAOnly")
                {
                    RealNBAOnly = true;
                }
            }
        }
    }
}