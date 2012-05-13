#region Copyright Notice

// Created by Lefteris Aslanoglou, (c) 2011-2012
// 
// Implementation of thesis
// "Application Development for Basketball Statistical Analysis in Natural Language"
// under the supervision of Prof. Athanasios Tsakalidis & MSc Alexandros Georgiou
// 
// All rights reserved. Unless specifically stated otherwise, the code in this file should 
// not be reproduced, edited and/or republished without explicit permission from the 
// author.

#endregion

#region Using Directives

using System;
using System.Windows;
using NBA_Stats_Tracker.Data;

#endregion

namespace NBA_Stats_Tracker.Windows
{
    /// <summary>
    /// Interaction logic for versusW.xaml
    /// </summary>
    public partial class versusW
    {
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
            lbl3Peff1.Content = String.Format("{0:F2}", ts1.averages[t.TPeff]);
            lbl3Pp1.Content = String.Format("{0:F3}", ts1.averages[t.TPp]);
            lblAPG1.Content = String.Format("{0:F1}", ts1.averages[t.APG]);
            lblBPG1.Content = String.Format("{0:F1}", ts1.averages[t.BPG]);
            lblFGeff1.Content = String.Format("{0:F2}", ts1.averages[t.FGeff]);
            lblFGp1.Content = String.Format("{0:F3}", ts1.averages[t.FGp]);
            lblFTeff1.Content = String.Format("{0:F2}", ts1.averages[t.FTeff]);
            lblFTp1.Content = String.Format("{0:F3}", ts1.averages[t.FTp]);
            lblORPG1.Content = String.Format("{0:F1}", ts1.averages[t.ORPG]);
            lblPAPG1.Content = String.Format("{0:F1}", ts1.averages[t.PAPG]);
            lblPPG1.Content = String.Format("{0:F1}", ts1.averages[t.PPG]);
            lblRecord1.Content = ts1.winloss[0].ToString() + "-" + ts1.winloss[1].ToString();
            lblRPG1.Content = String.Format("{0:F1}", ts1.averages[t.RPG]);
            lblSPG1.Content = String.Format("{0:F1}", ts1.averages[t.SPG]);
            lblTPG1.Content = String.Format("{0:F1}", ts1.averages[t.TPG]);
            lblWeff1.Content = String.Format("{0:F2}", ts1.averages[t.Weff]);
            lblWp1.Content = String.Format("{0:F3}", ts1.averages[t.Wp]);
            lblFPG1.Content = String.Format("{0:F1}", ts1.averages[t.FPG]);

            lblTeam2.Content = _team2;
            lbl3Peff2.Content = String.Format("{0:F2}", ts2.averages[t.TPeff]);
            lbl3Pp2.Content = String.Format("{0:F3}", ts2.averages[t.TPp]);
            lblAPG2.Content = String.Format("{0:F1}", ts2.averages[t.APG]);
            lblBPG2.Content = String.Format("{0:F1}", ts2.averages[t.BPG]);
            lblFGeff2.Content = String.Format("{0:F2}", ts2.averages[t.FGeff]);
            lblFGp2.Content = String.Format("{0:F3}", ts2.averages[t.FGp]);
            lblFTeff2.Content = String.Format("{0:F2}", ts2.averages[t.FTeff]);
            lblFTp2.Content = String.Format("{0:F3}", ts2.averages[t.FTp]);
            lblORPG2.Content = String.Format("{0:F1}", ts2.averages[t.ORPG]);
            lblPAPG2.Content = String.Format("{0:F1}", ts2.averages[t.PAPG]);
            lblPPG2.Content = String.Format("{0:F1}", ts2.averages[t.PPG]);
            lblRecord2.Content = ts2.winloss[0].ToString() + "-" + ts2.winloss[1].ToString();
            lblRPG2.Content = String.Format("{0:F1}", ts2.averages[t.RPG]);
            lblSPG2.Content = String.Format("{0:F1}", ts2.averages[t.SPG]);
            lblTPG2.Content = String.Format("{0:F1}", ts2.averages[t.TPG]);
            lblWeff2.Content = String.Format("{0:F2}", ts2.averages[t.Weff]);
            lblWp2.Content = String.Format("{0:F3}", ts2.averages[t.Wp]);
            lblFPG2.Content = String.Format("{0:F1}", ts2.averages[t.FPG]);

            if (ts1.averages[t.TPeff] > ts2.averages[t.TPeff])
                lbl3Peff1.FontWeight = FontWeights.Bold;
            else if (ts1.averages[t.TPeff] < ts2.averages[t.TPeff])
                lbl3Peff2.FontWeight = FontWeights.Bold;

