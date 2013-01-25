using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Windows;
using System.Windows.Input;
using NBA_Stats_Tracker.Helper.Miscellaneous;
using NBA_Stats_Tracker.Interop.REDitor;

namespace NBA_Stats_Tracker.Windows
{
    /// <summary>
    ///     Interaction logic for PickGamesWindow.xaml
    /// </summary>
    public partial class PickGamesWindow : Window
    {
        private readonly List<int> _teams = new List<int>();

        public PickGamesWindow()
        {
            InitializeComponent();
        }

        public PickGamesWindow(List<int> teams) : this()
        {
            _teams = teams;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            REDitor.SelectedDate = dtpToday.SelectedDate.GetValueOrDefault();
            DialogResult = true;
            Close();
        }

        private void btnAddGame_Click(object sender, RoutedEventArgs e)
        {
            if (lstAvailableAway.SelectedItems.Count == 1 && lstAvailableHome.SelectedItems.Count == 1)
            {
                object away = lstAvailableAway.SelectedItem;
                object home = lstAvailableHome.SelectedItem;
                lstSelectedGames.Items.Add(away + " @ " + home);
                REDitor.pickedTeams.Add(Misc.GetTeamIDFromDisplayName(MainWindow.tst, away.ToString()));
                REDitor.pickedTeams.Add(Misc.GetTeamIDFromDisplayName(MainWindow.tst, home.ToString()));
                lstAvailableAway.Items.Remove(home);
                lstAvailableHome.Items.Remove(away);
                lstAvailableHome.Items.Remove(home);
                lstAvailableAway.Items.Remove(away);
                /*
                if (lstAvailableAway.Items.Count == 0 && lstAvailableHome.Items.Count == 0)
                    btnOK.IsEnabled = true;
                */
            }
            else
            {
                SystemSounds.Beep.Play();
            }
        }

        private void lstSelectedGames_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lstSelectedGames.SelectedItems.Count == 1)
            {
                if (MessageBox.Show("Are you sure you want to remove \"" + lstSelectedGames.SelectedItem + "\"?") == MessageBoxResult.Yes)
                {
                    string[] parts = lstSelectedGames.SelectedItem.ToString().Split(new[] {" @ "}, StringSplitOptions.None);
                    lstSelectedGames.Items.Remove(lstSelectedGames.SelectedItem);
                    foreach (string part in parts)
                    {
                        REDitor.pickedTeams.Remove(Misc.GetTeamIDFromDisplayName(MainWindow.tst, part));
                        lstAvailableAway.Items.Add(part);
                        lstAvailableHome.Items.Add(part);
                    }

                    List<string> list = lstAvailableAway.Items.Cast<string>().ToList();
                    list.Sort();
                    lstAvailableAway.Items.Clear();
                    list.ForEach(item => lstAvailableAway.Items.Add(item));

                    list = lstAvailableHome.Items.Cast<string>().ToList();
                    list.Sort();
                    lstAvailableHome.Items.Clear();
                    list.ForEach(item => lstAvailableHome.Items.Add(item));

                    /*
                    if (lstAvailableAway.Items.Count != 0 || lstAvailableHome.Items.Count != 0)
                        btnOK.IsEnabled = false;
                    */
                }
            }
        }

        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {
            foreach (int team in _teams)
            {
                lstAvailableAway.Items.Add(MainWindow.tst[team].displayName);
                lstAvailableHome.Items.Add(MainWindow.tst[team].displayName);
            }

            dtpToday.SelectedDate = DateTime.Today;

            //btnOK.IsEnabled = false;
        }
    }
}