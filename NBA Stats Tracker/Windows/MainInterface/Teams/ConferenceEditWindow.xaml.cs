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
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using LeftosCommonLibrary;
using NBA_Stats_Tracker.Data.Other;
using NBA_Stats_Tracker.Data.Teams;
using NBA_Stats_Tracker.Windows.MiscTools;
using SQLite_Database;

#endregion

namespace NBA_Stats_Tracker.Windows.MainInterface.Teams
{
    /// <summary>
    ///     Used to edit a conference and its divisions.
    /// </summary>
    public partial class ConferenceEditWindow
    {
        private readonly Conference _curConf;

        private ConferenceEditWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ConferenceEditWindow" /> class.
        /// </summary>
        /// <param name="conf">The conference to be edited.</param>
        public ConferenceEditWindow(Conference conf) : this()
        {
            _curConf = conf;
            txtName.Text = conf.Name;
            txtDivisions.Text = "";
            MainWindow.Divisions.Where(division => division.ConferenceID == conf.ID)
                      .ToList()
                      .ForEach(division => txtDivisions.Text += division.Name + "\n");
            txtDivisions.Text = txtDivisions.Text.TrimEnd(new[] {'\n'});
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            ListWindow.MustUpdate = false;
            Close();
        }

        /// <summary>
        ///     Handles the Click event of the btnOK control.
        ///     The conference is renamed, the divisions are deleted and recreated, and if any teams are left in division IDs that no longer exist,
        ///     they get reassigned.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            var db = new SQLiteDatabase(MainWindow.CurrentDB);
            if (String.IsNullOrWhiteSpace(txtName.Text))
                txtName.Text = "League";

            MainWindow.Conferences.Single(conference => conference.ID == _curConf.ID).Name = txtName.Text;
            db.Update("Conferences", new Dictionary<string, string> {{"Name", txtName.Text}}, "ID = " + _curConf.ID);

            MainWindow.Divisions.RemoveAll(division => division.ConferenceID == _curConf.ID);
            db.Delete("Divisions", "Conference = " + _curConf.ID);

            var usedIDs = new List<int>();
            db.GetDataTable("SELECT ID FROM Divisions")
              .Rows.Cast<DataRow>()
              .ToList()
              .ForEach(row => usedIDs.Add(ParseCell.GetInt32(row, "ID")));

            var list = Tools.SplitLinesToList(txtDivisions.Text, false);
            foreach (var newDiv in list)
            {
                var newName = newDiv.Replace(':', '-');
                var i = 0;
                while (usedIDs.Contains(i))
                    i++;
                MainWindow.Divisions.Add(new Division {ID = i, Name = newName, ConferenceID = _curConf.ID});
                usedIDs.Add(i);
            }

            if (MainWindow.Divisions.Any(division => division.ConferenceID == _curConf.ID) == false)
            {
                var i = 0;
                while (usedIDs.Contains(i))
                    i++;
                MainWindow.Divisions.Add(new Division {ID = i, Name = txtName.Text, ConferenceID = _curConf.ID});
                usedIDs.Add(i);
            }

            foreach (var div in MainWindow.Divisions.Where(division => division.ConferenceID == _curConf.ID))
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

            ListWindow.MustUpdate = true;
            Close();
        }
    }
}