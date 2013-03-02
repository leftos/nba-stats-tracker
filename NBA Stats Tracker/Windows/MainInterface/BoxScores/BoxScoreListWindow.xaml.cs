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
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using NBA_Stats_Tracker.Data.BoxScores;
using NBA_Stats_Tracker.Data.SQLiteIO;
using SQLite_Database;

#endregion

namespace NBA_Stats_Tracker.Windows.MainInterface.BoxScores
{
    /// <summary>
    ///     Used for displaying a basic list of available box scores, in order to easily delete any of them.
    /// </summary>
    public partial class BoxScoreListWindow
    {
        private readonly ObservableCollection<BoxScoreEntry> _bsHist;
        private readonly SQLiteDatabase _db;

        public BoxScoreListWindow()
        {
            InitializeComponent();

            _db = new SQLiteDatabase(MainWindow.CurrentDB);
            /*
            string q = "select * from GameResults ORDER BY Date DESC";
            res = db.GetDataTable(q);

            dgvBoxScores.DataContext = res.DefaultView;
            */


            MainWindow.BSHist = SQLiteIO.GetAllBoxScoresFromDatabase(MainWindow.CurrentDB, MainWindow.TST);
            _bsHist = new ObservableCollection<BoxScoreEntry>(MainWindow.BSHist);

            dgvBoxScores.ItemsSource = _bsHist;
        }

        /// <summary>
        ///     Handles the MouseDoubleClick event of the dgvBoxScores control. The selected box score is displayed in the Box Score Window.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="MouseButtonEventArgs" /> instance containing the event data.
        /// </param>
        private void dgvBoxScores_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var boxScoreEntry = dgvBoxScores.SelectedItem as BoxScoreEntry;
            if (boxScoreEntry != null)
            {
                int id = boxScoreEntry.BS.ID;

                var bw = new BoxScoreWindow(BoxScoreWindow.Mode.ViewAndIgnore, id);
                try
                {
                    bw.ShowDialog();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception thrown while trying to open Box Score dialog: " + ex.Message);
                }
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        ///     Handles the Click event of the btnDelete control. Deletes all the specified Team Box Score, as well as any corresponding Player Box Scores, from the database.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult r =
                MessageBox.Show(
                    "Are you sure you want to delete this/these box score(s)?\n" + "This action cannot be undone.\n\n" +
                    "Any changes made to Team Stats by automatically adding this/these box score(s) to them won't be reverted by its deletion.",
                    "NBA Stats Tracker", MessageBoxButton.YesNo);
            if (r == MessageBoxResult.Yes)
            {
                foreach (BoxScoreEntry bse in dgvBoxScores.SelectedItems.Cast<BoxScoreEntry>().ToList())
                {
                    if (bse != null)
                    {
                        int id = bse.BS.ID;

                        _db.Delete("GameResults", "GameID = " + id);
                        _db.Delete("PlayerResults", "GameID = " + id);
                    }

                    _bsHist.Remove(bse);
                    MainWindow.BSHist.Remove(bse);
                }
            }
        }
    }
}