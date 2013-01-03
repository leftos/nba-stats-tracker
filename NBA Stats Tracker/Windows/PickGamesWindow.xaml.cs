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
using NBA_Stats_Tracker.Helper;

namespace NBA_Stats_Tracker.Windows
{
    /// <summary>
    /// Interaction logic for PickGamesWindow.xaml
    /// </summary>
    public partial class PickGamesWindow : Window
    {
        private List<int> _teams = new List<int>(); 

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
            Interop.InteropREditor.SelectedDate = dtpToday.SelectedDate.GetValueOrDefault();
            DialogResult = true;
            Close();
        }

        private void btnAddGame_Click(object sender, RoutedEventArgs e)
        {
            if (lstAvailableAway.SelectedItems.Count == 1 && lstAvailableHome.SelectedItems.Count == 1)
            {
                var away = lstAvailableAway.SelectedItem;
                var home = lstAvailableHome.SelectedItem;
                lstSelectedGames.Items.Add(away + " @ " + home);
                Interop.InteropREditor.pickedTeams.Add(MainWindow.TeamOrder[Misc.GetCurTeamFromDisplayName(MainWindow.tst, away.ToString())]);
                Interop.InteropREditor.pickedTeams.Add(MainWindow.TeamOrder[Misc.GetCurTeamFromDisplayName(MainWindow.tst, home.ToString())]);
                lstAvailableAway.Items.Remove(home);
                lstAvailableHome.Items.Remove(away);
                lstAvailableHome.Items.Remove(home);
                lstAvailableAway.Items.Remove(away);
                if (lstAvailableAway.Items.Count == 0 && lstAvailableHome.Items.Count == 0)
                    btnOK.IsEnabled = true;
            }
            else
            {
                System.Media.SystemSounds.Beep.Play();
            }
        }

        private void lstSelectedGames_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lstSelectedGames.SelectedItems.Count == 1)
            {
                if (MessageBox.Show("Are you sure you want to remove \"" + lstSelectedGames.SelectedItem + "\"?") == MessageBoxResult.Yes)
                {
                    var parts = lstSelectedGames.SelectedItem.ToString().Split(new []{" @ "}, StringSplitOptions.None);
                    lstSelectedGames.Items.Remove(lstSelectedGames.SelectedItem);
                    foreach (var part in parts)
                    {
                        Interop.InteropREditor.pickedTeams.Remove(
                            MainWindow.TeamOrder[Misc.GetCurTeamFromDisplayName(MainWindow.tst, part)]);
                        lstAvailableAway.Items.Add(part);
                        lstAvailableHome.Items.Add(part);
                    }

                    var list = lstAvailableAway.Items.Cast<string>().ToList();
                    list.Sort();
                    lstAvailableAway.Items.Clear();
                    list.ForEach(item => lstAvailableAway.Items.Add(item));

                    list = lstAvailableHome.Items.Cast<string>().ToList();
                    list.Sort();
                    lstAvailableHome.Items.Clear();
                    list.ForEach(item => lstAvailableHome.Items.Add(item));

                    if (lstAvailableAway.Items.Count != 0 || lstAvailableHome.Items.Count != 0)
                        btnOK.IsEnabled = false;
                }
            }
        }

        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {
            foreach (var team in _teams)
            {
                lstAvailableAway.Items.Add(MainWindow.tst[team].displayName);
                lstAvailableHome.Items.Add(MainWindow.tst[team].displayName);
            }

            dtpToday.SelectedDate = DateTime.Today;

            btnOK.IsEnabled = false;
        }
    }
}
