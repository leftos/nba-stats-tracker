using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using NBA_Stats_Tracker.Data;
using SQLite_Database;

namespace NBA_Stats_Tracker.Windows
{
    /// <summary>
    /// Interaction logic for ListWindow.xaml
    /// </summary>
    public partial class ListWindow : Window
    {
        public static bool mustUpdate;

        public ListWindow()
        {
            InitializeComponent();

            lstData.DisplayMemberPath = "Name";
            lstData.ItemsSource = MainWindow.Conferences;

            txbMessage.Text = "Double-click on the conference you want to edit, or click on Add to add a new one.";

            Title = "Edit Conferences";
        }

        private void lstData_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lstData.SelectedIndex == -1)
                return;

            var conf = (Conference) lstData.SelectedItem;
            ShowEditConferenceWindow(conf);
        }

        private void ShowEditConferenceWindow(Conference conf)
        {
            var cew = new ConferenceEditWindow(conf);
            cew.ShowDialog();

            if (mustUpdate)
            {
                lstData.ItemsSource = null;
                lstData.ItemsSource = MainWindow.Conferences;
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            var ibw = new InputBoxWindow("Enter the name for the new conference:");
            if (ibw.ShowDialog() == true)
            {
                string name = MainWindow.input.Replace(':', '-');
                if (MainWindow.Conferences.Any(conference => conference.Name == name))
                {
                    MessageBox.Show("There's already a conference with the name " + name + ".");
                    return;
                }
                var usedIDs = new List<int>();
                MainWindow.Conferences.ForEach(conference => usedIDs.Add(conference.ID));
                int i = 0;
                while (usedIDs.Contains(i))
                    i++;

                MainWindow.Conferences.Add(new Conference {ID = i, Name = name});

                var db = new SQLiteDatabase(MainWindow.currentDB);
                db.Insert("Conferences", new Dictionary<string, string> {{"ID", i.ToString()}, {"Name", name}});
                lstData.ItemsSource = null;
                lstData.ItemsSource = MainWindow.Conferences;

                var confToEdit = new Conference();
                foreach (Conference item in lstData.Items)
                {
                    if (item.Name == name)
                    {
                        confToEdit = item;
                        break;
                    }
                }

                ShowEditConferenceWindow(confToEdit);
            }
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            if (lstData.SelectedIndex == -1)
                return;

            var conf = (Conference) lstData.SelectedItem;
            MessageBoxResult r = MessageBox.Show("Are you sure you want to delete the " + conf.Name + " conference?", "NBA Stats Tracker",
                                                 MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (r == MessageBoxResult.No)
                return;

            var db = new SQLiteDatabase(MainWindow.currentDB);

            MainWindow.Conferences.Remove(conf);
            db.Delete("Conferences", "ID = " + conf.ID);
            MainWindow.Divisions.RemoveAll(division => division.ConferenceID == conf.ID);
            db.Delete("Divisions", "Conference = " + conf.ID);

            if (MainWindow.Conferences.Count == 0)
            {
                MainWindow.Conferences.Add(new Conference {ID = 0, Name = "League"});
                db.Insert("Conferences", new Dictionary<string, string> {{"ID", "0"}, {"Name", "League"}});
                MainWindow.Divisions.Add(new Division {ID = 0, Name = "League", ConferenceID = 0});
                db.Insert("Divisions", new Dictionary<string, string> {{"ID", "0"}, {"Name", "League"}, {"Conference", "0"}});
            }
            lstData.ItemsSource = null;
            lstData.ItemsSource = MainWindow.Conferences;

            TeamStats.CheckForInvalidDivisions();
        }
    }
}