using System;
using System.Windows;

namespace NBA_Stats_Tracker
{
    /// <summary>
    /// Interaction logic for versusW.xaml
    /// </summary>
    public partial class versusW : Window
    {
        public const int tPPG = 0, tPAPG = 1, tFGp = 2, tFGeff = 3, tTPp = 4, tTPeff = 5, tFTp = 6, tFTeff = 7, tRPG = 8, tORPG = 9, tDRPG = 10, tSPG = 11, tBPG = 12, tTPG = 13, tAPG = 14, tFPG = 15, tWp = 16, tWeff = 17, tPD = 18;

        public static string _team1 = "";
        public static string _team2 = "";

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
            lbl3Peff1.Content = String.Format("{0:F2}", ts1.averages[tTPeff]);
            lbl3Pp1.Content = String.Format("{0:F3}", ts1.averages[tTPp]);
            lblAPG1.Content = String.Format("{0:F1}", ts1.averages[tAPG]);
            lblBPG1.Content = String.Format("{0:F1}", ts1.averages[tBPG]);
            lblFGeff1.Content = String.Format("{0:F2}", ts1.averages[tFGeff]);
            lblFGp1.Content = String.Format("{0:F3}", ts1.averages[tFGp]);
            lblFTeff1.Content = String.Format("{0:F2}", ts1.averages[tFTeff]);
            lblFTp1.Content = String.Format("{0:F3}", ts1.averages[tFTp]);
            lblORPG1.Content = String.Format("{0:F1}", ts1.averages[tORPG]);
            lblPAPG1.Content = String.Format("{0:F1}", ts1.averages[tPAPG]);
            lblPPG1.Content = String.Format("{0:F1}", ts1.averages[tPPG]);
            lblRecord1.Content = ts1.winloss[0].ToString() + "-" + ts1.winloss[1].ToString();
            lblRPG1.Content = String.Format("{0:F1}", ts1.averages[tRPG]);
            lblSPG1.Content = String.Format("{0:F1}", ts1.averages[tSPG]);
            lblTPG1.Content = String.Format("{0:F1}", ts1.averages[tTPG]);
            lblWeff1.Content = String.Format("{0:F2}", ts1.averages[tWeff]);
            lblWp1.Content = String.Format("{0:F3}", ts1.averages[tWp]);
            lblFPG1.Content = String.Format("{0:F1}", ts1.averages[tFPG]);

            lblTeam2.Content = _team2;
            lbl3Peff2.Content = String.Format("{0:F2}", ts2.averages[tTPeff]);
            lbl3Pp2.Content = String.Format("{0:F3}", ts2.averages[tTPp]);
            lblAPG2.Content = String.Format("{0:F1}", ts2.averages[tAPG]);
            lblBPG2.Content = String.Format("{0:F1}", ts2.averages[tBPG]);
            lblFGeff2.Content = String.Format("{0:F2}", ts2.averages[tFGeff]);
            lblFGp2.Content = String.Format("{0:F3}", ts2.averages[tFGp]);
            lblFTeff2.Content = String.Format("{0:F2}", ts2.averages[tFTeff]);
            lblFTp2.Content = String.Format("{0:F3}", ts2.averages[tFTp]);
            lblORPG2.Content = String.Format("{0:F1}", ts2.averages[tORPG]);
            lblPAPG2.Content = String.Format("{0:F1}", ts2.averages[tPAPG]);
            lblPPG2.Content = String.Format("{0:F1}", ts2.averages[tPPG]);
            lblRecord2.Content = ts2.winloss[0].ToString() + "-" + ts2.winloss[1].ToString();
            lblRPG2.Content = String.Format("{0:F1}", ts2.averages[tRPG]);
            lblSPG2.Content = String.Format("{0:F1}", ts2.averages[tSPG]);
            lblTPG2.Content = String.Format("{0:F1}", ts2.averages[tTPG]);
            lblWeff2.Content = String.Format("{0:F2}", ts2.averages[tWeff]);
            lblWp2.Content = String.Format("{0:F3}", ts2.averages[tWp]);
            lblFPG2.Content = String.Format("{0:F1}", ts2.averages[tFPG]);

            if (ts1.averages[tTPeff] > ts2.averages[tTPeff])
                lbl3Peff1.FontWeight = FontWeights.Bold;
            else if (ts1.averages[tTPeff] < ts2.averages[tTPeff])
                lbl3Peff2.FontWeight = FontWeights.Bold;

