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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;
using NBA_Stats_Tracker.Data.PastStats;
using NBA_Stats_Tracker.Data.SQLiteIO;

#endregion

namespace NBA_Stats_Tracker.Windows
{
    /// <summary>
    ///     Interaction logic for AddStatsWindow.xaml
    /// </summary>
    public partial class AddStatsWindow : Window
    {
        private readonly int _id;
        private readonly bool _isTeam;

        public AddStatsWindow(bool isTeam, int id)
        {
            InitializeComponent();

            _isTeam = isTeam;
            _id = id;

            if (isTeam)
            {
                PTSList = new ObservableCollection<PastTeamStats>();
                dgGamesPlayedColumn.Visibility = Visibility.Collapsed;
                dgGamesStartedColumn.Visibility = Visibility.Collapsed;
                dgPlayerPointsColumn.Visibility = Visibility.Collapsed;
                dgTeamFColumn.Visibility = Visibility.Collapsed;
                dgTeamSColumn.Visibility = Visibility.Collapsed;
                string qr = "SELECT * FROM PastTeamStats WHERE TeamID = " + id + " ORDER BY \"SOrder\"";
                DataTable dt = MainWindow.DB.GetDataTable(qr);
                dt.Rows.Cast<DataRow>().ToList().ForEach(dr => PTSList.Add(new PastTeamStats(dr)));
                dgStats.ItemsSource = PTSList;
            }
            else
            {
                PPSList = new ObservableCollection<PastPlayerStats>();
                dgWinsColumn.Visibility = Visibility.Collapsed;
                dgLossesColumn.Visibility = Visibility.Collapsed;
                dgTeamPointsAgainstColumn.Visibility = Visibility.Collapsed;
                dgTeamPointsForColumn.Visibility = Visibility.Collapsed;
                string qr = "SELECT * FROM PastPlayerStats WHERE PlayerID = " + id + " ORDER BY \"SOrder\"";
                DataTable dt = MainWindow.DB.GetDataTable(qr);
                dt.Rows.Cast<DataRow>().ToList().ForEach(dr => PPSList.Add(new PastPlayerStats(dr)));
                dgStats.ItemsSource = PPSList;
            }
        }

        public static ObservableCollection<PastTeamStats> PTSList { get; set; }
        public static ObservableCollection<PastPlayerStats> PPSList { get; set; }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (_isTeam)
            {
                foreach (var pts in PTSList)
                {
                    pts.EndEdit();
                    pts.TeamID = _id;
                }
                SQLiteIO.SavePastTeamStatsToDatabase(MainWindow.DB, new List<PastTeamStats>(PTSList));
            }
            else
            {
                foreach (var pps in PPSList)
                {
                    pps.EndEdit();
                    pps.PlayerID = _id;
                }
                SQLiteIO.SavePastPlayerStatsToDatabase(MainWindow.DB, new List<PastPlayerStats>(PPSList));
            }

            DialogResult = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}