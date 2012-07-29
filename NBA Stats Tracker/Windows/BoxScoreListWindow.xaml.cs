#region Copyright Notice

// Created by Lefteris Aslanoglou, (c) 2011-2012
// 
// Implementation of thesis
// "Application Development for Basketball Statistical Analysis in Natural Language"
// under the supervision of Prof. Athanasios Tsakalidis & MSc Alexandros Georgiou,
// Computer Engineering & Informatics Department, University of Patras, Greece.
// 
// All rights reserved. Unless specifically stated otherwise, the code in this file should 
// not be reproduced, edited and/or republished without explicit permission from the 
// author.

#endregion

#region Using Directives

using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using NBA_Stats_Tracker.Data;
using SQLite_Database;

#endregion

namespace NBA_Stats_Tracker.Windows
{
    /// <summary>
    /// Interaction logic for BoxScoreListW.xaml
    /// </summary>
    public partial class BoxScoreListWindow
    {
        private readonly ObservableCollection<BoxScoreEntry> bshist;
        private readonly SQLiteDatabase db;

        public BoxScoreListWindow()
        {
            InitializeComponent();

            db = new SQLiteDatabase(MainWindow.currentDB);
            /*
            string q = "select * from GameResults ORDER BY Date DESC";
            res = db.GetDataTable(q);

            dgvBoxScores.DataContext = res.DefaultView;
            */


            MainWindow.bshist = SQLiteIO.GetAllBoxScoresFromDatabase(MainWindow.currentDB);
            bshist = new ObservableCollection<BoxScoreEntry>(MainWindow.bshist);

            dgvBoxScores.ItemsSource = bshist;
        }

        private void dgvBoxScores_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var boxScoreEntry = dgvBoxScores.SelectedItem as BoxScoreEntry;
            if (boxScoreEntry != null)
            {
                int id = boxScoreEntry.bs.id;

                var bw = new BoxScoreWindow(BoxScoreWindow.Mode.ViewAndIgnore, id);
                try
                {
                    bw.ShowDialog();
                }
                catch (Exception)
                {
                }
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult r =
                MessageBox.Show(
                    "Are you sure you want to delete this box score?\n" + "This action cannot be undone.\n\n" +
                    "Any changes made to Team Stats by automatically adding this box score to them won't be reverted by its deletion.",
                    "NBA Stats Tracker", MessageBoxButton.YesNo);
            if (r == MessageBoxResult.Yes)
            {
                var boxScoreEntry = dgvBoxScores.SelectedItem as BoxScoreEntry;
                if (boxScoreEntry != null)
                {
                    int id = boxScoreEntry.bs.id;

                    db.Delete("GameResults", "GameID = " + id);
                    db.Delete("PlayerResults", "GameID = " + id);
                }

                bshist.Remove(boxScoreEntry);
                MainWindow.bshist.Remove(boxScoreEntry);
            }
        }
    }
}