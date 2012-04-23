using System.Collections.Generic;
using System.Windows;

namespace NBA_2K12_Correct_Team_Stats
{
    /// <summary>
    /// Interaction logic for askTeam.xaml
    /// </summary>
    public partial class askTeamW : Window
    {
        public static bool _versus;

        public askTeamW(bool versus, int index = 0)
        {
            InitializeComponent();
            _versus = versus;

            if (!versus)
            {
                cmbTeams1.Items.Add("Mode 0");
                cmbTeams1.Items.Add("Mode 1");
                cmbTeams1.Items.Add("Mode 2");
                cmbTeams1.Items.Add("Mode 3");
                cmbTeams1.Items.Add("Mode 4");
                cmbTeams1.Items.Add("Mode 5");
                cmbTeams2.Visibility = Visibility.Hidden;
            }
            else
            {
                label1.Content = "Pick the two teams";
                cmbTeams2.Visibility = Visibility.Visible;
                foreach (KeyValuePair<string, int> kvp in MainWindow.TeamOrder)
                {
                    cmbTeams1.Items.Add(kvp.Key);
                    cmbTeams2.Items.Add(kvp.Key);
                }
            }

            cmbTeams1.SelectedIndex = index;
            if (index != 0)
                cmbTeams2.SelectedIndex = 0;
            else
                cmbTeams2.SelectedIndex = 1;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            if (!_versus)
            {
                StatsTracker.mode = cmbTeams1.SelectedItem.ToString();
            }
            else
            {
                versusW vw = new versusW(cmbTeams1.SelectedItem.ToString(), cmbTeams2.SelectedItem.ToString(), MainWindow.tst);
                vw.ShowDialog();
            }
            this.Close();
        }
    }
}
