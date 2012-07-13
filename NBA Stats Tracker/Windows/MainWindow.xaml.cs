#region Copyright Notice

// Created by Lefteris Aslanoglou, (c) 2011-2012
// 
// Implementation of thesis
// "Application Development for Basketball Statistical Analysis in Natural Language"
// under the supervision of Prof. Athanasios Tsakalidis & MSc Alexandros Georgiou,
// Computer Engineering & Informatics Department, University of Patras, Greece.
// 
// All rights reserved. Unless specifically stated otherwise, the code in this file should 
// not be reproduced, edited and/or republished without explicit permission from the 
// author.

#endregion

#region Using Directives

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Threading;
using LeftosCommonLibrary;
using Microsoft.Win32;
using NBA_Stats_Tracker.Data;
using NBA_Stats_Tracker.Helper;
using NBA_Stats_Tracker.Interop;
using SQLite_Database;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

#endregion

namespace NBA_Stats_Tracker.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public static readonly string AppDocsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) +
                                                    @"\NBA Stats Tracker\";

        public static readonly string AppTempPath = AppDocsPath + @"Temp\";
        public static string SavesPath = "";
        public static readonly string AppPath = Environment.CurrentDirectory + "\\";
        public static bool isCustom;

        public static string input = "";

        public static MainWindow mwInstance;

        public static Dictionary<int, TeamStats> tst = new Dictionary<int, TeamStats>();
        public static Dictionary<int, TeamStats> tstopp = new Dictionary<int, TeamStats>();
        private static Dictionary<int, TeamStats> realtst = new Dictionary<int, TeamStats>();
        public static Dictionary<int, PlayerStats> pst = new Dictionary<int, PlayerStats>();
        public static BoxScore bs;
        public static IList<BoxScoreEntry> bshist = new List<BoxScoreEntry>();
        public static PlayoffTree pt;
        public static string currentDB = "";
        public static string addInfo;
        public static int curSeason = 1;

        public static readonly ObservableCollection<KeyValuePair<int, string>> SeasonList =
            new ObservableCollection<KeyValuePair<int, string>>();

        public static List<BindingList<PlayerBoxScore>> pbsLists;
        public static BoxScoreEntry tempbse;

        public static SortedDictionary<string, int> TeamOrder;

        public static readonly List<string> West = new List<string>
                                                       {
                                                           "Thunder",
                                                           "Spurs",
                                                           "Trail Blazers",
                                                           "Clippers",
                                                           "Nuggets",
                                                           "Jazz",
                                                           "Lakers",
                                                           "Mavericks",
                                                           "Suns",
                                                           "Grizzlies",
                                                           "Kings",
                                                           "Timberwolves",
                                                           "Rockets",
                                                           "Hornets",
                                                           "Warriors"
                                                       };

        public static SQLiteDatabase db;
        public static bool loadingSeason;
        private static bool showUpdateMessage;

        private DispatcherTimer dispatcherTimer;
        private double progress;
        private Semaphore sem;
        private BackgroundWorker worker1 = new BackgroundWorker();

        public MainWindow()
        {
            InitializeComponent();

            mwInstance = this;
            bs = new BoxScore();

            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-US");

            //btnInject.Visibility = Visibility.Hidden;
            btnTest.Visibility = Visibility.Hidden;

            isCustom = true;

            if (Directory.Exists(AppDocsPath) == false) Directory.CreateDirectory(AppDocsPath);
            if (Directory.Exists(AppTempPath) == false) Directory.CreateDirectory(AppTempPath);

            tst[0] = new TeamStats("$$NewDB");
            tstopp[0] = new TeamStats("$$NewDB");

            for (int i = 0; i < 30; i++)
            {
                realtst[i] = new TeamStats();
            }

            //teamOrder = StatsTracker.setTeamOrder("Mode 0");
            TeamOrder = new SortedDictionary<string, int>();

            RegistryKey rk = null;

            try
            {
                rk = Registry.CurrentUser;
            }
            catch (Exception ex)
            {
                App.errorReport(ex, "Registry.CurrentUser");
            }

            Debug.Assert(rk != null, "rk != null");
            rk = rk.OpenSubKey(@"SOFTWARE\2K Sports\NBA 2K12");
            if (rk == null)
            {
                SavesPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                            @"\2K Sports\NBA 2K12\Saves\";
            }
            else
            {
                SavesPath = rk.GetValue("Saves").ToString();
            }

            checkForRedundantSettings();

            if (App.realNBAonly)
            {
                mnuFileGetRealStats_Click(null, null);
                MessageBox.Show("Nothing but net! Thanks for using NBA Stats Tracker!");
                Environment.Exit(-1);
            }
            else
            {
                rk = Registry.CurrentUser;
                int checkForUpdatesSetting = 1;
                try
                {
                    if (rk == null) throw new Exception();

                    rk = rk.OpenSubKey(@"SOFTWARE\Lefteris Aslanoglou\NBA Stats Tracker");
                    if (rk != null) checkForUpdatesSetting = Convert.ToInt32(rk.GetValue("CheckForUpdates", 1));
                }
                catch
                {
                    checkForUpdatesSetting = 1;
                }
                if (checkForUpdatesSetting == 1)
                {
                    mnuOptionsCheckForUpdates.IsChecked = true;
                    CheckForUpdates();
                }
                else
                {
                    mnuOptionsCheckForUpdates.IsChecked = false;
                }

                rk = Registry.CurrentUser;
                int importSetting = 0;
                try
                {
                    if (rk == null) throw new Exception();

                    rk = rk.OpenSubKey(@"SOFTWARE\Lefteris Aslanoglou\NBA Stats Tracker");
                    if (rk != null) importSetting = Convert.ToInt32(rk.GetValue("NBA2K12ImportMethod", 0));
                }
                catch
                {
                    importSetting = 0;
                }
                if (importSetting == 0)
                {
                    mnuOptionsImportREditor.IsChecked = true;
                }
                else
                {
                    mnuOptionsImportOld.IsChecked = true;
                }

                rk = Registry.CurrentUser;
                int ExportTeamsOnly = 1;
                try
                {
                    if (rk == null) throw new Exception();

                    rk = rk.OpenSubKey(@"SOFTWARE\Lefteris Aslanoglou\NBA Stats Tracker");
                    if (rk != null) ExportTeamsOnly = Convert.ToInt32(rk.GetValue("ExportTeamsOnly", 1));
                }
                catch
                {
                    ExportTeamsOnly = 1;
                }
                mnuOptionsExportTeamsOnly.IsChecked = ExportTeamsOnly == 1;

                rk = Registry.CurrentUser;
                int CompatibilityCheck = 1;
                try
                {
                    if (rk == null) throw new Exception();

                    rk = rk.OpenSubKey(@"SOFTWARE\Lefteris Aslanoglou\NBA Stats Tracker");
                    if (rk != null) CompatibilityCheck = Convert.ToInt32(rk.GetValue("CompatibilityCheck", 1));
                }
                catch
                {
                    CompatibilityCheck = 1;
                }
                mnuOptionsCompatibilityCheck.IsChecked = CompatibilityCheck == 1;
            }
        }

        public static string AppDocsPath1
        {
            get { return AppDocsPath; }
        }

        private static void checkForRedundantSettings()
        {
            string[] stgFiles = Directory.GetFiles(AppDocsPath, "*.cfg");
            if (Directory.Exists(SavesPath))
            {
                foreach (string file in stgFiles)
                {
                    string realfile = file.Substring(0, file.Length - 4);
                    if (File.Exists(SavesPath + Tools.getSafeFilename(realfile)) == false)
                        File.Delete(file);
                }
            }

            string[] bshFiles = Directory.GetFiles(AppDocsPath, "*.bsh");
            if (Directory.Exists(SavesPath))
            {
                foreach (string file in bshFiles)
                {
                    string realfile = file.Substring(0, file.Length - 4);
                    if (File.Exists(SavesPath + Tools.getSafeFilename(realfile)) == false)
                        File.Delete(file);
                }
            }
        }

        private void btnImport2K12_Click(object sender, RoutedEventArgs e)
        {
            if (mnuOptionsImportOld.IsChecked)
            {
                var ofd = new OpenFileDialog
                              {
                                  Title = "Please select the Career file you're playing...",
                                  Filter =
                                      "All NBA 2K12 Career Files (*.FXG; *.CMG; *.RFG; *.PMG; *.SMG)|*.FXG;*.CMG;*.RFG;*.PMG;*.SMG|"
                                      +
                                      "Association files (*.FXG)|*.FXG|My Player files (*.CMG)|*.CMG|Season files (*.RFG)|*.RFG|Playoff files (*.PMG)|*.PMG|" +
                                      "Create A Legend files (*.SMG)|*.SMG"
                              };
                if (Directory.Exists(App.SavesPath)) ofd.InitialDirectory = SavesPath;
                ofd.ShowDialog();
                if (ofd.FileName == "") return;

                isCustom = true;
                //prepareWindow(isCustom);
                TeamOrder = Interop2K12.setTeamOrder("Mode 0");

                Dictionary<int, TeamStats> temp;

                //TODO: Implement Opponents stats from 2K12 Save
                //Dictionary<int, TeamStats> tempopp = new TeamStats[1];
                Dictionary<int, TeamStats> tempopp = tstopp;

                Interop2K12.GetStatsFrom2K12Save(ofd.FileName, out temp, ref tempopp, ref TeamOrder, ref pt);
                if (temp.Count > 1)
                {
                    tst = new Dictionary<int, TeamStats>(temp);
                    tstopp = new Dictionary<int, TeamStats>(tempopp);
                }

                if (tst.Count != tstopp.Count)
                {
                    tstopp = new Dictionary<int, TeamStats>();
                    for (int i = 0; i < tst.Count; i++) tstopp[i] = new TeamStats(tst[i].name);
                }

                updateStatus(
                    "NBA 2K12 stats imported successfully! Verify that you want this by saving the current season.");
            }
            else
            {
                var fbd = new FolderBrowserDialog
                              {
                                  Description = "Select folder with REditor-exported CSVs",
                                  ShowNewFolderButton = false
                              };
                DialogResult dr = fbd.ShowDialog(this.GetIWin32Window());

                if (dr != System.Windows.Forms.DialogResult.OK) return;

                if (fbd.SelectedPath == "") return;

                int result = InteropREditor.ImportAll(ref tst, ref tstopp, ref TeamOrder, ref pst, fbd.SelectedPath);

                if (result != 0)
                {
                    MessageBox.Show(
                        "Import failed! Please reload your database immediatelly to avoid saving corrupt data.",
                        "NBA Stats Tracker", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                updateStatus(
                    "NBA 2K12 stats imported successfully! Verify that you want this by saving the current season.");
            }
        }

        private void mnuFileSaveAs_Click(object sender, RoutedEventArgs e)
        {
            var sfd = new SaveFileDialog {Filter = "NST Database (*.tst)|*.tst", InitialDirectory = AppDocsPath};
            sfd.ShowDialog();

            if (sfd.FileName == "") return;

            string file = sfd.FileName;

            string oldDB = currentDB + ".tmp";
            File.Copy(currentDB, oldDB, true);
            currentDB = oldDB;
            try
            {
                File.Delete(file);
            }
            catch
            {
                MessageBox.Show(
                    "Error while trying to overwrite file. Make sure the file is not in use by another program.");
                return;
            }
            SQLiteIO.saveAllSeasons(file);
            File.Delete(oldDB);
            updateStatus("All seasons saved successfully.");
        }

        private void mnuFileOpenCustom_Click(object sender, RoutedEventArgs e)
        {
            tst = new Dictionary<int, TeamStats>();
            TeamOrder = new SortedDictionary<string, int>();
            bshist = new List<BoxScoreEntry>();

            var ofd = new OpenFileDialog
                          {
                              Filter = "NST Database (*.tst)|*.tst",
                              InitialDirectory = AppDocsPath,
                              Title = "Please select the TST file that you want to edit..."
                          };
            ofd.ShowDialog();

            if (ofd.FileName == "") return;

            SQLiteIO.LoadSeason(ofd.FileName, out tst, out tstopp, out pst, out TeamOrder, ref pt, ref bshist);
            //tst = getCustomStats("", ref teamOrder, ref curPT, ref bshist);

            txtFile.Text = ofd.FileName;

            updateStatus(String.Format("{0} teams & {1} players loaded successfully!", tst.Count, pst.Count));
            currentDB = txtFile.Text;
            //txtFile.Text = "SQLite";

            //MessageBox.Show(bshist.Count.ToString());
        }

        public static void ChangeSeason(int _curSeason, int maxSeason)
        {
            curSeason = _curSeason;
            mwInstance.cmbSeasonNum.SelectedItem = curSeason.ToString();
        }

        private void btnLoadUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (SQLiteIO.isTSTEmpty())
            {
                updateStatus("No file is loaded or the file currently loaded is empty");
                return;
            }

            bs = new BoxScore();
            var bsW = new BoxScoreWindow();
            bsW.ShowDialog();

            ParseBoxScoreResult();
        }

        private void ParseBoxScoreResult()
        {
            if (bs.done == false) return;

            int id1 = TeamOrder[bs.Team1];
            int id2 = TeamOrder[bs.Team2];

            SQLiteIO.LoadSeason(currentDB, out tst, out tstopp, out pst, out TeamOrder, ref pt, ref bshist,
                                _curSeason: bs.SeasonNum);

            List<PlayerBoxScore> list = pbsLists.SelectMany(pbsList => pbsList).ToList();

            if (!bs.doNotUpdate)
            {
                AddTeamStatsFromBoxScore(bs, ref tst, ref tstopp, id1, id2);

                foreach (PlayerBoxScore pbs in list)
                {
                    if (pbs.PlayerID == -1) continue;
                    pst[pbs.PlayerID].AddBoxScore(pbs);
                }
            }

            if (bs.bshistid == -1)
            {
                var bse = new BoxScoreEntry(bs, bs.gamedate, list);
                bshist.Add(bse);
            }
            else
            {
                bshist[bs.bshistid].bs = bs;
            }

            SQLiteIO.saveSeasonToDatabase(currentDB, tst, tstopp, pst, curSeason, SQLiteIO.getMaxSeason(currentDB));

            updateStatus(
                "One or more Box Scores have been added/updated. Database saved.");
        }

        public static void AddTeamStatsFromBoxScore(BoxScore bsToAdd, ref TeamStats ts1, ref TeamStats ts2)
        {
            var _tst = new Dictionary<int, TeamStats> {{1, ts1}, {2, ts2}};
            var _tstopp = new Dictionary<int, TeamStats> {{1, new TeamStats()}, {2, new TeamStats()}};
            AddTeamStatsFromBoxScore(bsToAdd, ref _tst, ref _tstopp, 1, 2);
            ts1 = _tst[1];
            ts2 = _tst[2];
        }

        public static void AddTeamStatsFromBoxScore(BoxScore bsToAdd, ref Dictionary<int, TeamStats> _tst,
                                                    ref Dictionary<int, TeamStats> _tstopp, int id1, int id2)
        {
            TeamStats ts1 = _tst[id1];
            TeamStats ts2 = _tst[id2];
            TeamStats tsopp1 = _tstopp[id1];
            TeamStats tsopp2 = _tstopp[id2];
            if (!bsToAdd.isPlayoff)
            {
                // Add win & loss
                if (bsToAdd.PTS1 > bsToAdd.PTS2)
                {
                    ts1.winloss[0]++;
                    ts2.winloss[1]++;
                }
                else
                {
                    ts1.winloss[1]++;
                    ts2.winloss[0]++;
                }
                // Add minutes played
                ts1.stats[t.MINS] += bsToAdd.MINS1;
                ts2.stats[t.MINS] += bsToAdd.MINS2;

                // Add Points For
                ts1.stats[t.PF] += bsToAdd.PTS1;
                ts2.stats[t.PF] += bsToAdd.PTS2;

                // Add Points Against
                ts1.stats[t.PA] += bsToAdd.PTS2;
                ts2.stats[t.PA] += bsToAdd.PTS1;

                //
                ts1.stats[t.FGM] += bsToAdd.FGM1;
                ts2.stats[t.FGM] += bsToAdd.FGM2;

                ts1.stats[t.FGA] += bsToAdd.FGA1;
                ts2.stats[t.FGA] += bsToAdd.FGA2;

                //
                ts1.stats[t.TPM] += bsToAdd.TPM1;
                ts2.stats[t.TPM] += bsToAdd.TPM2;

                //
                ts1.stats[t.TPA] += bsToAdd.TPA1;
                ts2.stats[t.TPA] += bsToAdd.TPA2;

                //
                ts1.stats[t.FTM] += bsToAdd.FTM1;
                ts2.stats[t.FTM] += bsToAdd.FTM2;

                //
                ts1.stats[t.FTA] += bsToAdd.FTA1;
                ts2.stats[t.FTA] += bsToAdd.FTA2;

                //
                ts1.stats[t.OREB] += bsToAdd.OREB1;
                ts2.stats[t.OREB] += bsToAdd.OREB2;

                //
                ts1.stats[t.DREB] += Convert.ToUInt16(bsToAdd.REB1 - bsToAdd.OREB1);
                ts2.stats[t.DREB] += Convert.ToUInt16(bsToAdd.REB2 - bsToAdd.OREB2);

                //
                ts1.stats[t.STL] += bsToAdd.STL1;
                ts2.stats[t.STL] += bsToAdd.STL2;

                //
                ts1.stats[t.TO] += bsToAdd.TO1;
                ts2.stats[t.TO] += bsToAdd.TO2;

                //
                ts1.stats[t.BLK] += bsToAdd.BLK1;
                ts2.stats[t.BLK] += bsToAdd.BLK2;

                //
                ts1.stats[t.AST] += bsToAdd.AST1;
                ts2.stats[t.AST] += bsToAdd.AST2;

                //
                ts1.stats[t.FOUL] += bsToAdd.FOUL1;
                ts2.stats[t.FOUL] += bsToAdd.FOUL2;


                // Opponents Team Stats
                // Add win & loss
                if (bsToAdd.PTS1 > bsToAdd.PTS2)
                {
                    tsopp2.winloss[0]++;
                    tsopp1.winloss[1]++;
                }
                else
                {
                    tsopp2.winloss[1]++;
                    tsopp1.winloss[0]++;
                }
                // Add minutes played
                tsopp2.stats[t.MINS] += bsToAdd.MINS1;
                tsopp1.stats[t.MINS] += bsToAdd.MINS2;

                // Add Points For
                tsopp2.stats[t.PF] += bsToAdd.PTS1;
                tsopp1.stats[t.PF] += bsToAdd.PTS2;

                // Add Points Against
                tsopp2.stats[t.PA] += bsToAdd.PTS2;
                tsopp1.stats[t.PA] += bsToAdd.PTS1;

                //
                tsopp2.stats[t.FGM] += bsToAdd.FGM1;
                tsopp1.stats[t.FGM] += bsToAdd.FGM2;

                tsopp2.stats[t.FGA] += bsToAdd.FGA1;
                tsopp1.stats[t.FGA] += bsToAdd.FGA2;

                //
                tsopp2.stats[t.TPM] += bsToAdd.TPM1;
                tsopp1.stats[t.TPM] += bsToAdd.TPM2;

                //
                tsopp2.stats[t.TPA] += bsToAdd.TPA1;
                tsopp1.stats[t.TPA] += bsToAdd.TPA2;

                //
                tsopp2.stats[t.FTM] += bsToAdd.FTM1;
                tsopp1.stats[t.FTM] += bsToAdd.FTM2;

                //
                tsopp2.stats[t.FTA] += bsToAdd.FTA1;
                tsopp1.stats[t.FTA] += bsToAdd.FTA2;

                //
                tsopp2.stats[t.OREB] += bsToAdd.OREB1;
                tsopp1.stats[t.OREB] += bsToAdd.OREB2;

                //
                tsopp2.stats[t.DREB] += Convert.ToUInt16(bsToAdd.REB1 - bsToAdd.OREB1);
                tsopp1.stats[t.DREB] += Convert.ToUInt16(bsToAdd.REB2 - bsToAdd.OREB2);

                //
                tsopp2.stats[t.STL] += bsToAdd.STL1;
                tsopp1.stats[t.STL] += bsToAdd.STL2;

                //
                tsopp2.stats[t.TO] += bsToAdd.TO1;
                tsopp1.stats[t.TO] += bsToAdd.TO2;

                //
                tsopp2.stats[t.BLK] += bsToAdd.BLK1;
                tsopp1.stats[t.BLK] += bsToAdd.BLK2;

                //
                tsopp2.stats[t.AST] += bsToAdd.AST1;
                tsopp1.stats[t.AST] += bsToAdd.AST2;

                //
                tsopp2.stats[t.FOUL] += bsToAdd.FOUL1;
                tsopp1.stats[t.FOUL] += bsToAdd.FOUL2;
            }
            else
            {
                // Add win & loss
                if (bsToAdd.PTS1 > bsToAdd.PTS2)
                {
                    ts1.pl_winloss[0]++;
                    ts2.pl_winloss[1]++;
                }
                else
                {
                    ts1.pl_winloss[1]++;
                    ts2.pl_winloss[0]++;
                }
                // Add minutes played
                ts1.pl_stats[t.MINS] += bsToAdd.MINS1;
                ts2.pl_stats[t.MINS] += bsToAdd.MINS2;

                // Add Points For
                ts1.pl_stats[t.PF] += bsToAdd.PTS1;
                ts2.pl_stats[t.PF] += bsToAdd.PTS2;

                // Add Points Against
                ts1.pl_stats[t.PA] += bsToAdd.PTS2;
                ts2.pl_stats[t.PA] += bsToAdd.PTS1;

                //
                ts1.pl_stats[t.FGM] += bsToAdd.FGM1;
                ts2.pl_stats[t.FGM] += bsToAdd.FGM2;

                ts1.pl_stats[t.FGA] += bsToAdd.FGA1;
                ts2.pl_stats[t.FGA] += bsToAdd.FGA2;

                //
                ts1.pl_stats[t.TPM] += bsToAdd.TPM1;
                ts2.pl_stats[t.TPM] += bsToAdd.TPM2;

                //
                ts1.pl_stats[t.TPA] += bsToAdd.TPA1;
                ts2.pl_stats[t.TPA] += bsToAdd.TPA2;

                //
                ts1.pl_stats[t.FTM] += bsToAdd.FTM1;
                ts2.pl_stats[t.FTM] += bsToAdd.FTM2;

                //
                ts1.pl_stats[t.FTA] += bsToAdd.FTA1;
                ts2.pl_stats[t.FTA] += bsToAdd.FTA2;

                //
                ts1.pl_stats[t.OREB] += bsToAdd.OREB1;
                ts2.pl_stats[t.OREB] += bsToAdd.OREB2;

                //
                ts1.pl_stats[t.DREB] += Convert.ToUInt16(bsToAdd.REB1 - bsToAdd.OREB1);
                ts2.pl_stats[t.DREB] += Convert.ToUInt16(bsToAdd.REB2 - bsToAdd.OREB2);

                //
                ts1.pl_stats[t.STL] += bsToAdd.STL1;
                ts2.pl_stats[t.STL] += bsToAdd.STL2;

                //
                ts1.pl_stats[t.TO] += bsToAdd.TO1;
                ts2.pl_stats[t.TO] += bsToAdd.TO2;

                //
                ts1.pl_stats[t.BLK] += bsToAdd.BLK1;
                ts2.pl_stats[t.BLK] += bsToAdd.BLK2;

                //
                ts1.pl_stats[t.AST] += bsToAdd.AST1;
                ts2.pl_stats[t.AST] += bsToAdd.AST2;

                //
                ts1.pl_stats[t.FOUL] += bsToAdd.FOUL1;
                ts2.pl_stats[t.FOUL] += bsToAdd.FOUL2;


                // Opponents Team Stats
                // Add win & loss
                if (bsToAdd.PTS1 > bsToAdd.PTS2)
                {
                    tsopp2.pl_winloss[0]++;
                    tsopp1.pl_winloss[1]++;
                }
                else
                {
                    tsopp2.pl_winloss[1]++;
                    tsopp1.pl_winloss[0]++;
                }
                // Add minutes played
                tsopp2.pl_stats[t.MINS] += bsToAdd.MINS1;
                tsopp1.pl_stats[t.MINS] += bsToAdd.MINS2;

                // Add Points For
                tsopp2.pl_stats[t.PF] += bsToAdd.PTS1;
                tsopp1.pl_stats[t.PF] += bsToAdd.PTS2;

                // Add Points Against
                tsopp2.pl_stats[t.PA] += bsToAdd.PTS2;
                tsopp1.pl_stats[t.PA] += bsToAdd.PTS1;

                //
                tsopp2.pl_stats[t.FGM] += bsToAdd.FGM1;
                tsopp1.pl_stats[t.FGM] += bsToAdd.FGM2;

                tsopp2.pl_stats[t.FGA] += bsToAdd.FGA1;
                tsopp1.pl_stats[t.FGA] += bsToAdd.FGA2;

                //
                tsopp2.pl_stats[t.TPM] += bsToAdd.TPM1;
                tsopp1.pl_stats[t.TPM] += bsToAdd.TPM2;

                //
                tsopp2.pl_stats[t.TPA] += bsToAdd.TPA1;
                tsopp1.pl_stats[t.TPA] += bsToAdd.TPA2;

                //
                tsopp2.pl_stats[t.FTM] += bsToAdd.FTM1;
                tsopp1.pl_stats[t.FTM] += bsToAdd.FTM2;

                //
                tsopp2.pl_stats[t.FTA] += bsToAdd.FTA1;
                tsopp1.pl_stats[t.FTA] += bsToAdd.FTA2;

                //
                tsopp2.pl_stats[t.OREB] += bsToAdd.OREB1;
                tsopp1.pl_stats[t.OREB] += bsToAdd.OREB2;

                //
                tsopp2.pl_stats[t.DREB] += Convert.ToUInt16(bsToAdd.REB1 - bsToAdd.OREB1);
                tsopp1.pl_stats[t.DREB] += Convert.ToUInt16(bsToAdd.REB2 - bsToAdd.OREB2);

                //
                tsopp2.pl_stats[t.STL] += bsToAdd.STL1;
                tsopp1.pl_stats[t.STL] += bsToAdd.STL2;

                //
                tsopp2.pl_stats[t.TO] += bsToAdd.TO1;
                tsopp1.pl_stats[t.TO] += bsToAdd.TO2;

                //
                tsopp2.pl_stats[t.BLK] += bsToAdd.BLK1;
                tsopp1.pl_stats[t.BLK] += bsToAdd.BLK2;

                //
                tsopp2.pl_stats[t.AST] += bsToAdd.AST1;
                tsopp1.pl_stats[t.AST] += bsToAdd.AST2;

                //
                tsopp2.pl_stats[t.FOUL] += bsToAdd.FOUL1;
                tsopp1.pl_stats[t.FOUL] += bsToAdd.FOUL2;
            }

            ts1.calcAvg();
            ts2.calcAvg();
            tsopp1.calcAvg();
            tsopp2.calcAvg();

            _tst[id1] = ts1;
            _tst[id2] = ts2;
            _tstopp[id1] = tsopp1;
            _tstopp[id2] = tsopp2;
        }

        public static void CheckForUpdates(bool showMessage = false)
        {
            showUpdateMessage = showMessage;
            try
            {
                var webClient = new WebClient();
                if (!showMessage)
                {
                    webClient.DownloadFileCompleted += Completed;
                    webClient.DownloadFileAsync(new Uri("http://students.ceid.upatras.gr/~aslanoglou/nstversion.txt"),
                                                AppDocsPath + @"nstversion.txt");
                }
                else
                {
                    webClient.DownloadFile(new Uri("http://students.ceid.upatras.gr/~aslanoglou/nstversion.txt"),
                                           AppDocsPath + @"nstversion.txt");
                    Completed(null, null);
                }
            }
            catch
            {
            }
        }

        /*
        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
        }
        */

        private static void Completed(object sender, AsyncCompletedEventArgs e)
        {
            string[] updateInfo;
            string[] versionParts;
            try
            {
                updateInfo = File.ReadAllLines(AppDocsPath + @"nstversion.txt");
                versionParts = updateInfo[0].Split('.');
            }
            catch
            {
                return;
            }
            string[] curVersionParts = Assembly.GetExecutingAssembly().GetName().Version.ToString().Split('.');
            var iVP = new int[versionParts.Length];
            var iCVP = new int[versionParts.Length];
            for (int i = 0; i < versionParts.Length; i++)
            {
                iVP[i] = Convert.ToInt32(versionParts[i]);
                iCVP[i] = Convert.ToInt32(curVersionParts[i]);
                if (iCVP[i] > iVP[i]) break;
                if (iVP[i] > iCVP[i])
                {
                    string changelog = "\n\nVersion " + String.Join(".", versionParts);
                    try
                    {
                        for (int j = 2; j < updateInfo.Length; j++)
                        {
                            changelog += "\n" + updateInfo[j];
                        }
                    }
                    catch
                    {
                    }
                    MessageBoxResult mbr = MessageBox.Show(
                        "A new version is available! Would you like to download it?" + changelog, "NBA Stats Tracker",
                        MessageBoxButton.YesNo, MessageBoxImage.Information);
                    if (mbr == MessageBoxResult.Yes)
                    {
                        Process.Start(updateInfo[1]);
                        break;
                    }
                    return;
                }
            }
            if (showUpdateMessage) MessageBox.Show("No updates found!");
        }

        private void btnEraseSettings_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog
                          {
                              Title = "Please select the Career file you want to reset the settings for...",
                              Filter =
                                  "All NBA 2K12 Career Files (*.FXG; *.CMG; *.RFG; *.PMG; *.SMG)|*.FXG;*.CMG;*.RFG;*.PMG;*.SMG|"
                                  +
                                  "Association files (*.FXG)|*.FXG|My Player files (*.CMG)|*.CMG|Season files (*.RFG)|*.RFG|Playoff files (*.PMG)|*.PMG|" +
                                  "Create A Legend files (*.SMG)|*.SMG"
                          };
            if (Directory.Exists(SavesPath)) ofd.InitialDirectory = SavesPath;
            ofd.ShowDialog();
            if (ofd.FileName == "") return;

            string safefn = Tools.getSafeFilename(ofd.FileName);
            string SettingsFile = AppDocsPath + safefn + ".cfg";
            if (File.Exists(SettingsFile)) File.Delete(SettingsFile);
            MessageBox.Show(
                "Settings for this file have been erased. You'll be asked to set them again next time you import it.");
        }

        private void btnLeagueTSV_Click(object sender, RoutedEventArgs e)
        {
            const string header1 =
                "\tTeam\tGP\tW\tL\tPF\tPA\tFGM\tFGA\t3PM\t3PA\tFTM\tFTA\tOREB\tDREB\tSTL\tTO\tBLK\tAST\tFOUL\t";
            //string header2 = "Team\tW%\tWeff\tPPG\tPAPG\tFG%\tFGeff\t3P%\t3Peff\tFT%\tFTeff\tRPG\tORPG\tDRPG\tSPG\tBPG\tTPG\tAPG\tFPG";
            const string header2 =
                "W%\tWeff\tPPG\tPAPG\tFG%\tFGeff\t3P%\t3Peff\tFT%\tFTeff\tRPG\tORPG\tDRPG\tSPG\tBPG\tTPG\tAPG\tFPG";
            /*
            string data = String.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17}", 
                tst[id].getGames(), tst[id].winloss[0], tst[id].winloss[1], tst[id].stats[t.FGM], tst[id].stats[t.FGA], tst[id].stats[t.TPM], tst[id].stats[t.TPA],
                tst[id].stats[t.FTM], tst[id].stats[t.FTA], tst[
             */
            string data1 = "";
            for (int id = 0; id < 30; id++)
            {
                if (tst[id].name == "") continue;

                data1 += (id + 1).ToString() + "\t";
                foreach (var kvp in TeamOrder)
                {
                    if (kvp.Value == id)
                    {
                        data1 += kvp.Key + "\t";
                        break;
                    }
                }
                data1 += String.Format("{0}\t{1}\t{2}", tst[id].getGames(), tst[id].winloss[0], tst[id].winloss[1]);
                for (int j = 1; j <= 16; j++)
                {
                    if (j != 3)
                    {
                        data1 += "\t" + tst[id].stats[j].ToString();
                    }
                }
                data1 += "\t";
                data1 += String.Format("{0:F3}", tst[id].averages[t.Wp]) + "\t" +
                         String.Format("{0:F1}", tst[id].averages[t.Weff]);
                for (int j = 0; j <= 15; j++)
                {
                    switch (j)
                    {
                        case 2:
                        case 4:
                        case 6:
                            data1 += String.Format("\t{0:F3}", tst[id].averages[j]);
                            break;
                        default:
                            data1 += String.Format("\t{0:F1}", tst[id].averages[j]);
                            break;
                    }
                }
                data1 += "\n";
            }

            var sfd = new SaveFileDialog
                          {
                              Filter = "Tab-Separated Values file (*.tsv)|*.tsv",
                              InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                              Title = "Export To TSV"
                          };
            sfd.ShowDialog();
            if (sfd.FileName == "") return;

            var sw = new StreamWriter(sfd.FileName);
            sw.WriteLine(header1 + header2);
            sw.WriteLine(data1);
            sw.Close();
        }

        private void mnuExit_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(-1);
        }

        private void btnInject2K12_Click(object sender, RoutedEventArgs e)
        {
            if (mnuOptionsImportOld.IsChecked)
            {
                var ofd = new OpenFileDialog
                              {
                                  Title = "Please select the Career file you want to update...",
                                  Filter =
                                      "All NBA 2K12 Career Files (*.FXG; *.CMG; *.RFG; *.PMG; *.SMG)|*.FXG;*.CMG;*.RFG;*.PMG;*.SMG|"
                                      +
                                      "Association files (*.FXG)|*.FXG|My Player files (*.CMG)|*.CMG|Season files (*.RFG)|*.RFG|Playoff files (*.PMG)|*.PMG|" +
                                      "Create A Legend files (*.SMG)|*.SMG"
                              };
                if (Directory.Exists(SavesPath)) ofd.InitialDirectory = SavesPath;
                ofd.ShowDialog();

                if (ofd.FileName == "") return;
                string fn = ofd.FileName;

                Interop2K12.prepareOffsets(fn, tst, ref TeamOrder, ref pt);

                Dictionary<int, TeamStats> temp;
                var tempopp = new Dictionary<int, TeamStats>();

                Interop2K12.GetStatsFrom2K12Save(fn, out temp, ref tempopp, ref TeamOrder, ref pt);
                if (temp.Count == 1)
                {
                    MessageBox.Show("Couldn't get stats from " + Tools.getSafeFilename(fn) + ". Update failed.");
                    return;
                }
                bool incompatible = false;

                if (temp.Count != tst.Count) incompatible = true;
                else
                {
                    for (int i = 0; i < temp.Count; i++)
                    {
                        if (temp[i].name != tst[i].name)
                        {
                            incompatible = true;
                            break;
                        }

                        if ((!temp[i].winloss.SequenceEqual(tst[i].winloss)) ||
                            (!temp[i].pl_winloss.SequenceEqual(tst[i].pl_winloss)))
                        {
                            incompatible = true;
                            break;
                        }
                    }
                }

                if (incompatible)
                {
                    MessageBoxResult r =
                        MessageBox.Show(
                            "The file currently loaded seems incompatible with the NBA 2K save you're trying to save into." +
                            "\nThis could be happening for a number of reasons:\n\n" +
                            "1. The file currently loaded isn't one that had stats imported to it from your 2K save.\n" +
                            "2. The Win/Loss record for one or more teams would be different after this procedure.\n\n" +
                            "If you're updating using a box score, then either you're not using the NST database you imported your stats\n" +
                            "into before the game, or you entered the box score incorrectly. Remember that you need to import your stats\n" +
                            "into a database right before the game starts, let the game end and save the Association, and then update the\n" +
                            "database using the box score. If you follow these steps correctly, you shouldn't get this message when you try\n" +
                            "to export the stats from the database to your 2K save.\n\n" +
                            "Are you sure you want to continue? SAVE CORRUPTION MAY OCCUR, AND I WON'T BE HELD LIABLE FOR IT. ALWAYS KEEP BACKUPS.",
                            "NBA Stats Tracker", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    if (r == MessageBoxResult.No) return;
                }


                Interop2K12.updateSavegame(fn, tst, TeamOrder, pt);
                updateStatus("Injected custom Team Stats into " + Tools.getSafeFilename(fn) + " successfully!");
            }
            else
            {
                var fbd = new FolderBrowserDialog
                              {
                                  Description = "Select folder with REditor-exported CSVs",
                                  ShowNewFolderButton = false
                              };
                DialogResult dr = fbd.ShowDialog(this.GetIWin32Window());

                if (dr != System.Windows.Forms.DialogResult.OK) return;

                if (fbd.SelectedPath == "") return;

                if (mnuOptionsCompatibilityCheck.IsChecked)
                {
                    var temptst = new Dictionary<int, TeamStats>();
                    var temptstopp = new Dictionary<int, TeamStats>();
                    var temppst = new Dictionary<int, PlayerStats>();
                    int result = InteropREditor.ImportAll(ref temptst, ref temptstopp, ref TeamOrder, ref temppst,
                                                          fbd.SelectedPath, true);

                    if (result != 0)
                    {
                        MessageBox.Show("Export failed.");
                        return;
                    }

                    bool incompatible = false;

                    if (temptst.Count != tst.Count) incompatible = true;
                    else
                    {
                        for (int i = 0; i < temptst.Count; i++)
                        {
                            if (temptst[i].name != tst[i].name)
                            {
                                incompatible = true;
                                break;
                            }

                            if ((!temptst[i].winloss.SequenceEqual(tst[i].winloss)) ||
                                (!temptst[i].pl_winloss.SequenceEqual(tst[i].pl_winloss)))
                            {
                                incompatible = true;
                                break;
                            }
                        }
                    }

                    if (incompatible)
                    {
                        MessageBoxResult r =
                            MessageBox.Show(
                                "The file currently loaded seems incompatible with the NBA 2K save you're trying to save into." +
                                "\nThis could be happening for a number of reasons:\n\n" +
                                "1. The file currently loaded isn't one that had stats imported to it from your 2K save.\n" +
                                "2. The Win/Loss record for one or more teams would be different after this procedure.\n\n" +
                                "If you're updating using a box score, then either you're not using the NST database you imported your stats\n" +
                                "into before the game, or you entered the box score incorrectly. Remember that you need to import your stats\n" +
                                "into a database right before the game starts, let the game end and save the Association, and then update the\n" +
                                "database using the box score. If you follow these steps correctly, you shouldn't get this message when you try\n" +
                                "to export the stats from the database to your 2K save.\n\n" +
                                "Are you sure you want to continue? SAVE CORRUPTION MAY OCCUR, AND I WON'T BE HELD LIABLE FOR IT. ALWAYS KEEP BACKUPS.",
                                "NBA Stats Tracker", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                        if (r == MessageBoxResult.No) return;
                    }
                }

                int eresult = InteropREditor.ExportAll(tst, tstopp, pst, fbd.SelectedPath,
                                                       mnuOptionsExportTeamsOnly.IsChecked);

                if (eresult != 0)
                {
                    MessageBox.Show("Export failed.");
                    return;
                }
                updateStatus("Injected at " + fbd.SelectedPath + " successfully!");
            }
        }

        private void mnuHelpReadme_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(AppPath + @"\readme.txt");
        }

        private void mnuHelpAbout_Click(object sender, RoutedEventArgs e)
        {
            var aw = new AboutWindow();
            aw.ShowDialog();
        }

        private void mnuFileGetRealStats_Click(object sender, RoutedEventArgs e)
        {
            string file = "";

            if (!String.IsNullOrWhiteSpace(txtFile.Text))
            {
                MessageBoxResult r =
                    MessageBox.Show(
                        "This will overwrite the stats for the current season. Are you sure?\n\nClick Yes to overwrite.\nClick No to create a new file automatically. Any unsaved changes to the current file will be lost.\nClick Cancel to return to the main window.",
                        "NBA Stats Tracker", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (r == MessageBoxResult.Yes) file = currentDB;
                else if (r == MessageBoxResult.No) txtFile.Text = "";
                else return;
            }

            if (String.IsNullOrWhiteSpace(txtFile.Text))
            {
                file = AppDocsPath + "Real NBA Stats " + DateTime.Now.Year + "-" + DateTime.Now.Month + "-" +
                       DateTime.Now.Day + ".tst";
                if (File.Exists(file))
                {
                    if (App.realNBAonly)
                    {
                        MessageBoxResult r =
                            MessageBox.Show(
                                "Today's Real NBA Stats have already been downloaded and saved. Are you sure you want to re-download them?",
                                "NBA Stats Tracker", MessageBoxButton.YesNo, MessageBoxImage.Question,
                                MessageBoxResult.No);
                        if (r == MessageBoxResult.No)
                            return;
                    }
                    else
                    {
                        MessageBoxResult r =
                            MessageBox.Show(
                                "Today's Real NBA Stats have already been downloaded and saved. Are you sure you want to re-download them?",
                                "NBA Stats Tracker", MessageBoxButton.YesNo, MessageBoxImage.Question,
                                MessageBoxResult.No);
                        if (r == MessageBoxResult.No)
                        {
                            SQLiteIO.LoadSeason(file, out tst, out tstopp, out pst, out TeamOrder, ref pt, ref bshist);
                            txtFile.Text = file;
                            return;
                        }
                    }
                }
            }

            //var grsw = new getRealStatsW();
            //grsw.ShowDialog();
            TeamOrder = Interop2K12.setTeamOrder("Mode 0");

            var realtstopp = new Dictionary<int, TeamStats>();
            var realpst = new Dictionary<int, PlayerStats>();
            sem = new Semaphore(1, 1);

            mainGrid.Visibility = Visibility.Hidden;
            txbWait.Visibility = Visibility.Visible;

            progress = 0;

            //MessageBox.Show("Please wait after pressing OK, this could take a few minutes.");

            var TeamNamesShort = new Dictionary<string, string>
                                     {
                                         {"76ers", "PHI"},
                                         {"Bobcats", "CHA"},
                                         {"Bucks", "MIL"},
                                         {"Bulls", "CHI"},
                                         {"Cavaliers", "CLE"},
                                         {"Celtics", "BOS"},
                                         {"Clippers", "LAC"},
                                         {"Grizzlies", "MEM"},
                                         {"Hawks", "ATL"},
                                         {"Heat", "MIA"},
                                         {"Hornets", "NOH"},
                                         {"Jazz", "UTA"},
                                         {"Kings", "SAC"},
                                         {"Knicks", "NYK"},
                                         {"Lakers", "LAL"},
                                         {"Magic", "ORL"},
                                         {"Mavericks", "DAL"},
                                         {"Nets", "NJN"},
                                         {"Nuggets", "DEN"},
                                         {"Pacers", "IND"},
                                         {"Pistons", "DET"},
                                         {"Raptors", "TOR"},
                                         {"Rockets", "HOU"},
                                         {"Spurs", "SAS"},
                                         {"Suns", "PHO"},
                                         {"Thunder", "OKC"},
                                         {"Timberwolves", "MIN"},
                                         {"Trail Blazers", "POR"},
                                         {"Warriors", "GSW"},
                                         {"Wizards", "WAS"}
                                     };

            worker1 = new BackgroundWorker {WorkerReportsProgress = true, WorkerSupportsCancellation = true};

            worker1.DoWork += delegate
                                  {
                                      foreach (var kvp in TeamNamesShort)
                                      {
                                          Dictionary<int, PlayerStats> temppst;
                                          TeamStats realts;
                                          TeamStats realtsopp;
                                          InteropBR.ImportRealStats(kvp, out realts,
                                                                    out realtsopp, out temppst);
                                          realtst[TeamOrder[kvp.Key]] = realts;
                                          realtst[TeamOrder[kvp.Key]].ID = TeamOrder[kvp.Key];
                                          realtstopp[TeamOrder[kvp.Key]] = realtsopp;
                                          realtstopp[TeamOrder[kvp.Key]].ID = TeamOrder[kvp.Key];
                                          foreach (var kvp2 in temppst)
                                          {
                                              kvp2.Value.ID = realpst.Count;
                                              realpst.Add(realpst.Count, kvp2.Value);
                                          }
                                          worker1.ReportProgress(1);
                                      }
                                      InteropBR.AddPlayoffTeamStats(ref realtst, ref realtstopp);
                                  };

            worker1.ProgressChanged += delegate
                                           {
                                               sem.WaitOne();
                                               updateProgressBar();
                                               sem.Release();
                                           };

            worker1.RunWorkerCompleted += delegate
                                              {
                                                  if (realtst[0].name != "Canceled")
                                                  {
                                                      int len = realtst.Count;

                                                      tst = new Dictionary<int, TeamStats>();
                                                      tstopp = new Dictionary<int, TeamStats>();
                                                      for (int i = 0; i < len; i++)
                                                      {
                                                          foreach (var kvp in TeamOrder)
                                                          {
                                                              if (kvp.Value == i)
                                                              {
                                                                  tst[i] = new TeamStats(kvp.Key);
                                                                  tstopp[i] = new TeamStats(kvp.Key);
                                                                  break;
                                                              }
                                                          }
                                                      }

                                                      tst = realtst;
                                                      tstopp = realtstopp;
                                                      pst = realpst;
                                                      SQLiteIO.saveSeasonToDatabase(file, tst, tstopp, pst, curSeason,
                                                                                    SQLiteIO.getMaxSeason(file));
                                                      txtFile.Text = file;
                                                      SQLiteIO.LoadSeason(file, out tst, out tstopp, out pst,
                                                                          out TeamOrder,
                                                                          ref pt, ref bshist, _curSeason: curSeason);

                                                      txbWait.Visibility = Visibility.Hidden;
                                                      mainGrid.Visibility = Visibility.Visible;

                                                      updateStatus("The download of real NBA stats is done.");
                                                  }
                                              };

            worker1.RunWorkerAsync();
        }

        private void updateProgressBar()
        {
            progress += (double) 100/30;
            var percentage = (int) progress;
            if (percentage < 97)
            {
                status.Content = "Downloading NBA stats from Basketball-Reference.com (" + percentage + "% complete)...";
            }
            else
            {
                status.Content = "Season stats downloaded, now downloading playoff stats and saving...";
            }
        }

        // TODO: Implement Compare to Real again sometime
        private void btnCompareToReal_Click(object sender, RoutedEventArgs e)
        {
            /*
            var realteam = new TeamStats();

            if (File.Exists(AppDocsPath + cmbTeam1.SelectedItem + ".rst"))
            {
                var fi = new FileInfo(AppDocsPath + cmbTeam1.SelectedItem + ".rst");
                TimeSpan sinceLastModified = DateTime.Now - fi.LastWriteTime;
                if (sinceLastModified.Days >= 1)
                    realteam = Helper.getRealStats(cmbTeam1.SelectedItem.ToString());
                else
                    try
                    {
                        realteam = Helper.getRealStats(cmbTeam1.SelectedItem.ToString(), true);
                    }
                    catch
                    {
                        try
                        {
                            realteam = Helper.getRealStats(cmbTeam1.SelectedItem.ToString());
                        }
                        catch
                        {
                            MessageBox.Show(
                                "An incomplete real stats file is present and locked in the disk. Please restart NBA Stats Tracker and try again.");
                        }
                    }
            }
            else
            {
                realteam = Helper.getRealStats(cmbTeam1.SelectedItem.ToString());
            }
            TeamStats curteam = tst[teamOrder[cmbTeam1.SelectedItem.ToString()]];

            var vw = new VersusWindow(curteam, "Current", realteam, "Real");
            vw.ShowDialog();
            */
        }

        // TODO: Implement Compare to Other file again sometime
        private void btnCompareOtherFile_Click(object sender, RoutedEventArgs e)
        {
            /*
            var ofd = new OpenFileDialog
                          {
                              Title = "Select the TST file that has the team stats you want to compare to...",
                              Filter = "Team Stats files (*.tst)|*.tst",
                              InitialDirectory = AppDocsPath
                          };
            ofd.ShowDialog();

            string file = ofd.FileName;
            if (file != "")
            {
                string team = cmbTeam1.SelectedItem.ToString();
                var _newTeamOrder = new SortedDictionary<string, int>();

                FileStream stream = File.Open(ofd.FileName, FileMode.Open);
                var bf = new BinaryFormatter {AssemblyFormat = FormatterAssemblyStyle.Simple};

                var _newtst = new TeamStats[30];
                for (int i = 0; i < 30; i++)
                {
                    _newtst[i] = new TeamStats();
                    _newtst[i] = (TeamStats) bf.Deserialize(stream);
                    if (_newtst[i].name == "") continue;
                    try
                    {
                        _newTeamOrder.Add(_newtst[i].name, i);
                        _newtst[i].calcAvg();
                    }
                    catch
                    {
                    }
                }

                TeamStats newteam = _newtst[_newTeamOrder[team]];
                TeamStats curteam = tst[TeamOrder[team]];

                var vw = new VersusWindow(curteam, "Current", newteam, "Other");
                vw.ShowDialog();
            }
            */
        }

        private void txtFile_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(txtFile.Text)) return;

            txtFile.ScrollToHorizontalOffset(txtFile.GetRectFromCharacterIndex(txtFile.Text.Length).Right);
            currentDB = txtFile.Text;
            PopulateSeasonCombo();
            db = new SQLiteDatabase(currentDB);
        }

        private void PopulateSeasonCombo()
        {
            db = new SQLiteDatabase(currentDB);

            const string qr = "SELECT * FROM SeasonNames ORDER BY ID DESC";
            DataTable dataTable = db.GetDataTable(qr);
            SeasonList.Clear();
            foreach (DataRow row in dataTable.Rows)
            {
                int id = Tools.getInt(row, "ID");
                string name = Tools.getString(row, "Name");
                SeasonList.Add(new KeyValuePair<int, string>(id, name));
            }

            cmbSeasonNum.ItemsSource = SeasonList;

            cmbSeasonNum.SelectedValue = curSeason;
        }

        private void cmbSeasonNum_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbSeasonNum.SelectedIndex == -1) return;

            curSeason = ((KeyValuePair<int, string>) (((cmbSeasonNum)).SelectedItem)).Key;
            if (!loadingSeason) SQLiteIO.LoadSeason();
        }

        // TODO: Implement Trends again sometime
        private void btnTrends_Click(object sender, RoutedEventArgs e)
        {
            var ofd1 = new OpenFileDialog();
            if (txtFile.Text == "")
            {
                ofd1.Title = "Select the TST file that has the current team stats...";
                ofd1.Filter = "Team Stats files (*.tst)|*.tst";
                ofd1.InitialDirectory = AppDocsPath;
                ofd1.ShowDialog();

                if (ofd1.FileName == "") return;

                SQLiteIO.LoadSeason(ofd1.FileName, out tst, out tstopp, out pst, out TeamOrder, ref pt, ref bshist);
                //cmbTeam1.SelectedIndex = 0;
            }

            var ofd = new OpenFileDialog
                          {
                              Title = "Select the TST file that has the team stats you want to compare to...",
                              Filter = "Team Stats files (*.tst)|*.tst",
                              InitialDirectory = AppDocsPath
                          };
            ofd.ShowDialog();

            if (ofd.FileName == "") return;

            //string team = cmbTeam1.SelectedItem.ToString();

            Dictionary<int, TeamStats> curTST = tst;

            SortedDictionary<string, int> oldTeamOrder;
            var oldPT = new PlayoffTree();
            IList<BoxScoreEntry> oldbshist = new List<BoxScoreEntry>();
            Dictionary<int, TeamStats> oldTST;
            Dictionary<int, TeamStats> oldTSTopp;
            SQLiteIO.LoadSeason(ofd.FileName, out oldTST, out oldTSTopp, out pst, out oldTeamOrder, ref oldPT,
                                ref oldbshist,
                                false);

            var curR = new TeamRankings(tst);
            var oldR = new TeamRankings(oldTST);
            int[][] diffrnk = calculateDifferenceRanking(curR, oldR);
            float[][] diffavg = calculateDifferenceAverage(curTST, oldTST);

            int maxi = 0;
            int mini = 0;
            for (int i = 1; i < 30; i++)
            {
                if (diffavg[i][0] > diffavg[maxi][0])
                    maxi = i;
                if (diffavg[i][0] < diffavg[mini][0])
                    mini = i;
            }

            string team1 = tst[maxi].name;
            string str = String.Format(
                diffrnk[maxi][0] > 0
                    ? "Most improved in {7}, the {0}. They were #{1} ({4:F1}), climbing {3} places they are now at #{2} ({5:F1}), a {6:F1} {7} difference!"
                    : "Most improved in {7}, the {0}. They were #{1} ({4:F1}) and they are now at #{2} ({5:F1}), a {6:F1} {7} difference!",
                tst[maxi].name, oldR.rankings[maxi][0], curR.rankings[maxi][0], diffrnk[maxi][0],
                oldTST[maxi].averages[0], tst[maxi].averages[0], diffavg[maxi][0], "PPG");
            str += " ";
            str +=
                String.Format(
                    "Taking this improvement apart, their FG% went from {0:F3} to {1:F3}, 3P% was {2:F3} and now is {3:F3}, and FT% is now at {4:F3}, having been at {5:F3}.",
                    oldTST[maxi].averages[t.FGp], tst[maxi].averages[t.FGp], oldTST[maxi].averages[t.TPp],
                    tst[maxi].averages[t.TPp], tst[maxi].averages[t.FTp], oldTST[maxi].averages[t.FTp]);

            if (curR.rankings[maxi][t.FGeff] <= 5)
            {
                str += " ";
                if (oldR.rankings[maxi][t.FGeff] > 20)
                    str +=
                        "Huge leap in Field Goal efficiency. Back then they were on of the worst teams on the offensive end, now in the Top 5.";
                else if (oldR.rankings[maxi][t.FGeff] > 10)
                    str +=
                        "An average offensive team turned great. From the middle of the pack, they are now in Top 5 in Field Goal efficiency.";
                else if (oldR.rankings[maxi][t.FGeff] > 5)
                    str +=
                        "They were already hot, and they're just getting better. Moving on up from Top 10 in FGeff, to Top 5.";
                else
                    str +=
                        "They just know how to stay hot at the offensive end. Still in the Top 5 of the most efficient teams from the floor.";
            }
            if (curR.rankings[maxi][t.FTeff] <= 5)
                str +=
                    " They're not afraid of contact, and they know how to make the most from the line. Top 5 in Free Throw efficiency.";
            if (diffavg[maxi][t.APG] > 0)
                str +=
                    String.Format(
                        " They are getting better at finding the open man with a timely pass. {0:F1} improvement in assists per game.",
                        diffavg[maxi][t.APG]);
            if (diffavg[maxi][t.RPG] > 0) str += String.Format(" Their additional rebounds have helped as well.");
            if (diffavg[maxi][t.TPG] < 0)
                str += String.Format(" Also taking better care of the ball, making {0:F1} less turnovers per game.",
                                     -diffavg[maxi][t.TPG]);

            ///////////////////////////
            str += "$";
            ///////////////////////////

            string team2 = tst[mini].name;
            if (diffrnk[mini][0] < 0)
            {
                str +=
                    String.Format(
                        "On the other end, the {0} have lost some of their scoring power. They were #{1} ({4:F1}), dropping {3} places they are now at #{2} ({5:F1}).",
                        tst[mini].name, oldR.rankings[mini][0], curR.rankings[mini][0], -diffrnk[mini][0],
                        oldTST[mini].averages[0], tst[mini].averages[0]);
            }
            else
            {
                str +=
                    String.Format(
                        "On the other end, the {0} have lost some of their scoring power. They were #{1} ({4:F1}) and are now in #{2} ({5:F1}). Guess even that {6:F1} PPG drop wasn't enough to knock them down!",
                        tst[mini].name, oldR.rankings[mini][0], curR.rankings[mini][0], -diffrnk[mini][0],
                        oldTST[mini].averages[0], tst[mini].averages[0], -diffavg[mini][0]);
            }
            str += " ";
            str +=
                String.Format(
                    "So why has this happened? Their FG% went from {0:F3} to {1:F3}, 3P% was {2:F3} and now is {3:F3}, and FT% is now at {4:F3}, having been at {5:F3}.",
                    oldTST[mini].averages[t.FGp], tst[mini].averages[t.FGp], oldTST[mini].averages[t.TPp],
                    tst[mini].averages[t.TPp], tst[mini].averages[t.FTp], oldTST[mini].averages[t.FTp]);
            if (diffavg[mini][t.TPG] > 0)
                str +=
                    String.Format(
                        " You can't score as many points when you commit turnovers; they've seen them increase by {0:F1} per game.",
                        diffavg[mini][t.TPG]);

            var tw = new TrendsWindow(str, team1, team2);
            tw.ShowDialog();
        }

        private int[][] calculateDifferenceRanking(TeamRankings curR, TeamRankings newR)
        {
            var diff = new int[30][];
            for (int i = 0; i < 30; i++)
            {
                diff[i] = new int[18];
                for (int j = 0; j < 18; j++)
                {
                    diff[i][j] = newR.rankings[i][j] - curR.rankings[i][j];
                }
            }
            return diff;
        }

        private float[][] calculateDifferenceAverage(Dictionary<int, TeamStats> curTST,
                                                     Dictionary<int, TeamStats> oldTST)
        {
            var diff = new float[30][];
            for (int i = 0; i < 30; i++)
            {
                diff[i] = new float[18];
                for (int j = 0; j < 18; j++)
                {
                    diff[i][j] = curTST[i].averages[j] - oldTST[i].averages[j];
                }
            }
            return diff;
        }

        private void btnTest_Click(object sender, RoutedEventArgs e)
        {
            /*
            DataSet ds = RealStats.GetPlayoffTeamStats(@"http://www.basketball-reference.com/playoffs/NBA_2012.html");

            TestWindow tw = new TestWindow(ds);
            tw.ShowDialog();
            */
            var lbsw = new LiveBoxScoreWindow();
            lbsw.ShowDialog();
        }

        private void mnuHistoryBoxScores_Click(object sender, RoutedEventArgs e)
        {
            if (SQLiteIO.isTSTEmpty()) return;

            bs = new BoxScore();
            var bsw = new BoxScoreWindow(BoxScoreWindow.Mode.View);
            bsw.ShowDialog();

            UpdateBoxScore();
        }

        public static void UpdateBoxScore()
        {
            if (bs.bshistid != -1)
            {
                if (bs.done)
                {
                    List<PlayerBoxScore> list = pbsLists.SelectMany(pbsList => pbsList).ToList();

                    bshist[bs.bshistid].bs = bs;
                    bshist[bs.bshistid].pbsList = list;
                    bshist[bs.bshistid].date = bs.gamedate;
                    bshist[bs.bshistid].mustUpdate = true;
                }
            }
        }

        private void btnTeamOverview_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(currentDB)) return;
            if (SQLiteIO.isTSTEmpty())
            {
                MessageBox.Show("You need to create a team or import stats before using the Analysis features.");
                return;
            }

            var tow = new TeamOverviewWindow();
            tow.ShowDialog();
        }

        private void btnOpenCustom_Click(object sender, RoutedEventArgs e)
        {
            mnuFileOpenCustom_Click(null, null);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 3);
            //dispatcherTimer.Start();
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            status.FontWeight = FontWeights.Normal;
            status.Content = "Ready";
            dispatcherTimer.Stop();
        }

        private void updateStatus(string newStatus)
        {
            dispatcherTimer.Stop();
            status.FontWeight = FontWeights.Bold;
            status.Content = newStatus;
            dispatcherTimer.Start();
        }

        private void btnSaveTeamStats_Click(object sender, RoutedEventArgs e)
        {
            if (!isCustom)
            {
                mnuFileSaveAs_Click(null, null);
            }
            else
            {
                SQLiteIO.saveSeasonToDatabase(currentDB, tst, tstopp, pst, curSeason, SQLiteIO.getMaxSeason(currentDB));
                txtFile.Text = currentDB;
                mwInstance.updateStatus("File saved successfully. Season " + curSeason.ToString() + " updated.");
            }
        }

        private void btnLeagueOverview_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(currentDB)) return;
            /*
            if (!isCustom)
            {
                MessageBox.Show("Save the data into a Team Stats file before using the tool's features.");
                return;
            }
            */
            if (SQLiteIO.isTSTEmpty())
            {
                MessageBox.Show("You need to create a team or import stats before using any Analysis features.");
                return;
            }

            dispatcherTimer.Stop();
            var low = new LeagueOverviewWindow(tst, tstopp, pst);
            low.ShowDialog();
            dispatcherTimer.Start();
        }

        private void mnuFileNew_Click(object sender, RoutedEventArgs e)
        {
            var sfd = new SaveFileDialog {Filter = "NST Database (*.tst)|*.tst", InitialDirectory = AppDocsPath};
            sfd.ShowDialog();

            if (sfd.FileName == "") return;

            File.Delete(sfd.FileName);

            db = new SQLiteDatabase(sfd.FileName);

            SQLiteIO.prepareNewDB(db, 1, 1);

            curSeason = 1;

            tst = new Dictionary<int, TeamStats>();
            tst[0] = new TeamStats("$$NewDB");
            TeamOrder = new SortedDictionary<string, int>();

            txtFile.Text = sfd.FileName;

            //
            // tst = new TeamStats[2];
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(currentDB)) return;

            addInfo = "";
            var aw = new AddWindow(ref pst);
            aw.ShowDialog();

            if (!String.IsNullOrEmpty(addInfo))
            {
                if (addInfo != "$$NST Players Added")
                {
                    string[] parts = Regex.Split(addInfo, "\r\n");
                    List<string> newTeams = parts.Where(s => !String.IsNullOrWhiteSpace(s)).ToList();

                    int oldlen = tst.Count;
                    if (SQLiteIO.isTSTEmpty()) oldlen = 0;

                    for (int i = 0; i < newTeams.Count; i++)
                    {
                        int newid = oldlen + i;
                        tst[newid] = new TeamStats(newTeams[i]) {ID = newid};
                        tstopp[newid] = new TeamStats(newTeams[i]) {ID = newid};
                        TeamOrder.Add(newTeams[i], newid);
                    }
                    SQLiteIO.saveSeasonToDatabase();
                    updateStatus("Teams were added, database saved.");
                }
                else
                {
                    SQLiteIO.savePlayersToDatabase(currentDB, pst, curSeason, SQLiteIO.getMaxSeason(currentDB));
                    updateStatus("Players were added, database saved.");
                }
            }
        }

        private void btnGrabNBAStats_Click(object sender, RoutedEventArgs e)
        {
            mnuFileGetRealStats_Click(null, null);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void mnuMiscStartNewSeason_Click(object sender, RoutedEventArgs e)
        {
            if (!SQLiteIO.isTSTEmpty())
            {
                MessageBoxResult r =
                    MessageBox.Show(
                        "Are you sure you want to do this? This is an irreversible action.\nStats and box Scores will be retained, and you'll be able to use all the tool's features on them.",
                        "NBA Stats Tracker", MessageBoxButton.YesNo,
                        MessageBoxImage.Question);
                if (r == MessageBoxResult.Yes)
                {
                    curSeason = SQLiteIO.getMaxSeason(currentDB);
                    var ibw = new InputBoxWindow("Enter a name for the new season", (curSeason + 1).ToString());
                    ibw.ShowDialog();

                    string seasonName = String.IsNullOrWhiteSpace(input) ? (curSeason + 1).ToString() : input;

                    string q = "alter table Teams rename to TeamsS" + curSeason;
                    db.ExecuteNonQuery(q);

                    q = "alter table PlayoffTeams rename to PlayoffTeamsS" + curSeason;
                    db.ExecuteNonQuery(q);

                    q = "alter table Opponents rename to OpponentsS" + curSeason;
                    db.ExecuteNonQuery(q);

                    q = "alter table PlayoffOpponents rename to PlayoffOpponentsS" + curSeason;
                    db.ExecuteNonQuery(q);

                    q = "alter table Players rename to PlayersS" + curSeason;
                    db.ExecuteNonQuery(q);

                    curSeason++;

                    SQLiteIO.prepareNewDB(db, curSeason, curSeason, true);
                    db.Insert("SeasonNames",
                              new Dictionary<string, string> {{"ID", curSeason.ToString()}, {"Name", seasonName}});

                    foreach (int key in tst.Keys)
                    {
                        TeamStats ts = tst[key];
                        for (int i = 0; i < ts.stats.Length; i++)
                        {
                            ts.stats[i] = 0;
                            ts.pl_stats[i] = 0;
                        }
                        ts.winloss[0] = 0;
                        ts.winloss[1] = 0;
                        ts.pl_winloss[0] = 0;
                        ts.pl_winloss[1] = 0;
                        ts.calcAvg();
                        tst[key] = ts;
                    }

                    foreach (int key in tstopp.Keys)
                    {
                        TeamStats ts = tstopp[key];
                        for (int i = 0; i < ts.stats.Length; i++)
                        {
                            ts.stats[i] = 0;
                            ts.pl_stats[i] = 0;
                        }
                        ts.winloss[0] = 0;
                        ts.winloss[1] = 0;
                        ts.pl_winloss[0] = 0;
                        ts.pl_winloss[1] = 0;
                        ts.calcAvg();
                        tstopp[key] = ts;
                    }

                    foreach (var ps in pst)
                    {
                        for (int i = 0; i < ps.Value.stats.Length; i++)
                        {
                            ps.Value.stats[i] = 0;
                        }
                        ps.Value.isAllStar = false;
                        ps.Value.isNBAChampion = false;
                        ps.Value.CalcAvg();
                    }

                    SQLiteIO.saveSeasonToDatabase(currentDB, tst, tstopp, pst, curSeason, curSeason);
                    updateStatus("New season started. Database saved.");
                }
            }
        }

        private void btnSaveAllSeasons_Click(object sender, RoutedEventArgs e)
        {
            SQLiteIO.saveAllSeasons(currentDB);
        }

        private void btnPlayerOverview_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(currentDB)) return;
            if (SQLiteIO.isTSTEmpty())
            {
                MessageBox.Show("You need to create a team or import stats before using the Analysis features.");
                return;
            }

            var pow = new PlayerOverviewWindow();
            pow.ShowDialog();
        }

        private void mnuMiscImportBoxScores_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog
                          {
                              Filter = "NST Database (*.tst)|*.tst",
                              InitialDirectory = AppDocsPath,
                              Title = "Please select the TST file that you want to import from..."
                          };
            ofd.ShowDialog();

            if (ofd.FileName == "") return;

            string file = ofd.FileName;

            int maxSeason = SQLiteIO.getMaxSeason(file);
            for (int i = 1; i <= maxSeason; i++)
            {
                IList<BoxScoreEntry> newBShist = SQLiteIO.GetSeasonBoxScoresFromDatabase(file, i, maxSeason);

                foreach (BoxScoreEntry newbse in newBShist)
                {
                    bool doNotAdd = false;
                    foreach (BoxScoreEntry bse in bshist)
                    {
                        if (bse.bs.id == newbse.bs.id)
                        {
                            if (bse.bs.gamedate == newbse.bs.gamedate
                                && bse.bs.Team1 == newbse.bs.Team1 && bse.bs.Team2 == newbse.bs.Team2)
                            {
                                MessageBoxResult r;
                                if (bse.bs.PTS1 == newbse.bs.PTS1 && bse.bs.PTS2 == newbse.bs.PTS2)
                                {
                                    r =
                                        MessageBox.Show(
                                            "A box score with the same date, teams and score as one in the current database was found in the file being imported." +
                                            "\n" + bse.bs.gamedate.ToShortDateString() + ": " + bse.bs.Team1 + " " +
                                            bse.bs.PTS1 + " @ " + bse.bs.Team2 + " " + bse.bs.PTS2 +
                                            "\n\nClick Yes to only keep the box score that is already in this databse." +
                                            "\nClick No to only keep the box score that is being imported, replacing the one in this database." +
                                            "\nClick Cancel to keep both box scores.", "NBA Stats Tracker",
                                            MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                                }
                                else
                                {
                                    r =
                                        MessageBox.Show(
                                            "A box score with the same date, teams and score as one in the current database was found in the file being imported." +
                                            "\nCurrent: " + bse.bs.gamedate.ToShortDateString() + ": " + bse.bs.Team1 +
                                            " " +
                                            bse.bs.PTS1 + " @ " + bse.bs.Team2 + " " + bse.bs.PTS2 +
                                            "\nTo be imported: " + newbse.bs.gamedate.ToShortDateString() + ": " +
                                            newbse.bs.Team1 + " " + newbse.bs.PTS1 + " @ " + newbse.bs.Team2 + " " +
                                            newbse.bs.PTS2 +
                                            "\n\nClick Yes to only keep the box score that is already in this databse." +
                                            "\nClick No to only keep the box score that is being imported, replacing the one in this database." +
                                            "\nClick Cancel to keep both box scores.", "NBA Stats Tracker",
                                            MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                                }
                                if (r == MessageBoxResult.Yes)
                                {
                                    doNotAdd = true;
                                    break;
                                }

                                if (r == MessageBoxResult.No)
                                {
                                    bshist.Remove(bse);
                                    break;
                                }
                            }
                            newbse.bs.id = GetFreeBseId();
                            break;
                        }
                    }
                    if (!doNotAdd)
                    {
                        newbse.mustUpdate = true;
                        bshist.Add(newbse);
                    }
                }
            }
        }

        private int GetFreeBseId()
        {
            List<int> bseIDs = bshist.Select(bse => bse.bs.id).ToList();

            bseIDs.Sort();

            int i = 0;
            while (true)
            {
                if (!bseIDs.Contains(i)) return i;
                i++;
            }
        }

        private void mnuMiscEnableTeams_Click(object sender, RoutedEventArgs e)
        {
            addInfo = "";
            var etw = new DualListWindow(currentDB, curSeason, SQLiteIO.getMaxSeason(currentDB));
            etw.ShowDialog();

            if (addInfo == "$$TEAMSENABLED")
            {
                SQLiteIO.GetAllTeamStatsFromDatabase(currentDB, curSeason, out tst, out tstopp, out TeamOrder);
                updateStatus("Teams were enabled/disabled. Database saved.");
            }
        }

        private void mnuMiscDeleteBoxScores_Click(object sender, RoutedEventArgs e)
        {
            if (SQLiteIO.isTSTEmpty()) return;

            var bslw = new BoxScoreListWindow();
            bslw.ShowDialog();
            SQLiteIO.LoadSeason();
        }

        private void btnPlayerSearch_Click(object sender, RoutedEventArgs e)
        {
            if (SQLiteIO.isTSTEmpty()) return;

            var psw = new PlayerSearchWindow();
            psw.ShowDialog();
        }

        private void mnuMiscResetTeamStats_Click(object sender, RoutedEventArgs e)
        {
            if (!SQLiteIO.isTSTEmpty())
            {
                MessageBoxResult r =
                    MessageBox.Show(
                        "Are you sure you want to do this? This is an irreversible action.\nThis only applies to the current season.",
                        "Reset All Team Stats", MessageBoxButton.YesNo,
                        MessageBoxImage.Question);
                if (r == MessageBoxResult.Yes)
                {
                    foreach (int key in tst.Keys)
                        tst[key].ResetStats("All");

                    foreach (int key in tstopp.Keys)
                        tst[key].ResetStats("All");
                }
            }

            SQLiteIO.saveSeasonToDatabase();

            updateStatus("All Team Stats for current season have been reset. Database saved.");
        }

        private void mnuMiscResetPlayerStats_Click(object sender, RoutedEventArgs e)
        {
            if (!SQLiteIO.isTSTEmpty())
            {
                MessageBoxResult r =
                    MessageBox.Show(
                        "Are you sure you want to do this? This is an irreversible action.\nThis only applies to the current season.",
                        "Reset All Team Stats", MessageBoxButton.YesNo,
                        MessageBoxImage.Question);
                if (r == MessageBoxResult.Yes)
                {
                    foreach (PlayerStats ps in pst.Values)
                        ps.ResetStats();
                }
            }

            SQLiteIO.saveSeasonToDatabase();

            updateStatus("All Player Stats for current season have been reset. Database saved.");
        }

        private void mnuOptionsCheckForUpdates_Click(object sender, RoutedEventArgs e)
        {
            /*
            if (mnuOptionsCheckForUpdates.IsChecked) mnuOptionsCheckForUpdates.IsChecked = false;
            else mnuOptionsCheckForUpdates.IsChecked = true;
            */
            RegistryKey rk = Registry.CurrentUser;
            try
            {
                try
                {
                    rk = rk.OpenSubKey(@"SOFTWARE\Lefteris Aslanoglou\NBA Stats Tracker", true);
                    if (rk == null) throw new Exception();
                }
                catch (Exception)
                {
                    rk = Registry.CurrentUser;
                    rk.CreateSubKey(@"SOFTWARE\Lefteris Aslanoglou\NBA Stats Tracker");
                    rk = rk.OpenSubKey(@"SOFTWARE\Lefteris Aslanoglou\NBA Stats Tracker", true);
                    if (rk == null) throw new Exception();
                }

                rk.SetValue("CheckForUpdates", mnuOptionsCheckForUpdates.IsChecked ? 1 : 0);
            }
            catch
            {
                MessageBox.Show("Couldn't change check for updates setting.");
            }
        }

        private void mnuOptionsImportREditor_Click(object sender, RoutedEventArgs e)
        {
            if (!mnuOptionsImportREditor.IsChecked) mnuOptionsImportREditor.IsChecked = true;
            mnuOptionsImportOld.IsChecked = false;

            RegistryKey rk = Registry.CurrentUser;
            try
            {
                try
                {
                    rk = rk.OpenSubKey(@"SOFTWARE\Lefteris Aslanoglou\NBA Stats Tracker", true);
                    if (rk == null) throw new Exception();
                }
                catch (Exception)
                {
                    rk = Registry.CurrentUser;
                    rk.CreateSubKey(@"SOFTWARE\Lefteris Aslanoglou\NBA Stats Tracker");
                    rk = rk.OpenSubKey(@"SOFTWARE\Lefteris Aslanoglou\NBA Stats Tracker", true);
                    if (rk == null) throw new Exception();
                }

                rk.SetValue("NBA2K12ImportMethod", 0);
            }
            catch
            {
                MessageBox.Show("Couldn't change setting.");
            }
        }

        private void mnuOptionsImportOld_Click(object sender, RoutedEventArgs e)
        {
            if (!mnuOptionsImportOld.IsChecked) mnuOptionsImportOld.IsChecked = true;
            mnuOptionsImportREditor.IsChecked = false;

            RegistryKey rk = Registry.CurrentUser;
            try
            {
                try
                {
                    rk = rk.OpenSubKey(@"SOFTWARE\Lefteris Aslanoglou\NBA Stats Tracker", true);
                    if (rk == null) throw new Exception();
                }
                catch (Exception)
                {
                    rk = Registry.CurrentUser;
                    rk.CreateSubKey(@"SOFTWARE\Lefteris Aslanoglou\NBA Stats Tracker");
                    rk = rk.OpenSubKey(@"SOFTWARE\Lefteris Aslanoglou\NBA Stats Tracker", true);
                    if (rk == null) throw new Exception();
                }

                rk.SetValue("NBA2K12ImportMethod", 1);
            }
            catch
            {
                MessageBox.Show("Couldn't change setting.");
            }
        }

        private void mnuOptionsExportTeamsOnly_Click(object sender, RoutedEventArgs e)
        {
            RegistryKey rk = Registry.CurrentUser;
            try
            {
                try
                {
                    rk = rk.OpenSubKey(@"SOFTWARE\Lefteris Aslanoglou\NBA Stats Tracker", true);
                    if (rk == null) throw new Exception();
                }
                catch (Exception)
                {
                    rk = Registry.CurrentUser;
                    rk.CreateSubKey(@"SOFTWARE\Lefteris Aslanoglou\NBA Stats Tracker");
                    rk = rk.OpenSubKey(@"SOFTWARE\Lefteris Aslanoglou\NBA Stats Tracker", true);
                    if (rk == null) throw new Exception();
                }

                rk.SetValue("ExportTeamsOnly", mnuOptionsExportTeamsOnly.IsChecked ? 1 : 0);
            }
            catch
            {
                MessageBox.Show("Couldn't change setting.");
            }
        }

        private void mnuOptionsCompatibilityCheck_Click(object sender, RoutedEventArgs e)
        {
            RegistryKey rk = Registry.CurrentUser;
            try
            {
                try
                {
                    rk = rk.OpenSubKey(@"SOFTWARE\Lefteris Aslanoglou\NBA Stats Tracker", true);
                    if (rk == null) throw new Exception();
                }
                catch (Exception)
                {
                    rk = Registry.CurrentUser;
                    rk.CreateSubKey(@"SOFTWARE\Lefteris Aslanoglou\NBA Stats Tracker");
                    rk = rk.OpenSubKey(@"SOFTWARE\Lefteris Aslanoglou\NBA Stats Tracker", true);
                    if (rk == null) throw new Exception();
                }

                rk.SetValue("CompatibilityCheck", mnuOptionsCompatibilityCheck.IsChecked ? 1 : 0);
            }
            catch
            {
                MessageBox.Show("Couldn't change setting.");
            }
        }

        private void mnuMiscRenameCurrentSeason_Click(object sender, RoutedEventArgs e)
        {
            string curName = GetSeasonName(curSeason);
            var ibw = new InputBoxWindow("Enter the new name for the current season",
                                         curName);
            ibw.ShowDialog();

            if (!String.IsNullOrWhiteSpace(input))
            {
                SetSeasonName(curSeason, input);
                cmbSeasonNum.SelectedValue = curSeason;
            }
        }

        private void SetSeasonName(int season, string name)
        {
            for (int i = 0; i < SeasonList.Count; i++)
            {
                if (SeasonList[i].Key == season)
                {
                    SeasonList[i] = new KeyValuePair<int, string>(season, name);
                    break;
                }
            }

            SQLiteIO.SaveSeasonName(season);
        }

        public static string GetSeasonName(int season)
        {
            return SeasonList.Single(delegate(KeyValuePair<int, string> kvp)
                                         {
                                             if (kvp.Key == season) return true;
                                             return false;
                                         }).Value;
        }

        private void mnuMiscLiveBoxScore_Click(object sender, RoutedEventArgs e)
        {
            var lbsw = new LiveBoxScoreWindow();
            if (lbsw.ShowDialog() == true)
            {
                var bsw = new BoxScoreWindow(tempbse);
                bsw.ShowDialog();

                ParseBoxScoreResult();
            }
        }
    }
}