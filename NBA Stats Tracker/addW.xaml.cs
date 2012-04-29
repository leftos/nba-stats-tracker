using System.Windows;
using System.Data;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Windows.Data;
using System.Collections.ObjectModel;

namespace NBA_2K12_Correct_Team_Stats
{
    /// <summary>
    /// Interaction logic for addW.xaml
    /// </summary>
    public partial class addW : Window
    {

        public ObservableCollection<Player> Players { get; set; }
        public ObservableCollection<string> Teams { get; set; }
        public ObservableCollection<string> Positions { get; set; }
        private List<PlayerStats> pst;
        public addW(ref List<PlayerStats> pst)
        {
            InitializeComponent();

            this.pst = pst;

            Teams = new ObservableCollection<string>();
            foreach (KeyValuePair<string,int> kvp in MainWindow.TeamOrder)
            {
                Teams.Add(kvp.Key);
            }

            Positions = new ObservableCollection<string>() { "PG", "SG", "SF", "PF", "C" };

            Players = new ObservableCollection<Player>();

            teamColumn.ItemsSource = Teams;
            posColumn.ItemsSource = Positions;
            pos2Column.ItemsSource = Positions;
            dgvAddPlayers.ItemsSource = Players;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (tbcAdd.SelectedItem == tabTeams)
            {
                MainWindow.addInfo = txtTeams.Text;
            }
            else if (tbcAdd.SelectedItem == tabPlayers)
            {
                int i = MainWindow.GetMaxPlayerID(MainWindow.currentDB);
                foreach (Player p in Players)
                {
                    if (p.Position2 == "") p.Position2 = " ";
                    p.ID = ++i;
                    pst.Add(new PlayerStats(p));
                }
                MainWindow.addInfo = "$$NST Players Added";
            }

            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.addInfo = "";
            this.Close();
        }
    }
}
