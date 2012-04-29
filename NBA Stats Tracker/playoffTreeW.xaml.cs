using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NBA_2K12_Correct_Team_Stats
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class playoffTreeW : Window
    {
        private readonly Brush defaultBackground;
        private readonly PlayoffTree myPT;
        private bool valid = true;

        public playoffTreeW()
        {
            InitializeComponent();

            myPT = StatsTracker.tempPT;

            foreach (var kvp in MainWindow.TeamOrder)
            {
                if (MainWindow.West.Contains(kvp.Key))
                {
                    cmbTeam1.Items.Add(kvp.Key);
                    cmbTeam2.Items.Add(kvp.Key);
                    cmbTeam3.Items.Add(kvp.Key);
                    cmbTeam4.Items.Add(kvp.Key);
                    cmbTeam5.Items.Add(kvp.Key);
                    cmbTeam6.Items.Add(kvp.Key);
                    cmbTeam7.Items.Add(kvp.Key);
                    cmbTeam8.Items.Add(kvp.Key);
                }
                else
                {
                    cmbTeam9.Items.Add(kvp.Key);
                    cmbTeam10.Items.Add(kvp.Key);
                    cmbTeam11.Items.Add(kvp.Key);
                    cmbTeam12.Items.Add(kvp.Key);
                    cmbTeam13.Items.Add(kvp.Key);
                    cmbTeam14.Items.Add(kvp.Key);
                    cmbTeam15.Items.Add(kvp.Key);
                    cmbTeam16.Items.Add(kvp.Key);
                }
            }

            defaultBackground = cmbTeam1.Background;

            cmbTeam1.SelectedIndex = 0;
            cmbTeam2.SelectedIndex = 1;
            cmbTeam3.SelectedIndex = 2;
            cmbTeam4.SelectedIndex = 3;
            cmbTeam5.SelectedIndex = 4;
            cmbTeam6.SelectedIndex = 5;
            cmbTeam7.SelectedIndex = 6;
            cmbTeam8.SelectedIndex = 7;
            cmbTeam9.SelectedIndex = 0;
            cmbTeam10.SelectedIndex = 1;
            cmbTeam11.SelectedIndex = 2;
            cmbTeam12.SelectedIndex = 3;
            cmbTeam13.SelectedIndex = 4;
            cmbTeam14.SelectedIndex = 5;
            cmbTeam15.SelectedIndex = 6;
            cmbTeam16.SelectedIndex = 7;

            myPT.done = false;
        }

        private void checkIfSameTeamsWest(object sender, SelectionChangedEventArgs e)
        {
            var tree = new string[8];
            try
            {
                tree[0] = cmbTeam1.SelectedItem.ToString();
                tree[1] = cmbTeam2.SelectedItem.ToString();
                tree[2] = cmbTeam3.SelectedItem.ToString();
                tree[3] = cmbTeam4.SelectedItem.ToString();
                tree[4] = cmbTeam5.SelectedItem.ToString();
                tree[5] = cmbTeam6.SelectedItem.ToString();
                tree[6] = cmbTeam7.SelectedItem.ToString();
                tree[7] = cmbTeam8.SelectedItem.ToString();
            }
            catch
            {
                return;
            }

            var teams = new List<string>();
            bool found = false;
            for (int i = 0; i < 8; i++)
            {
                if (teams.Contains(tree[i]) == false)
                    teams.Add(tree[i]);
                else
                {
                    found = true;
                    break;
                }
            }

            if (found)
            {
                cmbTeam1.Background = Brushes.Red;
                cmbTeam2.Background = Brushes.Red;
                cmbTeam3.Background = Brushes.Red;
                cmbTeam4.Background = Brushes.Red;
                cmbTeam5.Background = Brushes.Red;
                cmbTeam6.Background = Brushes.Red;
                cmbTeam7.Background = Brushes.Red;
                cmbTeam8.Background = Brushes.Red;
                valid = false;
            }
            else
            {
                cmbTeam1.Background = defaultBackground;
                cmbTeam2.Background = defaultBackground;
                cmbTeam3.Background = defaultBackground;
                cmbTeam4.Background = defaultBackground;
                cmbTeam5.Background = defaultBackground;
                cmbTeam6.Background = defaultBackground;
                cmbTeam7.Background = defaultBackground;
                cmbTeam8.Background = defaultBackground;
                valid = true;
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (!valid)
                myPT.done = false;
            else
            {
                for (int i = 0; i < 16; i++)
                {
                    string cur = "cmbTeam" + (16 - i).ToString();
                    object item = ptGrid.FindName(cur);
                    var cmb = (ComboBox) item;
                    myPT.teams[i] = cmb.SelectedItem.ToString();
                }
                myPT.done = true;
            }

            Close();
        }

        private void checkIfSameTeamsEast(object sender, SelectionChangedEventArgs e)
        {
            var tree = new string[8];
            try
            {
                tree[0] = cmbTeam9.SelectedItem.ToString();
                tree[1] = cmbTeam10.SelectedItem.ToString();
                tree[2] = cmbTeam11.SelectedItem.ToString();
                tree[3] = cmbTeam12.SelectedItem.ToString();
                tree[4] = cmbTeam13.SelectedItem.ToString();
                tree[5] = cmbTeam14.SelectedItem.ToString();
                tree[6] = cmbTeam15.SelectedItem.ToString();
                tree[7] = cmbTeam16.SelectedItem.ToString();
            }
            catch
            {
                return;
            }

            var teams = new List<string>();
            bool found = false;
            for (int i = 0; i < 8; i++)
            {
                if (teams.Contains(tree[i]) == false)
                    teams.Add(tree[i]);
                else
                {
                    found = true;
                    break;
                }
            }

            if (found)
            {
                cmbTeam9.Background = Brushes.Red;
                cmbTeam10.Background = Brushes.Red;
                cmbTeam11.Background = Brushes.Red;
                cmbTeam12.Background = Brushes.Red;
                cmbTeam13.Background = Brushes.Red;
                cmbTeam14.Background = Brushes.Red;
                cmbTeam15.Background = Brushes.Red;
                cmbTeam16.Background = Brushes.Red;
                valid = false;
            }
            else
            {
                cmbTeam9.Background = defaultBackground;
                cmbTeam10.Background = defaultBackground;
                cmbTeam11.Background = defaultBackground;
                cmbTeam12.Background = defaultBackground;
                cmbTeam13.Background = defaultBackground;
                cmbTeam14.Background = defaultBackground;
                cmbTeam15.Background = defaultBackground;
                cmbTeam16.Background = defaultBackground;
                valid = true;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            StatsTracker.tempPT = myPT;
        }
    }
}