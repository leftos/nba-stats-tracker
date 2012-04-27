using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Microsoft.Win32;

namespace NBA_2K12_Correct_Team_Stats
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class boxScoreW : Window
    {
        Brush defaultBackground;

        public enum Mode { Update, View };
        public static Mode curmode = Mode.Update;

        public static BoxScore curBoxScore;

        public boxScoreW(Mode _curmode = Mode.Update)
        {
            InitializeComponent();

            prepareWindow(_curmode);
        }

        public boxScoreW(Mode _curmode, int id)
        {
            InitializeComponent();

            prepareWindow(_curmode);

            cbHistory.SelectedIndex = id;
        }

        private void prepareWindow(Mode _curmode)
        {
            if ((MainWindow.pt == null) || (MainWindow.pt.teams[0] == "Invalid"))
            {
                foreach (KeyValuePair<string, int> kvp in MainWindow.TeamOrder)
                {
                    cmbTeam1.Items.Add(kvp.Key);
                    cmbTeam2.Items.Add(kvp.Key);
                }
            }
            else
            {
                List<string> newteams = new List<string>();
                foreach (string team in MainWindow.pt.teams)
                    newteams.Add(team);
                newteams.Sort();
                foreach (string newteam in newteams)
                {
                    cmbTeam1.Items.Add(newteam);
                    cmbTeam2.Items.Add(newteam);
                }
            }

            defaultBackground = cmbTeam1.Background;

            cmbTeam1.SelectedIndex = 0;
            cmbTeam2.SelectedIndex = 1;

            MainWindow.bs.done = false;

            dtpGameDate.SelectedDate = DateTime.Today;

            txtSeasonNum.Text = MainWindow.curSeason.ToString();

            foreach (BoxScoreEntry cur in MainWindow.bshist)
            {
                cbHistory.Items.Add(cur.date.ToShortDateString() + " - " + cur.bs.Team1 + " @ " + cur.bs.Team2);
            }

            curmode = _curmode;

            calculateScore1();
            calculateScore2();

            if (curmode == Mode.View)
            {
                label1.Content = "Select a box score to view";
                chkDoNotUpdate.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            if (curmode == Mode.Update)
            {
                tryParseBS();
                if (MainWindow.bs.done == false) return;
            }
            else
            {
                if (MainWindow.isCustom)
                {
                    MessageBoxResult r = MessageBox.Show("Do you want to save any changes to this Box Score?", "NBA Stats Tracker", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                    if (r == MessageBoxResult.Cancel) return;
                    else if (r == MessageBoxResult.Yes)
                    {
                        tryParseBS();
                        if (MainWindow.bs.done == false) return;
                    }
                    else
                    {
                        MainWindow.bs.done = false;
                    }
                }
            }
            this.Close();
        }

        private void tryParseBS()
        {
            if (cmbTeam1.SelectedItem.ToString() == cmbTeam2.SelectedItem.ToString())
            {
                MessageBox.Show("You can't have the same team in both Home & Away.");
                return;
            }
            if ((txtPTS1.Text == "") || (txtPTS1.Text == "N/A") || (txtPTS2.Text == "") || (txtPTS2.Text == "N/A")) return;
            if (txtSeasonNum.Text == "")
            {
                MessageBox.Show("You have to enter the Season number (e.g. '1' for the first season of your Association, or '2010', etc.)");
                return;
            }
            try
            {
                try
                {
                    MainWindow.bs.id = curBoxScore.id;
                    MainWindow.bs.bshistid = curBoxScore.bshistid;
                }
                catch
                {
                    MainWindow.bs.id = -1;
                    MainWindow.bs.bshistid = -1;
                }
                MainWindow.bs.isPlayoff = chkIsPlayoff.IsChecked.GetValueOrDefault();
                MainWindow.bs.gamedate = dtpGameDate.SelectedDate.GetValueOrDefault();
                MainWindow.bs.SeasonNum = Convert.ToInt32(txtSeasonNum.Text);
                MainWindow.bs.Team1 = cmbTeam1.SelectedItem.ToString();
                MainWindow.bs.Team2 = cmbTeam2.SelectedItem.ToString();
                MainWindow.bs.PTS1 = Convert.ToUInt16(txtPTS1.Text);
                MainWindow.bs.REB1 = Convert.ToUInt16(txtREB1.Text);
                MainWindow.bs.AST1 = Convert.ToUInt16(txtAST1.Text);
                MainWindow.bs.STL1 = Convert.ToUInt16(txtSTL1.Text);
                MainWindow.bs.BLK1 = Convert.ToUInt16(txtBLK1.Text);
                MainWindow.bs.TO1 = Convert.ToUInt16(txtTO1.Text);
                MainWindow.bs.FGM1 = Convert.ToUInt16(txtFGM1.Text);
                MainWindow.bs.FGA1 = Convert.ToUInt16(txtFGA1.Text);
                MainWindow.bs.TPM1 = Convert.ToUInt16(txt3PM1.Text);
                MainWindow.bs.TPA1 = Convert.ToUInt16(txt3PA1.Text);

                if (MainWindow.bs.FGA1 < MainWindow.bs.FGM1)
                {
                    MessageBox.Show("The FGM stat can't be higher than the FGA stat.");
                    throw (new Exception());
                }
                if (MainWindow.bs.TPA1 < MainWindow.bs.TPM1)
                {
                    MessageBox.Show("The 3PM stat can't be higher than the 3PA stat.");
                    throw (new Exception());
                }
                if (MainWindow.bs.FGM1 < MainWindow.bs.TPM1)
                {
                    MessageBox.Show("The 3PM stat can't be higher than the FGM stat.");
                    throw (new Exception());
                }

                MainWindow.bs.FTM1 = Convert.ToUInt16(txtFTM1.Text);
                MainWindow.bs.FTA1 = Convert.ToUInt16(txtFTA1.Text);
                if (MainWindow.bs.FTA1 < MainWindow.bs.FTM1)
                {
                    MessageBox.Show("The FTM stat can't be higher than the FTA stat.");
                    throw (new Exception());
                }

                MainWindow.bs.OFF1 = Convert.ToUInt16(txtOFF1.Text);
                if (MainWindow.bs.OFF1 > MainWindow.bs.REB1)
                {
                    MessageBox.Show("The OFF stat can't be higher than the REB stat.");
                    throw (new Exception());
                }
                if (MainWindow.bs.FGA1 < MainWindow.bs.TPA1)
                {
                    MessageBox.Show("The 3PA stat can't be higher than the FGA stat.");
                    throw (new Exception());
                }

                MainWindow.bs.PF1 = Convert.ToUInt16(txtPF1.Text);
                MainWindow.bs.PTS2 = Convert.ToUInt16(txtPTS2.Text);
                MainWindow.bs.REB2 = Convert.ToUInt16(txtREB2.Text);
                MainWindow.bs.AST2 = Convert.ToUInt16(txtAST2.Text);
                MainWindow.bs.STL2 = Convert.ToUInt16(txtSTL2.Text);
                MainWindow.bs.BLK2 = Convert.ToUInt16(txtBLK2.Text);
                MainWindow.bs.TO2 = Convert.ToUInt16(txtTO2.Text);
                MainWindow.bs.FGM2 = Convert.ToUInt16(txtFGM2.Text);
                MainWindow.bs.FGA2 = Convert.ToUInt16(txtFGA2.Text);
                MainWindow.bs.TPM2 = Convert.ToUInt16(txt3PM2.Text);
                MainWindow.bs.TPA2 = Convert.ToUInt16(txt3PA2.Text);

                if (MainWindow.bs.FGA2 < MainWindow.bs.FGM2)
                {
                    MessageBox.Show("The FGM stat can't be higher than the FGA stat.");
                    throw (new Exception());
                }
                if (MainWindow.bs.TPA2 < MainWindow.bs.TPM2)
                {
                    MessageBox.Show("The 3PM stat can't be higher than the 3PA stat.");
                    throw (new Exception());
                }
                if (MainWindow.bs.FGM2 < MainWindow.bs.TPM2)
                {
                    MessageBox.Show("The 3PM stat can't be higher than the FGM stat.");
                    throw (new Exception());
                }
                if (MainWindow.bs.FGA2 < MainWindow.bs.TPA2)
                {
                    MessageBox.Show("The 3PA stat can't be higher than the FGA stat.");
                    throw (new Exception());
                }

                MainWindow.bs.FTM2 = Convert.ToUInt16(txtFTM2.Text);
                MainWindow.bs.FTA2 = Convert.ToUInt16(txtFTA2.Text);
                if (MainWindow.bs.FTA2 < MainWindow.bs.FTM2)
                {
                    MessageBox.Show("The FTM stat can't be higher than the FTA stat.");
                    throw (new Exception());
                }

                MainWindow.bs.OFF2 = Convert.ToUInt16(txtOFF2.Text);

                if (MainWindow.bs.OFF2 > MainWindow.bs.REB2)
                {
                    MessageBox.Show("The OFF stat can't be higher than the REB stat.");
                    throw (new Exception());
                }

                MainWindow.bs.PF2 = Convert.ToUInt16(txtPF2.Text);

                MainWindow.bs.doNotUpdate = chkDoNotUpdate.IsChecked.GetValueOrDefault();
                MainWindow.bs.done = true;
            }
            catch
            {
                MainWindow.bs.done = false;
            }
            return;
        }

        private void cmbTeam1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            checkIfSameTeams();
            tabTeam1.Header = cmbTeam1.SelectedItem;
        }

        private void checkIfSameTeams()
        {
            try
            {
                if (cmbTeam1.SelectedItem.ToString() == cmbTeam2.SelectedItem.ToString())
                {
                    cmbTeam1.Background = Brushes.Red;
                    cmbTeam2.Background = Brushes.Red;
                    return;
                }
                else
                {
                    cmbTeam1.Background = defaultBackground;
                    cmbTeam2.Background = defaultBackground;
                }

                if (MainWindow.pt.teams[0] != "Invalid")
                {
                    string Team1 = cmbTeam1.SelectedItem.ToString();
                    string Team2 = cmbTeam2.SelectedItem.ToString();
                    if (MainWindow.West.Contains(Team1))
                    {
                        if (!MainWindow.West.Contains(Team2))
                        {
                            cmbTeam1.Background = Brushes.Red;
                            cmbTeam2.Background = Brushes.Red;
                            return;
                        }
                        else
                        {
                            cmbTeam1.Background = defaultBackground;
                            cmbTeam2.Background = defaultBackground;
                        }
                    }
                    else
                    {
                        if (MainWindow.West.Contains(Team2))
                        {
                            cmbTeam1.Background = Brushes.Red;
                            cmbTeam2.Background = Brushes.Red;
                            return;
                        }
                        else
                        {
                            cmbTeam1.Background = defaultBackground;
                            cmbTeam2.Background = defaultBackground;
                        }
                    }
                }
            }
            catch
            { }
        }

        private void cmbTeam2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            checkIfSameTeams();
            tabTeam2.Header = cmbTeam2.SelectedItem;
        }

        private void txtFGM1_TextChanged(object sender, TextChangedEventArgs e)
        {
            calculateScore1();
        }

        private void calculateScore1(object sender, TextChangedEventArgs e)
        {
            try
            {
                int fgm = Convert.ToInt32(txtFGM1.Text);
                int tpm = Convert.ToInt32(txt3PM1.Text);
                int ftm = Convert.ToInt32(txtFTM1.Text);
                txtPTS1.Text = ((fgm - tpm) * 2 + tpm * 3 + ftm).ToString();

                int fga = Convert.ToInt32(txtFGA1.Text);
                int tpa = Convert.ToInt32(txt3PA1.Text);
                int fta = Convert.ToInt32(txtFTA1.Text);
                txbT1Avg.Text = String.Format("FG%: {0:F3}\t3P%: {1:F3}\tFT%{2:F3}", (float)fgm / fga, (float)tpm / tpa, (float)ftm / fta);
            }
            catch
            {
                txtPTS1.Text = "N/A";
                txbT1Avg.Text = "";
            }
        }

        private void txt3PM1_TextChanged(object sender, TextChangedEventArgs e)
        {
            calculateScore1();
        }

        private void txtFTM1_TextChanged(object sender, TextChangedEventArgs e)
        {
            calculateScore1();
        }

        private void txtFGM2_TextChanged(object sender, TextChangedEventArgs e)
        {
            calculateScore2();
        }

        private void calculateScore2(object sender, TextChangedEventArgs e)
        {
            try
            {
                int fgm = Convert.ToInt32(txtFGM2.Text);
                int tpm = Convert.ToInt32(txt3PM2.Text);
                int ftm = Convert.ToInt32(txtFTM2.Text);
                txtPTS2.Text = ((fgm - tpm) * 2 + tpm * 3 + ftm).ToString();

                int fga = Convert.ToInt32(txtFGA2.Text);
                int tpa = Convert.ToInt32(txt3PA2.Text);
                int fta = Convert.ToInt32(txtFTA2.Text);
                txbT2Avg.Text = String.Format("FG%: {0:F3}\t3P%: {1:F3}\tFT%: {2:F3}", (float)fgm / fga, (float)tpm / tpa, (float)ftm / fta);
            }
            catch
            {
                txtPTS2.Text = "N/A";
                txbT2Avg.Text = "";
            }
        }

        private void txt3PM2_TextChanged(object sender, TextChangedEventArgs e)
        {
            calculateScore2();
        }

        private void txtFTM2_TextChanged(object sender, TextChangedEventArgs e)
        {
            calculateScore2();
        }

        private void btnCSVOK_Click(object sender, RoutedEventArgs e)
        {
            tryParseBS();
            if (MainWindow.bs.done)
            {
                string header1 = "Team,PTS,REB,OREB,DREB,"
                + "AST,STL,BLK,TO,FGM,"
                + "FGA,FG%,3PM,3PA,3P%,"
                + "FTM,FTA,FT%,PF";

                string data1 = String.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11:F3},{12},{13},{14:F3},{15},{16},{17:F3},{18}",
                    cmbTeam1.SelectedItem.ToString(), MainWindow.bs.PTS1, MainWindow.bs.REB1, MainWindow.bs.OFF1, MainWindow.bs.REB1 - MainWindow.bs.OFF1,
                    MainWindow.bs.AST1, MainWindow.bs.STL1, MainWindow.bs.BLK1, MainWindow.bs.TO1, MainWindow.bs.FGM1,
                    MainWindow.bs.FGA1, MainWindow.bs.FGM1 / (float)MainWindow.bs.FGA1, MainWindow.bs.TPM1, MainWindow.bs.TPA1, MainWindow.bs.TPM1 / (float)MainWindow.bs.TPA1,
                    MainWindow.bs.FTM1, MainWindow.bs.FTA1, MainWindow.bs.FTM1 / (float)MainWindow.bs.FTA1, MainWindow.bs.PF1);

                string data2 = String.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11:F3},{12},{13},{14:F3},{15},{16},{17:F3},{18}",
                    cmbTeam2.SelectedItem.ToString(), MainWindow.bs.PTS2, MainWindow.bs.REB2, MainWindow.bs.OFF2, MainWindow.bs.REB2 - MainWindow.bs.OFF2,
                    MainWindow.bs.AST2, MainWindow.bs.STL2, MainWindow.bs.BLK2, MainWindow.bs.TO2, MainWindow.bs.FGM2,
                    MainWindow.bs.FGA2, MainWindow.bs.FGM2 / (float)MainWindow.bs.FGA2, MainWindow.bs.TPM2, MainWindow.bs.TPA2, MainWindow.bs.TPM2 / (float)MainWindow.bs.TPA2,
                    MainWindow.bs.FTM2, MainWindow.bs.FTA2, MainWindow.bs.FTM2 / (float)MainWindow.bs.FTA2, MainWindow.bs.PF2);

                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "Comma-Separated Values file (*.csv)|*.csv";
                sfd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                sfd.Title = "Select a file to save the CSV to...";
                sfd.ShowDialog();
                if (sfd.FileName == "") return;

                StreamWriter sw = new StreamWriter(sfd.FileName);
                sw.WriteLine(header1);
                sw.WriteLine(data1);
                sw.WriteLine(data2);
                sw.Close();
            }
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.bs.done = false;
            this.Close();
        }

        private void _bsAnyTextbox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            tb.SelectAll();
        }

        private void cbHistory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int i = cbHistory.SelectedIndex;
            BoxScore bs = MainWindow.bshist[i].bs;
            curBoxScore = MainWindow.bshist[i].bs;
            curBoxScore.bshistid = i;
            cmbTeam1.SelectedItem = bs.Team1;
            txtPTS1.Text = bs.PTS1.ToString();
            txtREB1.Text = bs.REB1.ToString();
            txtAST1.Text = bs.AST1.ToString();
            txtSTL1.Text = bs.STL1.ToString();
            txtBLK1.Text = bs.BLK1.ToString();
            txtTO1.Text = bs.TO1.ToString();
            txtFGM1.Text = bs.FGM1.ToString();
            txtFGA1.Text = bs.FGA1.ToString();
            txt3PM1.Text = bs.TPM1.ToString();
            txt3PA1.Text = bs.TPA1.ToString();
            txtFTM1.Text = bs.FTM1.ToString();
            txtFTA1.Text = bs.FTA1.ToString();
            txtOFF1.Text = bs.OFF1.ToString();
            txtPF1.Text = bs.PF1.ToString();
            cmbTeam2.SelectedItem = bs.Team2;
            txtPTS2.Text = bs.PTS2.ToString();
            txtREB2.Text = bs.REB2.ToString();
            txtAST2.Text = bs.AST2.ToString();
            txtSTL2.Text = bs.STL2.ToString();
            txtBLK2.Text = bs.BLK2.ToString();
            txtTO2.Text = bs.TO2.ToString();
            txtFGM2.Text = bs.FGM2.ToString();
            txtFGA2.Text = bs.FGA2.ToString();
            txt3PM2.Text = bs.TPM2.ToString();
            txt3PA2.Text = bs.TPA2.ToString();
            txtFTM2.Text = bs.FTM2.ToString();
            txtFTA2.Text = bs.FTA2.ToString();
            txtOFF2.Text = bs.OFF2.ToString();
            txtPF2.Text = bs.PF2.ToString();

            dtpGameDate.SelectedDate = bs.gamedate;
            txtSeasonNum.Text = bs.SeasonNum.ToString();
            chkIsPlayoff.IsChecked = bs.isPlayoff;

            calculateScore1();
            calculateScore2();
        }

        private void calculateScore1()
        {

        }

        private void calculateScore2()
        {

        }
    }
}
