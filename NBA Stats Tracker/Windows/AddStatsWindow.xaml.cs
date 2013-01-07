using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;
using NBA_Stats_Tracker.Data.PastStats;
using NBA_Stats_Tracker.Data.SQLiteIO;

namespace NBA_Stats_Tracker.Windows
{
    /// <summary>
    ///     Interaction logic for AddStatsWindow.xaml
    /// </summary>
    public partial class AddStatsWindow : Window
    {
        private readonly int id;
        private readonly bool isTeam;

        public AddStatsWindow(bool isTeam, int id)
        {
            InitializeComponent();

            this.isTeam = isTeam;
            this.id = id;

            if (isTeam)
            {
                ptsList = new ObservableCollection<PastTeamStats>();
                dgGamesPlayedColumn.Visibility = Visibility.Collapsed;
                dgGamesStartedColumn.Visibility = Visibility.Collapsed;
                dgPlayerPointsColumn.Visibility = Visibility.Collapsed;
                dgTeamFColumn.Visibility = Visibility.Collapsed;
                dgTeamSColumn.Visibility = Visibility.Collapsed;
                string qr = "SELECT * FROM PastTeamStats WHERE TeamID = " + id + " ORDER BY \"SOrder\"";
                DataTable dt = MainWindow.db.GetDataTable(qr);
                dt.Rows.Cast<DataRow>().ToList().ForEach(dr => ptsList.Add(new PastTeamStats(dr)));
                dgStats.ItemsSource = ptsList;
            }
            else
            {
                ppsList = new ObservableCollection<PastPlayerStats>();
                dgWinsColumn.Visibility = Visibility.Collapsed;
                dgLossesColumn.Visibility = Visibility.Collapsed;
                dgTeamPointsAgainstColumn.Visibility = Visibility.Collapsed;
                dgTeamPointsForColumn.Visibility = Visibility.Collapsed;
                string qr = "SELECT * FROM PastPlayerStats WHERE PlayerID = " + id + " ORDER BY \"SOrder\"";
                DataTable dt = MainWindow.db.GetDataTable(qr);
                dt.Rows.Cast<DataRow>().ToList().ForEach(dr => ppsList.Add(new PastPlayerStats(dr)));
                dgStats.ItemsSource = ppsList;
            }
        }

        public static ObservableCollection<PastTeamStats> ptsList { get; set; }
        public static ObservableCollection<PastPlayerStats> ppsList { get; set; }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (isTeam)
            {
                for (int i = 0; i < ptsList.Count; i++)
                {
                    ptsList[i].EndEdit();
                    ptsList[i].TeamID = id;
                }
                SQLiteIO.SavePastTeamStatsToDatabase(MainWindow.db, new List<PastTeamStats>(ptsList));
            }
            else
            {
                for (int i = 0; i < ppsList.Count; i++)
                {
                    ppsList[i].EndEdit();
                    ppsList[i].PlayerID = id;
                }
                SQLiteIO.SavePastPlayerStatsToDatabase(MainWindow.db, new List<PastPlayerStats>(ppsList));
            }

            DialogResult = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}