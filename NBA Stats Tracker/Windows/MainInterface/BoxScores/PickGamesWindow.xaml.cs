#region Copyright Notice

//    Copyright 2011-2013 Eleftherios Aslanoglou
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

#endregion

namespace NBA_Stats_Tracker.Windows.MainInterface.BoxScores
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Media;
    using System.Windows;
    using System.Windows.Input;

    using NBA_Stats_Tracker.Helper.Miscellaneous;
    using NBA_Stats_Tracker.Interop.REDitor;

    #endregion

    /// <summary>Interaction logic for PickGamesWindow.xaml</summary>
    public partial class PickGamesWindow : Window
    {
        private readonly List<int> _teams = new List<int>();

        public PickGamesWindow()
        {
            InitializeComponent();
        }

        public PickGamesWindow(List<int> teams)
            : this()
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
                var away = lstAvailableAway.SelectedItem;
                var home = lstAvailableHome.SelectedItem;
                lstSelectedGames.Items.Add(away + " @ " + home);
                REDitor.PickedTeams.Add(Misc.GetTeamIDFromDisplayName(MainWindow.TST, away.ToString()));
                REDitor.PickedTeams.Add(Misc.GetTeamIDFromDisplayName(MainWindow.TST, home.ToString()));
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
                if (MessageBox.Show(
                    "Are you sure you want to remove \"" + lstSelectedGames.SelectedItem + "\"?",
                    App.AppName,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    var parts = lstSelectedGames.SelectedItem.ToString().Split(new[] { " @ " }, StringSplitOptions.None);
                    lstSelectedGames.Items.Remove(lstSelectedGames.SelectedItem);
                    foreach (var part in parts)
                    {
                        REDitor.PickedTeams.Remove(Misc.GetTeamIDFromDisplayName(MainWindow.TST, part));
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

                    /*
                    if (lstAvailableAway.Items.Count != 0 || lstAvailableHome.Items.Count != 0)
                        btnOK.IsEnabled = false;
                    */
                }
            }
        }

        private void window_Loaded(object sender, RoutedEventArgs e)
        {
            var list = _teams.Select(team => MainWindow.TST[team].DisplayName).ToList();
            list.Sort();
            foreach (var team in list)
            {
                lstAvailableAway.Items.Add(team);
                lstAvailableHome.Items.Add(team);
            }

            dtpToday.SelectedDate = DateTime.Today;

            //btnOK.IsEnabled = false;
        }
    }
}