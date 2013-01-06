using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using LeftosCommonLibrary;
using NBA_Stats_Tracker.Data;
using NBA_Stats_Tracker.Data.Misc;
using NBA_Stats_Tracker.Data.Teams;
using SQLite_Database;

namespace NBA_Stats_Tracker.Windows
{
    /// <summary>
    /// Used to edit a conference and its divisions.
    /// </summary>
    public partial class ConferenceEditWindow
    {
        private readonly Conference curConf;

        private ConferenceEditWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConferenceEditWindow" /> class.
        /// </summary>
        /// <param name="conf">The conference to be edited.</param>
        public ConferenceEditWindow(Conference conf) : this()
        {
            curConf = conf;
            txtName.Text = conf.Name;
            txtDivisions.Text = "";
            MainWindow.Divisions.Where(division => division.ConferenceID == conf.ID)
                      .ToList()
                      .ForEach(division => txtDivisions.Text += division.Name + "\n");
            txtDivisions.Text = txtDivisions.Text.TrimEnd(new[] {'\n'});
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            ListWindow.mustUpdate = false;
            Close();
        }

        /// <summary>
        /// Handles the Click event of the btnOK control.
        /// The conference is renamed, the divisions are deleted and recreated, and if any teams are left in division IDs that no longer exist, 
        /// they get reassigned.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            var db = new SQLiteDatabase(MainWindow.currentDB);
            if (String.IsNullOrWhiteSpace(txtName.Text))
                txtName.Text = "League";

            MainWindow.Conferences.Single(conference => conference.ID == curConf.ID).Name = txtName.Text;
            db.Update("Conferences", new Dictionary<string, string> {{"Name", txtName.Text}}, "ID = " + curConf.ID);

            MainWindow.Divisions.RemoveAll(division => division.ConferenceID == curConf.ID);
            db.Delete("Divisions", "Conference = " + curConf.ID);

            var usedIDs = new List<int>();
            db.GetDataTable("SELECT ID FROM Divisions").Rows.Cast<DataRow>().ToList().ForEach(row => usedIDs.Add(Tools.getInt(row, "ID")));

            List<string> list = Tools.SplitLinesToList(txtDivisions.Text, false);
            foreach (string newDiv in list)
            {
                string newName = newDiv.Replace(':', '-');
                int i = 0;
                while (usedIDs.Contains(i))
                    i++;
                MainWindow.Divisions.Add(new Division {ID = i, Name = newName, ConferenceID = curConf.ID});
                usedIDs.Add(i);
            }

            if (MainWindow.Divisions.Any(division => division.ConferenceID == curConf.ID) == false)
            {
                int i = 0;
                while (usedIDs.Contains(i))
                    i++;
                MainWindow.Divisions.Add(new Division {ID = i, Name = txtName.Text, ConferenceID = curConf.ID});
                usedIDs.Add(i);
            }

            foreach (Division div in MainWindow.Divisions.Where(division => division.ConferenceID == curConf.ID))
            {
                db.Insert("Divisions",
                          new Dictionary<string, string>
                          {
                              {"ID", div.ID.ToString()},
                              {"Name", div.Name},
                              {"Conference", div.ConferenceID.ToString()}
                          });
            }

            TeamStats.CheckForInvalidDivisions();

            ListWindow.mustUpdate = true;
            Close();
        }
    }
}