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

#region Using Directives

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using LeftosCommonLibrary;
using NBA_Stats_Tracker.Data.Players;
using NBA_Stats_Tracker.Data.SQLiteIO;

#endregion

namespace NBA_Stats_Tracker.Windows.MainInterface.ToolWindows
{
    /// <summary>
    ///     Used for adding Teams and Players to the database.
    /// </summary>
    public partial class AddWindow
    {
        private readonly Dictionary<int, PlayerStats> _pst;

        public AddWindow(ref Dictionary<int, PlayerStats> pst)
        {
            InitializeComponent();

            _pst = pst;

            teams = new ObservableCollection<KeyValuePair<string, int>>();
            foreach (var kvp in MainWindow.TST)
            {
                teams.Add(new KeyValuePair<string, int>(kvp.Value.DisplayName, kvp.Key));
            }

            players = new ObservableCollection<Player>();

            teamColumn.ItemsSource = teams;
            dgvAddPlayers.ItemsSource = players;

            dgvAddPlayers.RowEditEnding += GenericEventHandlers.WPFDataGrid_RowEditEnding_GoToNewRowOnTab;
            dgvAddPlayers.PreviewKeyDown += GenericEventHandlers.Any_PreviewKeyDown_CheckTab;
            dgvAddPlayers.PreviewKeyUp += GenericEventHandlers.Any_PreviewKeyUp_CheckTab;
        }

        private ObservableCollection<Player> players { get; set; }
        private ObservableCollection<KeyValuePair<string, int>> teams { get; set; }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            var newpst = new Dictionary<int, PlayerStats>(_pst);

            if (Equals(tbcAdd.SelectedItem, tabTeams))
            {
                var lines = Tools.SplitLinesToList(txtTeams.Text, false);
                MainWindow.AddInfo = "";
                foreach (var line in lines)
                {
                    MainWindow.AddInfo += line + "\n";
                }
            }
            else if (Equals(tbcAdd.SelectedItem, tabPlayers))
            {
                var i = SQLiteIO.GetMaxPlayerID(MainWindow.CurrentDB);
                foreach (var p in players)
                {
                    if (String.IsNullOrWhiteSpace(p.LastName) || p.Team == -1)
                    {
                        MessageBox.Show("You have to enter the Last Name and Team for all players");
                        return;
                    }
                    p.ID = ++i;
                    newpst.Add(p.ID, new PlayerStats(p, true));
                }
                MainWindow.PST = newpst;
                MainWindow.AddInfo = "$$NST Players Added";
            }

            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.AddInfo = "";
            Close();
        }
    }
}