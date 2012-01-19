using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.ComponentModel;

namespace NBA_2K12_Correct_Team_Stats
{
    /// <summary>
    /// Interaction logic for getRealStatsW.xaml
    /// </summary>
    public partial class getRealStatsW : Window
    {
        public static TeamStats[] _tst = new TeamStats[30];

        public getRealStatsW()
        {
            InitializeComponent();

            for (int i = 0; i < 30; i++)
            {
                _tst[i] = new TeamStats();
            }

            lblProgress.Content = "Please wait...";
            int workersdone = 0;

            BackgroundWorker worker = new BackgroundWorker();

            worker.WorkerReportsProgress = true;

            worker.DoWork += delegate(object s, DoWorkEventArgs args)
            {
                int i = 0;
                SortedDictionary<string, int> realTeams = StatsTracker.setTeamOrder("Mode 0");
                foreach (KeyValuePair<string, int> kvp in realTeams)
                {
                    _tst[realTeams[kvp.Key]] = StatsTracker.getRealStats(kvp.Key);
                    if (_tst[realTeams[kvp.Key]].name == "Error")
                    {
                        MessageBox.Show("An error occured.");
                        this.Close();
                    }
                    i++;
                    worker.ReportProgress((int)((float)i * 100 / 30));
                }
            };

            worker.ProgressChanged += delegate(object s, ProgressChangedEventArgs args)
            {
                int percentage = args.ProgressPercentage;
                pb.Value = percentage;
                lblProgress.Content = "Please wait... (" + percentage + "% complete)";
            };

            worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
            {
                MainWindow.realtst = _tst;
                this.Close();
            };

            worker.RunWorkerAsync();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.realtst[0].name = "Canceled";
            this.Close();
        }
    }
}
