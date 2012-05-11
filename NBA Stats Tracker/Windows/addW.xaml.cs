#region Copyright Notice

// Created by Lefteris Aslanoglou, (c) 2011-2012
// 
// Implementation of thesis
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
using NBA_Stats_Tracker.Data;

#endregion

namespace NBA_Stats_Tracker.Windows
{
    /// <summary>
    /// Interaction logic for addW.xaml
    /// </summary>
    public partial class addW : Window
    {
        private readonly Dictionary<int, PlayerStats> pst;

        public addW(ref Dictionary<int, PlayerStats> pst)
        {
            InitializeComponent();

            this.pst = pst;

            Teams = new ObservableCollection<string>();
            foreach (KeyValuePair<string, int> kvp in MainWindow.TeamOrder)
            {
                Teams.Add(kvp.Key);
            }

            Positions = new ObservableCollection<string> {" ", "PG", "SG", "SF", "PF", "C"};
            var Positions2 = new ObservableCollection<string> {" ", "PG", "SG", "SF", "PF", "C"};

            Players = new ObservableCollection<Player>();

            teamColumn.ItemsSource = Teams;
            posColumn.ItemsSource = Positions;
            pos2Column.ItemsSource = Positions2;
            dgvAddPlayers.ItemsSource = Players;

            dgvAddPlayers.RowEditEnding += EventHandlers.WPFDataGrid_RowEditEnding_GoToNewRowOnTab;
            dgvAddPlayers.PreviewKeyDown += EventHandlers.Any_PreviewKeyDown_CheckTab;
            dgvAddPlayers.PreviewKeyUp += EventHandlers.Any_PreviewKeyUp_CheckTab;
        }

        private ObservableCollection<Player> Players { get; set; }
        private ObservableCollection<string> Teams { get; set; }
        private ObservableCollection<string> Positions { get; set; }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            var newpst = new Dictionary<int, PlayerStats>(pst);

            if (tbcAdd.SelectedItem == tabTeams)
            {
                MainWindow.addInfo = txtTeams.Text;
            }
            else if (tbcAdd.SelectedItem == tabPlayers)
            {
                int i = MainWindow.GetMaxPlayerID(MainWindow.currentDB);
                foreach (Player p in Players)
                {
                    if (String.IsNullOrWhiteSpace(p.LastName) || String.IsNullOrWhiteSpace(p.Team))
                    {
                        MessageBox.Show("You have to enter the Last Name, Position and Team for all players");
                        return;
                    }
                    if (p.Position == "") p.Position = " ";
                    if (p.Position2 == "") p.Position2 = " ";
                    p.ID = ++i;
                    newpst.Add(p.ID, new PlayerStats(p));
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