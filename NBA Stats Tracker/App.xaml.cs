using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace NBA_2K12_Correct_Team_Stats
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static bool realNBAonly;

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // Add code to output the exception details to a message box/event log/log file,   etc.
            // Be sure to include details about any inner exceptions
            try
            {
                var f = new StreamWriter(NBA_2K12_Correct_Team_Stats.MainWindow.AppDocsPath + @"\errorlog_unh.txt");

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

            MessageBox.Show(
                "NBA Stats Tracker encountered a critical error and will be terminated.\n\nAn Error Log has been saved at " +
                NBA_2K12_Correct_Team_Stats.MainWindow.AppDocsPath + @"\errorlog_unh.txt");

            // Prevent default unhandled exception processing
            e.Handled = true;

            Environment.Exit(-1);
        }

        public static void errorReport(Exception e, string additional = "")
        {
            // Add code to output the exception details to a message box/event log/log file,   etc.
            // Be sure to include details about any inner exceptions
            try
            {
                //StreamWriter f = new StreamWriter(NBA_2K12_Keep_My_Mod.MainWindow.SaveRootPath + @"\errorlog.txt");
                var f = new StreamWriter(NBA_2K12_Correct_Team_Stats.MainWindow.AppDocsPath + @"\errorlog.txt");

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
                MessageBox.Show("Can't create errorlog!\n\n" + ex + "\n\n" +
                                (e.InnerException == null ? "None" : e.InnerException.Message));
            }

            MessageBox.Show(
                "NBA Stats Tracker encountered a critical error and will be terminated.\n\nAn Error Log has been saved at " +
                NBA_2K12_Correct_Team_Stats.MainWindow.AppDocsPath + @"\errorlog.txt");

            Environment.Exit(-1);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            // Get Reference to the current Process
            Process thisProc = Process.GetCurrentProcess();
            // Check how many total processes have the same name as the current one
            if (Process.GetProcessesByName(thisProc.ProcessName).Length > 1)
            {
                // If ther is more than one, than it is already running.
                MessageBox.Show("Application is already running.");
                Environment.Exit(-1);
            }

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