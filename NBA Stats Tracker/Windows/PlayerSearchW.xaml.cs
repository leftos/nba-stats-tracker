using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NBA_Stats_Tracker.Data;
using SQLite_Database;

namespace NBA_Stats_Tracker.Windows
{
    /// <summary>
    /// Interaction logic for PlayerSearchW.xaml
    /// </summary>
    public partial class PlayerSearchW
    {
        private readonly List<string> Positions = new List<string> {"Any", " ", "PG", "SG", "SF", "PF", "C"};
        private readonly List<string> StringOptions = new List<string> {"Contains", "Is"};
        private readonly List<string> NumericOptions = new List<string> {"<", "<=", "=", ">=", ">"};
        private int curSeason;
        private int maxSeason;
        private Dictionary<int, TeamStats> tst, tstopp;
        private SortedDictionary<string, int> TeamOrder; 

        public PlayerSearchW()
        {
            InitializeComponent();

            cmbFirstNameSetting.ItemsSource = StringOptions;
            cmbFirstNameSetting.SelectedIndex = 0;
            cmbLastNameSetting.ItemsSource = StringOptions;
            cmbLastNameSetting.SelectedIndex = 0;

            cmbPTS.ItemsSource = NumericOptions;
            cmbPTS.SelectedIndex = 3;
            cmbFGM.ItemsSource = NumericOptions;
            cmbFGM.SelectedIndex = 3;
            cmbFGA.ItemsSource = NumericOptions;
            cmbFGA.SelectedIndex = 3;
            cmb3PM.ItemsSource = NumericOptions;
            cmb3PM.SelectedIndex = 3;
            cmb3PA.ItemsSource = NumericOptions;
            cmb3PA.SelectedIndex = 3;
            cmbFTM.ItemsSource = NumericOptions;
            cmbFTM.SelectedIndex = 3;
            cmbFTA.ItemsSource = NumericOptions;
            cmbFTA.SelectedIndex = 3;
            cmbREB.ItemsSource = NumericOptions;
            cmbREB.SelectedIndex = 3;
            cmbOREB.ItemsSource = NumericOptions;
            cmbOREB.SelectedIndex = 3;
            cmbAST.ItemsSource = NumericOptions;
            cmbAST.SelectedIndex = 3;
            cmbSTL.ItemsSource = NumericOptions;
            cmbSTL.SelectedIndex = 3;
            cmbBLK.ItemsSource = NumericOptions;
            cmbBLK.SelectedIndex = 3;
            cmbTO.ItemsSource = NumericOptions;
            cmbTO.SelectedIndex = 3;
            cmbFOUL.ItemsSource = NumericOptions;
            cmbFOUL.SelectedIndex = 3;
            cmbGP.ItemsSource = NumericOptions;
            cmbGP.SelectedIndex = 3;
            cmbGS.ItemsSource = NumericOptions;
            cmbGS.SelectedIndex = 3;

            cmbPPG.ItemsSource = NumericOptions;
            cmbPPG.SelectedIndex = 3;
            cmbFGp.ItemsSource = NumericOptions;
            cmbFGp.SelectedIndex = 3;
            cmbFGeff.ItemsSource = NumericOptions;
            cmbFGeff.SelectedIndex = 3;
            cmb3Pp.ItemsSource = NumericOptions;
            cmb3Pp.SelectedIndex = 3;
            cmb3Peff.ItemsSource = NumericOptions;
            cmb3Peff.SelectedIndex = 3;
            cmbFTp.ItemsSource = NumericOptions;
            cmbFTp.SelectedIndex = 3;
            cmbFTeff.ItemsSource = NumericOptions;
            cmbFTeff.SelectedIndex = 3;
            cmbRPG.ItemsSource = NumericOptions;
            cmbRPG.SelectedIndex = 3;
            cmbORPG.ItemsSource = NumericOptions;
            cmbORPG.SelectedIndex = 3;
            cmbAPG.ItemsSource = NumericOptions;
            cmbAPG.SelectedIndex = 3;
            cmbSPG.ItemsSource = NumericOptions;
            cmbSPG.SelectedIndex = 3;
            cmbBPG.ItemsSource = NumericOptions;
            cmbBPG.SelectedIndex = 3;
            cmbTPG.ItemsSource = NumericOptions;
            cmbTPG.SelectedIndex = 3;
            cmbFPG.ItemsSource = NumericOptions;
            cmbFPG.SelectedIndex = 3;

            cmbPosition1.ItemsSource = Positions;
            cmbPosition1.SelectedIndex = 0;
            cmbPosition2.ItemsSource = Positions;
            cmbPosition2.SelectedIndex = 0;

            PopulateSeasonCombo();
            cmbSeasonNum.SelectedItem = MainWindow.curSeason;

            //chkIsActive.IsChecked = null;
            //cmbTeam.SelectedItem = "- Any -";
            
            dgvPlayerStats.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
        }

