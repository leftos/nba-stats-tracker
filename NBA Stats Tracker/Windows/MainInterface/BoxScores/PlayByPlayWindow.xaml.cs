namespace NBA_Stats_Tracker.Windows.MainInterface.BoxScores
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Threading;

    using LeftosCommonLibrary;
    using LeftosCommonLibrary.CommonDialogs;

    using NBA_Stats_Tracker.Annotations;
    using NBA_Stats_Tracker.Data.BoxScores;
    using NBA_Stats_Tracker.Data.BoxScores.PlayByPlay;
    using NBA_Stats_Tracker.Data.Players;
    using NBA_Stats_Tracker.Data.SQLiteIO;
    using NBA_Stats_Tracker.Data.Teams;
    using NBA_Stats_Tracker.Helper.ListExtensions;
    using NBA_Stats_Tracker.Helper.Miscellaneous;

    #endregion

    /// <summary>Interaction logic for PlayByPlayWindow.xaml</summary>
    public partial class PlayByPlayWindow : Window, INotifyPropertyChanged
    {
        private readonly BoxScoreEntry _bse;
        private readonly Dictionary<int, PlayerStats> _pst;
        private readonly int _t1ID;
        private readonly int _t2ID;
        private readonly Dictionary<int, TeamStats> _tst;
        private int _awayPoints;
        private int _currentPeriod;
        private bool _exitedViaButton;
        private int _homePoints;
        private List<PlayerStats> _savedAwayActive;
        private int _savedAwayPoints;
        private List<PlayerStats> _savedAwaySubs;
        private List<PlayerStats> _savedHomeActive;
        private int _savedHomePoints;
        private List<PlayerStats> _savedHomeSubs;
        private int _savedPeriod;
        private double _savedShotClock;
        private double _savedTimeLeft;
        private double _shotClock;
        private DispatcherTimer _shotClockTimer;
        private double _timeLeft;
        private DispatcherTimer _timeLeftTimer;

        public PlayByPlayWindow()
        {
            InitializeComponent();

            SavedPlays = new List<PlayByPlayEntry>();
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
            Plays = new ObservableCollection<PlayByPlayEntry>(bse.PBPEList);

            Height = Tools.GetRegistrySetting("PBPHeight", MinHeight);
            Width = Tools.GetRegistrySetting("PBPWidth", MinWidth);
            Left = Tools.GetRegistrySetting("PBPX", Left);
            Top = Tools.GetRegistrySetting("PBPY", Top);
        }

        private ObservableCollection<PlayerStats> AwaySubs { get; set; }
        private ObservableCollection<PlayerStats> HomeSubs { get; set; }
        private ObservableCollection<PlayerStats> AwayActive { get; set; }
        private ObservableCollection<PlayerStats> HomeActive { get; set; }
        private ObservableCollection<ComboBoxItemWithIsEnabled> PlayersComboList { get; set; }
        private ObservableCollection<ComboBoxItemWithIsEnabled> PlayersComboList2 { get; set; }
        private ObservableCollection<PlayByPlayEntry> Plays { get; set; }
        public static List<PlayByPlayEntry> SavedPlays { get; set; }

        public int CurrentPeriod
        {
            get { return _currentPeriod; }
            set
            {
                _currentPeriod = value;
                OnPropertyChanged("CurrentPeriod");
            }
        }

        public int AwayPoints
        {
            get { return _awayPoints; }
            set
            {
                _awayPoints = value;
                OnPropertyChanged("AwayPoints");
            }
        }

        public int HomePoints
        {
            get { return _homePoints; }
            set
            {
                _homePoints = value;
                OnPropertyChanged("HomePoints");
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        private void window_Loaded(object sender, EventArgs e)
        {
            txbAwayTeam.Text = _tst[_t1ID].DisplayName;
            txbHomeTeam.Text = _tst[_t2ID].DisplayName;
            AwayPoints = _bse.BS.PTS1;
            HomePoints = _bse.BS.PTS2;

            CurrentPeriod = 1;

            resetTimeLeft();

            _timeLeftTimer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, 50) };
            _timeLeftTimer.Tick += _timeLeftTimer_Tick;

            resetShotClock();

            _shotClockTimer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, 50) };
            _shotClockTimer.Tick += _shotClockTimer_Tick;

            var awayPlayersIDs = _bse.PBSList.Where(pbs => pbs.TeamID == _t1ID).Select(pbs => pbs.PlayerID).ToList();
            AwaySubs = new ObservableCollection<PlayerStats>();
            awayPlayersIDs.ForEach(id => AwaySubs.Add(_pst[id]));
            AwaySubs.Sort((ps1, ps2) => String.Compare(ps1.FullName, ps2.FullName, StringComparison.CurrentCultureIgnoreCase));
            lstAwaySubs.ItemsSource = AwaySubs;

            AwayActive = new ObservableCollection<PlayerStats>();
            lstAwayActive.ItemsSource = AwayActive;

            var homePlayersIDs = _bse.PBSList.Where(pbs => pbs.TeamID == _t2ID).Select(pbs => pbs.PlayerID).ToList();
            HomeSubs = new ObservableCollection<PlayerStats>();
            homePlayersIDs.ForEach(id => HomeSubs.Add(_pst[id]));
            HomeSubs.Sort((ps1, ps2) => String.Compare(ps1.FullName, ps2.FullName, StringComparison.CurrentCultureIgnoreCase));
            lstHomeSubs.ItemsSource = HomeSubs;

            HomeActive = new ObservableCollection<PlayerStats>();
            lstHomeActive.ItemsSource = HomeActive;

            PlayersComboList = new ObservableCollection<ComboBoxItemWithIsEnabled>();
            PlayersComboList2 = new ObservableCollection<ComboBoxItemWithIsEnabled>();

            cmbEventType.ItemsSource = PlayByPlayEntry.EventTypes.Values;
            cmbEventType.SelectedIndex = 2;

            cmbShotOrigin.ItemsSource = ShotEntry.ShotOrigins.Values;
            cmbShotType.ItemsSource = ShotEntry.ShotTypes.Values;

            cmbPlayer1.ItemsSource = PlayersComboList;
            cmbPlayer2.ItemsSource = PlayersComboList2;

            if (Plays == null)
            {
                Plays = new ObservableCollection<PlayByPlayEntry>();
            }
            dgEvents.ItemsSource = Plays;

