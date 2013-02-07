#region Copyright Notice

// Created by Lefteris Aslanoglou, (c) 2011-2013
// 
// Initial development until v1.0 done as part of the implementation of thesis
// "Application Development for Basketball Statistical Analysis in Natural Language"
// under the supervision of Prof. Athanasios Tsakalidis & MSc Alexandros Georgiou
// 
// All rights reserved. Unless specifically stated otherwise, the code in this file should 
// not be reproduced, edited and/or republished without explicit permission from the 
// author.

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

namespace NBA_Stats_Tracker.Windows
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
            foreach (var kvp in MainWindow.TeamOrder)
            {
                teams.Add(new KeyValuePair<string, int>(kvp.Key, kvp.Value));
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
                List<string> lines = Tools.SplitLinesToList(txtTeams.Text, false);
                MainWindow.AddInfo = "";
                foreach (var line in lines)
                {
                    MainWindow.AddInfo += line + "\n";
                }
            }
            else if (Equals(tbcAdd.SelectedItem, tabPlayers))
            {
                int i = SQLiteIO.GetMaxPlayerID(MainWindow.CurrentDB);
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