        private string GetCurTeamFromDisplayName(string p)
        {
            foreach (var kvp in tst.Keys)
            {
                if (tst[kvp].displayName == p)
                {
                    return tst[kvp].name;
                }
            }
            return "$$TEAMNOTFOUND: " + p;
        }

        private string GetDisplayNameFromTeam(string p)
        {
            foreach (var kvp in tst.Keys)
            {
                if (tst[kvp].name == p)
                {
                    return tst[kvp].displayName;
                }
            }
            return "$$TEAMNOTFOUND: " + p;
        }

        private void PopulateSeasonCombo()
        {
            maxSeason = SQLiteIO.getMaxSeason(MainWindow.currentDB);

            for (int i = maxSeason; i >= 1; i--)
            {
                cmbSeasonNum.Items.Add(i);
            }
        }

        private void cmbSeasonNum_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            curSeason = Convert.ToInt32(cmbSeasonNum.SelectedItem.ToString());
            SQLiteIO.GetAllTeamStatsFromDatabase(MainWindow.currentDB, curSeason, out tst, out tstopp, out TeamOrder);

            var teams = (from kvp in TeamOrder where !tst[kvp.Value].isHidden select tst[kvp.Value].displayName).ToList();

            teams.Sort();
            teams.Insert(0, "- Any -");

            cmbTeam.ItemsSource = teams;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            string playersT = "Players";
            if (curSeason != maxSeason) playersT += "S" + curSeason;

            string q = "select * from " + playersT;

            string where = " WHERE ";
            if (!String.IsNullOrWhiteSpace(txtLastName.Text))
            {
                if (cmbLastNameSetting.SelectedItem.ToString() == "Contains")
                {
                    where += "LastName LIKE '%" + txtLastName.Text + "%' AND ";
                }
                else
                {
                    where += "LastName LIKE '" + txtLastName.Text + "' AND ";
                }
            }

            if (!String.IsNullOrWhiteSpace(txtFirstName.Text))
            {
                if (cmbFirstNameSetting.SelectedItem.ToString() == "Contains")
                {
                    where += "FirstName LIKE '%" + txtFirstName.Text + "%' AND ";
                }
                else
                {
                    where += "FirstName LIKE '" + txtFirstName.Text + "' AND ";
                }
            }

            if (cmbPosition1.SelectedIndex != -1 && cmbPosition1.SelectedItem.ToString() != "Any")
            {
                where += "Position1 LIKE '" + cmbPosition1.SelectedItem + "' AND ";
            }

            if (cmbPosition2.SelectedIndex != -1 && cmbPosition2.SelectedItem.ToString() != "Any")
            {
                where += "Position2 LIKE '" + cmbPosition2.SelectedItem + "' AND ";
            }

            if (chkIsActive.IsChecked.GetValueOrDefault())
            {
                where += "isActive LIKE 'True' AND ";
            }
            else if (chkIsActive.IsChecked != null)
            {
                where += "isActive LIKE 'False' AND ";
            }

            if (chkIsInjured.IsChecked.GetValueOrDefault())
            {
                where += "isInjured LIKE 'True' AND ";
            }
            else if (chkIsInjured.IsChecked != null)
            {
                where += "isInjured LIKE 'False' AND ";
            }

