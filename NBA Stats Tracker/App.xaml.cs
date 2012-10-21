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

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using NBA_Stats_Tracker.Interop;

#endregion

namespace NBA_Stats_Tracker
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public static bool realNBAonly;
        public static int openWindows = 1;
        public static PlayoffTree tempPT;

        public static readonly string AppDocsPath = NBA_Stats_Tracker.Windows.MainWindow.AppDocsPath;
        public static string SavesPath = NBA_Stats_Tracker.Windows.MainWindow.SavesPath;
        public static readonly string AppTempPath = NBA_Stats_Tracker.Windows.MainWindow.AppTempPath;
        public static string mode = "";

        /// <summary>
        /// Handles the DispatcherUnhandledException event of the App control.
        /// Makes sure that any unhandled exceptions produce an error report that includes a stack trace.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DispatcherUnhandledExceptionEventArgs" /> instance containing the event data.</param>
        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                var f = new StreamWriter(NBA_Stats_Tracker.Windows.MainWindow.AppDocsPath + @"\errorlog_unh.txt");

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
                            NBA_Stats_Tracker.Windows.MainWindow.AppDocsPath + @"\errorlog_unh.txt");

            Process.Start(NBA_Stats_Tracker.Windows.MainWindow.AppDocsPath + @"\errorlog_unh.txt");

            // Prevent default unhandled exception processing
            e.Handled = true;

            Environment.Exit(-1);
        }

        /// <summary>
        /// Forces a critical error to happen and produces an error-report which includes the stack trace.
        /// </summary>
        /// <param name="e">The e.</param>
        /// <param name="additional">The additional.</param>
        public static void errorReport(Exception e, string additional = "")
        {
            try
            {
                var f = new StreamWriter(NBA_Stats_Tracker.Windows.MainWindow.AppDocsPath + @"\errorlog.txt");

                f.WriteLine("Forced Exception Error Report for NBA Stats Tracker");
                f.WriteLine("Version " + Assembly.GetExecutingAssembly().GetName().Version);
                f.WriteLine("Additional: " + additional);
                f.WriteLine();
                f.Write(e.ToString());
                f.WriteLine();
                f.WriteLine();
                f.Write(e.InnerException == null ? "None" : e.InnerException.Message);
                f.WriteLine();
                f.WriteLine();
                f.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Can't create errorlog!\n\n" + ex + "\n\n" + (e.InnerException == null ? "None" : e.InnerException.Message));
            }

            MessageBox.Show("NBA Stats Tracker encountered a critical error and will be terminated.\n\nAn Error Log has been saved at " +
                            NBA_Stats_Tracker.Windows.MainWindow.AppDocsPath + @"\errorlog.txt");

            Environment.Exit(-1);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Application.Startup" /> event.
        /// Checks if the program is called with the -realnbaonly argument, which makes the program download the latest NBA stats and exit.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.StartupEventArgs" /> that contains the event data.</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            /*
            // Get Reference to the current Process
            Process thisProc = Process.GetCurrentProcess();
            // Check how many total processes have the same name as the current one
            if (Process.GetProcessesByName(thisProc.ProcessName).Length > 1)
            {
                // If ther is more than one, than it is already running.
                MessageBox.Show("Application is already running.");
                Environment.Exit(-1);
            }
			*/

            base.OnStartup(e);

            if (e.Args.Length > 0)
            {
                if (e.Args[0] == "-realnbaonly")
                {
                    realNBAonly = true;
                }
            }
        }
    }
}