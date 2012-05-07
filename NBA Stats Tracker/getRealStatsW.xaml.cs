using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;

namespace NBA_Stats_Tracker
{
    /// <summary>
    /// Interaction logic for getRealStatsW.xaml
    /// </summary>
    public partial class getRealStatsW : Window
    {
        public static TeamStats[] _tst = new TeamStats[30];
        public static Semaphore sem = new Semaphore(1, 1);

        private readonly BackgroundWorker worker1 = new BackgroundWorker();
        private readonly BackgroundWorker worker2 = new BackgroundWorker();
        private readonly BackgroundWorker worker3 = new BackgroundWorker();
        private int workersdone;

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

            worker1.DoWork += delegate
                                  {
                                      int i = 0;
                                      SortedDictionary<string, int> realTeams = NSTHelper.setTeamOrder("Mode 0");
                                      IEnumerable<KeyValuePair<string, int>> subset = realTeams.Take(10);
                                      foreach (var kvp in subset)
                                      {
                                          if (worker1.CancellationPending) return;
                                          _tst[realTeams[kvp.Key]] = NSTHelper.getRealStats(kvp.Key);
                                          if (_tst[realTeams[kvp.Key]].name == "Error")
                                          {
                                              MessageBox.Show("An error occured.");
                                              Close();
                                          }
                                          i++;
                                          worker1.ReportProgress(1);
                                      }
                                  };

            worker1.ProgressChanged += delegate
                                           {
                                               sem.WaitOne();
                                               updateProgressBar();
                                               sem.Release();
                                           };

            worker1.RunWorkerCompleted += delegate { checkIfAllWorkersDone(); };

            worker2 = new BackgroundWorker();

            worker2.WorkerReportsProgress = true;
            worker2.WorkerSupportsCancellation = true;

            worker2.DoWork += delegate
                                  {
                                      int i = 0;
                                      SortedDictionary<string, int> realTeams = NSTHelper.setTeamOrder("Mode 0");
                                      IEnumerable<KeyValuePair<string, int>> subset = realTeams.Skip(10).Take(10);
                                      foreach (var kvp in subset)
                                      {
                                          if (worker2.CancellationPending) return;
                                          _tst[realTeams[kvp.Key]] = NSTHelper.getRealStats(kvp.Key);
                                          if (_tst[realTeams[kvp.Key]].name == "Error")
                                          {
                                              MessageBox.Show("An error occured.");
                                              Close();
                                          }
                                          i++;
                                          worker2.ReportProgress(1);
                                      }
                                  };

            worker2.ProgressChanged += delegate
                                           {
                                               sem.WaitOne();
                                               updateProgressBar();
                                               sem.Release();
                                           };

            worker2.RunWorkerCompleted += delegate { checkIfAllWorkersDone(); };

            worker3 = new BackgroundWorker();

            worker3.WorkerReportsProgress = true;
            worker3.WorkerSupportsCancellation = true;

            worker3.DoWork += delegate
                                  {
                                      int i = 0;
                                      SortedDictionary<string, int> realTeams = NSTHelper.setTeamOrder("Mode 0");
                                      IEnumerable<KeyValuePair<string, int>> subset = realTeams.Skip(20);
                                      foreach (var kvp in subset)
                                      {
                                          if (worker3.CancellationPending) return;
                                          _tst[realTeams[kvp.Key]] = NSTHelper.getRealStats(kvp.Key);
                                          if (_tst[realTeams[kvp.Key]].name == "Error")
                                          {
                                              MessageBox.Show("An error occured.");
                                              Close();
                                          }
                                          i++;
                                          worker3.ReportProgress(1);
                                      }
                                  };

            worker3.ProgressChanged += delegate
                                           {
                                               sem.WaitOne();
                                               updateProgressBar();
                                               sem.Release();
                                           };

            worker3.RunWorkerCompleted += delegate { checkIfAllWorkersDone(); };

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
                Close();
            }
        }

        private void updateProgressBar()
        {
            pb.Value += (double) 10/3;
            var percentage = (int) pb.Value;
            lblProgress.Content = "Please wait... (" + percentage + "% complete)";
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.realtst[0].name = "Canceled";
            Close();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            worker1.CancelAsync();
            worker2.CancelAsync();
            worker3.CancelAsync();
        }
    }
}