            if (chkIsAllStar.IsChecked.GetValueOrDefault())
            {
                where += "isAllStar LIKE 'True' AND ";
            }
            else if (chkIsAllStar.IsChecked != null)
            {
                where += "isAllStar LIKE 'False' AND ";
            }

            if (chkIsNBAChampion.IsChecked.GetValueOrDefault())
            {
                where += "isNBAChampion LIKE 'True' AND ";
            }
            else if (chkIsNBAChampion.IsChecked != null)
            {
                where += "isNBAChampion LIKE 'False' AND ";
            }

            if (cmbTeam.SelectedItem != null && !String.IsNullOrEmpty(cmbTeam.SelectedItem.ToString()) && chkIsActive.IsChecked.GetValueOrDefault() && cmbTeam.SelectedItem.ToString() != "- Any -")
            {
                where += "TeamFin LIKE '" + GetCurTeamFromDisplayName(cmbTeam.SelectedItem.ToString()) + "' AND ";
            }

            if (!String.IsNullOrWhiteSpace(txtPTS.Text))
            {
                where += "PTS " + cmbPTS.SelectedItem + " " + txtPTS.Text + " AND ";
            }

            if (!String.IsNullOrWhiteSpace(txtFGM.Text))
            {
                where += "FGM " + cmbFGM.SelectedItem + " " + txtFGM.Text + " AND ";
            }

            if (!String.IsNullOrWhiteSpace(txtFGA.Text))
            {
                where += "FGA " + cmbFGA.SelectedItem + " " + txtFGA.Text + " AND ";
            }

            if (!String.IsNullOrWhiteSpace(txt3PM.Text))
            {
                where += "TPM " + cmb3PM.SelectedItem + " " + txt3PM.Text + " AND ";
            }

            if (!String.IsNullOrWhiteSpace(txt3PA.Text))
            {
                where += "TPA " + cmb3PA.SelectedItem + " " + txt3PA.Text + " AND ";
            }

            if (!String.IsNullOrWhiteSpace(txtFTM.Text))
            {
                where += "FTM " + cmbFTM.SelectedItem + " " + txtFTM.Text + " AND ";
            }

            if (!String.IsNullOrWhiteSpace(txtFTA.Text))
            {
                where += "FTA " + cmbFTA.SelectedItem + " " + txtFTA.Text + " AND ";
            }

            if (!String.IsNullOrWhiteSpace(txtREB.Text))
            {
                where += "(OREB+DREB) " + cmbREB.SelectedItem + " " + txtREB.Text + " AND ";
            }

            if (!String.IsNullOrWhiteSpace(txtOREB.Text))
            {
                where += "OREB " + cmbOREB.SelectedItem + " " + txtOREB.Text + " AND ";
            }

            if (!String.IsNullOrWhiteSpace(txtSTL.Text))
            {
                where += "STL " + cmbSTL.SelectedItem + " " + txtSTL.Text + " AND ";
            }

            if (!String.IsNullOrWhiteSpace(txtTO.Text))
            {
                where += "TOS " + cmbTO.SelectedItem + " " + txtTO.Text + " AND ";
            }

            if (!String.IsNullOrWhiteSpace(txtBLK.Text))
            {
                where += "BLK " + cmbBLK.SelectedItem + " " + txtBLK.Text + " AND ";
            }

            if (!String.IsNullOrWhiteSpace(txtAST.Text))
            {
                where += "AST " + cmbAST.SelectedItem + " " + txtAST.Text + " AND ";
            }

            if (!String.IsNullOrWhiteSpace(txtFOUL.Text))
            {
                where += "FOUL " + cmbFOUL.SelectedItem + " " + txtFOUL.Text + " AND ";
            }

            if (!String.IsNullOrWhiteSpace(txtPPG.Text))
            {
                where += "cast(PTS as REAL)/GP " + cmbPPG.SelectedItem + " " + txtPPG.Text + " AND ";
            }