#if DEBUG
            for (var i = 0; i < 5; i++)
            {
                HomeActive.Add(HomeSubs[0]);
                HomeSubs.RemoveAt(0);
                AwayActive.Add(AwaySubs[0]);
                AwaySubs.RemoveAt(0);
            }
            sortPlayerLists();
#endif
        }

        private void resetShotClock()
        {
            _shotClock = MainWindow.ShotClockDuration;
            updateShotClockIndication(_shotClock);
        }

        private void _shotClockTimer_Tick(object sender, EventArgs e)
        {
            _shotClock -= 0.05;
            if (_shotClock < 0.01)
            {
                _shotClockTimer.Stop();
                _shotClock = 0;
            }
            updateShotClockIndication(_shotClock);
        }

        private void updateShotClockIndication(double shotClock)
        {
            var pair = PlayByPlayEntry.ShotClockToStringPair(shotClock);

            txbShotClockLeftInt.Text = pair.Key;
            txbShotClockLeftDec.Text = pair.Value;
        }

        private void _timeLeftTimer_Tick(object sender, EventArgs e)
        {
            _timeLeft -= 0.05;
            if (_timeLeft < 0.01)
            {
                _timeLeftTimer.Stop();
                _timeLeft = 0;
            }
            updateTimeLeftIndication(_timeLeft);
        }

        private void updateTimeLeftIndication(double timeLeft)
        {
            var pair = PlayByPlayEntry.TimeLeftToStringPair(timeLeft);

            txbTimeLeftInt.Text = pair.Key;
            txbTimeLeftDec.Text = pair.Value;
        }

        public static double ConvertTimeStringToDouble(string s)
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
            for (var i = intParts.Length - 2; i >= 0; i--)
            {
                factor *= 60;
                intPart += Convert.ToDouble(intParts[i]) * factor;
            }
            return intPart + decPart;
        }

        private void window_Closing(object sender, CancelEventArgs e)
        {
            if (!_exitedViaButton)
            {
                e.Cancel = true;
            }

            Tools.SetRegistrySetting("PBPHeight", Height);
            Tools.SetRegistrySetting("PBPWidth", Width);
            Tools.SetRegistrySetting("PBPX", Left);
            Tools.SetRegistrySetting("PBPY", Top);
        }

        private void btnTimeLeftStartStop_Click(object sender, RoutedEventArgs e)
        {
            if (_timeLeftTimer.IsEnabled)
            {
                _timeLeftTimer.Stop();
                _shotClockTimer.Stop();
            }
            else
            {
                _timeLeftTimer.Start();
            }
        }

        private void btnShotClockStartStop_Click(object sender, RoutedEventArgs e)
        {
            if (_shotClockTimer.IsEnabled)
            {
                _shotClockTimer.Stop();
            }
            else
            {
                _timeLeftTimer.Start();
                _shotClockTimer.Start();
            }
        }

        private void btnTimeLeftReset_Click(object sender, RoutedEventArgs e)
        {
            resetTimeLeft();
        }

        private void resetTimeLeft()
        {
            _timeLeft = (MainWindow.GameLength / MainWindow.NumberOfPeriods) * 60;
            updateTimeLeftIndication(_timeLeft);
        }

        private void btnShotClockReset_Click(object sender, RoutedEventArgs e)
        {
            resetShotClock();
        }

        private void btnTimeLeftSet_Click(object sender, RoutedEventArgs e)
        {
            var ibw = new InputBoxWindow("Enter the time left:", SQLiteIO.GetSetting("LastTimeLeftSet", "0:00"), "NBA Stats Tracker");
            if (ibw.ShowDialog() == false)
            {
                return;
            }

            var timeLeft = _timeLeft;
            try
            {
                timeLeft = ConvertTimeStringToDouble(InputBoxWindow.UserInput);
            }
            catch
            {
                return;
            }

            _timeLeft = timeLeft;
            updateTimeLeftIndication(_timeLeft);
            SQLiteIO.SetSetting("LastTimeLeftSet", InputBoxWindow.UserInput);
        }

        private void btnShotClockSet_Click(object sender, RoutedEventArgs e)
        {
            var ibw = new InputBoxWindow(
                "Enter the shot clock left:", SQLiteIO.GetSetting("LastShotClockSet", "0.0"), "NBA Stats Tracker");
            if (ibw.ShowDialog() == false)
            {
                return;
            }

            var shotClock = _shotClock;
            try
            {
                shotClock = ConvertTimeStringToDouble(InputBoxWindow.UserInput);
            }
            catch
            {
                return;
            }

            _shotClock = shotClock;
            updateShotClockIndication(_shotClock);
            SQLiteIO.SetSetting("LastShotClockSet", InputBoxWindow.UserInput);
        }

        private void btnAwayDoSubs_Click(object sender, RoutedEventArgs e)
        {
            var inCount = lstAwaySubs.SelectedItems.Count;
            var outCount = lstAwayActive.SelectedItems.Count;
            var activeCount = lstAwayActive.Items.Count;
            var diff = inCount - outCount;

            if (activeCount + diff != 5)
            {
                return;
            }

            var playersIn = lstAwaySubs.SelectedItems.Cast<PlayerStats>().ToList();
            var playersOut = lstAwayActive.SelectedItems.Cast<PlayerStats>().ToList();
            foreach (var player in playersIn)
            {
                AwaySubs.Remove(player);
                AwayActive.Add(player);
            }
            foreach (var player in playersOut)
            {
                AwaySubs.Add(player);
                AwayActive.Remove(player);
            }
            sortPlayerLists();
        }

        private void sortPlayerLists()
        {
            AwaySubs.Sort((ps1, ps2) => String.Compare(ps1.FullName, ps2.FullName, StringComparison.CurrentCultureIgnoreCase));
            AwayActive.Sort((ps1, ps2) => String.Compare(ps1.FullName, ps2.FullName, StringComparison.CurrentCultureIgnoreCase));
            HomeSubs.Sort((ps1, ps2) => String.Compare(ps1.FullName, ps2.FullName, StringComparison.CurrentCultureIgnoreCase));
            HomeActive.Sort((ps1, ps2) => String.Compare(ps1.FullName, ps2.FullName, StringComparison.CurrentCultureIgnoreCase));

            PlayersComboList.Clear();
            PlayersComboList.Add(new ComboBoxItemWithIsEnabled(txbAwayTeam.Text, false));
            AwayActive.ToList().ForEach(ps => PlayersComboList.Add(new ComboBoxItemWithIsEnabled(ps.ToString(), true, ps.ID)));
            PlayersComboList.Add(new ComboBoxItemWithIsEnabled(txbHomeTeam.Text, false));
            HomeActive.ToList().ForEach(ps => PlayersComboList.Add(new ComboBoxItemWithIsEnabled(ps.ToString(), true, ps.ID)));

            populatePlayer2Combo();
        }

        private void populatePlayer2Combo()
        {
            PlayersComboList2.Clear();
            if (cmbEventType.SelectedIndex == -1)
            {
                return;
            }
            var curEventKey = PlayByPlayEntry.EventTypes.Single(pair => pair.Value == cmbEventType.SelectedItem.ToString()).Key;
            if (curEventKey <= 0 || cmbPlayer1.SelectedIndex == -1)
            {
                PlayersComboList2 = new ObservableCollection<ComboBoxItemWithIsEnabled>(PlayersComboList);
            }
            else
            {
                var curPlayer = cmbPlayer1.SelectedItem as ComboBoxItemWithIsEnabled;
                if (curPlayer == null)
                {
                    cmbPlayer2.ItemsSource = PlayersComboList2;
                    return;
                }
                var curPlayerTeam = _bse.PBSList.Single(pbs => pbs.PlayerID == curPlayer.ID).TeamID;
                List<PlayerStats> list;
                if (PlayByPlayEntry.UseOpposingTeamAsPlayer2.Contains(curEventKey))
                {
                    if (curPlayerTeam == _t1ID)
                    {
                        PlayersComboList2.Add(new ComboBoxItemWithIsEnabled(txbHomeTeam.Text, false));
                        list = HomeActive.ToList();
                    }
                    else
                    {
                        PlayersComboList2.Add(new ComboBoxItemWithIsEnabled(txbAwayTeam.Text, false));
                        list = AwayActive.ToList();
                    }
                }
                else
                {
                    if (curPlayerTeam == _t1ID)
                    {
                        PlayersComboList2.Add(new ComboBoxItemWithIsEnabled(txbAwayTeam.Text, false));
                        list = curEventKey != 13 ? AwayActive.Where(ps => ps.ID != curPlayer.ID).ToList() : AwayActive.ToList();
                    }
                    else
                    {
                        PlayersComboList2.Add(new ComboBoxItemWithIsEnabled(txbHomeTeam.Text, false));
                        list = curEventKey != 13 ? HomeActive.Where(ps => ps.ID != curPlayer.ID).ToList() : HomeActive.ToList();
                    }
                }
                list.ForEach(ps => PlayersComboList2.Add(new ComboBoxItemWithIsEnabled(ps.ToString(), true, ps.ID)));
            }
            cmbPlayer2.ItemsSource = PlayersComboList2;
        }

        private void btnHomeDoSubs_Click(object sender, RoutedEventArgs e)
        {
            var inCount = lstHomeSubs.SelectedItems.Count;
            var outCount = lstHomeActive.SelectedItems.Count;
            var activeCount = lstHomeActive.Items.Count;
            var diff = inCount - outCount;

            if (activeCount + diff != 5)
            {
                return;
            }

            var playersIn = lstHomeSubs.SelectedItems.Cast<PlayerStats>().ToList();
            var playersOut = lstHomeActive.SelectedItems.Cast<PlayerStats>().ToList();
            foreach (var player in playersIn)
            {
                HomeSubs.Remove(player);
                HomeActive.Add(player);
            }
            foreach (var player in playersOut)
            {
                HomeSubs.Add(player);
                HomeActive.Remove(player);
            }
            sortPlayerLists();
        }

        private void cmbEventType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbEventType.SelectedIndex == -1)
            {
                return;
            }

            var curEventKey = PlayByPlayEntry.EventTypes.Single(pair => pair.Value == cmbEventType.SelectedItem.ToString()).Key;

            txtEventDesc.IsEnabled = cmbEventType.SelectedItem.ToString() == "Other";

            grdShotEvent.IsEnabled = curEventKey == 1;
            txbLocationLabel.Text = curEventKey == 1 ? "Shot Distance" : "Location";
            txtLocationDesc.IsEnabled = false;
            cmbLocationShotDistance.ItemsSource = curEventKey == 1
                                                      ? ShotEntry.ShotDistances.Values
                                                      : PlayByPlayEntry.EventLocations.Values;
            if (curEventKey == 3 || curEventKey == 4)
            {
                cmbLocationShotDistance.SelectedIndex = 0;
                cmbLocationShotDistance.IsEnabled = false;
            }
            else
            {
                cmbLocationShotDistance.IsEnabled = true;
            }

            try
            {
                var definition = PlayByPlayEntry.Player2Definition[curEventKey];
                txbPlayer2Label.Text = definition;
                cmbPlayer2.IsEnabled = true;
                populatePlayer2Combo();
            }
            catch (KeyNotFoundException)
            {
                txbPlayer2Label.Text = "Not Applicable";
                cmbPlayer2.IsEnabled = false;
            }
        }

        private void cmbLocationShotDistance_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var curEventKey = PlayByPlayEntry.EventTypes.Single(pair => pair.Value == cmbEventType.SelectedItem.ToString()).Key;
            if (curEventKey != 1 && cmbLocationShotDistance.SelectedIndex != -1)
            {
                var curDistanceKey =
                    PlayByPlayEntry.EventLocations.Single(pair => pair.Value == cmbLocationShotDistance.SelectedItem.ToString()).Key;
                txtLocationDesc.IsEnabled = (curDistanceKey == -1 && curEventKey != 3 && curEventKey != 4);
            }
            else
            {
                txtLocationDesc.IsEnabled = false;
            }
        }

        private void cmbPlayer1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            populatePlayer2Combo();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (cmbEventType.SelectedIndex == -1 || cmbPlayer1.SelectedIndex == -1 || cmbLocationShotDistance.SelectedIndex == -1)
            {
                return;
            }
            if (grdShotEvent.IsEnabled && (cmbShotOrigin.SelectedIndex == -1 || cmbShotType.SelectedIndex == -1))
            {
                return;
            }
            var play = createPlayByPlayEntryFromCurrent();
            if (chkUpdate.IsChecked == true)
            {
                if (play.EventType == PlayByPlayEntry.ShotAttemptEventType && play.ShotEntry.IsMade)
                {
                    if (play.ShotEntry.Distance > 0 && play.ShotEntry.Distance < 5)
                    {
                        if (play.Team1PlayerIDs.Contains(play.Player1ID))
                        {
                            AwayPoints += 2;
                        }
                        else
                        {
                            HomePoints += 2;
                        }
                    }
                    else if (play.ShotEntry.Distance == 5)
                    {
                        if (play.Team1PlayerIDs.Contains(play.Player1ID))
                        {
                            AwayPoints += 3;
                        }
                        else
                        {
                            HomePoints += 3;
                        }
                    }
                }
                else if (play.EventType == 3)
                {
                    if (play.Team1PlayerIDs.Contains(play.Player1ID))
                    {
                        AwayPoints++;
                    }
                    else
                    {
                        HomePoints++;
                    }
                }
                play.T1PTS = AwayPoints;
                play.T2PTS = HomePoints;
            }
            Plays.Add(play);
            Plays.Sort(new PlayByPlayEntryComparerAsc());
            dgEvents.ItemsSource = Plays;
        }

        private PlayByPlayEntry createPlayByPlayEntryFromCurrent()
        {
            var curPlayer = cmbPlayer1.SelectedItem as ComboBoxItemWithIsEnabled;
            var curPlayerTeam = _bse.PBSList.Single(pbs => pbs.PlayerID == curPlayer.ID).TeamID;
            var teamName = _tst[curPlayerTeam].DisplayName;
            var play = new PlayByPlayEntry
                {
                    DisplayPlayer1 = cmbPlayer1.SelectedItem.ToString(),
                    DisplayPlayer2 = cmbPlayer2.SelectedIndex != -1 ? cmbPlayer2.SelectedItem.ToString() : "",
                    DisplayTeam = teamName,
                    EventDesc = txtEventDesc.IsEnabled ? txtEventDesc.Text : "",
                    EventType = PlayByPlayEntry.EventTypes.Single(item => item.Value == cmbEventType.SelectedItem.ToString()).Key,
                    GameID = _bse.BS.ID,
                    Location =
                        grdShotEvent.IsEnabled
                            ? -2
                            : PlayByPlayEntry.EventLocations.Single(item => item.Value == cmbLocationShotDistance.SelectedItem.ToString())
                                             .Key,
                    LocationDesc = txtLocationDesc.IsEnabled ? txtLocationDesc.Text : "",
                    Player1ID = curPlayer.ID,
                    Player2ID =
                        (cmbPlayer2.IsEnabled && cmbPlayer2.SelectedIndex != -1)
                            ? (cmbPlayer2.SelectedItem as ComboBoxItemWithIsEnabled).ID
                            : -1,
                    T1PTS = Convert.ToInt32(AwayPoints),
                    T2PTS = Convert.ToInt32(HomePoints),
                    Team1PlayerIDs = AwayActive.Select(ps => ps.ID).ToList(),
                    Team2PlayerIDs = HomeActive.Select(ps => ps.ID).ToList(),
                    ShotEntry =
                        grdShotEvent.IsEnabled
                            ? new ShotEntry(
                                  ShotEntry.ShotDistances.Single(item => item.Value == cmbLocationShotDistance.SelectedItem.ToString()).Key,
                                  ShotEntry.ShotOrigins.Single(item => item.Value == cmbShotOrigin.SelectedItem.ToString()).Key,
                                  ShotEntry.ShotTypes.Single(item => item.Value == cmbShotType.SelectedItem.ToString()).Key,
                                  chkShotIsMade.IsChecked == true,
                                  chkShotIsAssisted.IsChecked == true)
                            : null,
                    TimeLeft = _timeLeft,
                    ShotClockLeft = _shotClock,
                    Quarter = CurrentPeriod
                };
            return play;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (dgEvents.SelectedIndex == -1)
            {
                return;
            }
            if (btnEdit.Content.ToString() == "Edit")
            {
                var selectedPlay = dgEvents.SelectedItem as PlayByPlayEntry;
                dgEvents.IsEnabled = false;
                btnSave.IsEnabled = false;
                btnCancel.IsEnabled = false;
                btnAdd.IsEnabled = false;
                btnDelete.IsEnabled = false;
                _shotClockTimer.Stop();
                _timeLeftTimer.Stop();
                _savedTimeLeft = _timeLeft;
                _savedShotClock = _shotClock;
                _savedAwayActive = AwayActive.ToList();
                _savedAwaySubs = AwaySubs.ToList();
                _savedHomeActive = HomeActive.ToList();
                _savedHomeSubs = HomeSubs.ToList();
                _savedAwayPoints = Convert.ToInt32(AwayPoints);
                _savedHomePoints = Convert.ToInt32(HomePoints);
                _savedPeriod = CurrentPeriod;

                _timeLeft = selectedPlay.TimeLeft;
                updateTimeLeftIndication(_timeLeft);

                _shotClock = selectedPlay.ShotClockLeft;
                updateShotClockIndication(_shotClock);

                AwayActive = new ObservableCollection<PlayerStats>(selectedPlay.Team1PlayerIDs.Select(id => _pst[id]).ToList());
                AwaySubs =
                    new ObservableCollection<PlayerStats>(
                        _bse.PBSList.Where(pbs => pbs.TeamID == _t1ID && !selectedPlay.Team1PlayerIDs.Contains(pbs.PlayerID))
                            .Select(pbs => _pst[pbs.PlayerID])
                            .ToList());
                HomeActive = new ObservableCollection<PlayerStats>(selectedPlay.Team2PlayerIDs.Select(id => _pst[id]).ToList());
                HomeSubs =
                    new ObservableCollection<PlayerStats>(
                        _bse.PBSList.Where(pbs => pbs.TeamID == _t2ID && !selectedPlay.Team2PlayerIDs.Contains(pbs.PlayerID))
                            .Select(pbs => _pst[pbs.PlayerID])
                            .ToList());

                lstAwayActive.ItemsSource = AwayActive;
                lstAwaySubs.ItemsSource = AwaySubs;
                lstHomeActive.ItemsSource = HomeActive;
                lstHomeSubs.ItemsSource = HomeSubs;

                sortPlayerLists();

                cmbEventType.SelectedItem = PlayByPlayEntry.EventTypes[selectedPlay.EventType];
                txtEventDesc.Text = selectedPlay.EventDesc;
                cmbPlayer1.SelectedItem = PlayersComboList.Single(item => item.ID == selectedPlay.Player1ID);
                cmbPlayer2.SelectedItem = selectedPlay.Player2ID != -1
                                              ? PlayersComboList2.Single(item => item.ID == selectedPlay.Player2ID)
                                              : null;
                cmbLocationShotDistance.SelectedItem = selectedPlay.EventType != PlayByPlayEntry.ShotAttemptEventType
                                                           ? PlayByPlayEntry.EventLocations[selectedPlay.Location]
                                                           : ShotEntry.ShotDistances[selectedPlay.ShotEntry.Distance];
                txtLocationDesc.Text = selectedPlay.LocationDesc;
                cmbShotOrigin.SelectedItem = selectedPlay.EventType == PlayByPlayEntry.ShotAttemptEventType
                                                 ? ShotEntry.ShotOrigins[selectedPlay.ShotEntry.Origin]
                                                 : null;
                cmbShotType.SelectedItem = selectedPlay.EventType == PlayByPlayEntry.ShotAttemptEventType
                                               ? ShotEntry.ShotTypes[selectedPlay.ShotEntry.Type]
                                               : null;
                chkShotIsMade.IsChecked = selectedPlay.EventType == PlayByPlayEntry.ShotAttemptEventType
                                          && selectedPlay.ShotEntry.IsMade;
                chkShotIsAssisted.IsChecked = selectedPlay.EventType == PlayByPlayEntry.ShotAttemptEventType
                                              && selectedPlay.ShotEntry.IsAssisted;
                AwayPoints = selectedPlay.T1PTS;
                HomePoints = selectedPlay.T2PTS;
                CurrentPeriod = selectedPlay.Quarter;

                btnEdit.Content = "Save";
            }
            else
            {
                if (cmbEventType.SelectedIndex == -1 || cmbPlayer1.SelectedIndex == -1 || cmbLocationShotDistance.SelectedIndex == -1)
                {
                    return;
                }
                if (grdShotEvent.IsEnabled && (cmbShotOrigin.SelectedIndex == -1 || cmbShotType.SelectedIndex == -1))
                {
                    return;
                }
                var play = createPlayByPlayEntryFromCurrent();
                Plays.Remove(dgEvents.SelectedItem as PlayByPlayEntry);
                Plays.Add(play);
                Plays.Sort(new PlayByPlayEntryComparerAsc());

                _timeLeft = _savedTimeLeft;
                updateTimeLeftIndication(_timeLeft);
                _shotClock = _savedShotClock;
                updateShotClockIndication(_shotClock);
                AwayActive = new ObservableCollection<PlayerStats>(_savedAwayActive);
                AwaySubs = new ObservableCollection<PlayerStats>(_savedAwaySubs);
                HomeActive = new ObservableCollection<PlayerStats>(_savedHomeActive);
                HomeSubs = new ObservableCollection<PlayerStats>(_savedHomeSubs);
                AwayPoints = _savedAwayPoints;
                HomePoints = _savedHomePoints;
                CurrentPeriod = _savedPeriod;

                lstAwayActive.ItemsSource = AwayActive;
                lstAwaySubs.ItemsSource = AwaySubs;
                lstHomeActive.ItemsSource = HomeActive;
                lstHomeSubs.ItemsSource = HomeSubs;

                sortPlayerLists();

                dgEvents.IsEnabled = true;
                btnSave.IsEnabled = true;
                btnCancel.IsEnabled = true;
                btnAdd.IsEnabled = true;
                btnDelete.IsEnabled = true;

                btnEdit.Content = "Edit";
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgEvents.SelectedIndex == -1)
            {
                return;
            }

            var selectedPlay = dgEvents.SelectedItem as PlayByPlayEntry;
            Plays.Remove(selectedPlay);
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(
                "Are you sure you want to exit the Play By Play Editor without saving changes?",
                App.AppName,
                MessageBoxButton.YesNo,
                MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _exitedViaButton = true;
                DialogResult = false;
                Close();
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            SavedPlays = new List<PlayByPlayEntry>(Plays);
            _exitedViaButton = true;
            DialogResult = true;
            Close();
        }
    }
}