            if (ts1.averages[tTPp] > ts2.averages[tTPp])
                lbl3Pp1.FontWeight = FontWeights.Bold;
            else if (ts1.averages[tTPp] < ts2.averages[tTPp])
                lbl3Pp2.FontWeight = FontWeights.Bold;

            if (ts1.averages[tAPG] > ts2.averages[tAPG])
                lblAPG1.FontWeight = FontWeights.Bold;
            else if (ts1.averages[tAPG] < ts2.averages[tAPG])
                lblAPG2.FontWeight = FontWeights.Bold;

            if (ts1.averages[tBPG] > ts2.averages[tBPG])
                lblBPG1.FontWeight = FontWeights.Bold;
            else if (ts1.averages[tBPG] < ts2.averages[tBPG])
                lblBPG2.FontWeight = FontWeights.Bold;

            if (ts1.averages[tFGeff] > ts2.averages[tFGeff])
                lblFGeff1.FontWeight = FontWeights.Bold;
            else if (ts1.averages[tFGeff] < ts2.averages[tFGeff])
                lblFGeff2.FontWeight = FontWeights.Bold;

            if (ts1.averages[tFGp] > ts2.averages[tFGp])
                lblFGp1.FontWeight = FontWeights.Bold;
            else if (ts1.averages[tFGp] < ts2.averages[tFGp])
                lblFGp2.FontWeight = FontWeights.Bold;

            if (ts1.averages[tFPG] < ts2.averages[tFPG])
                lblFPG1.FontWeight = FontWeights.Bold;
            else if (ts1.averages[tFPG] > ts2.averages[tFPG])
                lblFPG2.FontWeight = FontWeights.Bold;

            if (ts1.averages[tFTeff] > ts2.averages[tFTeff])
                lblFTeff1.FontWeight = FontWeights.Bold;
            else if (ts1.averages[tFTeff] < ts2.averages[tFTeff])
                lblFTeff2.FontWeight = FontWeights.Bold;

            if (ts1.averages[tFTp] > ts2.averages[tFTp])
                lblFTp1.FontWeight = FontWeights.Bold;
            else if (ts1.averages[tFTp] < ts2.averages[tFTp])
                lblFTp2.FontWeight = FontWeights.Bold;

            if (ts1.averages[tORPG] > ts2.averages[tORPG])
                lblORPG1.FontWeight = FontWeights.Bold;
            else if (ts1.averages[tORPG] < ts2.averages[tORPG])
                lblORPG2.FontWeight = FontWeights.Bold;

            if (ts1.averages[tPAPG] < ts2.averages[tPAPG])
                lblPAPG1.FontWeight = FontWeights.Bold;
            else if (ts1.averages[tPAPG] > ts2.averages[tPAPG])
                lblPAPG2.FontWeight = FontWeights.Bold;

            if (ts1.averages[tPPG] > ts2.averages[tPPG])
                lblPPG1.FontWeight = FontWeights.Bold;
            else if (ts1.averages[tPPG] < ts2.averages[tPPG])
                lblPPG2.FontWeight = FontWeights.Bold;

            if (ts1.averages[tRPG] > ts2.averages[tRPG])
                lblRPG1.FontWeight = FontWeights.Bold;
            else if (ts1.averages[tRPG] < ts2.averages[tRPG])
                lblRPG2.FontWeight = FontWeights.Bold;

            if (ts1.averages[tSPG] > ts2.averages[tSPG])
                lblSPG1.FontWeight = FontWeights.Bold;
            else if (ts1.averages[tSPG] < ts2.averages[tSPG])
                lblSPG2.FontWeight = FontWeights.Bold;

            if (ts1.averages[tTPG] < ts2.averages[tTPG])
                lblTPG1.FontWeight = FontWeights.Bold;
            else if (ts1.averages[tTPG] > ts2.averages[tTPG])
                lblTPG2.FontWeight = FontWeights.Bold;

            if (ts1.averages[tWp] > ts2.averages[tWp])
                lblWp1.FontWeight = FontWeights.Bold;
            else if (ts1.averages[tWp] < ts2.averages[tWp])
                lblWp2.FontWeight = FontWeights.Bold;

            if (ts1.averages[tWeff] > ts2.averages[tWeff])
                lblWeff1.FontWeight = FontWeights.Bold;
            else if (ts1.averages[tWeff] < ts2.averages[tWeff])
                lblWeff2.FontWeight = FontWeights.Bold;
        }
    }
}