            if (!String.IsNullOrWhiteSpace(txtFGp.Text))
            {
                where += "cast(FGM as REAL)/FGA " + cmbFGp.SelectedItem + " " + txtFGp.Text + " AND ";
            }

            if (!String.IsNullOrWhiteSpace(txtFGeff.Text))
            {
                where += "(cast(FGM as REAL)/FGA)*FGM/GP " + cmbFGeff.SelectedItem + " " + txtFGeff.Text + " AND ";
            }

            if (!String.IsNullOrWhiteSpace(txt3Pp.Text))
            {
                where += "cast(TPM as REAL)/TPA " + cmb3Pp.SelectedItem + " " + txt3Pp.Text + " AND ";
            }

            if (!String.IsNullOrWhiteSpace(txt3Peff.Text))
            {
                where += "(cast(TPM as REAL)/TPA)*TPM/GP " + cmb3Peff.SelectedItem + " " + txt3Peff.Text + " AND ";
            }

            if (!String.IsNullOrWhiteSpace(txtFTp.Text))
            {
                where += "cast(FTM as REAL)/FTA " + cmbFTp.SelectedItem + " " + txtFTp.Text + " AND ";
            }

            if (!String.IsNullOrWhiteSpace(txtFTeff.Text))
            {
                where += "(cast(FTM as REAL)/FTA)*FTM/GP " + cmbFTeff.SelectedItem + " " + txtFTeff.Text + " AND ";
            }

            if (!String.IsNullOrWhiteSpace(txtORPG.Text))
            {
                where += "cast(OREB as REAL)/GP " + cmbORPG.SelectedItem + " " + txtORPG.Text + " AND ";
            }

            if (!String.IsNullOrWhiteSpace(txtRPG.Text))
            {
                where += "cast((OREB+DREB) as REAL)/GP " + cmbRPG.SelectedItem + " " + txtRPG.Text + " AND ";
            }

            if (!String.IsNullOrWhiteSpace(txtSPG.Text))
            {
                where += "cast(STL as REAL)/GP " + cmbSPG.SelectedItem + " " + txtSPG.Text + " AND ";
            }

            if (!String.IsNullOrWhiteSpace(txtTPG.Text))
            {
                where += "cast(TOS as REAL)/GP " + cmbTPG.SelectedItem + " " + txtTPG.Text + " AND ";
            }

            if (!String.IsNullOrWhiteSpace(txtBLK.Text))
            {
                where += "cast(BLK as REAL)/GP " + cmbBPG.SelectedItem + " " + txtBPG.Text + " AND ";
            }

            if (!String.IsNullOrWhiteSpace(txtAPG.Text))
            {
                where += "cast(AST as REAL)/GP " + cmbAPG.SelectedItem + " " + txtAPG.Text + " AND ";
            }

            if (!String.IsNullOrWhiteSpace(txtFPG.Text))
            {
                where += "cast(FOUL as REAL)/GP " + cmbFPG.SelectedItem + " " + txtFPG.Text + " AND ";
            }

            where = where.Remove(where.Length - 4);

            var db = new SQLiteDatabase(MainWindow.currentDB);
            DataTable res;
            try
            {
                res = db.GetDataTable(q + where);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Invalid query.\n\n" + ex.Message);
                return;
            }

            var psr = new ObservableCollection<PlayerStatsRow>();

            foreach (DataRow dr in res.Rows)
            {
                psr.Add(new PlayerStatsRow(new PlayerStats(dr)));
            }

            dgvPlayerStats.ItemsSource = psr;
            tbcPlayerSearch.SelectedItem = tabResults;
        }

        private void dg_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        private void dgvPlayerStats_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void cmbTeam_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbTeam.SelectedIndex != -1) chkIsActive.IsChecked = true;
        }

        private void chkIsActive_Click(object sender, RoutedEventArgs e)
        {
            if (chkIsActive.IsChecked.GetValueOrDefault()) cmbTeam.SelectedIndex = 0;
            else cmbTeam.SelectedIndex = -1;
        }
    }
}
