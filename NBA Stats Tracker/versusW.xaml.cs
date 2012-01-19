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

namespace NBA_2K12_Correct_Team_Stats
{
    /// <summary>
    /// Interaction logic for versusW.xaml
    /// </summary>
    public partial class versusW : Window
    {
        public static string _team1 = "";
        public static string _team2 = "";

        public const int PPG = 0, PAPG = 1, FGp = 2, FGeff = 3, TPp = 4, TPeff = 5,
            FTp = 6, FTeff = 7, RPG = 8, ORPG = 9, DRPG = 10, SPG = 11, BPG = 12,
            TPG = 13, APG = 14, FPG = 15, Wp = 16, Weff = 17;

        public versusW(string team1, string team2, TeamStats[] ts)
        {
            InitializeComponent();

            _team1 = team1;
            _team2 = team2;

            TeamStats ts1 = ts[MainWindow.TeamOrder[_team1]];
            TeamStats ts2 = ts[MainWindow.TeamOrder[_team2]];

            prepareWindow(ts1, ts2);
        }

        public versusW(TeamStats ts1, string desc1, TeamStats ts2, string desc2)
        {
            InitializeComponent();

            _team1 = ts1.name + " (" + desc1 + ")";
            _team2 = ts2.name + " (" + desc2 + ")";

            prepareWindow(ts1, ts2);
        }

