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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using LeftosCommonLibrary;
using Microsoft.Win32;
using NBA_Stats_Tracker.Data;
using NBA_Stats_Tracker.Interop;
using SQLite_Database;

#endregion

namespace NBA_Stats_Tracker.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static string AppDocsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) +
                                           @"\NBA Stats Tracker\";

        public static string AppTempPath = AppDocsPath + @"Temp\";
        public static string SavesPath = "";
        public static string AppPath = Environment.CurrentDirectory + "\\";
        public static bool isCustom;

        public static string input = "";

        public static MainWindow mwInstance;

        public static TeamStats[] tst = new TeamStats[1];
        public static TeamStats[] tstopp = new TeamStats[1];
        public static TeamStats[] realtst = new TeamStats[30];
        public static Dictionary<int, PlayerStats> pst = new Dictionary<int, PlayerStats>();
        public static BoxScore bs;
        public static IList<BoxScoreEntry> bshist = new List<BoxScoreEntry>();
        public static PlayoffTree pt;
        public static string ext;
        public static string myTeam;
        public static string currentDB = "";
        public static string addInfo;
        public static int curSeason = 1;
        public static List<BindingList<PlayerBoxScore>> pbsLists;

        public static SortedDictionary<string, int> TeamOrder;

        public static List<string> West = new List<string>
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

            btnSave.Visibility = Visibility.Hidden;
            btnCRC.Visibility = Visibility.Hidden;
            btnSaveCustomTeam.Visibility = Visibility.Hidden;
            //btnInject.Visibility = Visibility.Hidden;
            btnTest.Visibility = Visibility.Hidden;

            isCustom = true;

            if (
                Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) +
                                 @"\NBA 2K12 Correct Team Stats"))
                if (Directory.Exists(AppDocsPath) == false)
                    Directory.Move(
                        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) +
                        @"\NBA 2K12 Correct Team Stats",
                        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\NBA Stats Tracker");

            if (Directory.Exists(AppDocsPath) == false) Directory.CreateDirectory(AppDocsPath);
            if (Directory.Exists(AppTempPath) == false) Directory.CreateDirectory(AppTempPath);

            tst[0] = new TeamStats("$$NewDB");
            tstopp[0] = new TeamStats("$$NewDB");

            for (int i = 0; i < 30; i++)
            {
                realtst[i] = new TeamStats();
            }

            //TeamOrder = StatsTracker.setTeamOrder("Mode 0");
            TeamOrder = new SortedDictionary<string, int>();

            foreach (KeyValuePair<string, int> kvp in TeamOrder)
            {
                cmbTeam1.Items.Add(kvp.Key);
            }

            RegistryKey rk = null;

            try
            {
                rk = Registry.CurrentUser;
            }
            catch (Exception ex)
            {
                App.errorReport(ex, "Registry.CurrentUser");
            }

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
                mnuFileGetRealStats_Click(null, new RoutedEventArgs());
                MessageBox.Show("Nothing but net! Thanks for using NBA Stats Tracker!");
                Environment.Exit(-1);
            }
            else
            {
                checkForUpdates();
            }
        }

        public static string AppDocsPath1
        {
            get { return AppDocsPath; }
        }

        public static void checkForRedundantSettings()
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
            var ofd = new OpenFileDialog();
            ofd.Title = "Please select the Career file you're playing...";
            ofd.Filter = "All NBA 2K12 Career Files (*.FXG; *.CMG; *.RFG; *.PMG; *.SMG)|*.FXG;*.CMG;*.RFG;*.PMG;*.SMG|"
                         +
                         "Association files (*.FXG)|*.FXG|My Player files (*.CMG)|*.CMG|Season files (*.RFG)|*.RFG|Playoff files (*.PMG)|*.PMG|" +
                         "Create A Legend files (*.SMG)|*.SMG";
            if (Directory.Exists(SavesPath)) ofd.InitialDirectory = SavesPath;
            ofd.ShowDialog();
            if (ofd.FileName == "") return;

            cmbTeam1.SelectedIndex = -1;

            isCustom = true;
            //prepareWindow(isCustom);
            TeamOrder = Helper.setTeamOrder("Mode 0");

            var temp = new TeamStats[1];

            //TODO: Implement Opponents stats from 2K12 Save
            //TeamStats[] tempopp = new TeamStats[1];
            TeamStats[] tempopp = tstopp;

            Interop2K12.GetStatsFrom2K12Save(ofd.FileName, ref temp, ref tempopp, ref TeamOrder, ref pt);
            if (temp.Length > 1)
            {
                tst = temp;
                tstopp = tempopp;
                populateTeamsComboBox(TeamOrder, pt);
            }

            if (tst.Length != tstopp.Length)
            {
                tstopp = new TeamStats[tst.Length];
                for (int i = 0; i < tst.Length; i++) tstopp[i] = new TeamStats(tst[i].name);
            }

            cmbTeam1.SelectedIndex = 0;

            updateStatus("NBA 2K12 stats imported successfully! Verify that you want this by saving the current season.");
        }

        private void btnCRC_Click(object sender, RoutedEventArgs e)
        {
            String hash = Tools.getCRC(txtFile.Text);

            MessageBox.Show(hash);
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            Interop2K12.updateSavegame(txtFile.Text, tst, TeamOrder, pt);
        }

        private void cmbTeam1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                string team = cmbTeam1.SelectedItem.ToString();
                int id = TeamOrder[team];
                txtW1.Text = tst[id].winloss[0].ToString();
                txtL1.Text = tst[id].winloss[1].ToString();
                txtPF1.Text = tst[id].stats[t.PF].ToString();
                txtPA1.Text = tst[id].stats[t.PA].ToString();
                txtFGM1.Text = tst[id].stats[t.FGM].ToString();
                txtFGA1.Text = tst[id].stats[t.FGA].ToString();
                txt3PM1.Text = tst[id].stats[t.TPM].ToString();
                txt3PA1.Text = tst[id].stats[t.TPA].ToString();
                txtFTM1.Text = tst[id].stats[t.FTM].ToString();
                txtFTA1.Text = tst[id].stats[t.FTA].ToString();
                txtOREB1.Text = tst[id].stats[t.OREB].ToString();
                txtDREB1.Text = tst[id].stats[t.DREB].ToString();
                txtSTL1.Text = tst[id].stats[t.STL].ToString();
                txtTO1.Text = tst[id].stats[t.TO].ToString();
                txtBLK1.Text = tst[id].stats[t.BLK].ToString();
                txtAST1.Text = tst[id].stats[t.AST].ToString();
                txtFOUL1.Text = tst[id].stats[t.FOUL].ToString();
            }
            catch
            {
            }
        }

        private void mnuFileSaveAs_Click(object sender, RoutedEventArgs e)
        {
            var sfd = new SaveFileDialog();
            sfd.Filter = "NST Database (*.tst)|*.tst";
            sfd.InitialDirectory = AppDocsPath;
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
            tst = new TeamStats[30];
            TeamOrder = new SortedDictionary<string, int>();
            bshist = new List<BoxScoreEntry>();

            var ofd = new OpenFileDialog();
            ofd.Filter = "NST Database (*.tst)|*.tst";
            ofd.InitialDirectory = AppDocsPath;
            ofd.Title = "Please select the TST file that you want to edit...";
            ofd.ShowDialog();

            if (ofd.FileName == "") return;

            SQLiteIO.LoadSeason(ofd.FileName, ref tst, ref tstopp, ref pst, ref TeamOrder, ref pt, ref bshist);
            //tst = getCustomStats("", ref TeamOrder, ref pt, ref bshist);

            cmbTeam1.SelectedIndex = -1;
            cmbTeam1.SelectedIndex = 0;
            txtFile.Text = ofd.FileName;

            updateStatus(tst.GetLength(0).ToString() + " teams loaded successfully");
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
            var bsW = new boxScoreW();
            bsW.ShowDialog();

            if (bs.done == false) return;

            int id1 = -1;
            int id2 = -1;

            id1 = TeamOrder[bs.Team1];
            id2 = TeamOrder[bs.Team2];

            SQLiteIO.LoadSeason(currentDB, ref tst, ref tstopp, ref pst, ref TeamOrder, ref pt, ref bshist,
                       _curSeason: bs.SeasonNum);

            var list = new List<PlayerBoxScore>();
            foreach (BindingList<PlayerBoxScore> pbsList in pbsLists)
            {
                foreach (PlayerBoxScore pbs in pbsList)
                {
                    list.Add(pbs);
                }
            }

            if (!bs.doNotUpdate)
            {
                AddTeamStatsFromBoxScore(bs, ref tst[id1], ref tst[id2], ref tstopp[id1], ref tstopp[id2]);

                foreach (PlayerBoxScore pbs in list)
                {
                    if (pbs.PlayerID == -1) continue;
                    pst[pbs.PlayerID].AddBoxScore(pbs);
                }
            }

            cmbTeam1.SelectedIndex = -1;
            cmbTeam1.SelectedItem = bs.Team1;

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

        public static void AddTeamStatsFromBoxScore(BoxScore bs, ref TeamStats ts1, ref TeamStats ts2)
        {
            var tsopp1 = new TeamStats();
            var tsopp2 = new TeamStats();
            AddTeamStatsFromBoxScore(bs, ref ts1, ref ts2, ref tsopp1, ref tsopp2);
        }

        public static void AddTeamStatsFromBoxScore(BoxScore bs, ref TeamStats ts1, ref TeamStats ts2,
                                                    ref TeamStats tsopp1, ref TeamStats tsopp2)
        {
            if (!bs.isPlayoff)
            {
                // Add win & loss
                if (bs.PTS1 > bs.PTS2)
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
                ts1.stats[t.MINS] += bs.MINS1;
                ts2.stats[t.MINS] += bs.MINS2;

                // Add Points For
                ts1.stats[t.PF] += bs.PTS1;
                ts2.stats[t.PF] += bs.PTS2;

                // Add Points Against
                ts1.stats[t.PA] += bs.PTS2;
                ts2.stats[t.PA] += bs.PTS1;

                //
                ts1.stats[t.FGM] += bs.FGM1;
                ts2.stats[t.FGM] += bs.FGM2;

                ts1.stats[t.FGA] += bs.FGA1;
                ts2.stats[t.FGA] += bs.FGA2;

                //
                ts1.stats[t.TPM] += bs.TPM1;
                ts2.stats[t.TPM] += bs.TPM2;

                //
                ts1.stats[t.TPA] += bs.TPA1;
                ts2.stats[t.TPA] += bs.TPA2;

                //
                ts1.stats[t.FTM] += bs.FTM1;
                ts2.stats[t.FTM] += bs.FTM2;

                //
                ts1.stats[t.FTA] += bs.FTA1;
                ts2.stats[t.FTA] += bs.FTA2;

                //
                ts1.stats[t.OREB] += bs.OREB1;
                ts2.stats[t.OREB] += bs.OREB2;

                //
                ts1.stats[t.DREB] += Convert.ToUInt16(bs.REB1 - bs.OREB1);
                ts2.stats[t.DREB] += Convert.ToUInt16(bs.REB2 - bs.OREB2);

                //
                ts1.stats[t.STL] += bs.STL1;
                ts2.stats[t.STL] += bs.STL2;

                //
                ts1.stats[t.TO] += bs.TO1;
                ts2.stats[t.TO] += bs.TO2;

                //
                ts1.stats[t.BLK] += bs.BLK1;
                ts2.stats[t.BLK] += bs.BLK2;

                //
                ts1.stats[t.AST] += bs.AST1;
                ts2.stats[t.AST] += bs.AST2;

                //
                ts1.stats[t.FOUL] += bs.FOUL1;
                ts2.stats[t.FOUL] += bs.FOUL2;


                // Opponents Team Stats
                // Add win & loss
                if (bs.PTS1 > bs.PTS2)
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
                tsopp2.stats[t.MINS] += bs.MINS1;
                tsopp1.stats[t.MINS] += bs.MINS2;

                // Add Points For
                tsopp2.stats[t.PF] += bs.PTS1;
                tsopp1.stats[t.PF] += bs.PTS2;

                // Add Points Against
                tsopp2.stats[t.PA] += bs.PTS2;
                tsopp1.stats[t.PA] += bs.PTS1;

                //
                tsopp2.stats[t.FGM] += bs.FGM1;
                tsopp1.stats[t.FGM] += bs.FGM2;

                tsopp2.stats[t.FGA] += bs.FGA1;
                tsopp1.stats[t.FGA] += bs.FGA2;

                //
                tsopp2.stats[t.TPM] += bs.TPM1;
                tsopp1.stats[t.TPM] += bs.TPM2;

                //
                tsopp2.stats[t.TPA] += bs.TPA1;
                tsopp1.stats[t.TPA] += bs.TPA2;

                //
                tsopp2.stats[t.FTM] += bs.FTM1;
                tsopp1.stats[t.FTM] += bs.FTM2;

                //
                tsopp2.stats[t.FTA] += bs.FTA1;
                tsopp1.stats[t.FTA] += bs.FTA2;

                //
                tsopp2.stats[t.OREB] += bs.OREB1;
                tsopp1.stats[t.OREB] += bs.OREB2;

                //
                tsopp2.stats[t.DREB] += Convert.ToUInt16(bs.REB1 - bs.OREB1);
                tsopp1.stats[t.DREB] += Convert.ToUInt16(bs.REB2 - bs.OREB2);

                //
                tsopp2.stats[t.STL] += bs.STL1;
                tsopp1.stats[t.STL] += bs.STL2;

                //
                tsopp2.stats[t.TO] += bs.TO1;
                tsopp1.stats[t.TO] += bs.TO2;

                //
                tsopp2.stats[t.BLK] += bs.BLK1;
                tsopp1.stats[t.BLK] += bs.BLK2;

                //
                tsopp2.stats[t.AST] += bs.AST1;
                tsopp1.stats[t.AST] += bs.AST2;

                //
                tsopp2.stats[t.FOUL] += bs.FOUL1;
                tsopp1.stats[t.FOUL] += bs.FOUL2;
            }
            else
            {
                // Add win & loss
                if (bs.PTS1 > bs.PTS2)
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
                ts1.pl_stats[t.MINS] += bs.MINS1;
                ts2.pl_stats[t.MINS] += bs.MINS2;

                // Add Points For
                ts1.pl_stats[t.PF] += bs.PTS1;
                ts2.pl_stats[t.PF] += bs.PTS2;

                // Add Points Against
                ts1.pl_stats[t.PA] += bs.PTS2;
                ts2.pl_stats[t.PA] += bs.PTS1;

                //
                ts1.pl_stats[t.FGM] += bs.FGM1;
                ts2.pl_stats[t.FGM] += bs.FGM2;

                ts1.pl_stats[t.FGA] += bs.FGA1;
                ts2.pl_stats[t.FGA] += bs.FGA2;

                //
                ts1.pl_stats[t.TPM] += bs.TPM1;
                ts2.pl_stats[t.TPM] += bs.TPM2;

                //
                ts1.pl_stats[t.TPA] += bs.TPA1;
                ts2.pl_stats[t.TPA] += bs.TPA2;

                //
                ts1.pl_stats[t.FTM] += bs.FTM1;
                ts2.pl_stats[t.FTM] += bs.FTM2;

                //
                ts1.pl_stats[t.FTA] += bs.FTA1;
                ts2.pl_stats[t.FTA] += bs.FTA2;

                //
                ts1.pl_stats[t.OREB] += bs.OREB1;
                ts2.pl_stats[t.OREB] += bs.OREB2;

                //
                ts1.pl_stats[t.DREB] += Convert.ToUInt16(bs.REB1 - bs.OREB1);
                ts2.pl_stats[t.DREB] += Convert.ToUInt16(bs.REB2 - bs.OREB2);

                //
                ts1.pl_stats[t.STL] += bs.STL1;
                ts2.pl_stats[t.STL] += bs.STL2;

                //
                ts1.pl_stats[t.TO] += bs.TO1;
                ts2.pl_stats[t.TO] += bs.TO2;

                //
                ts1.pl_stats[t.BLK] += bs.BLK1;
                ts2.pl_stats[t.BLK] += bs.BLK2;

                //
                ts1.pl_stats[t.AST] += bs.AST1;
                ts2.pl_stats[t.AST] += bs.AST2;

                //
                ts1.pl_stats[t.FOUL] += bs.FOUL1;
                ts2.pl_stats[t.FOUL] += bs.FOUL2;


                // Opponents Team Stats
                // Add win & loss
                if (bs.PTS1 > bs.PTS2)
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
                tsopp2.pl_stats[t.MINS] += bs.MINS1;
                tsopp1.pl_stats[t.MINS] += bs.MINS2;

                // Add Points For
                tsopp2.pl_stats[t.PF] += bs.PTS1;
                tsopp1.pl_stats[t.PF] += bs.PTS2;

                // Add Points Against
                tsopp2.pl_stats[t.PA] += bs.PTS2;
                tsopp1.pl_stats[t.PA] += bs.PTS1;

                //
                tsopp2.pl_stats[t.FGM] += bs.FGM1;
                tsopp1.pl_stats[t.FGM] += bs.FGM2;

                tsopp2.pl_stats[t.FGA] += bs.FGA1;
                tsopp1.pl_stats[t.FGA] += bs.FGA2;

                //
                tsopp2.pl_stats[t.TPM] += bs.TPM1;
                tsopp1.pl_stats[t.TPM] += bs.TPM2;

                //
                tsopp2.pl_stats[t.TPA] += bs.TPA1;
                tsopp1.pl_stats[t.TPA] += bs.TPA2;

                //
                tsopp2.pl_stats[t.FTM] += bs.FTM1;
                tsopp1.pl_stats[t.FTM] += bs.FTM2;

                //
                tsopp2.pl_stats[t.FTA] += bs.FTA1;
                tsopp1.pl_stats[t.FTA] += bs.FTA2;

                //
                tsopp2.pl_stats[t.OREB] += bs.OREB1;
                tsopp1.pl_stats[t.OREB] += bs.OREB2;

                //
                tsopp2.pl_stats[t.DREB] += Convert.ToUInt16(bs.REB1 - bs.OREB1);
                tsopp1.pl_stats[t.DREB] += Convert.ToUInt16(bs.REB2 - bs.OREB2);

                //
                tsopp2.pl_stats[t.STL] += bs.STL1;
                tsopp1.pl_stats[t.STL] += bs.STL2;

                //
                tsopp2.pl_stats[t.TO] += bs.TO1;
                tsopp1.pl_stats[t.TO] += bs.TO2;

                //
                tsopp2.pl_stats[t.BLK] += bs.BLK1;
                tsopp1.pl_stats[t.BLK] += bs.BLK2;

                //
                tsopp2.pl_stats[t.AST] += bs.AST1;
                tsopp1.pl_stats[t.AST] += bs.AST2;

                //
                tsopp2.pl_stats[t.FOUL] += bs.FOUL1;
                tsopp1.pl_stats[t.FOUL] += bs.FOUL2;
            }

            ts1.calcAvg();
            ts2.calcAvg();
            tsopp1.calcAvg();
            tsopp2.calcAvg();
        }

        private void populateTeamsComboBox(SortedDictionary<string, int> TeamOrder, PlayoffTree pt)
        {
            bool done = false;

            cmbTeam1.Items.Clear();
            if (pt != null)
            {
                if (pt.teams[0] != "Invalid")
                {
                    var newteams = new List<string>();
                    for (int i = 0; i < 16; i++)
                        newteams.Add(pt.teams[i]);
                    newteams.Sort();
                    for (int i = 0; i < 16; i++)
                        cmbTeam1.Items.Add(newteams[i]);
                    done = true;
                }
            }

            if (!done)
            {
                foreach (KeyValuePair<string, int> kvp in TeamOrder)
                    cmbTeam1.Items.Add(kvp.Key);
            }
        }

        private static void checkForUpdates()
        {
            try
            {
                var webClient = new WebClient();
                webClient.DownloadFileCompleted += Completed;
                //webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
                webClient.DownloadFileAsync(new Uri("http://students.ceid.upatras.gr/~aslanoglou/nstversion.txt"),
                                            AppDocsPath + @"nstversion.txt");
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
                }
            }
        }

        private void btnEraseSettings_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Title = "Please select the Career file you want to reset the settings for...";
            ofd.Filter = "All NBA 2K12 Career Files (*.FXG; *.CMG; *.RFG; *.PMG; *.SMG)|*.FXG;*.CMG;*.RFG;*.PMG;*.SMG|"
                         +
                         "Association files (*.FXG)|*.FXG|My Player files (*.CMG)|*.CMG|Season files (*.RFG)|*.RFG|Playoff files (*.PMG)|*.PMG|" +
                         "Create A Legend files (*.SMG)|*.SMG";
            if (Directory.Exists(SavesPath)) ofd.InitialDirectory = SavesPath;
            ofd.ShowDialog();
            if (ofd.FileName == "") return;

            string safefn = Tools.getSafeFilename(ofd.FileName);
            string SettingsFile = AppDocsPath + safefn + ".cfg";
            if (File.Exists(SettingsFile)) File.Delete(SettingsFile);
            MessageBox.Show(
                "Settings for this file have been erased. You'll be asked to set them again next time you import it.");
        }

        private void _AnyTextbox_GotFocus(object sender, RoutedEventArgs e)
        {
            var tb = (TextBox) sender;
            tb.SelectAll();
        }

        private void btnShowAvg_Click(object sender, RoutedEventArgs e)
        {
            string msg = TeamStats.TeamAveragesAndRankings(cmbTeam1.SelectedItem.ToString(), tst, TeamOrder);
            if (msg != "")
            {
                var cw = new copyableW(msg, cmbTeam1.SelectedItem.ToString(), TextAlignment.Center);
                cw.ShowDialog();
            }
        }

        private void btnScout_Click(object sender, RoutedEventArgs e)
        {
            int id = -1;
            try
            {
                id = TeamOrder[cmbTeam1.SelectedItem.ToString()];
            }
            catch
            {
                return;
            }

            int[][] rating = TeamStats.CalculateTeamRankings(tst);
            if (rating.Length != 1)
            {
                string msg = TeamStats.TeamScoutingReport(rating, id, cmbTeam1.SelectedItem.ToString());
                var cw = new copyableW(msg, "Scouting Report", TextAlignment.Left);
                cw.ShowDialog();
            }
        }

        private void btnTeamCSV_Click(object sender, RoutedEventArgs e)
        {
            int id = -1;
            try
            {
                id = TeamOrder[cmbTeam1.SelectedItem.ToString()];
            }
            catch
            {
                return;
            }

            string header1 = "GP,W,L,PF,PA,FGM,FGA,3PM,3PA,FTM,FTA,OREB,DREB,STL,TO,BLK,AST,FOUL";
            /*
            string data = String.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17}", 
                tst[id].getGames(), tst[id].winloss[0], tst[id].winloss[1], tst[id].stats[t.FGM], tst[id].stats[t.FGA], tst[id].stats[t.TPM], tst[id].stats[t.TPA],
                tst[id].stats[t.FTM], tst[id].stats[t.FTA], tst[
             */
            string data1 = String.Format("{0},{1},{2}", tst[id].getGames(), tst[id].winloss[0], tst[id].winloss[1]);
            for (int j = 1; j <= 16; j++)
            {
                if (j != 3)
                {
                    data1 += "," + tst[id].stats[j].ToString();
                }
            }

            NumberFormatInfo nfi = new CultureInfo("en-US", false).NumberFormat;

            string header2 = "W%,Weff,PPG,PAPG,FG%,FGeff,3P%,3Peff,FT%,FTeff,RPG,ORPG,DRPG,SPG,BPG,TPG,APG,FPG";
            string data2 = String.Format("{0:F3}", tst[id].averages[t.Wp]) + "," +
                           String.Format("{0:F1}", tst[id].averages[t.Weff]);
            for (int j = 0; j <= 15; j++)
            {
                switch (j)
                {
                    case 2:
                    case 4:
                    case 6:
                        data2 += String.Format(",{0:F3}", tst[id].averages[j]);
                        break;
                    default:
                        data2 += String.Format(",{0:F1}", tst[id].averages[j]);
                        break;
                }
            }

            int[][] rankings = TeamStats.CalculateTeamRankings(tst);

            string data3 = String.Format("{0:F3}", rankings[id][t.Wp]) + "," +
                           String.Format("{0:F1}", rankings[id][t.Weff]);
            for (int j = 0; j <= 15; j++)
            {
                switch (j)
                {
                    case 1:
                    case 13:
                    case 15:
                        data3 += "," + (31 - rankings[id][j]).ToString();
                        break;
                    default:
                        data3 += "," + rankings[id][j].ToString();
                        break;
                }
            }

            var sfd = new SaveFileDialog();
            sfd.Filter = "Comma-Separated Values file (*.csv)|*.csv";
            sfd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            sfd.Title = "Select a file to save the CSV to...";
            sfd.ShowDialog();
            if (sfd.FileName == "") return;

            var sw = new StreamWriter(sfd.FileName);
            /*
            sw.WriteLine(header1);
            sw.WriteLine(data1);
            sw.WriteLine();
            sw.WriteLine(header2);
            sw.WriteLine(data2);
            sw.WriteLine(data3);
            */
            sw.WriteLine(header1 + "," + header2);
            sw.WriteLine(data1 + "," + data2);
            sw.WriteLine();
            sw.WriteLine(header2);
            sw.WriteLine(data3);
            sw.Close();
        }

        private void btnLeagueCSV_Click(object sender, RoutedEventArgs e)
        {
            string header1 = ",Team,GP,W,L,PF,PA,FGM,FGA,3PM,3PA,FTM,FTA,OREB,DREB,STL,TO,BLK,AST,FOUL,";
            //string header2 = "Team,W%,Weff,PPG,PAPG,FG%,FGeff,3P%,3Peff,FT%,FTeff,RPG,ORPG,DRPG,SPG,BPG,TPG,APG,FPG";
            string header2 = "W%,Weff,PPG,PAPG,FG%,FGeff,3P%,3Peff,FT%,FTeff,RPG,ORPG,DRPG,SPG,BPG,TPG,APG,FPG";
            /*
            string data = String.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17}", 
                tst[id].getGames(), tst[id].winloss[0], tst[id].winloss[1], tst[id].stats[t.FGM], tst[id].stats[t.FGA], tst[id].stats[t.TPM], tst[id].stats[t.TPA],
                tst[id].stats[t.FTM], tst[id].stats[t.FTA], tst[
             */
            string data1 = "";
            for (int id = 0; id < 30; id++)
            {
                if (tst[id].name == "") continue;

                data1 += (id + 1).ToString() + ",";
                foreach (KeyValuePair<string, int> kvp in TeamOrder)
                {
                    if (kvp.Value == id)
                    {
                        data1 += kvp.Key + ",";
                        break;
                    }
                }
                data1 += String.Format("{0},{1},{2}", tst[id].getGames(), tst[id].winloss[0], tst[id].winloss[1]);
                for (int j = 1; j <= 16; j++)
                {
                    if (j != 3)
                    {
                        data1 += "," + tst[id].stats[j].ToString();
                    }
                }
                data1 += ",";
                data1 += String.Format("{0:F3}", tst[id].averages[t.Wp]) + "," +
                         String.Format("{0:F1}", tst[id].averages[t.Weff]);
                for (int j = 0; j <= 15; j++)
                {
                    switch (j)
                    {
                        case 2:
                        case 4:
                        case 6:
                            data1 += String.Format(",{0:F3}", tst[id].averages[j]);
                            break;
                        default:
                            data1 += String.Format(",{0:F1}", tst[id].averages[j]);
                            break;
                    }
                }
                data1 += "\n";
            }

            var sfd = new SaveFileDialog();
            sfd.Filter = "Comma-Separated Values file (*.csv)|*.csv";
            sfd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            sfd.Title = "Select a file to save the CSV to...";
            sfd.ShowDialog();
            if (sfd.FileName == "") return;

            var sw = new StreamWriter(sfd.FileName);
            sw.WriteLine(header1 + header2);
            sw.WriteLine(data1);
            sw.Close();
        }

        private void mnuFileOpen_Click(object sender, RoutedEventArgs e)
        {
            btnImport2K12_Click(sender, e);
        }

        private void btnSaveCustomTeam_Click(object sender, RoutedEventArgs e)
        {
            int id = TeamOrder[cmbTeam1.SelectedItem.ToString()];
            tst[id].winloss[0] = Convert.ToByte(txtW1.Text);
            tst[id].winloss[1] = Convert.ToByte(txtL1.Text);
            tst[id].stats[t.PF] = Convert.ToUInt16(txtPF1.Text);
            tst[id].stats[t.PA] = Convert.ToUInt16(txtPA1.Text);
            tst[id].stats[t.FGM] = Convert.ToUInt16(txtFGM1.Text);
            tst[id].stats[t.FGA] = Convert.ToUInt16(txtFGA1.Text);
            tst[id].stats[t.TPM] = Convert.ToUInt16(txt3PM1.Text);
            tst[id].stats[t.TPA] = Convert.ToUInt16(txt3PA1.Text);
            tst[id].stats[t.FTM] = Convert.ToUInt16(txtFTM1.Text);
            tst[id].stats[t.FTA] = Convert.ToUInt16(txtFTA1.Text);
            tst[id].stats[t.OREB] = Convert.ToUInt16(txtOREB1.Text);
            tst[id].stats[t.DREB] = Convert.ToUInt16(txtDREB1.Text);
            tst[id].stats[t.STL] = Convert.ToUInt16(txtSTL1.Text);
            tst[id].stats[t.TO] = Convert.ToUInt16(txtTO1.Text);
            tst[id].stats[t.BLK] = Convert.ToUInt16(txtBLK1.Text);
            tst[id].stats[t.AST] = Convert.ToUInt16(txtAST1.Text);
            tst[id].stats[t.FOUL] = Convert.ToUInt16(txtFOUL1.Text);

            tst[id].calcAvg();
        }

        private void mnuExit_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(-1);
        }

        private void btnInject_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Title = "Please select the Career file you want to update...";
            ofd.Filter = "All NBA 2K12 Career Files (*.FXG; *.CMG; *.RFG; *.PMG; *.SMG)|*.FXG;*.CMG;*.RFG;*.PMG;*.SMG|"
                         +
                         "Association files (*.FXG)|*.FXG|My Player files (*.CMG)|*.CMG|Season files (*.RFG)|*.RFG|Playoff files (*.PMG)|*.PMG|" +
                         "Create A Legend files (*.SMG)|*.SMG";
            if (Directory.Exists(SavesPath)) ofd.InitialDirectory = SavesPath;
            ofd.ShowDialog();

            if (ofd.FileName == "") return;
            string fn = ofd.FileName;

            Interop2K12.prepareOffsets(fn, tst, ref TeamOrder, ref pt);

            var temp = new TeamStats[1];
            var tempopp = new TeamStats[1];

            Interop2K12.GetStatsFrom2K12Save(fn, ref temp, ref tempopp, ref TeamOrder, ref pt);
            if (temp.Length == 1)
            {
                MessageBox.Show("Couldn't get stats from " + Tools.getSafeFilename(fn) + ". Update failed.");
                return;
            }
            else
            {
                bool incompatible = false;

                if (temp.Length != tst.Length) incompatible = true;
                else
                {
                    for (int i = 0; i < temp.Length; i++)
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
            }


            Interop2K12.updateSavegame(fn, tst, TeamOrder, pt);
            updateStatus("Injected custom Team Stats into " + Tools.getSafeFilename(fn) + " successfully!");
            cmbTeam1.SelectedIndex = -1;
            cmbTeam1.SelectedIndex = 0;
        }

        private void btnCompare_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var aw = new askTeamW(true, cmbTeam1.SelectedIndex);
                aw.ShowDialog();
            }
            catch
            {
            }
        }

        private void mnuHelpReadme_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(AppPath + @"\readme.txt");
        }

        private void mnuHelpAbout_Click(object sender, RoutedEventArgs e)
        {
            var aw = new aboutW();
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
                            SQLiteIO.LoadSeason(file, ref tst, ref tstopp, ref pst, ref TeamOrder, ref pt, ref bshist);

                            cmbTeam1.SelectedIndex = -1;
                            cmbTeam1.SelectedIndex = 0;
                            txtFile.Text = file;
                            return;
                        }
                    }
                }
            }

            //var grsw = new getRealStatsW();
            //grsw.ShowDialog();
            TeamOrder = Helper.setTeamOrder("Mode 0");

            var realtstopp = new TeamStats[realtst.Length];
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

            worker1 = new BackgroundWorker();
            worker1.WorkerReportsProgress = true;
            worker1.WorkerSupportsCancellation = true;

            worker1.DoWork += delegate
                                  {
                                      foreach (KeyValuePair<string, string> kvp in TeamNamesShort)
                                      {
                                          var temppst = new Dictionary<int, PlayerStats>();
                                          InteropBR.ImportRealStats(kvp, out realtst[TeamOrder[kvp.Key]],
                                                                    out realtstopp[TeamOrder[kvp.Key]], out temppst);
                                          foreach (KeyValuePair<int, PlayerStats> kvp2 in temppst)
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
                                                      int len = realtst.GetLength(0);

                                                      tst = new TeamStats[len];
                                                      tstopp = new TeamStats[len];
                                                      for (int i = 0; i < len; i++)
                                                      {
                                                          foreach (KeyValuePair<string, int> kvp in TeamOrder)
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
                                                      cmbTeam1.SelectedIndex = -1;
                                                      cmbTeam1.SelectedIndex = 0;
                                                      txtFile.Text = file;
                                                      SQLiteIO.LoadSeason(file, ref tst, ref tstopp, ref pst, ref TeamOrder,
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
                status.Content = "Downloading real NBA stats (" + percentage + "% complete)...";
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
            TeamStats curteam = tst[TeamOrder[cmbTeam1.SelectedItem.ToString()]];

            var vw = new versusW(curteam, "Current", realteam, "Real");
            vw.ShowDialog();
            */
        }

        private void btnCompareOtherFile_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Title = "Select the TST file that has the team stats you want to compare to...";
            ofd.Filter = "Team Stats files (*.tst)|*.tst";
            ofd.InitialDirectory = AppDocsPath;
            ofd.ShowDialog();

            string file = ofd.FileName;
            if (file != "")
            {
                string team = cmbTeam1.SelectedItem.ToString();
                string safefn = Tools.getSafeFilename(file);
                var _newTeamOrder = new SortedDictionary<string, int>();

                FileStream stream = File.Open(ofd.FileName, FileMode.Open);
                var bf = new BinaryFormatter();
                bf.AssemblyFormat = FormatterAssemblyStyle.Simple;

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

                var vw = new versusW(curteam, "Current", newteam, "Other");
                vw.ShowDialog();
            }
        }

        private void txtFile_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtFile.ScrollToHorizontalOffset(txtFile.GetRectFromCharacterIndex(txtFile.Text.Length).Right);
            currentDB = txtFile.Text;
            PopulateSeasonCombo();
            db = new SQLiteDatabase(currentDB);
        }

        private void PopulateSeasonCombo()
        {
            for (int i = SQLiteIO.getMaxSeason(MainWindow.currentDB); i > 0; i--)
            {
                cmbSeasonNum.Items.Add(i.ToString());
            }

            cmbSeasonNum.SelectedItem = curSeason.ToString();
        }

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

                SQLiteIO.LoadSeason(ofd1.FileName, ref tst, ref tstopp, ref pst, ref TeamOrder, ref pt, ref bshist, true);
                cmbTeam1.SelectedIndex = 0;
            }

            var ofd = new OpenFileDialog();
            ofd.Title = "Select the TST file that has the team stats you want to compare to...";
            ofd.Filter = "Team Stats files (*.tst)|*.tst";
            ofd.InitialDirectory = AppDocsPath;
            ofd.ShowDialog();

            if (ofd.FileName == "") return;

            string team = cmbTeam1.SelectedItem.ToString();
            int id = TeamOrder[team];

            TeamStats[] curTST = tst;

            var oldTeamOrder = new SortedDictionary<string, int>();
            var oldPT = new PlayoffTree();
            IList<BoxScoreEntry> oldbshist = new List<BoxScoreEntry>();
            var oldTST = new TeamStats[1];
            var oldTSTopp = new TeamStats[1];
            SQLiteIO.LoadSeason(ofd.FileName, ref oldTST, ref oldTSTopp, ref pst, ref oldTeamOrder, ref oldPT, ref oldbshist,
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

            string str = "";

            string team1 = tst[maxi].name;
            if (diffrnk[maxi][0] > 0)
            {
                str =
                    String.Format(
                        "Most improved in {7}, the {0}. They were #{1} ({4:F1}), climbing {3} places they are now at #{2} ({5:F1}), a {6:F1} {7} difference!",
                        tst[maxi].name, oldR.rankings[maxi][0], curR.rankings[maxi][0], diffrnk[maxi][0],
                        oldTST[maxi].averages[0], tst[maxi].averages[0], diffavg[maxi][0], "PPG");
            }
            else
            {
                str =
                    String.Format(
                        "Most improved in {7}, the {0}. They were #{1} ({4:F1}) and they are now at #{2} ({5:F1}), a {6:F1} {7} difference!",
                        tst[maxi].name, oldR.rankings[maxi][0], curR.rankings[maxi][0], diffrnk[maxi][0],
                        oldTST[maxi].averages[0], tst[maxi].averages[0], diffavg[maxi][0], "PPG");
            }
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

            var tw = new trendsW(str, team1, team2);
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

        private float[][] calculateDifferenceAverage(TeamStats[] curTST, TeamStats[] oldTST)
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

            testW tw = new testW(ds);
            tw.ShowDialog();
            */
        }

        private void mnuHistoryBoxScores_Click(object sender, RoutedEventArgs e)
        {
            if (SQLiteIO.isTSTEmpty()) return;

            bs = new BoxScore();
            var bsw = new boxScoreW(boxScoreW.Mode.View);
            bsw.ShowDialog();

            UpdateBoxScore();
        }

        public static void UpdateBoxScore()
        {
            if (bs.bshistid != -1)
            {
                if (bs.done)
                {
                    var list = new List<PlayerBoxScore>();
                    foreach (BindingList<PlayerBoxScore> pbsList in pbsLists)
                    {
                        foreach (PlayerBoxScore pbs in pbsList)
                        {
                            list.Add(pbs);
                        }
                    }

                    bshist[bs.bshistid].bs = bs;
                    bshist[bs.bshistid].pbsList = list;
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

            var tow = new teamOverviewW();
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
            var low = new leagueOverviewW(tst, tstopp, pst);
            low.ShowDialog();
            dispatcherTimer.Start();
        }

        private void mnuFileNew_Click(object sender, RoutedEventArgs e)
        {
            var sfd = new SaveFileDialog();
            sfd.Filter = "NST Database (*.tst)|*.tst";
            sfd.InitialDirectory = AppDocsPath;
            sfd.ShowDialog();

            if (sfd.FileName == "") return;

            File.Delete(sfd.FileName);

            db = new SQLiteDatabase(sfd.FileName);

            SQLiteIO.prepareNewDB(db, 1, 1);

            curSeason = 1;
            txbCurSeason.Text = "Current Season: 1/1";

            tst = new TeamStats[1];
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
            var aw = new addW(ref pst);
            aw.ShowDialog();

            if (!String.IsNullOrEmpty(addInfo))
            {
                if (addInfo != "$$NST Players Added")
                {
                    string[] parts = Regex.Split(addInfo, "\r\n");
                    var newTeams = new List<string>();
                    foreach (string s in parts)
                    {
                        if (!String.IsNullOrWhiteSpace(s))
                            newTeams.Add(s);
                    }

                    int oldlen = tst.GetLength(0);
                    if (SQLiteIO.isTSTEmpty()) oldlen = 0;

                    Array.Resize(ref tst, oldlen + newTeams.Count);
                    Array.Resize(ref tstopp, oldlen + newTeams.Count);

                    for (int i = 0; i < newTeams.Count; i++)
                    {
                        tst[oldlen + i] = new TeamStats(newTeams[i]);
                        tstopp[oldlen + i] = new TeamStats(newTeams[i]);
                        TeamOrder.Add(newTeams[i], oldlen + i);
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

                    string q = "alter table Teams rename to TeamsS" + curSeason;
                    int code = db.ExecuteNonQuery(q);

                    q = "alter table PlayoffTeams rename to PlayoffTeamsS" + curSeason;
                    code = db.ExecuteNonQuery(q);

                    q = "alter table Opponents rename to OpponentsS" + curSeason;
                    code = db.ExecuteNonQuery(q);

                    q = "alter table PlayoffOpponents rename to PlayoffOpponentsS" + curSeason;
                    code = db.ExecuteNonQuery(q);

                    q = "alter table Players rename to PlayersS" + curSeason;
                    code = db.ExecuteNonQuery(q);

                    curSeason++;

                    SQLiteIO.prepareNewDB(db, curSeason, curSeason, true);

                    txbCurSeason.Text = "Current Season: " + curSeason.ToString() + "/" + curSeason.ToString();
                    foreach (TeamStats ts in tst)
                    {
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
                    }

                    foreach (TeamStats ts in tstopp)
                    {
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
                    }

                    foreach (KeyValuePair<int, PlayerStats> ps in pst)
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

            var pow = new playerOverviewW();
            pow.ShowDialog();
        }

        private void mnuMiscImportBoxScores_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "NST Database (*.tst)|*.tst";
            ofd.InitialDirectory = AppDocsPath;
            ofd.Title = "Please select the TST file that you want to import from...";
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
                                else if (r == MessageBoxResult.No)
                                {
                                    bshist.Remove(bse);
                                    break;
                                }
                            }
                            newbse.bs.id = GetFreeBseId();
                            break;
                        }
                    }
                    if (!doNotAdd) bshist.Add(newbse);
                }
            }
        }

        private int GetFreeBseId()
        {
            var bseIDs = new List<int>();
            foreach (BoxScoreEntry bse in bshist)
            {
                bseIDs.Add(bse.bs.id);
            }

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
            var etw = new enableTeamsW(currentDB, curSeason, SQLiteIO.getMaxSeason(currentDB));
            etw.ShowDialog();

            if (addInfo == "$$TEAMSENABLED")
            {
                SQLiteIO.GetAllTeamStatsFromDatabase(currentDB, curSeason, ref tst, ref tstopp, ref TeamOrder);
                updateStatus("Teams were enabled/disabled. Database saved.");
            }
        }

        private void cmbSeasonNum_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            curSeason = Convert.ToInt32(cmbSeasonNum.SelectedItem.ToString());
            if (!loadingSeason) SQLiteIO.LoadSeason();
        }

        private void mnuMiscDeleteBoxScores_Click(object sender, RoutedEventArgs e)
        {
            BoxScoreListW bslw = new BoxScoreListW();
            bslw.ShowDialog();
            SQLiteIO.LoadSeason();
        }
    }
}