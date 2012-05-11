using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using NBA_Stats_Tracker.Data;
using SQLite_Database;

namespace NBA_Stats_Tracker.Windows
{
    /// <summary>
    /// Interaction logic for BoxScoreListW.xaml
    /// </summary>
    public partial class BoxScoreListW : Window
    {
        private DataTable res;
        private ObservableCollection<BoxScoreEntry> bshist;
        private SQLiteDatabase db;

        public BoxScoreListW()
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

        private void dgvBoxScores_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var boxScoreEntry = dgvBoxScores.SelectedItem as BoxScoreEntry;
            if (boxScoreEntry != null)
            {
                int id = boxScoreEntry.bs.id;

                boxScoreW bw = new boxScoreW(boxScoreW.Mode.ViewAndIgnore, id);
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
                    "Are you sure you want to delete this box score?\n" +
                    "This action cannot be undone.\n\n" +
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
