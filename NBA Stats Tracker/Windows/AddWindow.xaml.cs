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
        private readonly Dictionary<int, PlayerStats> pst;

        public AddWindow(ref Dictionary<int, PlayerStats> pst)
        {
            InitializeComponent();

            this.pst = pst;

            Teams = new ObservableCollection<KeyValuePair<string, int>>();
            foreach (var kvp in MainWindow.TeamOrder)
            {
                Teams.Add(new KeyValuePair<string, int>(kvp.Key, kvp.Value));
            }

            Players = new ObservableCollection<Player>();

            teamColumn.ItemsSource = Teams;
            dgvAddPlayers.ItemsSource = Players;

            dgvAddPlayers.RowEditEnding += GenericEventHandlers.WPFDataGrid_RowEditEnding_GoToNewRowOnTab;
            dgvAddPlayers.PreviewKeyDown += GenericEventHandlers.Any_PreviewKeyDown_CheckTab;
            dgvAddPlayers.PreviewKeyUp += GenericEventHandlers.Any_PreviewKeyUp_CheckTab;
        }

        private ObservableCollection<Player> Players { get; set; }
        private ObservableCollection<KeyValuePair<string, int>> Teams { get; set; }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            var newpst = new Dictionary<int, PlayerStats>(pst);

            if (tbcAdd.SelectedItem == tabTeams)
            {
                List<string> lines = Tools.SplitLinesToList(txtTeams.Text, false);
                MainWindow.addInfo = "";
                foreach (var line in lines)
                {
                    MainWindow.addInfo += line + "\n";
                }
            }
            else if (tbcAdd.SelectedItem == tabPlayers)
            {
                int i = SQLiteIO.GetMaxPlayerID(MainWindow.currentDB);
                foreach (var p in Players)
                {
                    if (String.IsNullOrWhiteSpace(p.LastName) || p.Team == -1)
                    {
                        MessageBox.Show("You have to enter the Last Name and Team for all players");
                        return;
                    }
                    p.ID = ++i;
                    newpst.Add(p.ID, new PlayerStats(p, true));
                }
                MainWindow.pst = newpst;
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