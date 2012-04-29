using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

namespace NBA_2K12_Correct_Team_Stats
{
    /// <summary>
    /// Interaction logic for addW.xaml
    /// </summary>
    public partial class addW : Window
    {
        private readonly List<PlayerStats> pst;
        private ObservableCollection<Player> Players { get; set; }
        private ObservableCollection<string> Teams { get; set; }
        private ObservableCollection<string> Positions { get; set; }

        public addW(ref List<PlayerStats> pst)
        {
            InitializeComponent();

            this.pst = pst;

            Teams = new ObservableCollection<string>();
            foreach (var kvp in MainWindow.TeamOrder)
            {
                Teams.Add(kvp.Key);
            }

            Positions = new ObservableCollection<string> {"PG", "SG", "SF", "PF", "C"};

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

            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.addInfo = "";
            Close();
        }
    }
}