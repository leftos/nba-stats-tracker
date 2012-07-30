using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using LeftosCommonLibrary;
using NBA_Stats_Tracker.Data;
using SQLite_Database;

namespace NBA_Stats_Tracker.Windows
{
    /// <summary>
    /// Interaction logic for ConferenceEditWindow.xaml
    /// </summary>
    public partial class ConferenceEditWindow : Window
    {
        private Conference curConf;

        private ConferenceEditWindow()
        {
            InitializeComponent();
        }

        public ConferenceEditWindow(Conference conf) : this()
        {
            curConf = conf;
            txtName.Text = conf.Name;
            txtDivisions.Text = "";
            MainWindow.Divisions.Where(division => division.ConferenceID == conf.ID).ToList().ForEach(
                division => txtDivisions.Text += division.Name + "\n");
            txtDivisions.Text = txtDivisions.Text.TrimEnd(new char[] {'\n'});
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            ListWindow.mustUpdate = false;
            Close();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            SQLiteDatabase db = new SQLiteDatabase(MainWindow.currentDB);
            if (String.IsNullOrWhiteSpace(txtName.Text))
                txtName.Text = "League";

            MainWindow.Conferences.Single(conference => conference.ID == curConf.ID).Name = txtName.Text;
            db.Update("Conferences", new Dictionary<string, string> {{"Name", txtName.Text}}, "ID = " + curConf.ID);

            MainWindow.Divisions.RemoveAll(division => division.ConferenceID == curConf.ID);
            db.Delete("Divisions", "Conference = " + curConf.ID);

            List<int> usedIDs = new List<int>();
            db.GetDataTable("SELECT ID FROM Divisions").Rows.Cast<DataRow>().ToList().ForEach(row => usedIDs.Add(Tools.getInt(row, "ID")));

            var list = Tools.SplitLinesToList(txtDivisions.Text, false);
            foreach (var newDiv in list)
            {
                var newName = newDiv.Replace(':', '-');
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
                MainWindow.Divisions.Add(new Division
                                             {
                                                 ID = i,
                                                 Name = txtName.Text,
                                                 ConferenceID = curConf.ID
                                             });
                usedIDs.Add(i);
            }

            foreach (var div in MainWindow.Divisions.Where(division => division.ConferenceID == curConf.ID))
            {
                db.Insert("Divisions",
                          new Dictionary<string, string> {{"ID", div.ID.ToString()}, {"Name", div.Name}, {"Conference", div.ConferenceID.ToString()}});
            }

            TeamStats.CheckForInvalidDivisions();

            ListWindow.mustUpdate = true;
            Close();
        }
    }
}
