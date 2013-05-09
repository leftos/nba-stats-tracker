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

namespace NBA_Stats_Tracker.Windows.MainInterface.BoxScores
{
    using System.ComponentModel;

    using LeftosCommonLibrary;

    using NBA_Stats_Tracker.Data.BoxScores;
    using NBA_Stats_Tracker.Data.Players;
    using NBA_Stats_Tracker.Data.SQLiteIO;
    using NBA_Stats_Tracker.Data.Teams;

    /// <summary>
    /// Interaction logic for PlayByPlayWindow.xaml
    /// </summary>
    public partial class PlayByPlayWindow : Window
    {
        private Dictionary<int, TeamStats> _tst;
        private Dictionary<int, PlayerStats> _pst;
        private BoxScoreEntry _bse;
        private int _t1ID;
        private int _t2ID;
        private double _timeLeft;

        public PlayByPlayWindow()
        {
            InitializeComponent();
        }

        public PlayByPlayWindow(
            Dictionary<int, TeamStats> tst, Dictionary<int, PlayerStats> pst, BoxScoreEntry bse, int t1ID, int t2ID)
            : this()
        {
            _tst = tst;
            _pst = pst;
            _bse = bse;
            _t1ID = t1ID;
            _t2ID = t2ID;
        }

        private void window_Initialized(object sender, EventArgs e)
        {
            Height = Tools.GetRegistrySetting("PBPHeight", MinHeight);
            Width = Tools.GetRegistrySetting("PBPWidth", MinWidth);
            Left = Tools.GetRegistrySetting("PBPX", Left);
            Top = Tools.GetRegistrySetting("PBPY", Top);

            txbAwayTeam.Text = _tst[_t1ID].DisplayName;
            txbHomeTeam.Text = _tst[_t2ID].DisplayName;
            txtAwayScore.Text = _bse.BS.PTS1.ToString();
            txtHomeScore.Text = _bse.BS.PTS2.ToString();

            txtPeriod.Text = "1";
            txbTimeLeftInt.Text = (MainWindow.GameLength / MainWindow.NumberOfPeriods).ToString();
            _timeLeft = convertTimeStringToDouble("12:00.0");
        }

        private double convertTimeStringToDouble(string s)
        {
            var parts = s.Split('.');
            double decPart = 0;
            if (parts.Length == 2)
            {
                decPart = Convert.ToDouble("0." + parts[1]);
            }
            var intParts = parts[0].Split(':');
            double intPart = 0;
            intPart += Convert.ToDouble(intParts[intParts.Length - 1]);
            var factor = 1;
            for (int i = intParts.Length - 2; i >= 0; i--)
            {
                factor *= 60;
                intPart += Convert.ToDouble(intParts[i]) * factor;
            }
            return intPart + decPart;
        }

        private void window_Closing(object sender, CancelEventArgs e)
        {
            Tools.SetRegistrySetting("PBPHeight", Height);
            Tools.SetRegistrySetting("PBPWidth", Width);
            Tools.SetRegistrySetting("PBPX", Left);
            Tools.SetRegistrySetting("PBPY", Top);
        }
    }
}