            if (ts1.averages[t.TPp] > ts2.averages[t.TPp])
                lbl3Pp1.FontWeight = FontWeights.Bold;
            else if (ts1.averages[t.TPp] < ts2.averages[t.TPp])
                lbl3Pp2.FontWeight = FontWeights.Bold;

            if (ts1.averages[t.APG] > ts2.averages[t.APG])
                lblAPG1.FontWeight = FontWeights.Bold;
            else if (ts1.averages[t.APG] < ts2.averages[t.APG])
                lblAPG2.FontWeight = FontWeights.Bold;

            if (ts1.averages[t.BPG] > ts2.averages[t.BPG])
                lblBPG1.FontWeight = FontWeights.Bold;
            else if (ts1.averages[t.BPG] < ts2.averages[t.BPG])
                lblBPG2.FontWeight = FontWeights.Bold;

            if (ts1.averages[t.FGeff] > ts2.averages[t.FGeff])
                lblFGeff1.FontWeight = FontWeights.Bold;
            else if (ts1.averages[t.FGeff] < ts2.averages[t.FGeff])
                lblFGeff2.FontWeight = FontWeights.Bold;

            if (ts1.averages[t.FGp] > ts2.averages[t.FGp])
                lblFGp1.FontWeight = FontWeights.Bold;
            else if (ts1.averages[t.FGp] < ts2.averages[t.FGp])
                lblFGp2.FontWeight = FontWeights.Bold;

            if (ts1.averages[t.FPG] < ts2.averages[t.FPG])
                lblFPG1.FontWeight = FontWeights.Bold;
            else if (ts1.averages[t.FPG] > ts2.averages[t.FPG])
                lblFPG2.FontWeight = FontWeights.Bold;

            if (ts1.averages[t.FTeff] > ts2.averages[t.FTeff])
                lblFTeff1.FontWeight = FontWeights.Bold;
            else if (ts1.averages[t.FTeff] < ts2.averages[t.FTeff])
                lblFTeff2.FontWeight = FontWeights.Bold;

            if (ts1.averages[t.FTp] > ts2.averages[t.FTp])
                lblFTp1.FontWeight = FontWeights.Bold;
            else if (ts1.averages[t.FTp] < ts2.averages[t.FTp])
                lblFTp2.FontWeight = FontWeights.Bold;

            if (ts1.averages[t.ORPG] > ts2.averages[t.ORPG])
                lblORPG1.FontWeight = FontWeights.Bold;
            else if (ts1.averages[t.ORPG] < ts2.averages[t.ORPG])
                lblORPG2.FontWeight = FontWeights.Bold;

            if (ts1.averages[t.PAPG] < ts2.averages[t.PAPG])
                lblPAPG1.FontWeight = FontWeights.Bold;
            else if (ts1.averages[t.PAPG] > ts2.averages[t.PAPG])
                lblPAPG2.FontWeight = FontWeights.Bold;

            if (ts1.averages[t.PPG] > ts2.averages[t.PPG])
                lblPPG1.FontWeight = FontWeights.Bold;
            else if (ts1.averages[t.PPG] < ts2.averages[t.PPG])
                lblPPG2.FontWeight = FontWeights.Bold;

            if (ts1.averages[t.RPG] > ts2.averages[t.RPG])
                lblRPG1.FontWeight = FontWeights.Bold;
            else if (ts1.averages[t.RPG] < ts2.averages[t.RPG])
                lblRPG2.FontWeight = FontWeights.Bold;

            if (ts1.averages[t.SPG] > ts2.averages[t.SPG])
                lblSPG1.FontWeight = FontWeights.Bold;
            else if (ts1.averages[t.SPG] < ts2.averages[t.SPG])
                lblSPG2.FontWeight = FontWeights.Bold;

            if (ts1.averages[t.TPG] < ts2.averages[t.TPG])
                lblTPG1.FontWeight = FontWeights.Bold;
            else if (ts1.averages[t.TPG] > ts2.averages[t.TPG])
                lblTPG2.FontWeight = FontWeights.Bold;

            if (ts1.averages[t.Wp] > ts2.averages[t.Wp])
                lblWp1.FontWeight = FontWeights.Bold;
            else if (ts1.averages[t.Wp] < ts2.averages[t.Wp])
                lblWp2.FontWeight = FontWeights.Bold;

            if (ts1.averages[t.Weff] > ts2.averages[t.Weff])
                lblWeff1.FontWeight = FontWeights.Bold;
            else if (ts1.averages[t.Weff] < ts2.averages[t.Weff])
                lblWeff2.FontWeight = FontWeights.Bold;
        }
    }
}