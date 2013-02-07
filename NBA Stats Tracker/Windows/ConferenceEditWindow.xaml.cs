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
using System.Data;
using System.Linq;
using System.Windows;
using LeftosCommonLibrary;
using NBA_Stats_Tracker.Data.Other;
using NBA_Stats_Tracker.Data.Teams;
using SQLite_Database;

#endregion

namespace NBA_Stats_Tracker.Windows
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
              .ForEach(row => usedIDs.Add(DataRowCellParsers.GetInt32(row, "ID")));

            List<string> list = Tools.SplitLinesToList(txtDivisions.Text, false);
            foreach (var newDiv in list)
            {
                string newName = newDiv.Replace(':', '-');
                int i = 0;
                while (usedIDs.Contains(i))
                    i++;
                MainWindow.Divisions.Add(new Division {ID = i, Name = newName, ConferenceID = _curConf.ID});
                usedIDs.Add(i);
            }

            if (MainWindow.Divisions.Any(division => division.ConferenceID == _curConf.ID) == false)
            {
                int i = 0;
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