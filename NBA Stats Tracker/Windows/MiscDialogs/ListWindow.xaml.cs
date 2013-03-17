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

namespace NBA_Stats_Tracker.Windows.MiscDialogs
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;

    using LeftosCommonLibrary.CommonDialogs;

    using NBA_Stats_Tracker.Data.Other;
    using NBA_Stats_Tracker.Data.Teams;
    using NBA_Stats_Tracker.Windows.MainInterface;
    using NBA_Stats_Tracker.Windows.MainInterface.Teams;

    using SQLite_Database;

    /// <summary>Implements a multi-purpose single list window with add & remove buttons.</summary>
    public partial class ListWindow
    {
        public static bool MustUpdate;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ListWindow" /> class.
        /// </summary>
        public ListWindow()
        {
            InitializeComponent();

            lstData.DisplayMemberPath = "Name";
            lstData.ItemsSource = MainWindow.Conferences;

            txbMessage.Text = "Double-click on the conference you want to edit, or click on Add to add a new one.";

            Title = "Edit Conferences";
        }

        /// <summary>
        ///     Handles the MouseDoubleClick event of the lstData control. When editing conferences, it displays the Edit Conference Window
        ///     for the selected conference.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="MouseButtonEventArgs" /> instance containing the event data.
        /// </param>
        private void lstData_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lstData.SelectedIndex == -1)
            {
                return;
            }

            var conf = (Conference) lstData.SelectedItem;
            showEditConferenceWindow(conf);
        }

        /// <summary>Shows the edit conference window, and reloads the conferences the window is closed, if required.</summary>
        /// <param name="conf">The conf.</param>
        private void showEditConferenceWindow(Conference conf)
        {
            var cew = new ConferenceEditWindow(conf);
            cew.ShowDialog();

            if (MustUpdate)
            {
                lstData.ItemsSource = null;
                lstData.ItemsSource = MainWindow.Conferences;
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>Handles the Click event of the btnAdd control. Allows the user to add a new item.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            var ibw = new InputBoxWindow("Enter the name for the new conference:");
            if (ibw.ShowDialog() == true)
            {
                string name = InputBoxWindow.UserInput.Replace(':', '-');
                if (MainWindow.Conferences.Any(conference => conference.Name == name))
                {
                    MessageBox.Show("There's already a conference with the name " + name + ".");
                    return;
                }
                var usedIDs = new List<int>();
                MainWindow.Conferences.ForEach(conference => usedIDs.Add(conference.ID));
                int i = 0;
                while (usedIDs.Contains(i))
                {
                    i++;
                }

                MainWindow.Conferences.Add(new Conference { ID = i, Name = name });

                var db = new SQLiteDatabase(MainWindow.CurrentDB);
                db.Insert("Conferences", new Dictionary<string, string> { { "ID", i.ToString() }, { "Name", name } });
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

                showEditConferenceWindow(confToEdit);
            }
        }

        /// <summary>Handles the Click event of the btnRemove control. Allows the user to remove an item.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            if (lstData.SelectedIndex == -1)
            {
                return;
            }

            var conf = (Conference) lstData.SelectedItem;
            MessageBoxResult r = MessageBox.Show(
                "Are you sure you want to delete the " + conf.Name + " conference?",
                "NBA Stats Tracker",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            if (r == MessageBoxResult.No)
            {
                return;
            }

            var db = new SQLiteDatabase(MainWindow.CurrentDB);

            MainWindow.Conferences.Remove(conf);
            db.Delete("Conferences", "ID = " + conf.ID);
            MainWindow.Divisions.RemoveAll(division => division.ConferenceID == conf.ID);
            db.Delete("Divisions", "Conference = " + conf.ID);

            if (MainWindow.Conferences.Count == 0)
            {
                MainWindow.Conferences.Add(new Conference { ID = 0, Name = "League" });
                db.Insert("Conferences", new Dictionary<string, string> { { "ID", "0" }, { "Name", "League" } });
                MainWindow.Divisions.Add(new Division { ID = 0, Name = "League", ConferenceID = 0 });
                db.Insert("Divisions", new Dictionary<string, string> { { "ID", "0" }, { "Name", "League" }, { "Conference", "0" } });
            }
            lstData.ItemsSource = null;
            lstData.ItemsSource = MainWindow.Conferences;

            TeamStats.CheckForInvalidDivisions();
        }
    }
}