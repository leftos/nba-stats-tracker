using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NBA_Stats_Tracker
{
    /// <summary>
    /// Interaction logic for enableTeamsW.xaml
    /// </summary>
    public partial class enableTeamsW : Window
    {
        private string _currentDB;
        private int _curSeason;
        private int _maxSeason;

        public enableTeamsW(string currentDB, int curSeason, int maxSeason)
        {
            InitializeComponent();
            _currentDB = currentDB;
            _curSeason = curSeason;
            _maxSeason = maxSeason;

            lblCurSeason.Content = "Current Season: " + this._curSeason + "/" + _maxSeason;

            string teamsT = "Teams";
            string pl_teamsT = "PlayoffTeams";
            string oppT = "Opponents";
            string pl_oppT = "PlayoffOpponents";
            if (_curSeason != _maxSeason)
            {
                string s = "S" + _curSeason;
                teamsT += s;
                pl_teamsT += s;
                oppT += s;
                pl_oppT += s;
            }

            
            string q = "select DisplayName, isHidden from " + teamsT + " ORDER BY DisplayName ASC";
            
            SQLiteDatabase db = new SQLiteDatabase(_currentDB);
            DataTable res = db.GetDataTable(q);

            foreach (DataRow r in res.Rows)
            {
                if (!NSTHelper.getBoolean(r, "isHidden"))
                {
                    lstEnabled.Items.Add(NSTHelper.getString(r, "DisplayName"));
                }
                else
                {
                    lstDisabled.Items.Add(NSTHelper.getString(r, "DisplayName"));
                }
            }
        }

        private void btnEnable_Click(object sender, RoutedEventArgs e)
        {
            string[] names = new string[lstDisabled.SelectedItems.Count];
            lstDisabled.SelectedItems.CopyTo(names, 0);

            foreach (string name in names)
            {
                lstEnabled.Items.Add(name);
                lstDisabled.Items.Remove(name);
            }
        }

        private void btnDisable_Click(object sender, RoutedEventArgs e)
        {
            string[] names = new string[lstEnabled.SelectedItems.Count];
            lstEnabled.SelectedItems.CopyTo(names, 0);

            foreach (string name in names)
            {
                lstDisabled.Items.Add(name);
                lstEnabled.Items.Remove(name);
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            SQLiteDatabase db = new SQLiteDatabase(_currentDB);

            string teamsT = "Teams";
            string pl_teamsT = "PlayoffTeams";
            string oppT = "Opponents";
            string pl_oppT = "PlayoffOpponents";
            if (_curSeason != _maxSeason)
            {
                string s = "S" + _curSeason;
                teamsT += s;
                pl_teamsT += s;
                oppT += s;
                pl_oppT += s;
            }

            foreach (string name in lstEnabled.Items)
            {
                Dictionary<string, string> dict = new Dictionary<string, string>();
                dict.Add("isHidden", "False");
                db.Update(teamsT, dict, "DisplayName LIKE '" + name + "'");
                db.Update(pl_teamsT, dict, "DisplayName LIKE '" + name + "'");
                db.Update(oppT, dict, "DisplayName LIKE '" + name + "'");
                db.Update(pl_oppT, dict, "DisplayName LIKE '" + name + "'");
            }

            foreach (string name in lstDisabled.Items)
            {
                Dictionary<string, string> dict = new Dictionary<string, string>();
                dict.Add("isHidden", "True");
                db.Update(teamsT, dict, "DisplayName LIKE '" + name + "'");
                db.Update(pl_teamsT, dict, "DisplayName LIKE '" + name + "'");
                db.Update(oppT, dict, "DisplayName LIKE '" + name + "'");
                db.Update(pl_oppT, dict, "DisplayName LIKE '" + name + "'");
            }

            MainWindow.addInfo = "$$TEAMSENABLED";
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.addInfo = "";
            Close();
        }
    }
}
