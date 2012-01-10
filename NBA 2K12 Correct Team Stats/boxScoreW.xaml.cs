using System;
using System.Collections.Generic;
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
using System.Media;

namespace NBA_2K12_Correct_Team_Stats
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class boxScoreW : Window
    {
        Brush defaultBackground;

        public boxScoreW()
        {
            InitializeComponent();

            if (MainWindow.pt.teams[0] == "Invalid")
            {
                foreach (KeyValuePair<string, int> kvp in MainWindow.TeamNames)
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
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            if (cmbTeam1.SelectedItem.ToString() == cmbTeam2.SelectedItem.ToString())
            {
                MessageBox.Show("You can't have the same team in both Home & Away.");
                return;
            }
            if ((txtPTS1.Text == "") || (txtPTS1.Text == "N/A") || (txtPTS2.Text == "") || (txtPTS2.Text == "N/A")) return;
            try
            {
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

                if (MainWindow.bs.FGM1 < MainWindow.bs.TPM1)
                {
                    MessageBox.Show("The 3PM stat can't be higher than the FGM stat.");
                    return;
                }

                MainWindow.bs.FTM1 = Convert.ToUInt16(txtFTM1.Text);
                MainWindow.bs.FTA1 = Convert.ToUInt16(txtFTA1.Text);
                MainWindow.bs.OFF1 = Convert.ToUInt16(txtOFF1.Text);

                if (MainWindow.bs.OFF1 > MainWindow.bs.REB1)
                {
                    MessageBox.Show("The OFF stat can't be higher than the REB stat.");
                    return;
                }
                if (MainWindow.bs.FGA1 < MainWindow.bs.TPA1)
                {
                    MessageBox.Show("The 3PA stat can't be higher than the FGA stat.");
                    return;
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

                if (MainWindow.bs.FGM2 < MainWindow.bs.TPM2)
                {
                    MessageBox.Show("The 3PM stat can't be higher than the FGM stat.");
                    return;
                }
                if (MainWindow.bs.FGA2 < MainWindow.bs.TPA2)
                {
                    MessageBox.Show("The 3PA stat can't be higher than the FGA stat.");
                    return;
                }

                MainWindow.bs.FTM2 = Convert.ToUInt16(txtFTM2.Text);
                MainWindow.bs.FTA2 = Convert.ToUInt16(txtFTA2.Text);
                MainWindow.bs.OFF2 = Convert.ToUInt16(txtOFF2.Text);

                if (MainWindow.bs.OFF2 > MainWindow.bs.REB2)
                {
                    MessageBox.Show("The OFF stat can't be higher than the REB stat.");
                    return;
                }

                MainWindow.bs.PF2 = Convert.ToUInt16(txtPF2.Text);
                MainWindow.bs.done = true;
            }
            catch
            {
                MainWindow.bs.done = false;
            }
            this.Close();
        }

        private void cmbTeam1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            checkIfSameTeams();
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
        }

        private void txtFGM1_TextChanged(object sender, TextChangedEventArgs e)
        {
            calculateScore1();
        }

        private void calculateScore1()
        {
            try
            {
                txtPTS1.Text = ((Convert.ToInt32(txtFGM1.Text) - Convert.ToInt32(txt3PM1.Text)) * 2 + Convert.ToInt32(txt3PM1.Text) * 3 + Convert.ToInt32(txtFTM1.Text)).ToString();
            }
            catch
            {
                txtPTS1.Text = "N/A";
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

        private void calculateScore2()
        {
            try
            {
                txtPTS2.Text = ((Convert.ToInt32(txtFGM2.Text) - Convert.ToInt32(txt3PM2.Text)) * 2 + Convert.ToInt32(txt3PM2.Text) * 3 + Convert.ToInt32(txtFTM2.Text)).ToString();
            }
            catch
            {
                txtPTS2.Text = "N/A";
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
    }
}