        private void prepareWindow(TeamStats ts1, TeamStats ts2)
        {
            lblTeam1.Content = _team1;
            lbl3Peff1.Content = String.Format("{0:F2}", ts1.averages[TPeff]);
            lbl3Pp1.Content = String.Format("{0:F3}", ts1.averages[TPp]);
            lblAPG1.Content = String.Format("{0:F1}", ts1.averages[APG]);
            lblBPG1.Content = String.Format("{0:F1}", ts1.averages[BPG]);
            lblFGeff1.Content = String.Format("{0:F2}", ts1.averages[FGeff]);
            lblFGp1.Content = String.Format("{0:F3}", ts1.averages[FGp]);
            lblFTeff1.Content = String.Format("{0:F2}", ts1.averages[FTeff]);
            lblFTp1.Content = String.Format("{0:F3}", ts1.averages[FTp]);
            lblORPG1.Content = String.Format("{0:F1}", ts1.averages[ORPG]);
            lblPAPG1.Content = String.Format("{0:F1}", ts1.averages[PAPG]);
            lblPPG1.Content = String.Format("{0:F1}", ts1.averages[PPG]);
            lblRecord1.Content = ts1.winloss[0].ToString() + "-" + ts1.winloss[1].ToString();
            lblRPG1.Content = String.Format("{0:F1}", ts1.averages[RPG]);
            lblSPG1.Content = String.Format("{0:F1}", ts1.averages[SPG]);
            lblTPG1.Content = String.Format("{0:F1}", ts1.averages[TPG]);
            lblWeff1.Content = String.Format("{0:F2}", ts1.averages[Weff]);
            lblWp1.Content = String.Format("{0:F3}", ts1.averages[Wp]);
            lblFPG1.Content = String.Format("{0:F1}", ts1.averages[FPG]);

            lblTeam2.Content = _team2;
            lbl3Peff2.Content = String.Format("{0:F2}", ts2.averages[TPeff]);
            lbl3Pp2.Content = String.Format("{0:F3}", ts2.averages[TPp]);
            lblAPG2.Content = String.Format("{0:F1}", ts2.averages[APG]);
            lblBPG2.Content = String.Format("{0:F1}", ts2.averages[BPG]);
            lblFGeff2.Content = String.Format("{0:F2}", ts2.averages[FGeff]);
            lblFGp2.Content = String.Format("{0:F3}", ts2.averages[FGp]);
            lblFTeff2.Content = String.Format("{0:F2}", ts2.averages[FTeff]);
            lblFTp2.Content = String.Format("{0:F3}", ts2.averages[FTp]);
            lblORPG2.Content = String.Format("{0:F1}", ts2.averages[ORPG]);
            lblPAPG2.Content = String.Format("{0:F1}", ts2.averages[PAPG]);
            lblPPG2.Content = String.Format("{0:F1}", ts2.averages[PPG]);
            lblRecord2.Content = ts2.winloss[0].ToString() + "-" + ts2.winloss[1].ToString();
            lblRPG2.Content = String.Format("{0:F1}", ts2.averages[RPG]);
            lblSPG2.Content = String.Format("{0:F1}", ts2.averages[SPG]);
            lblTPG2.Content = String.Format("{0:F1}", ts2.averages[TPG]);
            lblWeff2.Content = String.Format("{0:F2}", ts2.averages[Weff]);
            lblWp2.Content = String.Format("{0:F3}", ts2.averages[Wp]);
            lblFPG2.Content = String.Format("{0:F1}", ts2.averages[FPG]);

            if (ts1.averages[TPeff] > ts2.averages[TPeff])
                lbl3Peff1.FontWeight = FontWeights.Bold;
            else if (ts1.averages[TPeff] < ts2.averages[TPeff])
                lbl3Peff2.FontWeight = FontWeights.Bold;

            if (ts1.averages[TPp] > ts2.averages[TPp])
                lbl3Pp1.FontWeight = FontWeights.Bold;
            else if (ts1.averages[TPp] < ts2.averages[TPp])
                lbl3Pp2.FontWeight = FontWeights.Bold;

            if (ts1.averages[APG] > ts2.averages[APG])
                lblAPG1.FontWeight = FontWeights.Bold;
            else if (ts1.averages[APG] < ts2.averages[APG])
                lblAPG2.FontWeight = FontWeights.Bold;

            if (ts1.averages[BPG] > ts2.averages[BPG])
                lblBPG1.FontWeight = FontWeights.Bold;
            else if (ts1.averages[BPG] < ts2.averages[BPG])
                lblBPG2.FontWeight = FontWeights.Bold;

            if (ts1.averages[FGeff] > ts2.averages[FGeff])
                lblFGeff1.FontWeight = FontWeights.Bold;
            else if (ts1.averages[FGeff] < ts2.averages[FGeff])
                lblFGeff2.FontWeight = FontWeights.Bold;

            if (ts1.averages[FGp] > ts2.averages[FGp])
                lblFGp1.FontWeight = FontWeights.Bold;
            else if (ts1.averages[FGp] < ts2.averages[FGp])
                lblFGp2.FontWeight = FontWeights.Bold;

            if (ts1.averages[FPG] < ts2.averages[FPG])
                lblFPG1.FontWeight = FontWeights.Bold;
            else if (ts1.averages[FPG] > ts2.averages[FPG])
                lblFPG2.FontWeight = FontWeights.Bold;

            if (ts1.averages[FTeff] > ts2.averages[FTeff])
                lblFTeff1.FontWeight = FontWeights.Bold;
            else if (ts1.averages[FTeff] < ts2.averages[FTeff])
                lblFTeff2.FontWeight = FontWeights.Bold;

            if (ts1.averages[FTp] > ts2.averages[FTp])
                lblFTp1.FontWeight = FontWeights.Bold;
            else if (ts1.averages[FTp] < ts2.averages[FTp])
                lblFTp2.FontWeight = FontWeights.Bold;

            if (ts1.averages[ORPG] > ts2.averages[ORPG])
                lblORPG1.FontWeight = FontWeights.Bold;
            else if (ts1.averages[ORPG] < ts2.averages[ORPG])
                lblORPG2.FontWeight = FontWeights.Bold;

            if (ts1.averages[PAPG] < ts2.averages[PAPG])
                lblPAPG1.FontWeight = FontWeights.Bold;
            else if (ts1.averages[PAPG] > ts2.averages[PAPG])
                lblPAPG2.FontWeight = FontWeights.Bold;

            if (ts1.averages[PPG] > ts2.averages[PPG])
                lblPPG1.FontWeight = FontWeights.Bold;
            else if (ts1.averages[PPG] < ts2.averages[PPG])
                lblPPG2.FontWeight = FontWeights.Bold;

            if (ts1.averages[RPG] > ts2.averages[RPG])
                lblRPG1.FontWeight = FontWeights.Bold;
            else if (ts1.averages[RPG] < ts2.averages[RPG])
                lblRPG2.FontWeight = FontWeights.Bold;

            if (ts1.averages[SPG] > ts2.averages[SPG])
                lblSPG1.FontWeight = FontWeights.Bold;
            else if (ts1.averages[SPG] < ts2.averages[SPG])
                lblSPG2.FontWeight = FontWeights.Bold;

            if (ts1.averages[TPG] < ts2.averages[TPG])
                lblTPG1.FontWeight = FontWeights.Bold;
            else if (ts1.averages[TPG] > ts2.averages[TPG])
                lblTPG2.FontWeight = FontWeights.Bold;

            if (ts1.averages[Wp] > ts2.averages[Wp])
                lblWp1.FontWeight = FontWeights.Bold;
            else if (ts1.averages[Wp] < ts2.averages[Wp])
                lblWp2.FontWeight = FontWeights.Bold;

            if (ts1.averages[Weff] > ts2.averages[Weff])
                lblWeff1.FontWeight = FontWeights.Bold;
            else if (ts1.averages[Weff] < ts2.averages[Weff])
                lblWeff2.FontWeight = FontWeights.Bold;
        }
    }
}
