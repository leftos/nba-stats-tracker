using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;

namespace NBA_2K12_Correct_Team_Stats
{
    /// <summary>
    /// Interaction logic for getRealStatsW.xaml
    /// </summary>
    public partial class getRealStatsW : Window
    {
        public static TeamStats[] _tst = new TeamStats[30];
        public static Semaphore sem = new Semaphore(1, 1);
        int workersdone = 0;

        BackgroundWorker worker1 = new BackgroundWorker();
        BackgroundWorker worker2 = new BackgroundWorker();
        BackgroundWorker worker3 = new BackgroundWorker();

        public getRealStatsW()
        {
            InitializeComponent();

            for (int i = 0; i < 30; i++)
            {
                _tst[i] = new TeamStats();
            }

            lblProgress.Content = "Please wait...";

            worker1 = new BackgroundWorker();

            worker1.WorkerReportsProgress = true;
            worker1.WorkerSupportsCancellation = true;

            worker1.DoWork += delegate(object s, DoWorkEventArgs args)
            {
                int i = 0;
                SortedDictionary<string, int> realTeams = StatsTracker.setTeamOrder("Mode 0");
                var subset = realTeams.Take(10);
                foreach (KeyValuePair<string, int> kvp in subset)
                {
                    if (worker1.CancellationPending) return;
                    _tst[realTeams[kvp.Key]] = StatsTracker.getRealStats(kvp.Key);
                    if (_tst[realTeams[kvp.Key]].name == "Error")
                    {
                        MessageBox.Show("An error occured.");
                        this.Close();
                    }
                    i++;
                    worker1.ReportProgress(1);
                }
            };

            worker1.ProgressChanged += delegate(object s, ProgressChangedEventArgs args)
            {
                sem.WaitOne(); 
                updateProgressBar();
                sem.Release();
            };

            worker1.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
            {
                checkIfAllWorkersDone();
            };

            worker2 = new BackgroundWorker();

            worker2.WorkerReportsProgress = true;
            worker2.WorkerSupportsCancellation = true;

            worker2.DoWork += delegate(object s, DoWorkEventArgs args)
            {
                int i = 0;
                SortedDictionary<string, int> realTeams = StatsTracker.setTeamOrder("Mode 0");
                var subset = realTeams.Skip(10).Take(10);
                foreach (KeyValuePair<string, int> kvp in subset)
                {
                    if (worker2.CancellationPending) return;
                    _tst[realTeams[kvp.Key]] = StatsTracker.getRealStats(kvp.Key);
                    if (_tst[realTeams[kvp.Key]].name == "Error")
                    {
                        MessageBox.Show("An error occured.");
                        this.Close();
                    }
                    i++;
                    worker2.ReportProgress(1);
                }
            };

            worker2.ProgressChanged += delegate(object s, ProgressChangedEventArgs args)
            {
                sem.WaitOne();
                updateProgressBar();
                sem.Release();
            };

            worker2.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
            {
                checkIfAllWorkersDone();
            };

            worker3 = new BackgroundWorker();

            worker3.WorkerReportsProgress = true;
            worker3.WorkerSupportsCancellation = true;

            worker3.DoWork += delegate(object s, DoWorkEventArgs args)
            {
                int i = 0;
                SortedDictionary<string, int> realTeams = StatsTracker.setTeamOrder("Mode 0");
                var subset = realTeams.Skip(20);
                foreach (KeyValuePair<string, int> kvp in subset)
                {
                    if (worker3.CancellationPending) return;
                    _tst[realTeams[kvp.Key]] = StatsTracker.getRealStats(kvp.Key);
                    if (_tst[realTeams[kvp.Key]].name == "Error")
                    {
                        MessageBox.Show("An error occured.");
                        this.Close();
                    }
                    i++;
                    worker3.ReportProgress(1);
                }
            };

            worker3.ProgressChanged += delegate(object s, ProgressChangedEventArgs args)
            {
                sem.WaitOne();
                updateProgressBar();
                sem.Release();
            };

            worker3.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
            {
                checkIfAllWorkersDone();
            };

            worker1.RunWorkerAsync();
            worker2.RunWorkerAsync();
            worker3.RunWorkerAsync();
        }

        private void checkIfAllWorkersDone()
        {
            workersdone++;
            if (workersdone == 3)
            {
                MainWindow.realtst = _tst;
                this.Close();
            }
        }

        private void updateProgressBar()
        {
            pb.Value += (double)10 / 3;
            int percentage = (int)pb.Value;
            lblProgress.Content = "Please wait... (" + percentage + "% complete)";
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.realtst[0].name = "Canceled";
            this.Close();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            worker1.CancelAsync();
            worker2.CancelAsync();
            worker3.CancelAsync();
        }
    }
}
