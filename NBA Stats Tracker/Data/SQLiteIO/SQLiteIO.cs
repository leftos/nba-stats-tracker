#region Copyright Notice

// Created by Lefteris Aslanoglou, (c) 2011-2013
// 
// Initial development until v1.0 done as part of the implementation of thesis
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
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using LeftosCommonLibrary;
using NBA_Stats_Tracker.Data.BoxScores;
using NBA_Stats_Tracker.Data.Other;
using NBA_Stats_Tracker.Data.PastStats;
using NBA_Stats_Tracker.Data.Players;
using NBA_Stats_Tracker.Data.Teams;
using NBA_Stats_Tracker.Windows;
using SQLite_Database;

#endregion

namespace NBA_Stats_Tracker.Data.SQLiteIO
{
    /// <summary>
    ///     Implements all SQLite-related input/output methods.
    /// </summary>
    internal static class SQLiteIO
    {
        private static bool upgrading;

        private static string createCareerHighsQuery =
            "CREATE TABLE \"CareerHighs\" (\"PlayerID\" INTEGER ,\"MINS\" INTEGER , \"PTS\" INTEGER ,\"REB\" INTEGER ," +
            "\"AST\" INTEGER ,\"STL\" INTEGER ,\"BLK\" INTEGER ,\"TOS\" INTEGER ,\"FGM\" INTEGER ,\"FGA\" INTEGER ," +
            "\"TPM\" INTEGER ,\"TPA\" INTEGER ,\"FTM\" INTEGER ,\"FTA\" INTEGER ,\"OREB\" INTEGER , \"DREB\" INTEGER, " +
            "\"FOUL\" INTEGER, PRIMARY KEY (\"PlayerID\") )";

        /// <summary>
        ///     Saves the database to a new file.
        /// </summary>
        /// <param name="file">The file to save to.</param>
        /// <returns>
        ///     <c>true</c> if the operation succeeded, <c>false</c> otherwise.
        /// </returns>
        public static bool SaveDatabaseAs(string file)
        {
            string oldDB = MainWindow.currentDB + ".tmp";
            File.Copy(MainWindow.currentDB, oldDB, true);
            MainWindow.currentDB = oldDB;
            try
            {
                File.Delete(file);
            }
            catch
            {
                MessageBox.Show("Error while trying to overwrite file. Make sure the file is not in use by another program.");
                return false;
            }
            saveAllSeasons(file);
            SetSetting(file, "Game Length", MainWindow.gameLength);
            SetSetting(file, "Season Length", MainWindow.seasonLength);
            File.Delete(oldDB);
            return true;
        }

        /// <summary>
        ///     Saves all seasons to the specified database.
        /// </summary>
        /// <param name="file">The file.</param>
        public static void saveAllSeasons(string file)
        {
            string oldDB = MainWindow.currentDB;
            int oldSeason = MainWindow.curSeason;

            int maxSeason = getMaxSeason(oldDB);

            if (MainWindow.tf.isBetween)
            {
                MainWindow.tf = new Timeframe(oldSeason);
                MainWindow.UpdateAllData();
            }
            saveSeasonToDatabase(file, MainWindow.tst, MainWindow.tstopp, MainWindow.pst, MainWindow.curSeason, maxSeason);

            for (int i = 1; i <= maxSeason; i++)
            {
                if (i != oldSeason)
                {
                    LoadSeason(oldDB, i, doNotLoadBoxScores: true);
                    saveSeasonToDatabase(file, MainWindow.tst, MainWindow.tstopp, MainWindow.pst, MainWindow.curSeason, maxSeason,
                                         doNotSaveBoxScores: true);
                }
            }
            LoadSeason(file, oldSeason, doNotLoadBoxScores: true);
        }

        /// <summary>
        ///     Saves the conferences and divisions to a specified database.
        /// </summary>
        /// <param name="file">The database.</param>
        public static void SaveConferencesAndDivisions(string file)
        {
            var db = new SQLiteDatabase(file);
            db.ClearTable("Conferences");
            foreach (var conf in MainWindow.Conferences)
            {
                db.Insert("Conferences", new Dictionary<string, string> {{"ID", conf.ID.ToString()}, {"Name", conf.Name}});
            }
            db.ClearTable("Divisions");
            foreach (var div in MainWindow.Divisions)
            {
                db.Insert("Divisions",
                          new Dictionary<string, string>
                          {
                              {"ID", div.ID.ToString()},
                              {"Name", div.Name},
                              {"Conference", div.ConferenceID.ToString()}
                          });
            }
        }

        /// <summary>
        ///     Saves the season to the current database.
        /// </summary>
        public static void saveSeasonToDatabase()
        {
            saveSeasonToDatabase(MainWindow.currentDB, MainWindow.tst, MainWindow.tstopp, MainWindow.pst, MainWindow.curSeason,
                                 getMaxSeason(MainWindow.currentDB));
        }

        /// <summary>
        ///     Saves the season to a specified database.
        /// </summary>
        /// <param name="file">The database.</param>
        /// <param name="tstToSave">The TeamStats dictionary to save.</param>
        /// <param name="tstoppToSave">The opposing TeamStats dictionary to save.</param>
        /// <param name="pstToSave">The PlayerStats dictionary to save.</param>
        /// <param name="season">The season ID.</param>
        /// <param name="maxSeason">The max season ID.</param>
        /// <param name="doNotSaveBoxScores">
        ///     if set to <c>true</c>, will not save box scores.
        /// </param>
        /// <param name="partialUpdate">
        ///     if set to <c>true</c>, a partial update will be made (i.e. any pre-existing data won't be cleared before writing the current data).
        /// </param>
        public static void saveSeasonToDatabase(string file, Dictionary<int, TeamStats> tstToSave, Dictionary<int, TeamStats> tstoppToSave,
                                                Dictionary<int, PlayerStats> pstToSave, int season, int maxSeason,
                                                bool doNotSaveBoxScores = false, bool partialUpdate = false)
        {
            // Delete the file and create it from scratch. If partial updating is implemented later, maybe
            // we won't delete the file before all this.
            //File.Delete(file); 

            // Isn't really needed since we delete the file, but is left for partial updating efforts later.
            bool FileExists = File.Exists(file);

            // SQLite
            //try
            //{
            MainWindow.db = new SQLiteDatabase(file);
            if (!FileExists)
                prepareNewDB(MainWindow.db, season, maxSeason);

            SaveConferencesAndDivisions(file);

            SaveSeasonName(season);

            SaveTeamsToDatabase(file, tstToSave, tstoppToSave, season, maxSeason);

            #region Save Player Stats

            SavePlayersToDatabase(file, pstToSave, season, maxSeason, partialUpdate);

            #endregion

            #region Save Box Scores

            if (!doNotSaveBoxScores)
            {
                const string q = "select GameID from GameResults;";
                DataTable res = MainWindow.db.GetDataTable(q);
                List<int> idList = (from DataRow r in res.Rows select Convert.ToInt32(r[0].ToString())).ToList();

                var sqlinsert = new List<Dictionary<string, string>>();
                for (int i = 0; i < MainWindow.bshist.Count; i++)
                {
                    BoxScoreEntry bse = MainWindow.bshist[i];
                    string md5 = Tools.GetMD5((new Random()).Next().ToString());
                    if ((!FileExists) || (bse.bs.id == -1) || (!idList.Contains(bse.bs.id)) || (bse.mustUpdate))
                    {
                        var dict2 = new Dictionary<string, string>
                                    {
                                        {"Team1ID", bse.bs.Team1ID.ToString()},
                                        {"Team2ID", bse.bs.Team2ID.ToString()},
                                        {"Date", String.Format("{0:yyyy-MM-dd HH:mm:ss}", bse.bs.gamedate)},
                                        {"SeasonNum", bse.bs.SeasonNum.ToString()},
                                        {"IsPlayoff", bse.bs.isPlayoff.ToString()},
                                        {"T1PTS", bse.bs.PTS1.ToString()},
                                        {"T1REB", bse.bs.REB1.ToString()},
                                        {"T1AST", bse.bs.AST1.ToString()},
                                        {"T1STL", bse.bs.STL1.ToString()},
                                        {"T1BLK", bse.bs.BLK1.ToString()},
                                        {"T1TOS", bse.bs.TO1.ToString()},
                                        {"T1FGM", bse.bs.FGM1.ToString()},
                                        {"T1FGA", bse.bs.FGA1.ToString()},
                                        {"T13PM", bse.bs.TPM1.ToString()},
                                        {"T13PA", bse.bs.TPA1.ToString()},
                                        {"T1FTM", bse.bs.FTM1.ToString()},
                                        {"T1FTA", bse.bs.FTA1.ToString()},
                                        {"T1OREB", bse.bs.OREB1.ToString()},
                                        {"T1FOUL", bse.bs.FOUL1.ToString()},
                                        {"T1MINS", bse.bs.MINS1.ToString()},
                                        {"T2PTS", bse.bs.PTS2.ToString()},
                                        {"T2REB", bse.bs.REB2.ToString()},
                                        {"T2AST", bse.bs.AST2.ToString()},
                                        {"T2STL", bse.bs.STL2.ToString()},
                                        {"T2BLK", bse.bs.BLK2.ToString()},
                                        {"T2TOS", bse.bs.TO2.ToString()},
                                        {"T2FGM", bse.bs.FGM2.ToString()},
                                        {"T2FGA", bse.bs.FGA2.ToString()},
                                        {"T23PM", bse.bs.TPM2.ToString()},
                                        {"T23PA", bse.bs.TPA2.ToString()},
                                        {"T2FTM", bse.bs.FTM2.ToString()},
                                        {"T2FTA", bse.bs.FTA2.ToString()},
                                        {"T2OREB", bse.bs.OREB2.ToString()},
                                        {"T2FOUL", bse.bs.FOUL2.ToString()},
                                        {"T2MINS", bse.bs.MINS2.ToString()},
                                        {"HASH", md5}
                                    };

                        if (idList.Contains(bse.bs.id))
                        {
                            MainWindow.db.Update("GameResults", dict2, "GameID = " + bse.bs.id);
                        }
                        else
                        {
                            MainWindow.db.Insert("GameResults", dict2);

                            int lastid =
                                Convert.ToInt32(
                                    MainWindow.db.GetDataTable("select GameID from GameResults where HASH LIKE \"" + md5 + "\"").Rows[0][
                                        "GameID"].ToString());
                            bse.bs.id = lastid;
                        }
                        MainWindow.db.Delete("PlayerResults", "GameID = " + bse.bs.id);

                        //var used = new List<int>();
                        for (int j = 0; j < bse.pbsList.Count; j++)
                        {
                            PlayerBoxScore pbs = bse.pbsList[j];
                            //int id = GetFreePlayerResultID(file, used);
                            //used.Add(id);
                            dict2 = new Dictionary<string, string>
                                    {
//{"ID", id.ToString()},
                                        {"GameID", bse.bs.id.ToString()},
                                        {"PlayerID", pbs.PlayerID.ToString()},
                                        {"TeamID", pbs.TeamID.ToString()},
                                        {"isStarter", pbs.isStarter.ToString()},
                                        {"playedInjured", pbs.playedInjured.ToString()},
                                        {"isOut", pbs.isOut.ToString()},
                                        {"MINS", pbs.MINS.ToString()},
                                        {"PTS", pbs.PTS.ToString()},
                                        {"REB", pbs.REB.ToString()},
                                        {"AST", pbs.AST.ToString()},
                                        {"STL", pbs.STL.ToString()},
                                        {"BLK", pbs.BLK.ToString()},
                                        {"TOS", pbs.TOS.ToString()},
                                        {"FGM", pbs.FGM.ToString()},
                                        {"FGA", pbs.FGA.ToString()},
                                        {"TPM", pbs.TPM.ToString()},
                                        {"TPA", pbs.TPA.ToString()},
                                        {"FTM", pbs.FTM.ToString()},
                                        {"FTA", pbs.FTA.ToString()},
                                        {"OREB", pbs.OREB.ToString()},
                                        {"FOUL", pbs.FOUL.ToString()}
                                    };

                            sqlinsert.Add(dict2);
                            /*
                            if (sqlinsert.Count == 500)
                            {
                                int linesAffected = MainWindow.db.InsertMany("PlayerResults", sqlinsert);
                                //Thread.Sleep(2000);
                                DataTable dt = MainWindow.db.GetDataTable("SELECT * FROM PlayerResults");
                                Debug.Print(dt.Rows.Count.ToString() + " " + linesAffected.ToString());
                                sqlinsert.Clear();
                            }
                            */
                            //db.Insert("PlayerResults", dict2);
                        }
                    }
                }
                if (sqlinsert.Count > 0)
                {
                    MainWindow.db.InsertManyTransaction("PlayerResults", sqlinsert);
                    //int linesAffected = MainWindow.db.InsertMany("PlayerResults", sqlinsert);
                    //Thread.Sleep(500);
                    //DataTable dt = MainWindow.db.GetDataTable("SELECT * FROM PlayerResults");
                    //Debug.Print(dt.Rows.Count.ToString() + " " + linesAffected.ToString());
                }
            }

            #endregion

            MainWindow.mwInstance.txtFile.Text = file;
            MainWindow.currentDB = file;

            //}
            //catch (Exception ex)
            //{
            //App.errorReport(ex, "Trying to save team stats - SQLite");
            //}
        }

        /// <summary>
        ///     Saves the name of the season.
        /// </summary>
        /// <param name="season">The season.</param>
        /// <exception cref="System.Exception">Raised if the specified season ID doesn't correspond to a season existing in the database.</exception>
        public static void SaveSeasonName(int season)
        {
            var dict = new Dictionary<string, string> {{"ID", season.ToString()}};
            try
            {
                dict.Add("Name", MainWindow.GetSeasonName(season));
            }
            catch
            {
                dict.Add("Name", season.ToString());
            }
            try
            {
                int result = MainWindow.db.Update("SeasonNames", dict, "ID = " + season.ToString());
                if (result < 1)
                    throw (new Exception());
            }
            catch (Exception)
            {
                MainWindow.db.Insert("SeasonNames", dict);
            }
        }

        /// <summary>
        ///     Saves the teams to a specified database.
        /// </summary>
        /// <param name="file">The database.</param>
        /// <param name="tstToSave">The TeamStats dictionary to save.</param>
        /// <param name="tstoppToSave">The opposing TeamStats dictionary to save.</param>
        /// <param name="season">The season ID.</param>
        /// <param name="maxSeason">The max season ID.</param>
        private static void SaveTeamsToDatabase(string file, Dictionary<int, TeamStats> tstToSave, Dictionary<int, TeamStats> tstoppToSave,
                                                int season, int maxSeason)
        {
            var _db = new SQLiteDatabase(file);
            string teamsT = "Teams";
            string pl_teamsT = "PlayoffTeams";
            string oppT = "Opponents";
            string pl_oppT = "PlayoffOpponents";

            if (season != maxSeason)
            {
                teamsT += "S" + season;
                pl_teamsT += "S" + season;
                oppT += "S" + season;
                pl_oppT += "S" + season;
            }

            _db.ClearTable(teamsT);
            _db.ClearTable(pl_teamsT);
            _db.ClearTable(oppT);
            _db.ClearTable(pl_oppT);

            String q = "select Name from " + teamsT + ";";

            try
            {
                _db.GetDataTable(q);
            }
            catch
            {
                prepareNewDB(_db, season, maxSeason, onlyNewSeason: true);
                _db.GetDataTable(q);
            }

            var seasonList = new List<Dictionary<string, string>>(500);
            var playoffList = new List<Dictionary<string, string>>(500);
            int i = 0;
            foreach (var key in tstToSave.Keys)
            {
                //bool found = false;

                var dict = new Dictionary<string, string>
                           {
                               {"ID", tstToSave[key].ID.ToString()},
                               {"Name", tstToSave[key].name},
                               {"DisplayName", tstToSave[key].displayName},
                               {"isHidden", tstToSave[key].isHidden.ToString()},
                               {"Division", tstToSave[key].division.ToString()},
                               {"Conference", tstToSave[key].conference.ToString()},
                               {"WIN", tstToSave[key].winloss[0].ToString()},
                               {"LOSS", tstToSave[key].winloss[1].ToString()},
                               {"MINS", tstToSave[key].stats[t.MINS].ToString()},
                               {"PF", tstToSave[key].stats[t.PF].ToString()},
                               {"PA", tstToSave[key].stats[t.PA].ToString()},
                               {"FGM", tstToSave[key].stats[t.FGM].ToString()},
                               {"FGA", tstToSave[key].stats[t.FGA].ToString()},
                               {"TPM", tstToSave[key].stats[t.TPM].ToString()},
                               {"TPA", tstToSave[key].stats[t.TPA].ToString()},
                               {"FTM", tstToSave[key].stats[t.FTM].ToString()},
                               {"FTA", tstToSave[key].stats[t.FTA].ToString()},
                               {"OREB", tstToSave[key].stats[t.OREB].ToString()},
                               {"DREB", tstToSave[key].stats[t.DREB].ToString()},
                               {"STL", tstToSave[key].stats[t.STL].ToString()},
                               {"TOS", tstToSave[key].stats[t.TOS].ToString()},
                               {"BLK", tstToSave[key].stats[t.BLK].ToString()},
                               {"AST", tstToSave[key].stats[t.AST].ToString()},
                               {"FOUL", tstToSave[key].stats[t.FOUL].ToString()},
                               {"OFFSET", tstToSave[key].offset.ToString()}
                           };

                seasonList.Add(dict);

                var pl_dict = new Dictionary<string, string>
                              {
                                  {"ID", MainWindow.TeamOrder[tstToSave[key].name].ToString()},
                                  {"Name", tstToSave[key].name},
                                  {"DisplayName", tstToSave[key].displayName},
                                  {"isHidden", tstToSave[key].isHidden.ToString()},
                                  {"Division", tstToSave[key].division.ToString()},
                                  {"Conference", tstToSave[key].conference.ToString()},
                                  {"WIN", tstToSave[key].pl_winloss[0].ToString()},
                                  {"LOSS", tstToSave[key].pl_winloss[1].ToString()},
                                  {"MINS", tstToSave[key].pl_stats[t.MINS].ToString()},
                                  {"PF", tstToSave[key].pl_stats[t.PF].ToString()},
                                  {"PA", tstToSave[key].pl_stats[t.PA].ToString()},
                                  {"FGM", tstToSave[key].pl_stats[t.FGM].ToString()},
                                  {"FGA", tstToSave[key].pl_stats[t.FGA].ToString()},
                                  {"TPM", tstToSave[key].pl_stats[t.TPM].ToString()},
                                  {"TPA", tstToSave[key].pl_stats[t.TPA].ToString()},
                                  {"FTM", tstToSave[key].pl_stats[t.FTM].ToString()},
                                  {"FTA", tstToSave[key].pl_stats[t.FTA].ToString()},
                                  {"OREB", tstToSave[key].pl_stats[t.OREB].ToString()},
                                  {"DREB", tstToSave[key].pl_stats[t.DREB].ToString()},
                                  {"STL", tstToSave[key].pl_stats[t.STL].ToString()},
                                  {"TOS", tstToSave[key].pl_stats[t.TOS].ToString()},
                                  {"BLK", tstToSave[key].pl_stats[t.BLK].ToString()},
                                  {"AST", tstToSave[key].pl_stats[t.AST].ToString()},
                                  {"FOUL", tstToSave[key].pl_stats[t.FOUL].ToString()},
                                  {"OFFSET", tstToSave[key].pl_offset.ToString()}
                              };

                playoffList.Add(pl_dict);

                i++;

                if (i == 500)
                {
                    _db.InsertManyUnion(teamsT, seasonList);
                    _db.InsertManyUnion(pl_teamsT, playoffList);
                    i = 0;
                    seasonList.Clear();
                    playoffList.Clear();
                }
                /*
                foreach (DataRow r in res.Rows)
                {
                    if (r[0].ToString().Equals(ts.name))
                    {
                        _db.Update(teamsT, dict, "Name LIKE \'" + ts.name + "\'");
                        _db.Update(pl_teamsT, pl_dict, "Name LIKE \'" + ts.name + "\'");
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    _db.Insert(teamsT, dict);
                    _db.Insert(pl_teamsT, pl_dict);
                }
                */
            }
            if (i > 0)
            {
                _db.InsertManyUnion(teamsT, seasonList);
                _db.InsertManyUnion(pl_teamsT, playoffList);
            }

            seasonList = new List<Dictionary<string, string>>(500);
            playoffList = new List<Dictionary<string, string>>(500);
            i = 0;
            foreach (var key in tstoppToSave.Keys)
            {
                //bool found = false;

                var dict = new Dictionary<string, string>
                           {
                               {"ID", MainWindow.TeamOrder[tstoppToSave[key].name].ToString()},
                               {"Name", tstoppToSave[key].name},
                               {"DisplayName", tstoppToSave[key].displayName},
                               {"isHidden", tstoppToSave[key].isHidden.ToString()},
                               {"Division", tstoppToSave[key].division.ToString()},
                               {"Conference", tstoppToSave[key].conference.ToString()},
                               {"WIN", tstoppToSave[key].winloss[0].ToString()},
                               {"LOSS", tstoppToSave[key].winloss[1].ToString()},
                               {"MINS", tstoppToSave[key].stats[t.MINS].ToString()},
                               {"PF", tstoppToSave[key].stats[t.PF].ToString()},
                               {"PA", tstoppToSave[key].stats[t.PA].ToString()},
                               {"FGM", tstoppToSave[key].stats[t.FGM].ToString()},
                               {"FGA", tstoppToSave[key].stats[t.FGA].ToString()},
                               {"TPM", tstoppToSave[key].stats[t.TPM].ToString()},
                               {"TPA", tstoppToSave[key].stats[t.TPA].ToString()},
                               {"FTM", tstoppToSave[key].stats[t.FTM].ToString()},
                               {"FTA", tstoppToSave[key].stats[t.FTA].ToString()},
                               {"OREB", tstoppToSave[key].stats[t.OREB].ToString()},
                               {"DREB", tstoppToSave[key].stats[t.DREB].ToString()},
                               {"STL", tstoppToSave[key].stats[t.STL].ToString()},
                               {"TOS", tstoppToSave[key].stats[t.TOS].ToString()},
                               {"BLK", tstoppToSave[key].stats[t.BLK].ToString()},
                               {"AST", tstoppToSave[key].stats[t.AST].ToString()},
                               {"FOUL", tstoppToSave[key].stats[t.FOUL].ToString()},
                               {"OFFSET", tstoppToSave[key].offset.ToString()}
                           };

                seasonList.Add(dict);

                var pl_dict = new Dictionary<string, string>
                              {
                                  {"ID", MainWindow.TeamOrder[tstoppToSave[key].name].ToString()},
                                  {"Name", tstoppToSave[key].name},
                                  {"DisplayName", tstoppToSave[key].displayName},
                                  {"isHidden", tstoppToSave[key].isHidden.ToString()},
                                  {"Division", tstoppToSave[key].division.ToString()},
                                  {"Conference", tstoppToSave[key].conference.ToString()},
                                  {"WIN", tstoppToSave[key].pl_winloss[0].ToString()},
                                  {"LOSS", tstoppToSave[key].pl_winloss[1].ToString()},
                                  {"MINS", tstoppToSave[key].pl_stats[t.MINS].ToString()},
                                  {"PF", tstoppToSave[key].pl_stats[t.PF].ToString()},
                                  {"PA", tstoppToSave[key].pl_stats[t.PA].ToString()},
                                  {"FGM", tstoppToSave[key].pl_stats[t.FGM].ToString()},
                                  {"FGA", tstoppToSave[key].pl_stats[t.FGA].ToString()},
                                  {"TPM", tstoppToSave[key].pl_stats[t.TPM].ToString()},
                                  {"TPA", tstoppToSave[key].pl_stats[t.TPA].ToString()},
                                  {"FTM", tstoppToSave[key].pl_stats[t.FTM].ToString()},
                                  {"FTA", tstoppToSave[key].pl_stats[t.FTA].ToString()},
                                  {"OREB", tstoppToSave[key].pl_stats[t.OREB].ToString()},
                                  {"DREB", tstoppToSave[key].pl_stats[t.DREB].ToString()},
                                  {"STL", tstoppToSave[key].pl_stats[t.STL].ToString()},
                                  {"TOS", tstoppToSave[key].pl_stats[t.TOS].ToString()},
                                  {"BLK", tstoppToSave[key].pl_stats[t.BLK].ToString()},
                                  {"AST", tstoppToSave[key].pl_stats[t.AST].ToString()},
                                  {"FOUL", tstoppToSave[key].pl_stats[t.FOUL].ToString()},
                                  {"OFFSET", tstoppToSave[key].pl_offset.ToString()}
                              };

                playoffList.Add(pl_dict);

                i++;

                if (i == 500)
                {
                    _db.InsertManyUnion(oppT, seasonList);
                    _db.InsertManyUnion(pl_oppT, playoffList);
                    i = 0;
                    seasonList.Clear();
                    playoffList.Clear();
                }

                /*
                foreach (DataRow r in res.Rows)
                {
                    if (r[0].ToString().Equals(ts.name))
                    {
                        _db.Update(oppT, dict, "Name LIKE \'" + ts.name + "\'");
                        _db.Update(pl_oppT, pl_dict, "Name LIKE \'" + ts.name + "\'");
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    _db.Insert(oppT, dict);
                    _db.Insert(pl_oppT, pl_dict);
                }
                */
            }
            if (i > 0)
            {
                _db.InsertManyUnion(oppT, seasonList);
                _db.InsertManyUnion(pl_oppT, playoffList);
            }
        }

        /// <summary>
        ///     Saves the players to a specified database.
        /// </summary>
        /// <param name="file">The database.</param>
        /// <param name="playerStats">The player stats.</param>
        /// <param name="season">The season ID.</param>
        /// <param name="maxSeason">The max season ID.</param>
        /// <param name="partialUpdate">
        ///     if set to <c>true</c>, a partial update will be made (i.e. any pre-existing data won't be cleared before writing the current data).
        /// </param>
        public static void SavePlayersToDatabase(string file, Dictionary<int, PlayerStats> playerStats, int season, int maxSeason,
                                                 bool partialUpdate = false)
        {
            var _db = new SQLiteDatabase(file);

            string playersT = "Players";
            string pl_playersT = "PlayoffPlayers";

            if (season != maxSeason)
            {
                playersT += "S" + season.ToString();
                pl_playersT += "S" + season.ToString();
            }

            if (!partialUpdate)
            {
                MainWindow.db.ClearTable(playersT);
                MainWindow.db.ClearTable(pl_playersT);
                MainWindow.db.ClearTable("CareerHighs");
            }
            string q = "select ID from " + playersT + ";";
            DataTable res = MainWindow.db.GetDataTable(q);

            //var idList = (from DataRow dr in res.Rows select Convert.ToInt32(dr["ID"].ToString())).ToList();

            var sqlinsert = new List<Dictionary<string, string>>();
            var pl_sqlinsert = new List<Dictionary<string, string>>();
            var ch_sqlinsert = new List<Dictionary<string, string>>();
            int i = 0;

            foreach (var kvp in playerStats)
            {
                PlayerStats ps = kvp.Value;
                if (partialUpdate)
                {
                    _db.Delete(playersT, "ID = " + ps.ID);
                    _db.Delete(pl_playersT, "ID = " + ps.ID);
                    _db.Delete("CareerHighs", "PlayerID = " + ps.ID);
                }
                var dict = new Dictionary<string, string>
                           {
                               {"ID", ps.ID.ToString()},
                               {"LastName", ps.LastName},
                               {"FirstName", ps.FirstName},
                               {"Position1", ps.Position1.ToString()},
                               {"Position2", ps.Position2.ToString()},
                               {"isActive", ps.isActive.ToString()},
                               {"YearOfBirth", ps.YearOfBirth.ToString()},
                               {"YearsPro", ps.YearsPro.ToString()},
                               {"isHidden", ps.isHidden.ToString()},
                               {"InjuryType", ps.Injury.InjuryType.ToString()},
                               {"CustomInjuryName", ps.Injury.CustomInjuryName},
                               {"InjuryDaysLeft", ps.Injury.InjuryDaysLeft.ToString()},
                               {"TeamFin", ps.TeamF.ToString()},
                               {"TeamSta", ps.TeamS.ToString()},
                               {"GP", ps.stats[p.GP].ToString()},
                               {"GS", ps.stats[p.GS].ToString()},
                               {"MINS", ps.stats[p.MINS].ToString()},
                               {"PTS", ps.stats[p.PTS].ToString()},
                               {"FGM", ps.stats[p.FGM].ToString()},
                               {"FGA", ps.stats[p.FGA].ToString()},
                               {"TPM", ps.stats[p.TPM].ToString()},
                               {"TPA", ps.stats[p.TPA].ToString()},
                               {"FTM", ps.stats[p.FTM].ToString()},
                               {"FTA", ps.stats[p.FTA].ToString()},
                               {"OREB", ps.stats[p.OREB].ToString()},
                               {"DREB", ps.stats[p.DREB].ToString()},
                               {"STL", ps.stats[p.STL].ToString()},
                               {"TOS", ps.stats[p.TOS].ToString()},
                               {"BLK", ps.stats[p.BLK].ToString()},
                               {"AST", ps.stats[p.AST].ToString()},
                               {"FOUL", ps.stats[p.FOUL].ToString()},
                               {"isAllStar", ps.isAllStar.ToString()},
                               {"isNBAChampion", ps.isNBAChampion.ToString()},
                               {"ContractY1", ps.Contract.TryGetSalary(1).ToString()},
                               {"ContractY2", ps.Contract.TryGetSalary(2).ToString()},
                               {"ContractY3", ps.Contract.TryGetSalary(3).ToString()},
                               {"ContractY4", ps.Contract.TryGetSalary(4).ToString()},
                               {"ContractY5", ps.Contract.TryGetSalary(5).ToString()},
                               {"ContractY6", ps.Contract.TryGetSalary(6).ToString()},
                               {"ContractY7", ps.Contract.TryGetSalary(7).ToString()},
                               {"ContractOption", ((byte) ps.Contract.Option).ToString()},
                               {"Height", ps.height.ToString()},
                               {"Weight", ps.weight.ToString()}
                           };
                var pl_dict = new Dictionary<string, string>
                              {
                                  {"ID", ps.ID.ToString()},
                                  {"GP", ps.pl_stats[p.GP].ToString()},
                                  {"GS", ps.pl_stats[p.GS].ToString()},
                                  {"MINS", ps.pl_stats[p.MINS].ToString()},
                                  {"PTS", ps.pl_stats[p.PTS].ToString()},
                                  {"FGM", ps.pl_stats[p.FGM].ToString()},
                                  {"FGA", ps.pl_stats[p.FGA].ToString()},
                                  {"TPM", ps.pl_stats[p.TPM].ToString()},
                                  {"TPA", ps.pl_stats[p.TPA].ToString()},
                                  {"FTM", ps.pl_stats[p.FTM].ToString()},
                                  {"FTA", ps.pl_stats[p.FTA].ToString()},
                                  {"OREB", ps.pl_stats[p.OREB].ToString()},
                                  {"DREB", ps.pl_stats[p.DREB].ToString()},
                                  {"STL", ps.pl_stats[p.STL].ToString()},
                                  {"TOS", ps.pl_stats[p.TOS].ToString()},
                                  {"BLK", ps.pl_stats[p.BLK].ToString()},
                                  {"AST", ps.pl_stats[p.AST].ToString()},
                                  {"FOUL", ps.pl_stats[p.FOUL].ToString()}
                              };
                var ch_dict = new Dictionary<string, string>
                              {
                                  {"PlayerID", ps.ID.ToString()},
                                  {"MINS", ps.careerHighs[p.MINS].ToString()},
                                  {"PTS", ps.careerHighs[p.PTS].ToString()},
                                  {"FGM", ps.careerHighs[p.FGM].ToString()},
                                  {"FGA", ps.careerHighs[p.FGA].ToString()},
                                  {"TPM", ps.careerHighs[p.TPM].ToString()},
                                  {"TPA", ps.careerHighs[p.TPA].ToString()},
                                  {"FTM", ps.careerHighs[p.FTM].ToString()},
                                  {"FTA", ps.careerHighs[p.FTA].ToString()},
                                  {"REB", ps.careerHighs[p.REB].ToString()},
                                  {"OREB", ps.careerHighs[p.OREB].ToString()},
                                  {"DREB", ps.careerHighs[p.DREB].ToString()},
                                  {"STL", ps.careerHighs[p.STL].ToString()},
                                  {"TOS", ps.careerHighs[p.TOS].ToString()},
                                  {"BLK", ps.careerHighs[p.BLK].ToString()},
                                  {"AST", ps.careerHighs[p.AST].ToString()},
                                  {"FOUL", ps.careerHighs[p.FOUL].ToString()}
                              };

                sqlinsert.Add(dict);
                pl_sqlinsert.Add(pl_dict);
                ch_sqlinsert.Add(ch_dict);
                i++;
            }

            if (i > 0)
            {
                _db.InsertManyTransaction(playersT, sqlinsert);
                _db.InsertManyTransaction(pl_playersT, pl_sqlinsert);
                _db.InsertManyTransaction("CareerHighs", ch_sqlinsert);
            }
        }

        public static void SavePastTeamStatsToDatabase(SQLiteDatabase db, List<PastTeamStats> statsList)
        {
            int teamID;
            try
            {
                teamID = statsList[0].TeamID;
            }
            catch
            {
                return;
            }

            db.Delete("PastTeamStats", "TeamID = " + teamID);

            var sqlinsert = new List<Dictionary<string, string>>();
            var usedIDs = new List<int>();
            foreach (var pts in statsList)
            {
                int idToUse = GetFreeID(MainWindow.currentDB, "PastTeamStats", used: usedIDs);
                usedIDs.Add(idToUse);
                var dict = new Dictionary<string, string>
                           {
                               {"ID", idToUse.ToString()},
                               {"TeamID", pts.TeamID.ToString()},
                               {"SeasonName", pts.SeasonName},
                               {"SOrder", pts.Order.ToString()},
                               {"isPlayoff", pts.isPlayoff.ToString()},
                               {"WIN", pts.Wins.ToString()},
                               {"LOSS", pts.Losses.ToString()},
                               {"MINS", pts.MINS.ToString()},
                               {"PF", pts.PF.ToString()},
                               {"PA", pts.PA.ToString()},
                               {"FGM", pts.FGM.ToString()},
                               {"FGA", pts.FGA.ToString()},
                               {"TPM", pts.TPM.ToString()},
                               {"TPA", pts.TPA.ToString()},
                               {"FTM", pts.FTM.ToString()},
                               {"FTA", pts.FTA.ToString()},
                               {"OREB", pts.OREB.ToString()},
                               {"DREB", pts.DREB.ToString()},
                               {"STL", pts.STL.ToString()},
                               {"TOS", pts.TOS.ToString()},
                               {"BLK", pts.BLK.ToString()},
                               {"AST", pts.AST.ToString()},
                               {"FOUL", pts.FOUL.ToString()},
                           };
                sqlinsert.Add(dict);
            }
            db.InsertManyTransaction("PastTeamStats", sqlinsert);
        }

        public static void SavePastPlayerStatsToDatabase(SQLiteDatabase db, List<PastPlayerStats> statsList)
        {
            statsList.GroupBy(stat => stat.PlayerID)
                     .Select(pair => pair.Key)
                     .ToList()
                     .ForEach(playerID => db.Delete("PastPlayerStats", "PlayerID = " + playerID));

            var sqlinsert = new List<Dictionary<string, string>>();
            var usedIDs = new List<int>();
            foreach (var pps in statsList)
            {
                int idToUse = GetFreeID(MainWindow.currentDB, "PastPlayerStats", used: usedIDs);
                usedIDs.Add(idToUse);
                var dict = new Dictionary<string, string>
                           {
                               {"ID", idToUse.ToString()},
                               {"PlayerID", pps.PlayerID.ToString()},
                               {"SeasonName", pps.SeasonName},
                               {"SOrder", pps.Order.ToString()},
                               {"isPlayoff", pps.isPlayoff.ToString()},
                               {"TeamFin", pps.TeamFName},
                               {"TeamSta", pps.TeamSName},
                               {"GP", pps.GP.ToString()},
                               {"GS", pps.GS.ToString()},
                               {"MINS", pps.MINS.ToString()},
                               {"PTS", pps.PTS.ToString()},
                               {"FGM", pps.FGM.ToString()},
                               {"FGA", pps.FGA.ToString()},
                               {"TPM", pps.TPM.ToString()},
                               {"TPA", pps.TPA.ToString()},
                               {"FTM", pps.FTM.ToString()},
                               {"FTA", pps.FTA.ToString()},
                               {"OREB", pps.OREB.ToString()},
                               {"DREB", pps.DREB.ToString()},
                               {"STL", pps.STL.ToString()},
                               {"TOS", pps.TOS.ToString()},
                               {"BLK", pps.BLK.ToString()},
                               {"AST", pps.AST.ToString()},
                               {"FOUL", pps.FOUL.ToString()},
                           };
                sqlinsert.Add(dict);
            }
            db.InsertManyTransaction("PastPlayerStats", sqlinsert);
        }

        /// <summary>
        ///     Prepares a new DB, or adds a new season to a pre-existing database.
        /// </summary>
        /// <param name="db">The database.</param>
        /// <param name="curSeason">The current season ID.</param>
        /// <param name="maxSeason">The max season ID.</param>
        /// <param name="onlyNewSeason">
        ///     if set to <c>true</c>, a new season will be added to a pre-existing database.
        /// </param>
        public static void prepareNewDB(SQLiteDatabase db, int curSeason, int maxSeason, bool onlyNewSeason = false)
        {
            try
            {
                String qr;

                if (!onlyNewSeason)
                {
                    qr = @"DROP TABLE IF EXISTS ""GameResults""";
                    db.ExecuteNonQuery(qr);
                    qr =
                        @"CREATE TABLE ""GameResults"" (""GameID"" INTEGER PRIMARY KEY NOT NULL ,""Team1ID"" INTEGER NOT NULL ,""Team2ID"" INTEGER NOT NULL, ""Date"" DATE NOT NULL ,""SeasonNum"" INTEGER NOT NULL ,""IsPlayoff"" TEXT NOT NULL DEFAULT ('FALSE') ,""T1PTS"" INTEGER NOT NULL ,""T1REB"" INTEGER NOT NULL ,""T1AST"" INTEGER NOT NULL ,""T1STL"" INTEGER NOT NULL ,""T1BLK"" INTEGER NOT NULL ,""T1TOS"" INTEGER NOT NULL ,""T1FGM"" INTEGER NOT NULL ,""T1FGA"" INTEGER NOT NULL ,""T13PM"" INTEGER NOT NULL ,""T13PA"" INTEGER NOT NULL ,""T1FTM"" INTEGER NOT NULL ,""T1FTA"" INTEGER NOT NULL ,""T1OREB"" INTEGER NOT NULL ,""T1FOUL"" INTEGER NOT NULL,""T1MINS"" INTEGER NOT NULL ,""T2PTS"" INTEGER NOT NULL ,""T2REB"" INTEGER NOT NULL ,""T2AST"" INTEGER NOT NULL ,""T2STL"" INTEGER NOT NULL ,""T2BLK"" INTEGER NOT NULL ,""T2TOS"" INTEGER NOT NULL ,""T2FGM"" INTEGER NOT NULL ,""T2FGA"" INTEGER NOT NULL ,""T23PM"" INTEGER NOT NULL ,""T23PA"" INTEGER NOT NULL ,""T2FTM"" INTEGER NOT NULL ,""T2FTA"" INTEGER NOT NULL ,""T2OREB"" INTEGER NOT NULL ,""T2FOUL"" INTEGER NOT NULL,""T2MINS"" INTEGER NOT NULL, ""HASH"" TEXT )";
                    db.ExecuteNonQuery(qr);
                    qr = @"DROP TABLE IF EXISTS ""PlayerResults""";
                    db.ExecuteNonQuery(qr);
                    qr =
                        @"CREATE TABLE ""PlayerResults"" (""GameID"" INTEGER NOT NULL ,""PlayerID"" INTEGER NOT NULL ,""TeamID"" INTEGER NOT NULL ,""isStarter"" TEXT, ""playedInjured"" TEXT, ""isOut"" TEXT, ""MINS"" INTEGER NOT NULL DEFAULT (0), ""PTS"" INTEGER NOT NULL ,""REB"" INTEGER NOT NULL ,""AST"" INTEGER NOT NULL ,""STL"" INTEGER NOT NULL ,""BLK"" INTEGER NOT NULL ,""TOS"" INTEGER NOT NULL ,""FGM"" INTEGER NOT NULL ,""FGA"" INTEGER NOT NULL ,""TPM"" INTEGER NOT NULL ,""TPA"" INTEGER NOT NULL ,""FTM"" INTEGER NOT NULL ,""FTA"" INTEGER NOT NULL ,""OREB"" INTEGER NOT NULL ,""FOUL"" INTEGER NOT NULL DEFAULT (0), PRIMARY KEY (""GameID"", ""PlayerID"") )";
                    db.ExecuteNonQuery(qr);
                    qr = @"DROP TABLE IF EXISTS ""Misc""";
                    db.ExecuteNonQuery(qr);
                    qr = @"CREATE TABLE ""Misc"" (""Setting"" TEXT PRIMARY KEY,""Value"" TEXT)";
                    db.ExecuteNonQuery(qr);
                    qr = @"DROP TABLE IF EXISTS ""SeasonNames""";
                    db.ExecuteNonQuery(qr);
                    qr = @"CREATE TABLE ""SeasonNames"" (""ID"" INTEGER PRIMARY KEY NOT NULL , ""Name"" TEXT)";
                    db.ExecuteNonQuery(qr);
                    db.Insert("SeasonNames", new Dictionary<string, string> {{"ID", curSeason.ToString()}, {"Name", curSeason.ToString()}});
                    qr = @"DROP TABLE IF EXISTS ""Divisions""";
                    db.ExecuteNonQuery(qr);
                    qr = @"CREATE TABLE ""Divisions"" (""ID"" INTEGER PRIMARY KEY NOT NULL , ""Name"" TEXT, ""Conference"" INTEGER)";
                    db.ExecuteNonQuery(qr);
                    db.Insert("Divisions", new Dictionary<string, string> {{"ID", "0"}, {"Name", "League"}, {"Conference", "0"}});
                    qr = @"DROP TABLE IF EXISTS ""Conferences""";
                    db.ExecuteNonQuery(qr);
                    qr = @"CREATE TABLE ""Conferences"" (""ID"" INTEGER PRIMARY KEY NOT NULL , ""Name"" TEXT)";
                    db.ExecuteNonQuery(qr);
                    db.Insert("Conferences", new Dictionary<string, string> {{"ID", "0"}, {"Name", "League"}});
                    qr = @"DROP TABLE IF EXISTS ""CareerHighs""";
                    db.ExecuteNonQuery(qr);
                    qr = createCareerHighsQuery;
                    db.ExecuteNonQuery(qr);

                    CreatePastPlayerAndTeamStatsTables(db);
                }
                string teamsT = "Teams";
                string pl_teamsT = "PlayoffTeams";
                string oppT = "Opponents";
                string pl_oppT = "PlayoffOpponents";
                string playersT = "Players";
                string pl_playersT = "PlayoffPlayers";
                if (curSeason != maxSeason)
                {
                    string s = "S" + curSeason.ToString();
                    teamsT += s;
                    pl_teamsT += s;
                    oppT += s;
                    pl_oppT += s;
                    playersT += s;
                    pl_playersT += s;
                }
                qr = string.Format(@"DROP TABLE IF EXISTS ""{0}""", pl_teamsT);
                db.ExecuteNonQuery(qr);
                qr =
                    string.Format(
                        @"CREATE TABLE ""{0}"" (""ID"" INTEGER PRIMARY KEY NOT NULL ,""Name"" TEXT NOT NULL ,""DisplayName"" TEXT NOT NULL,""isHidden"" TEXT NOT NULL, ""Division"" INTEGER, ""Conference"" INTEGER, ""WIN"" INTEGER NOT NULL ,""LOSS"" INTEGER NOT NULL ,""MINS"" INTEGER, ""PF"" INTEGER NOT NULL ,""PA"" INTEGER NOT NULL ,""FGM"" INTEGER NOT NULL ,""FGA"" INTEGER NOT NULL ,""TPM"" INTEGER NOT NULL ,""TPA"" INTEGER NOT NULL ,""FTM"" INTEGER NOT NULL ,""FTA"" INTEGER NOT NULL ,""OREB"" INTEGER NOT NULL ,""DREB"" INTEGER NOT NULL ,""STL"" INTEGER NOT NULL ,""TOS"" INTEGER NOT NULL ,""BLK"" INTEGER NOT NULL ,""AST"" INTEGER NOT NULL ,""FOUL"" INTEGER NOT NULL ,""OFFSET"" INTEGER)",
                        pl_teamsT);
                db.ExecuteNonQuery(qr);

                qr = string.Format(@"DROP TABLE IF EXISTS ""{0}""", teamsT);
                db.ExecuteNonQuery(qr);
                qr =
                    string.Format(
                        @"CREATE TABLE ""{0}"" (""ID"" INTEGER PRIMARY KEY NOT NULL ,""Name"" TEXT NOT NULL ,""DisplayName"" TEXT NOT NULL,""isHidden"" TEXT NOT NULL, ""Division"" INTEGER, ""Conference"" INTEGER, ""WIN"" INTEGER NOT NULL ,""LOSS"" INTEGER NOT NULL ,""MINS"" INTEGER, ""PF"" INTEGER NOT NULL ,""PA"" INTEGER NOT NULL ,""FGM"" INTEGER NOT NULL ,""FGA"" INTEGER NOT NULL ,""TPM"" INTEGER NOT NULL ,""TPA"" INTEGER NOT NULL ,""FTM"" INTEGER NOT NULL ,""FTA"" INTEGER NOT NULL ,""OREB"" INTEGER NOT NULL ,""DREB"" INTEGER NOT NULL ,""STL"" INTEGER NOT NULL ,""TOS"" INTEGER NOT NULL ,""BLK"" INTEGER NOT NULL ,""AST"" INTEGER NOT NULL ,""FOUL"" INTEGER NOT NULL ,""OFFSET"" INTEGER)",
                        teamsT);
                db.ExecuteNonQuery(qr);

                qr = string.Format(@"DROP TABLE IF EXISTS ""{0}""", pl_oppT);
                db.ExecuteNonQuery(qr);
                qr =
                    string.Format(
                        @"CREATE TABLE ""{0}"" (""ID"" INTEGER PRIMARY KEY NOT NULL ,""Name"" TEXT NOT NULL ,""DisplayName"" TEXT NOT NULL,""isHidden"" TEXT NOT NULL, ""Division"" INTEGER, ""Conference"" INTEGER, ""WIN"" INTEGER NOT NULL ,""LOSS"" INTEGER NOT NULL ,""MINS"" INTEGER, ""PF"" INTEGER NOT NULL ,""PA"" INTEGER NOT NULL ,""FGM"" INTEGER NOT NULL ,""FGA"" INTEGER NOT NULL ,""TPM"" INTEGER NOT NULL ,""TPA"" INTEGER NOT NULL ,""FTM"" INTEGER NOT NULL ,""FTA"" INTEGER NOT NULL ,""OREB"" INTEGER NOT NULL ,""DREB"" INTEGER NOT NULL ,""STL"" INTEGER NOT NULL ,""TOS"" INTEGER NOT NULL ,""BLK"" INTEGER NOT NULL ,""AST"" INTEGER NOT NULL ,""FOUL"" INTEGER NOT NULL ,""OFFSET"" INTEGER)",
                        pl_oppT);
                db.ExecuteNonQuery(qr);

                qr = string.Format(@"DROP TABLE IF EXISTS ""{0}""", oppT);
                db.ExecuteNonQuery(qr);
                qr =
                    string.Format(
                        @"CREATE TABLE ""{0}"" (""ID"" INTEGER PRIMARY KEY NOT NULL ,""Name"" TEXT NOT NULL ,""DisplayName"" TEXT NOT NULL,""isHidden"" TEXT NOT NULL, ""Division"" INTEGER, ""Conference"" INTEGER, ""WIN"" INTEGER NOT NULL ,""LOSS"" INTEGER NOT NULL ,""MINS"" INTEGER, ""PF"" INTEGER NOT NULL ,""PA"" INTEGER NOT NULL ,""FGM"" INTEGER NOT NULL ,""FGA"" INTEGER NOT NULL ,""TPM"" INTEGER NOT NULL ,""TPA"" INTEGER NOT NULL ,""FTM"" INTEGER NOT NULL ,""FTA"" INTEGER NOT NULL ,""OREB"" INTEGER NOT NULL ,""DREB"" INTEGER NOT NULL ,""STL"" INTEGER NOT NULL ,""TOS"" INTEGER NOT NULL ,""BLK"" INTEGER NOT NULL ,""AST"" INTEGER NOT NULL ,""FOUL"" INTEGER NOT NULL ,""OFFSET"" INTEGER)",
                        oppT);
                db.ExecuteNonQuery(qr);

                qr = string.Format(@"DROP TABLE IF EXISTS ""{0}""", playersT);
                db.ExecuteNonQuery(qr);
                qr =
                    string.Format(
                        @"CREATE TABLE ""{0}"" (""ID"" INTEGER PRIMARY KEY NOT NULL ,""LastName"" TEXT NOT NULL ,""FirstName"" TEXT NOT NULL ,""Position1"" TEXT,""Position2"" TEXT,""isActive"" TEXT,""YearOfBirth"" INTEGER,""YearsPro"" INTEGER, ""isHidden"" TEXT,""InjuryType"" INTEGER, ""CustomInjuryName"" TEXT, ""InjuryDaysLeft"" INTEGER,""TeamFin"" INTEGER,""TeamSta"" INTEGER,""GP"" INTEGER,""GS"" INTEGER,""MINS"" INTEGER NOT NULL DEFAULT (0) ,""PTS"" INTEGER NOT NULL ,""FGM"" INTEGER NOT NULL ,""FGA"" INTEGER NOT NULL ,""TPM"" INTEGER NOT NULL ,""TPA"" INTEGER NOT NULL ,""FTM"" INTEGER NOT NULL ,""FTA"" INTEGER NOT NULL ,""OREB"" INTEGER NOT NULL ,""DREB"" INTEGER NOT NULL ,""STL"" INTEGER NOT NULL ,""TOS"" INTEGER NOT NULL ,""BLK"" INTEGER NOT NULL ,""AST"" INTEGER NOT NULL ,""FOUL"" INTEGER NOT NULL ,""isAllStar"" TEXT,""isNBAChampion"" TEXT, ""ContractY1"" INTEGER, ""ContractY2"" INTEGER, ""ContractY3"" INTEGER, ""ContractY4"" INTEGER, ""ContractY5"" INTEGER, ""ContractY6"" INTEGER, ""ContractY7"" INTEGER, ""ContractOption"" TEXT, ""Height"" REAL, ""Weight"" REAL)",
                        playersT);
                db.ExecuteNonQuery(qr);

                qr = string.Format(@"DROP TABLE IF EXISTS ""{0}""", pl_playersT);
                db.ExecuteNonQuery(qr);
                qr =
                    string.Format(
                        @"CREATE TABLE ""{0}"" (""ID"" INTEGER PRIMARY KEY NOT NULL ,""GP"" INTEGER,""GS"" INTEGER,""MINS"" INTEGER NOT NULL DEFAULT (0) ,""PTS"" INTEGER NOT NULL ,""FGM"" INTEGER NOT NULL ,""FGA"" INTEGER NOT NULL ,""TPM"" INTEGER NOT NULL ,""TPA"" INTEGER NOT NULL ,""FTM"" INTEGER NOT NULL ,""FTA"" INTEGER NOT NULL ,""OREB"" INTEGER NOT NULL ,""DREB"" INTEGER NOT NULL ,""STL"" INTEGER NOT NULL ,""TOS"" INTEGER NOT NULL ,""BLK"" INTEGER NOT NULL ,""AST"" INTEGER NOT NULL ,""FOUL"" INTEGER NOT NULL)",
                        pl_playersT);
                db.ExecuteNonQuery(qr);
            }
            catch
            {
            }
        }

        private static void CreatePastPlayerAndTeamStatsTables(SQLiteDatabase db)
        {
            string qr;
            qr = string.Format(@"DROP TABLE IF EXISTS ""{0}""", "PastPlayerStats");
            db.ExecuteNonQuery(qr);
            qr =
                string.Format(
                    @"CREATE TABLE ""{0}"" (""ID"" INTEGER PRIMARY KEY , ""PlayerID"" INTEGER, ""SeasonName"" TEXT, ""SOrder"" TEXT, ""isPlayoff"" TEXT , ""TeamFin"" TEXT,""TeamSta"" TEXT,""GP"" INTEGER,""GS"" INTEGER,""MINS"" INTEGER  DEFAULT (0) ,""PTS"" INTEGER  ,""FGM"" INTEGER  ,""FGA"" INTEGER  ,""TPM"" INTEGER  ,""TPA"" INTEGER  ,""FTM"" INTEGER  ,""FTA"" INTEGER  ,""OREB"" INTEGER  ,""DREB"" INTEGER  ,""STL"" INTEGER  ,""TOS"" INTEGER  ,""BLK"" INTEGER  ,""AST"" INTEGER  ,""FOUL"" INTEGER)",
                    "PastPlayerStats");
            db.ExecuteNonQuery(qr);

            qr = string.Format(@"DROP TABLE IF EXISTS ""{0}""", "PastTeamStats");
            db.ExecuteNonQuery(qr);
            qr =
                string.Format(
                    @"CREATE TABLE ""{0}"" (""ID"" INTEGER PRIMARY KEY  , ""TeamID"" INTEGER, ""SeasonName"" TEXT, ""SOrder"" TEXT, ""isPlayoff"" TEXT , ""WIN"" INTEGER  ,""LOSS"" INTEGER  ,""MINS"" INTEGER, ""PF"" INTEGER  ,""PA"" INTEGER  ,""FGM"" INTEGER  ,""FGA"" INTEGER  ,""TPM"" INTEGER  ,""TPA"" INTEGER  ,""FTM"" INTEGER  ,""FTA"" INTEGER  ,""OREB"" INTEGER  ,""DREB"" INTEGER  ,""STL"" INTEGER  ,""TOS"" INTEGER  ,""BLK"" INTEGER  ,""AST"" INTEGER  ,""FOUL"" INTEGER)",
                    "PastTeamStats");
            db.ExecuteNonQuery(qr);
        }

        /// <summary>
        ///     Gets the max season ID in a database.
        /// </summary>
        /// <param name="file">The database.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">The database requested doesn't exist.</exception>
        public static int getMaxSeason(string file)
        {
            try
            {
                if (!File.Exists(file))
                    throw (new Exception("The database requested doesn't exist."));

                var _db = new SQLiteDatabase(file);

                const string q = "select Name from sqlite_master";
                DataTable res = _db.GetDataTable(q);

                int maxseason = (from DataRow r in res.Rows
                                 select r["Name"].ToString()
                                 into name where name.Length > 5 && name.Substring(0, 5) == "Teams"
                                 select Convert.ToInt32(name.Substring(6, 1))).Concat(new[] {0}).Max();

                maxseason++;

                return maxseason;
            }
            catch
            {
                return 1;
            }
        }

        /// <summary>
        ///     Sets a setting value in the current database.
        /// </summary>
        /// <typeparam name="T">The type of value to save.</typeparam>
        /// <param name="setting">The setting.</param>
        /// <param name="value">The value.</param>
        public static void SetSetting<T>(string setting, T value)
        {
            SetSetting(MainWindow.currentDB, setting, value);
        }

        /// <summary>
        ///     Sets a setting value in the specified database.
        /// </summary>
        /// <typeparam name="T">The type of value to save.</typeparam>
        /// <param name="file">The file.</param>
        /// <param name="setting">The setting.</param>
        /// <param name="value">The value.</param>
        public static void SetSetting<T>(string file, string setting, T value)
        {
            var db = new SQLiteDatabase(file);

            string val = value.ToString();
            string q = "select * from Misc where Setting LIKE \"" + setting + "\"";

            int rowCount = db.GetDataTable(q).Rows.Count;

            if (rowCount == 1)
            {
                db.Update("Misc", new Dictionary<string, string> {{"Value", val}}, "Setting LIKE \"" + setting + "\"");
            }
            else
            {
                db.Insert("Misc", new Dictionary<string, string> {{"Setting", setting}, {"Value", val}});
            }
        }

        /// <summary>
        ///     Gets a setting value from the current database.
        /// </summary>
        /// <typeparam name="T">The type of value to get.</typeparam>
        /// <param name="setting">The setting.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public static T GetSetting<T>(string setting, T defaultValue)
        {
            return GetSetting(MainWindow.currentDB, setting, defaultValue);
        }

        /// <summary>
        ///     Gets a setting value from the specified database.
        /// </summary>
        /// <typeparam name="T">The type of value to get.</typeparam>
        /// <param name="file">The file.</param>
        /// <param name="setting">The setting.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public static T GetSetting<T>(string file, string setting, T defaultValue)
        {
            var db = new SQLiteDatabase(file);

            string q = "select Value from Misc where Setting LIKE \"" + setting + "\"";
            string value = db.ExecuteScalar(q);

            if (String.IsNullOrEmpty(value))
                return defaultValue;

            return (T) Convert.ChangeType(value, typeof (T));
        }

        /// <summary>
        ///     Gets the team stats from a specified database.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="teamID">The team.</param>
        /// <param name="season">The season.</param>
        /// <param name="ts">The resulting team stats.</param>
        /// <param name="tsopp">The resulting opposing team stats.</param>
        public static void GetTeamStatsFromDatabase(string file, int teamID, int season, out TeamStats ts, out TeamStats tsopp)
        {
            var _db = new SQLiteDatabase(file);

            String q;
            int maxSeason = getMaxSeason(file);

            if (season == 0)
                season = maxSeason;

            if (maxSeason == season)
            {
                q = "select * from Teams where ID = " + teamID;
            }
            else
            {
                q = "select * from TeamsS" + season.ToString() + " where ID = " + teamID;
            }

            DataTable res = _db.GetDataTable(q);

            ts = new TeamStats();

            DataRow r = res.Rows[0];
            ts = new TeamStats(teamID, r["Name"].ToString());

            // For compatibility reasons, if properties added after v0.10.5.1 don't exist,
            // we create them without error.
            try
            {
                ts.displayName = r["DisplayName"].ToString();
            }
            catch (Exception)
            {
                ts.displayName = ts.name;
            }

            try
            {
                ts.isHidden = Tools.getBoolean(r, "isHidden");
            }
            catch (Exception)
            {
                ts.isHidden = false;
            }

            try
            {
                ts.division = Tools.getInt(r, "Division");
            }
            catch (Exception)
            {
                ts.division = 0;
            }

            ts.offset = Convert.ToInt32(r["OFFSET"].ToString());

            GetTeamStatsFromDataRow(ref ts, r);


            if (maxSeason == season)
            {
                q = "select * from PlayoffTeams where ID = " + teamID;
            }
            else
            {
                q = "select * from PlayoffTeamsS" + season.ToString() + " where ID = " + teamID;
            }
            res = _db.GetDataTable(q);

            r = res.Rows[0];
            ts.pl_offset = Convert.ToInt32(r["OFFSET"].ToString());
            ts.pl_winloss[0] = Convert.ToByte(r["WIN"].ToString());
            ts.pl_winloss[1] = Convert.ToByte(r["LOSS"].ToString());
            ts.pl_stats[t.MINS] = Convert.ToUInt16(r["MINS"].ToString());
            ts.pl_stats[t.PF] = Convert.ToUInt16(r["PF"].ToString());
            ts.pl_stats[t.PA] = Convert.ToUInt16(r["PA"].ToString());
            ts.pl_stats[t.FGM] = Convert.ToUInt16(r["FGM"].ToString());
            ts.pl_stats[t.FGA] = Convert.ToUInt16(r["FGA"].ToString());
            ts.pl_stats[t.TPM] = Convert.ToUInt16(r["TPM"].ToString());
            ts.pl_stats[t.TPA] = Convert.ToUInt16(r["TPA"].ToString());
            ts.pl_stats[t.FTM] = Convert.ToUInt16(r["FTM"].ToString());
            ts.pl_stats[t.FTA] = Convert.ToUInt16(r["FTA"].ToString());
            ts.pl_stats[t.OREB] = Convert.ToUInt16(r["OREB"].ToString());
            ts.pl_stats[t.DREB] = Convert.ToUInt16(r["DREB"].ToString());
            ts.pl_stats[t.STL] = Convert.ToUInt16(r["STL"].ToString());
            ts.pl_stats[t.TOS] = Convert.ToUInt16(r["TOS"].ToString());
            ts.pl_stats[t.BLK] = Convert.ToUInt16(r["BLK"].ToString());
            ts.pl_stats[t.AST] = Convert.ToUInt16(r["AST"].ToString());
            ts.pl_stats[t.FOUL] = Convert.ToUInt16(r["FOUL"].ToString());

            ts.CalcAvg();

            if (maxSeason == season)
            {
                q = "select * from Opponents where ID = " + teamID;
            }
            else
            {
                q = "select * from OpponentsS" + season.ToString() + " where ID = " + teamID;
            }

            res = _db.GetDataTable(q);

            r = res.Rows[0];
            tsopp = new TeamStats(teamID, r["Name"].ToString());

            // For compatibility reasons, if properties added after v0.10.5.1 don't exist,
            // we create them without error.

            try
            {
                tsopp.displayName = r["DisplayName"].ToString();
            }
            catch (Exception)
            {
                tsopp.displayName = tsopp.name;
            }

            try
            {
                tsopp.isHidden = Tools.getBoolean(r, "isHidden");
            }
            catch (Exception)
            {
                tsopp.isHidden = false;
            }

            try
            {
                tsopp.division = Tools.getInt(r, "Division");
            }
            catch (Exception)
            {
                tsopp.division = 0;
            }

            tsopp.offset = Convert.ToInt32(r["OFFSET"].ToString());
            tsopp.winloss[0] = Convert.ToByte(r["WIN"].ToString());
            tsopp.winloss[1] = Convert.ToByte(r["LOSS"].ToString());
            tsopp.stats[t.MINS] = Convert.ToUInt16(r["MINS"].ToString());
            tsopp.stats[t.PF] = Convert.ToUInt16(r["PF"].ToString());
            tsopp.stats[t.PA] = Convert.ToUInt16(r["PA"].ToString());
            tsopp.stats[t.FGM] = Convert.ToUInt16(r["FGM"].ToString());
            tsopp.stats[t.FGA] = Convert.ToUInt16(r["FGA"].ToString());
            tsopp.stats[t.TPM] = Convert.ToUInt16(r["TPM"].ToString());
            tsopp.stats[t.TPA] = Convert.ToUInt16(r["TPA"].ToString());
            tsopp.stats[t.FTM] = Convert.ToUInt16(r["FTM"].ToString());
            tsopp.stats[t.FTA] = Convert.ToUInt16(r["FTA"].ToString());
            tsopp.stats[t.OREB] = Convert.ToUInt16(r["OREB"].ToString());
            tsopp.stats[t.DREB] = Convert.ToUInt16(r["DREB"].ToString());
            tsopp.stats[t.STL] = Convert.ToUInt16(r["STL"].ToString());
            tsopp.stats[t.TOS] = Convert.ToUInt16(r["TOS"].ToString());
            tsopp.stats[t.BLK] = Convert.ToUInt16(r["BLK"].ToString());
            tsopp.stats[t.AST] = Convert.ToUInt16(r["AST"].ToString());
            tsopp.stats[t.FOUL] = Convert.ToUInt16(r["FOUL"].ToString());

            if (maxSeason == season)
            {
                q = "select * from PlayoffOpponents where ID = " + teamID;
            }
            else
            {
                q = "select * from PlayoffOpponentsS" + season.ToString() + " where ID = " + teamID;
            }
            res = _db.GetDataTable(q);

            r = res.Rows[0];
            tsopp.pl_offset = Convert.ToInt32(r["OFFSET"].ToString());
            tsopp.pl_winloss[0] = Convert.ToByte(r["WIN"].ToString());
            tsopp.pl_winloss[1] = Convert.ToByte(r["LOSS"].ToString());
            tsopp.pl_stats[t.MINS] = Convert.ToUInt16(r["MINS"].ToString());
            tsopp.pl_stats[t.PF] = Convert.ToUInt16(r["PF"].ToString());
            tsopp.pl_stats[t.PA] = Convert.ToUInt16(r["PA"].ToString());
            tsopp.pl_stats[t.FGM] = Convert.ToUInt16(r["FGM"].ToString());
            tsopp.pl_stats[t.FGA] = Convert.ToUInt16(r["FGA"].ToString());
            tsopp.pl_stats[t.TPM] = Convert.ToUInt16(r["TPM"].ToString());
            tsopp.pl_stats[t.TPA] = Convert.ToUInt16(r["TPA"].ToString());
            tsopp.pl_stats[t.FTM] = Convert.ToUInt16(r["FTM"].ToString());
            tsopp.pl_stats[t.FTA] = Convert.ToUInt16(r["FTA"].ToString());
            tsopp.pl_stats[t.OREB] = Convert.ToUInt16(r["OREB"].ToString());
            tsopp.pl_stats[t.DREB] = Convert.ToUInt16(r["DREB"].ToString());
            tsopp.pl_stats[t.STL] = Convert.ToUInt16(r["STL"].ToString());
            tsopp.pl_stats[t.TOS] = Convert.ToUInt16(r["TOS"].ToString());
            tsopp.pl_stats[t.BLK] = Convert.ToUInt16(r["BLK"].ToString());
            tsopp.pl_stats[t.AST] = Convert.ToUInt16(r["AST"].ToString());
            tsopp.pl_stats[t.FOUL] = Convert.ToUInt16(r["FOUL"].ToString());

            tsopp.CalcAvg();
        }

        public static void GetTeamStatsFromDataRow(ref TeamStats ts, DataRow r, bool isPlayoff = false)
        {
            ts.ID = Convert.ToInt32(r["ID"].ToString());
            if (!isPlayoff)
            {
                ts.winloss[0] = Convert.ToByte(r["WIN"].ToString());
                ts.winloss[1] = Convert.ToByte(r["LOSS"].ToString());
                ts.stats[t.MINS] = Convert.ToUInt16(r["MINS"].ToString());
                ts.stats[t.PF] = Convert.ToUInt16(r["PF"].ToString());
                ts.stats[t.PA] = Convert.ToUInt16(r["PA"].ToString());
                ts.stats[t.FGM] = Convert.ToUInt16(r["FGM"].ToString());
                ts.stats[t.FGA] = Convert.ToUInt16(r["FGA"].ToString());
                ts.stats[t.TPM] = Convert.ToUInt16(r["TPM"].ToString());
                ts.stats[t.TPA] = Convert.ToUInt16(r["TPA"].ToString());
                ts.stats[t.FTM] = Convert.ToUInt16(r["FTM"].ToString());
                ts.stats[t.FTA] = Convert.ToUInt16(r["FTA"].ToString());
                ts.stats[t.OREB] = Convert.ToUInt16(r["OREB"].ToString());
                ts.stats[t.DREB] = Convert.ToUInt16(r["DREB"].ToString());
                ts.stats[t.STL] = Convert.ToUInt16(r["STL"].ToString());
                ts.stats[t.TOS] = Convert.ToUInt16(r["TOS"].ToString());
                ts.stats[t.BLK] = Convert.ToUInt16(r["BLK"].ToString());
                ts.stats[t.AST] = Convert.ToUInt16(r["AST"].ToString());
                ts.stats[t.FOUL] = Convert.ToUInt16(r["FOUL"].ToString());
            }
            else
            {
                ts.pl_winloss[0] = Convert.ToByte(r["WIN"].ToString());
                ts.pl_winloss[1] = Convert.ToByte(r["LOSS"].ToString());
                ts.pl_stats[t.MINS] = Convert.ToUInt16(r["MINS"].ToString());
                ts.pl_stats[t.PF] = Convert.ToUInt16(r["PF"].ToString());
                ts.pl_stats[t.PA] = Convert.ToUInt16(r["PA"].ToString());
                ts.pl_stats[t.FGM] = Convert.ToUInt16(r["FGM"].ToString());
                ts.pl_stats[t.FGA] = Convert.ToUInt16(r["FGA"].ToString());
                ts.pl_stats[t.TPM] = Convert.ToUInt16(r["TPM"].ToString());
                ts.pl_stats[t.TPA] = Convert.ToUInt16(r["TPA"].ToString());
                ts.pl_stats[t.FTM] = Convert.ToUInt16(r["FTM"].ToString());
                ts.pl_stats[t.FTA] = Convert.ToUInt16(r["FTA"].ToString());
                ts.pl_stats[t.OREB] = Convert.ToUInt16(r["OREB"].ToString());
                ts.pl_stats[t.DREB] = Convert.ToUInt16(r["DREB"].ToString());
                ts.pl_stats[t.STL] = Convert.ToUInt16(r["STL"].ToString());
                ts.pl_stats[t.TOS] = Convert.ToUInt16(r["TOS"].ToString());
                ts.pl_stats[t.BLK] = Convert.ToUInt16(r["BLK"].ToString());
                ts.pl_stats[t.AST] = Convert.ToUInt16(r["AST"].ToString());
                ts.pl_stats[t.FOUL] = Convert.ToUInt16(r["FOUL"].ToString());
            }
            ts.CalcAvg();
        }

        /// <summary>
        ///     Gets all team stats from the specified database.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="season">The season.</param>
        /// <param name="_tst">The resulting team stats dictionary.</param>
        /// <param name="_tstopp">The resulting opposing team stats dictionary.</param>
        /// <param name="TeamOrder">The resulting team order.</param>
        public static void GetAllTeamStatsFromDatabase(string file, int season, out Dictionary<int, TeamStats> _tst,
                                                       out Dictionary<int, TeamStats> _tstopp, out SortedDictionary<string, int> TeamOrder)
        {
            var _db = new SQLiteDatabase(file);

            String q;
            int maxSeason = getMaxSeason(file);

            if (season == 0)
                season = maxSeason;

            if (maxSeason == season)
            {
                q = "select ID from Teams;";
            }
            else
            {
                q = "select ID from TeamsS" + season.ToString() + ";";
            }

            DataTable res = _db.GetDataTable(q);

            _tst = new Dictionary<int, TeamStats>();
            _tstopp = new Dictionary<int, TeamStats>();
            TeamOrder = new SortedDictionary<string, int>();
            int i = 0;

            foreach (DataRow r in res.Rows)
            {
                TeamStats ts;
                TeamStats tsopp;
                int teamID = Tools.getInt(r, "ID");
                GetTeamStatsFromDatabase(file, teamID, season, out ts, out tsopp);
                _tst[i] = ts;
                _tstopp[i] = tsopp;
                TeamOrder.Add(ts.name, i);
                i++;
            }
        }

        /// <summary>
        ///     Default implementation of LoadSeason; calls LoadSeason to update MainWindow data
        /// </summary>
        public static void LoadSeason(int season = 0, bool doNotLoadBoxScores = false)
        {
            LoadSeason(MainWindow.currentDB, out MainWindow.tst, out MainWindow.tstopp, out MainWindow.pst, out MainWindow.TeamOrder,
                       ref MainWindow.bshist, out MainWindow.splitTeamStats, out MainWindow.splitPlayerStats,
                       out MainWindow.SeasonTeamRankings, out MainWindow.SeasonPlayerRankings, out MainWindow.PlayoffTeamRankings,
                       out MainWindow.PlayoffPlayerRankings, out MainWindow.DisplayNames, season == 0 ? MainWindow.curSeason : season,
                       doNotLoadBoxScores);
        }

        /// <summary>
        ///     Default implementation of LoadSeason; calls LoadSeason to update MainWindow data
        /// </summary>
        public static void LoadSeason(string file, int season = 0, bool doNotLoadBoxScores = false)
        {
            LoadSeason(file, out MainWindow.tst, out MainWindow.tstopp, out MainWindow.pst, out MainWindow.TeamOrder, ref MainWindow.bshist,
                       out MainWindow.splitTeamStats, out MainWindow.splitPlayerStats, out MainWindow.SeasonTeamRankings,
                       out MainWindow.SeasonPlayerRankings, out MainWindow.PlayoffTeamRankings, out MainWindow.PlayoffPlayerRankings,
                       out MainWindow.DisplayNames, season == 0 ? MainWindow.curSeason : season, doNotLoadBoxScores);
        }

        /// <summary>
        ///     Loads a specific season from the specified database.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="tst">The resulting team stats dictionary.</param>
        /// <param name="tstopp">The resulting opposing team stats dictionary.</param>
        /// <param name="pst">The resulting player stats dictionary.</param>
        /// <param name="teamOrder">The resulting team order.</param>
        /// <param name="bshist">The box score history container.</param>
        /// <param name="curSeason">The current season ID.</param>
        /// <param name="doNotLoadBoxScores">
        ///     if set to <c>true</c>, box scores will not be parsed.
        /// </param>
        public static void LoadSeason(string file, out Dictionary<int, TeamStats> tst, out Dictionary<int, TeamStats> tstopp,
                                      out Dictionary<int, PlayerStats> pst, out SortedDictionary<string, int> teamOrder,
                                      ref List<BoxScoreEntry> bshist, out Dictionary<int, Dictionary<string, TeamStats>> splitTeamStats,
                                      out Dictionary<int, Dictionary<string, PlayerStats>> splitPlayerStats, out TeamRankings teamRankings,
                                      out PlayerRankings playerRankings, out TeamRankings playoffTeamRankings,
                                      out PlayerRankings playoffPlayerRankings, out Dictionary<int, string> displayNames, int curSeason = 0,
                                      bool doNotLoadBoxScores = false)
        {
            MainWindow.loadingSeason = true;

            bool mustSave = false;
            if (!upgrading)
            {
                mustSave = CheckIfDBNeedsUpgrade(file);
            }

            int maxSeason = getMaxSeason(file);

            if (curSeason == 0)
            {
                curSeason = maxSeason;
                if (MainWindow.tf.SeasonNum == 0)
                {
                    MainWindow.tf.SeasonNum = maxSeason;
                }
            }

            LoadDivisionsAndConferences(file);

            MainWindow.tf.SeasonNum = curSeason;
            if (mustSave)
            {
                GetAllTeamStatsFromDatabase(file, curSeason, out tst, out tstopp, out teamOrder);

                pst = GetPlayersFromDatabase(file, tst, tstopp, teamOrder, curSeason, maxSeason);

                if (!doNotLoadBoxScores)
                    bshist = GetSeasonBoxScoresFromDatabase(file, curSeason, maxSeason, tst);

                splitTeamStats = null;
                splitPlayerStats = null;
                teamRankings = null;
                playerRankings = null;
                playoffTeamRankings = null;
                playoffPlayerRankings = null;
                displayNames = null;
            }
            else
            {
                PopulateAll(MainWindow.tf, out tst, out tstopp, out teamOrder, out pst, out splitTeamStats, out splitPlayerStats, out bshist,
                            out teamRankings, out playerRankings, out playoffTeamRankings, out playoffPlayerRankings, out displayNames);
            }

            MainWindow.currentDB = file;

            MainWindow.ChangeSeason(curSeason);

            if (mustSave)
            {
                upgrading = true;
                string backupName = file + ".UpgradeBackup.tst";
                try
                {
                    File.Delete(backupName);
                }
                catch
                {
                }
                try
                {
                    File.Copy(file, backupName);
                }
                catch
                {
                }
                SaveDatabaseAs(file);
                File.Delete(backupName);
                upgrading = false;
            }

            MainWindow.loadingSeason = false;
        }

        /// <summary>
        ///     Loads the divisions and conferences.
        /// </summary>
        /// <param name="file">The file.</param>
        public static void LoadDivisionsAndConferences(string file)
        {
            var db = new SQLiteDatabase(file);

            /*
            string q = "SELECT Divisions.ID As DivID, Conferences.ID As ConfID, Divisions.Name As DivName, " +
            "Conferences.Name as ConfName, Divisions.Conference As DivConf FROM Divisions " +
            "INNER JOIN Conferences ON Conference = Conferences.ID";
            */
            string q = "SELECT * FROM Divisions";
            DataTable res = db.GetDataTable(q);

            MainWindow.Divisions.Clear();
            foreach (DataRow row in res.Rows)
            {
                MainWindow.Divisions.Add(new Division
                                         {
                                             ID = Tools.getInt(row, "ID"),
                                             Name = Tools.getString(row, "Name"),
                                             ConferenceID = Tools.getInt(row, "Conference")
                                         });
            }

            q = "SELECT * FROM Conferences";
            res = db.GetDataTable(q);

            MainWindow.Conferences.Clear();
            foreach (DataRow row in res.Rows)
            {
                MainWindow.Conferences.Add(new Conference {ID = Tools.getInt(row, "ID"), Name = Tools.getString(row, "Name")});
            }
        }

        /// <summary>
        ///     Checks for missing and changed fields in older databases and upgrades them to the current format.
        /// </summary>
        /// <param name="file">The path to the database.</param>
        private static bool CheckIfDBNeedsUpgrade(string file)
        {
            var db = new SQLiteDatabase(file);

            bool mustSave = false;

            #region SeasonNames

            // Check for missing SeasonNames table (v0.11)

            string qr = "SELECT * FROM SeasonNames";
            DataTable dt;
            try
            {
                dt = db.GetDataTable(qr);
            }
            catch (Exception)
            {
                int maxSeason = getMaxSeason(file);
                qr = "CREATE TABLE \"SeasonNames\" (\"ID\" INTEGER PRIMARY KEY  NOT NULL , \"Name\" TEXT)";
                db.ExecuteNonQuery(qr);

                for (int i = 1; i <= maxSeason; i++)
                {
                    db.Insert("SeasonNames", new Dictionary<string, string> {{"ID", i.ToString()}, {"Name", i.ToString()}});
                }
            }

            #endregion

            #region PastPlayerAndTeamStats

            qr = "SELECT * FROM PastPlayerStats";
            try
            {
                dt = db.GetDataTable(qr);
            }
            catch (Exception)
            {
                CreatePastPlayerAndTeamStatsTables(db);
            }

            #endregion

            #region Misc

            qr = "SELECT * FROM sqlite_master WHERE name = \"Misc\"";
            try
            {
                dt = db.GetDataTable(qr);
                foreach (DataRow dr in dt.Rows)
                {
                    if (dr["name"].ToString() == "Misc")
                    {
                        if (dr["sql"].ToString().Contains("CurSeason"))
                        {
                            qr = "DROP TABLE IF EXISTS \"Misc\"";
                            db.ExecuteNonQuery(qr);
                            qr = "CREATE TABLE \"Misc\" (\"Setting\" TEXT PRIMARY KEY,\"Value\" TEXT)";
                            db.ExecuteNonQuery(qr);
                        }
                        break;
                    }
                }
            }
            catch (Exception)
            {
            }

            #endregion

            #region PlayerResults

            qr = "SELECT * FROM sqlite_master WHERE name = \"PlayerResults\"";
            try
            {
                dt = db.GetDataTable(qr);
                foreach (DataRow dr in dt.Rows)
                {
                    if (dr["name"].ToString() == "PlayerResults")
                    {
                        if (dr["sql"].ToString().Contains("\"ID\""))
                        {
                            mustSave = true;
                        }
                        break;
                    }
                }
            }
            catch (Exception)
            {
            }

            #endregion

            #region Teams

            #endregion

            #region Players

            qr = "SELECT * FROM sqlite_master WHERE name = \"Players\"";
            try
            {
                dt = db.GetDataTable(qr);
                foreach (DataRow dr in dt.Rows)
                {
                    if (dr["name"].ToString() == "Players")
                    {
                        if (!dr["sql"].ToString().Contains("\"isHidden\""))
                        {
                            mustSave = true;
                        }
                        else if (!dr["sql"].ToString().Contains("\"YearOfBirth\""))
                        {
                            mustSave = true;
                            if (dr["sql"].ToString().Contains("\"Age\""))
                            {
                                var ibw =
                                    new InputBoxWindow(
                                        "NBA Stats Tracker has replaced the 'Age' field for players with 'Year of Birth'.\n" +
                                        "Please enter the year by which all players' year of birth should be calculated.",
                                        DateTime.Now.Year.ToString());
                                if (ibw.ShowDialog() == false)
                                {
                                    MainWindow.input = DateTime.Now.Year.ToString();
                                }
                            }
                        }
                        else if (!dr["sql"].ToString().Contains("\"ContractY1\""))
                        {
                            mustSave = true;
                        }
                        else if (!dr["sql"].ToString().Contains("\"Height\""))
                        {
                            mustSave = true;
                        }
                        else if (dr["sql"].ToString().Contains("\"TeamFin\" TEXT"))
                        {
                            mustSave = true;
                        }
                        else if (dr["sql"].ToString().Contains("\"isInjured\""))
                        {
                            mustSave = true;
                        }
                        break;
                    }
                }
            }
            catch (Exception)
            {
            }

            #endregion

            #region Playoff Players

            qr = "SELECT * FROM PlayoffPlayers";
            try
            {
                dt = db.GetDataTable(qr);
            }
            catch (Exception)
            {
                mustSave = true;
            }

            #endregion

            #region Divisions and Conferences

            qr = "SELECT * FROM sqlite_master WHERE name = \"Teams\"";
            try
            {
                dt = db.GetDataTable(qr);
                foreach (DataRow dr in dt.Rows)
                {
                    if (dr["name"].ToString() == "Teams")
                    {
                        if (!dr["sql"].ToString().Contains("\"Division\""))
                        {
                            mustSave = true;
                            qr = "DROP TABLE IF EXISTS \"Divisions\"";
                            db.ExecuteNonQuery(qr);
                            qr = "CREATE TABLE \"Divisions\" (\"ID\" INTEGER PRIMARY KEY  NOT NULL , \"Name\" TEXT, \"Conference\" INTEGER)";
                            db.ExecuteNonQuery(qr);
                            db.Insert("Divisions", new Dictionary<string, string> {{"ID", "0"}, {"Name", "League"}, {"Conference", "0"}});
                            qr = "DROP TABLE IF EXISTS \"Conferences\"";
                            db.ExecuteNonQuery(qr);
                            qr = "CREATE TABLE \"Conferences\" (\"ID\" INTEGER PRIMARY KEY  NOT NULL , \"Name\" TEXT)";
                            db.ExecuteNonQuery(qr);
                            db.Insert("Conferences", new Dictionary<string, string> {{"ID", "0"}, {"Name", "League"}});
                        }
                        break;
                    }
                }
            }
            catch (Exception)
            {
            }

            #endregion

            #region CareerHighs

            qr = "SELECT * FROM CareerHighs";
            try
            {
                db.GetDataTable(qr);
            }
            catch (Exception)
            {
                qr = createCareerHighsQuery;
                db.ExecuteNonQuery(qr);
            }

            #endregion

            return mustSave;
        }

        /// <summary>
        ///     Gets all box scores from the specified database.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns></returns>
        public static List<BoxScoreEntry> GetAllBoxScoresFromDatabase(string file, Dictionary<int, TeamStats> tst)
        {
            int maxSeason = getMaxSeason(file);

            var bshist = new List<BoxScoreEntry>();

            for (int i = maxSeason; i >= 1; i--)
            {
                List<BoxScoreEntry> temp = GetSeasonBoxScoresFromDatabase(MainWindow.currentDB, i, maxSeason, tst);

                foreach (var bse in temp)
                {
                    bshist.Add(bse);
                }
            }

            return bshist;
        }

        /// <summary>
        ///     Gets the season's box scores from the specified database.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="curSeason">The current season ID.</param>
        /// <param name="maxSeason">The max season ID.</param>
        /// <returns></returns>
        public static List<BoxScoreEntry> GetSeasonBoxScoresFromDatabase(string file, int curSeason, int maxSeason,
                                                                         Dictionary<int, TeamStats> tst)
        {
            var _db = new SQLiteDatabase(file);

            string q = "select * from GameResults WHERE SeasonNum = " + curSeason + " ORDER BY Date DESC;";
            DataTable res2 = _db.GetDataTable(q);

            string teamsT = "Teams";
            if (curSeason != maxSeason)
                teamsT += "S" + curSeason;

            DataTable res;
            var DisplayNames = new Dictionary<int, string>();
            try
            {
                q = "select ID, DisplayName from " + teamsT;
                res = _db.GetDataTable(q);

                foreach (DataRow r in res.Rows)
                {
                    DisplayNames.Add(Convert.ToInt32(r["ID"].ToString()), r["DisplayName"].ToString());
                }
            }
            catch
            {
                q = "select ID, Name from " + teamsT;
                res = _db.GetDataTable(q);

                foreach (DataRow r in res.Rows)
                {
                    DisplayNames.Add(Convert.ToInt32(r["ID"].ToString()), r["Name"].ToString());
                }
            }

            var _bshist = new List<BoxScoreEntry>(res2.Rows.Count);
            Parallel.ForEach(res2.Rows.Cast<DataRow>(), r =>
                                                        {
                                                            var bs = new TeamBoxScore(r, tst);

                                                            var bse = new BoxScoreEntry(bs)
                                                                      {
                                                                          date = bs.gamedate,
                                                                          Team1Display = DisplayNames[bs.Team1ID],
                                                                          Team2Display = DisplayNames[bs.Team2ID]
                                                                      };

                                                            string q2 = "select * from PlayerResults WHERE GameID = " + bs.id.ToString();
                                                            DataTable res3 = _db.GetDataTable(q2);
                                                            bse.pbsList = new List<PlayerBoxScore>(res3.Rows.Count);

                                                            Parallel.ForEach(res3.Rows.Cast<DataRow>(),
                                                                             r3 => bse.pbsList.Add(new PlayerBoxScore(r3, tst)));

                                                            _bshist.Add(bse);
                                                        });
            return _bshist;
        }

        public static List<BoxScoreEntry> GetTimeframedBoxScoresFromDatabase(string file, DateTime startDate, DateTime endDate,
                                                                             Dictionary<int, TeamStats> tst)
        {
            var _db = new SQLiteDatabase(file);

            string q = "select * from GameResults";
            q = SQLiteDatabase.AddDateRangeToSQLQuery(q, startDate, endDate, true);
            DataTable res2 = _db.GetDataTable(q);
            Dictionary<int, string> DisplayNames = GetTimeframedDisplayNames(file, startDate, endDate);

            var _bshist = new List<BoxScoreEntry>(res2.Rows.Count);
            Parallel.ForEach(res2.Rows.Cast<DataRow>(), r =>
                                                        {
                                                            var bs = new TeamBoxScore(r, tst);

                                                            var bse = new BoxScoreEntry(bs)
                                                                      {
                                                                          date = bs.gamedate,
                                                                          Team1Display = DisplayNames[bs.Team1ID],
                                                                          Team2Display = DisplayNames[bs.Team2ID]
                                                                      };

                                                            string q2 = "select * from PlayerResults WHERE GameID = " + bs.id.ToString();
                                                            DataTable res3 = _db.GetDataTable(q2);
                                                            bse.pbsList = new List<PlayerBoxScore>(res3.Rows.Count);

                                                            Parallel.ForEach(res3.Rows.Cast<DataRow>(),
                                                                             r3 => bse.pbsList.Add(new PlayerBoxScore(r3, tst)));

                                                            _bshist.Add(bse);
                                                        });
            return _bshist;
        }

        private static Dictionary<int, string> GetAllDisplayNames(string file)
        {
            var DisplayNames = new Dictionary<int, string>();

            int maxSeason = getMaxSeason(file);
            for (int i = maxSeason; i >= 0; i--)
            {
                GetSeasonDisplayNames(file, i, ref DisplayNames);
            }
            return DisplayNames;
        }

        public static Dictionary<int, string> GetTimeframedDisplayNames(string file, DateTime startDate, DateTime endDate)
        {
            var DisplayNames = new Dictionary<int, string>();

            List<int> seasons = GetSeasonsInTimeframe(startDate, endDate);
            seasons.Reverse();

            foreach (var i in seasons)
            {
                GetSeasonDisplayNames(file, i, ref DisplayNames);
            }
            return DisplayNames;
        }

        private static List<int> GetSeasonsInTimeframe(DateTime startDate, DateTime endDate)
        {
            string q = "SELECT SeasonNum FROM GameResults";
            q = SQLiteDatabase.AddDateRangeToSQLQuery(q, startDate, endDate, true);
            q += " GROUP BY SeasonNum";
            var seasons = new List<int>();
            MainWindow.db.GetDataTable(q).Rows.Cast<DataRow>().ToList().ForEach(row => seasons.Add(Tools.getInt(row, "SeasonNum")));
            if (seasons.Count == 0)
            {
                seasons.Add(1);
            }
            else
            {
                seasons.Sort();
                seasons.Reverse();
            }
            return seasons;
        }

        public static void GetSeasonDisplayNames(string file, int curSeason, ref Dictionary<int, string> DisplayNames)
        {
            string teamsT = "Teams";
            if (curSeason != getMaxSeason(file))
                teamsT += "S" + curSeason;
            string q;

            DisplayNames = new Dictionary<int, string>();
            var _db = new SQLiteDatabase(file);

            DataTable res;
            try
            {
                q = "select ID, DisplayName from " + teamsT;
                res = _db.GetDataTable(q);

                foreach (DataRow r in res.Rows)
                {
                    int id = Convert.ToInt32(r["ID"].ToString());
                    string displayName = r["DisplayName"].ToString();
                    if (!DisplayNames.Keys.Contains(id))
                    {
                        DisplayNames.Add(id, displayName);
                    }
                    else
                    {
                        string cur = DisplayNames[id];
                        string[] parts = cur.Split(new[] {", "}, StringSplitOptions.None);
                        if (!parts.Contains(displayName))
                            DisplayNames[id] += ", " + displayName;
                    }
                }
            }
            catch
            {
                q = "select ID, Name from " + teamsT;
                res = _db.GetDataTable(q);

                foreach (DataRow r in res.Rows)
                {
                    int id = Convert.ToInt32(r["ID"].ToString());
                    string displayName = r["Name"].ToString();
                    if (!DisplayNames.Keys.Contains(id))
                    {
                        DisplayNames.Add(id, displayName);
                    }
                    else
                    {
                        string cur = DisplayNames[id];
                        string[] parts = cur.Split(new[] {"/"}, StringSplitOptions.None);
                        if (!parts.Contains(displayName))
                            DisplayNames[id] += "/" + displayName;
                    }
                }
            }
        }

        /// <summary>
        ///     Gets the players from database.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="_tst">The team stats dictionary.</param>
        /// <param name="_tstopp">The opposing team stats dictionary.</param>
        /// <param name="_TeamOrder">The team order.</param>
        /// <param name="curSeason">The current season ID.</param>
        /// <param name="maxSeason">The maximum season ID.</param>
        /// <returns></returns>
        public static Dictionary<int, PlayerStats> GetPlayersFromDatabase(string file, Dictionary<int, TeamStats> _tst,
                                                                          Dictionary<int, TeamStats> _tstopp,
                                                                          SortedDictionary<string, int> _TeamOrder, int curSeason,
                                                                          int maxSeason)
        {
            string q;

            var _db = new SQLiteDatabase(file);

            if (curSeason == maxSeason)
            {
                q = "select * from Players;";
            }
            else
            {
                q = "select * from PlayersS" + curSeason.ToString() + ";";
            }
            DataTable res = _db.GetDataTable(q);

            Dictionary<int, PlayerStats> _pst =
                (from DataRow r in res.Rows.AsParallel() select new PlayerStats(r, _tst)).ToDictionary(ps => ps.ID);
            PlayerStats.CalculateAllMetrics(ref _pst, _tst, _tstopp, _TeamOrder);

            if (curSeason == maxSeason)
            {
                q = "select * from PlayoffPlayers;";
            }
            else
            {
                q = "select * from PlayoffPlayersS" + curSeason.ToString() + ";";
            }

            try
            {
                res = _db.GetDataTable(q);
            }
            catch (Exception ex)
            {
                if (ex.Message.ToLowerInvariant().Contains("no such table"))
                    return _pst;
            }

            foreach (DataRow r in res.Rows)
            {
                int id = Tools.getInt(r, "ID");
                _pst[id].UpdatePlayoffStats(r);
            }

            q = "SELECT * FROM CareerHighs";

            try
            {
                res = _db.GetDataTable(q);
            }
            catch (Exception ex)
            {
                if (ex.Message.ToLowerInvariant().Contains("no such table"))
                    return _pst;
            }

            foreach (DataRow r in res.Rows)
            {
                int id = Tools.getInt(r, "PlayerID");
                if (_pst.Keys.Contains(id))
                {
                    _pst[id].UpdateCareerHighs(r);
                }
            }

            PlayerStats.CalculateAllMetrics(ref _pst, _tst, _tstopp, _TeamOrder, playoffs: true);

            return _pst;
        }

        /// <summary>
        ///     Determines whether the TeamStats dictionary is empty.
        /// </summary>
        /// <returns>
        ///     <c>true</c> if the TeamStats dictionary is empty; otherwise, <c>false</c>.
        /// </returns>
        public static bool isTSTEmpty()
        {
            if (String.IsNullOrWhiteSpace(MainWindow.currentDB))
                return true;

            MainWindow.db = new SQLiteDatabase(MainWindow.currentDB);

            string teamsT = "Teams";
            if (MainWindow.curSeason != getMaxSeason(MainWindow.currentDB))
                teamsT += "S" + MainWindow.curSeason;
            string q = "select Name from " + teamsT;
            DataTable res = MainWindow.db.GetDataTable(q);

            try
            {
                if (res.Rows[0]["Name"].ToString() == "$$NewDB")
                    return true;

                return false;
            }
            catch
            {
                return true;
            }
        }

        public static void RepairDB(ref Dictionary<int, PlayerStats> pst)
        {
            List<int> list = pst.Keys.ToList();
            foreach (var key in list)
            {
                PlayerStats ps = pst[key];
                if (ps.isActive && ps.TeamF == -1)
                {
                    ps.isActive = false;
                }
            }
        }

        /// <summary>
        ///     Gets the max player ID.
        /// </summary>
        /// <param name="dbFile">The db file.</param>
        /// <returns></returns>
        public static int GetMaxPlayerID(string dbFile)
        {
            var db = new SQLiteDatabase(dbFile);
            int max = getMaxSeason(dbFile);

            string q;
            DataTable res;

            var maxList = new List<int>();

            for (int i = 1; i < max; i++)
            {
                q = "select ID from PlayersS" + i + " ORDER BY ID DESC LIMIT 1;";
                res = db.GetDataTable(q);
                maxList.Add(Convert.ToInt32(res.Rows[0]["ID"].ToString()));
            }
            q = "select ID from Players ORDER BY ID DESC LIMIT 1;";
            res = db.GetDataTable(q);

            try
            {
                maxList.Add(Convert.ToInt32(res.Rows[0]["ID"].ToString()));
                maxList.Sort();
                maxList.Reverse();
                return maxList[0];
            }
            catch
            {
                return -1;
            }
        }

        /// <summary>
        ///     Gets a free player result ID.
        /// </summary>
        /// <param name="dbFile">The db file.</param>
        /// <param name="used">Additional player result IDs to assume used.</param>
        /// <returns></returns>
        private static int GetFreePlayerResultID(string dbFile, List<int> used)
        {
            var db = new SQLiteDatabase(dbFile);

            const string q = "select ID from PlayerResults ORDER BY ID ASC;";
            DataTable res = db.GetDataTable(q);

            int i;
            for (i = 0; i < res.Rows.Count; i++)
            {
                if (Convert.ToInt32(res.Rows[i]["ID"].ToString()) != i)
                {
                    if (!used.Contains(i))
                        return i;
                }
            }
            i = res.Rows.Count;
            while (true)
            {
                if (!used.Contains(i))
                    return i;

                i++;
            }
        }

        /// <summary>
        ///     Gets the first free ID from the specified table.
        /// </summary>
        /// <param name="dbFile">The db file.</param>
        /// <param name="table">The table.</param>
        /// <param name="columnName">Name of the column; "ID" by default.</param>
        /// <returns></returns>
        public static int GetFreeID(string dbFile, string table, string columnName = "ID", List<int> used = null)
        {
            var db = new SQLiteDatabase(dbFile);
            if (used == null)
                used = new List<int>();

            string q = "select " + columnName + " from " + table + " ORDER BY " + columnName + " ASC;";
            DataTable res = db.GetDataTable(q);
            res.Rows.Cast<DataRow>().ToList().ForEach(r => used.Add(Convert.ToInt32(r["ID"].ToString())));
            int i = 0;
            while (true)
            {
                if (used.Contains(i))
                    i++;
                else
                    return i;
            }
        }

        /// <summary>
        ///     Saves the current team stats dictionaries to the current database.
        /// </summary>
        public static void SaveTeamsToDatabase()
        {
            SaveTeamsToDatabase(MainWindow.currentDB, MainWindow.tst, MainWindow.tstopp, MainWindow.curSeason,
                                getMaxSeason(MainWindow.currentDB));
        }

        public static void PopulateAll(Timeframe tf, out Dictionary<int, TeamStats> tst, out Dictionary<int, TeamStats> tstopp,
                                       out SortedDictionary<string, int> TeamOrder, out Dictionary<int, PlayerStats> pst,
                                       out Dictionary<int, Dictionary<string, TeamStats>> splitTeamStats,
                                       out Dictionary<int, Dictionary<string, PlayerStats>> splitPlayerStats, out List<BoxScoreEntry> bshist,
                                       out TeamRankings teamRankings, out PlayerRankings playerRankings,
                                       out TeamRankings playoffTeamRankings, out PlayerRankings playoffPlayerRankings,
                                       out Dictionary<int, string> DisplayNames)
        {
            tst = new Dictionary<int, TeamStats>();
            tstopp = new Dictionary<int, TeamStats>();
            TeamOrder = new SortedDictionary<string, int>();
            pst = new Dictionary<int, PlayerStats>();
            splitTeamStats = new Dictionary<int, Dictionary<string, TeamStats>>();
            splitPlayerStats = new Dictionary<int, Dictionary<string, PlayerStats>>();
            bshist = new List<BoxScoreEntry>();
            int curSeason = tf.SeasonNum;
            int maxSeason = getMaxSeason(MainWindow.currentDB);
            SQLiteDatabase db = MainWindow.db;

            string q;
            DataTable res;

            #region Prepare Teams & Players Dictionaries

            DisplayNames = new Dictionary<int, string>();

            if (!tf.isBetween)
            {
                GetAllTeamStatsFromDatabase(MainWindow.currentDB, tf.SeasonNum, out tst, out tstopp, out TeamOrder);
                foreach (var ts in tst)
                {
                    DisplayNames.Add(ts.Value.ID, ts.Value.displayName);
                }
                pst = GetPlayersFromDatabase(MainWindow.currentDB, tst, tstopp, TeamOrder, curSeason, maxSeason);
            }
            else
            {
                List<int> seasons = GetSeasonsInTimeframe(tf.StartDate, tf.EndDate);

                foreach (var i in seasons)
                {
                    q = "SELECT * FROM Teams" + AddSuffix(i, maxSeason) + " WHERE isHidden LIKE \"False\"";
                    res = db.GetDataTable(q);
                    foreach (DataRow dr in res.Rows)
                    {
                        int teamID = Tools.getInt(dr, "ID");
                        if (!tst.Keys.Contains(teamID))
                        {
                            tst.Add(teamID,
                                    new TeamStats
                                    {
                                        ID = teamID,
                                        name = Tools.getString(dr, "Name"),
                                        displayName = Tools.getString(dr, "DisplayName")
                                    });
                            tstopp.Add(teamID,
                                       new TeamStats
                                       {
                                           ID = teamID,
                                           name = Tools.getString(dr, "Name"),
                                           displayName = Tools.getString(dr, "DisplayName")
                                       });
                            TeamOrder.Add(Tools.getString(dr, "Name"), teamID);
                            DisplayNames.Add(Tools.getInt(dr, "ID"), Tools.getString(dr, "DisplayName"));
                        }
                    }

                    q = "SELECT * FROM Players" + AddSuffix(i, maxSeason) + " WHERE isHidden LIKE \"False\"";
                    res = db.GetDataTable(q);
                    foreach (DataRow dr in res.Rows)
                    {
                        int playerID = Tools.getInt(dr, "ID");
                        if (!pst.Keys.Contains(playerID))
                        {
                            pst.Add(playerID, new PlayerStats(dr, tst));
                            pst[playerID].ResetStats();
                        }
                    }

                    q = "SELECT * FROM CareerHighs";
                    res = db.GetDataTable(q);
                    foreach (DataRow dr in res.Rows)
                    {
                        int playerID = Tools.getInt(dr, "PlayerID");
                        if (pst.Keys.Contains(playerID))
                        {
                            pst[playerID].UpdateCareerHighs(dr);
                        }
                    }
                }
            }

            DisplayNames.Add(-1, "");

            RepairDB(ref pst);

            #endregion

            #region Prepare Split Dictionaries

            foreach (var id in TeamOrder.Values)
            {
                splitTeamStats.Add(id, new Dictionary<string, TeamStats>());
                splitTeamStats[id].Add("Wins", new TeamStats());
                splitTeamStats[id].Add("Losses", new TeamStats());
                splitTeamStats[id].Add("Home", new TeamStats());
                splitTeamStats[id].Add("Away", new TeamStats());
                splitTeamStats[id].Add("Season", new TeamStats());
                splitTeamStats[id].Add("Playoffs", new TeamStats());
                foreach (var pair in TeamOrder)
                {
                    if (pair.Value != id)
                    {
                        splitTeamStats[id].Add("vs " + DisplayNames[pair.Value], new TeamStats());
                    }
                }
                if (!tf.isBetween)
                {
                    string q2 = "SELECT Date FROM GameResults WHERE SeasonNum = " + tf.SeasonNum + " GROUP BY Date ORDER BY Date ASC";
                    DataTable dataTable = db.GetDataTable(q2);
                    if (dataTable.Rows.Count == 0)
                    {
                        tf.StartDate = DateTime.Today.AddMonths(-1).AddDays(1);
                        tf.EndDate = DateTime.Today;
                    }
                    else
                    {
                        tf.StartDate = Convert.ToDateTime(dataTable.Rows[0][0].ToString());
                        tf.EndDate = Convert.ToDateTime(dataTable.Rows[dataTable.Rows.Count - 1][0].ToString());
                    }
                }
                DateTime dCur = tf.StartDate;
                while (true)
                {
                    if (new DateTime(dCur.Year, dCur.Month, 1) == new DateTime(tf.EndDate.Year, tf.EndDate.Month, 1))
                    {
                        splitTeamStats[id].Add("M " + dCur.Year + " " + dCur.Month.ToString().PadLeft(2, '0'), new TeamStats());
                        break;
                    }
                    else
                    {
                        splitTeamStats[id].Add("M " + dCur.Year + " " + dCur.Month.ToString().PadLeft(2, '0'), new TeamStats());
                        dCur = new DateTime(dCur.Year, dCur.Month, 1).AddMonths(1);
                    }
                }
            }

            foreach (var id in pst.Keys)
            {
                splitPlayerStats.Add(id, new Dictionary<string, PlayerStats>());
                splitPlayerStats[id].Add("Wins", new PlayerStats {ID = id});
                splitPlayerStats[id].Add("Losses", new PlayerStats {ID = id});
                splitPlayerStats[id].Add("Home", new PlayerStats {ID = id});
                splitPlayerStats[id].Add("Away", new PlayerStats {ID = id});
                splitPlayerStats[id].Add("Season", new PlayerStats {ID = id});
                splitPlayerStats[id].Add("Playoffs", new PlayerStats {ID = id});

                string qr_teams =
                    String.Format(
                        "select TeamID from PlayerResults INNER JOIN GameResults ON " + "(PlayerResults.GameID = GameResults.GameID) " +
                        " WHERE PlayerID = {0}", id);
                if (tf.isBetween)
                {
                    qr_teams = SQLiteDatabase.AddDateRangeToSQLQuery(qr_teams, tf.StartDate, tf.EndDate);
                }
                else
                {
                    string s = " AND SeasonNum = " + tf.SeasonNum;
                    qr_teams += s;
                }
                qr_teams += " GROUP BY TeamID";
                res = db.GetDataTable(qr_teams);
                foreach (DataRow r in res.Rows)
                {
                    splitPlayerStats[id].Add("with " + DisplayNames[Tools.getInt(r, "TeamID")], new PlayerStats {ID = id});
                }

                foreach (var pair in TeamOrder)
                {
                    splitPlayerStats[id].Add("vs " + DisplayNames[pair.Value], new PlayerStats {ID = id});
                }
                DateTime dCur = tf.StartDate;

                while (true)
                {
                    if (new DateTime(dCur.Year, dCur.Month, 1) == new DateTime(tf.EndDate.Year, tf.EndDate.Month, 1))
                    {
                        splitPlayerStats[id].Add("M " + dCur.Year + " " + dCur.Month.ToString().PadLeft(2, '0'), new PlayerStats {ID = id});
                        break;
                    }
                    else
                    {
                        splitPlayerStats[id].Add("M " + dCur.Year + " " + dCur.Month.ToString().PadLeft(2, '0'), new PlayerStats {ID = id});
                        dCur = new DateTime(dCur.Year, dCur.Month, 1).AddMonths(1);
                    }
                }
            }

            #endregion

            #region Box Scores

            if (!tf.isBetween)
            {
                bshist = GetSeasonBoxScoresFromDatabase(MainWindow.currentDB, tf.SeasonNum, maxSeason, tst);
            }
            else
            {
                bshist = GetTimeframedBoxScoresFromDatabase(MainWindow.currentDB, tf.StartDate, tf.EndDate, tst);
            }

            bshist.Sort((bse1, bse2) => bse1.bs.gamedate.CompareTo(bse2.bs.gamedate));
            bshist.Reverse();

            if (tf.isBetween)
            {
                foreach (var bse in bshist)
                {
                    TeamStats.AddTeamStatsFromBoxScore(bse.bs, ref tst, ref tstopp, bse.bs.Team1ID, bse.bs.Team2ID);

                    foreach (var pbs in bse.pbsList)
                    {
                        PlayerStats ps = pst.Single(pair => pair.Value.ID == pbs.PlayerID).Value;
                        ps.AddBoxScore(pbs, bse.bs.isPlayoff);
                    }
                }
                /*
                TeamStats.CalculateAllMetrics(ref tst, tstopp);
                TeamStats.CalculateAllMetrics(ref tst, tstopp, playoffs: true);
                TeamStats.CalculateAllMetrics(ref tstopp, tst);
                TeamStats.CalculateAllMetrics(ref tstopp, tst, playoffs: true);
                */
                PlayerStats.CalculateAllMetrics(ref pst, tst, tstopp, TeamOrder);
                PlayerStats.CalculateAllMetrics(ref pst, tst, tstopp, TeamOrder, playoffs: true);
            }

            foreach (var bse in bshist)
            {
                int t1ID = bse.bs.Team1ID;
                int t2ID = bse.bs.Team2ID;
                TeamBoxScore bs = bse.bs;
                TeamStats tsH = splitTeamStats[t2ID]["Home"];
                TeamStats tsA = splitTeamStats[t1ID]["Away"];
                TeamStats.AddTeamStatsFromBoxScore(bs, ref tsA, ref tsH, true);
                TeamStats tsOH = splitTeamStats[t2ID]["vs " + DisplayNames[t1ID]];
                TeamStats tsOA = splitTeamStats[t1ID]["vs " + DisplayNames[t2ID]];
                TeamStats.AddTeamStatsFromBoxScore(bs, ref tsOA, ref tsOH, true);
                TeamStats tsDH = splitTeamStats[t2ID]["M " + bs.gamedate.Year + " " + bs.gamedate.Month.ToString().PadLeft(2, '0')];
                TeamStats tsDA = splitTeamStats[t1ID]["M " + bs.gamedate.Year + " " + bs.gamedate.Month.ToString().PadLeft(2, '0')];
                TeamStats.AddTeamStatsFromBoxScore(bs, ref tsDA, ref tsDH, true);
                if (!bse.bs.isPlayoff)
                {
                    TeamStats tsSH = splitTeamStats[t2ID]["Season"];
                    TeamStats tsSA = splitTeamStats[t1ID]["Season"];
                    TeamStats.AddTeamStatsFromBoxScore(bs, ref tsSA, ref tsSH, true);
                }
                else
                {
                    TeamStats tsSH = splitTeamStats[t2ID]["Playoffs"];
                    TeamStats tsSA = splitTeamStats[t1ID]["Playoffs"];
                    TeamStats.AddTeamStatsFromBoxScore(bs, ref tsSA, ref tsSH, true);
                }
                if (bs.PTS1 > bs.PTS2)
                {
                    TeamStats ts2 = splitTeamStats[t2ID]["Losses"];
                    TeamStats ts1 = splitTeamStats[t1ID]["Wins"];
                    TeamStats.AddTeamStatsFromBoxScore(bs, ref ts1, ref ts2, true);

                    foreach (var pbs in bse.pbsList)
                    {
                        if (pbs.TeamID == t1ID)
                        {
                            splitPlayerStats[pbs.PlayerID]["Wins"].AddBoxScore(pbs);
                        }
                        else
                        {
                            splitPlayerStats[pbs.PlayerID]["Losses"].AddBoxScore(pbs);
                        }
                    }
                }
                else
                {
                    TeamStats ts1 = splitTeamStats[t1ID]["Losses"];
                    TeamStats ts2 = splitTeamStats[t2ID]["Wins"];
                    TeamStats.AddTeamStatsFromBoxScore(bs, ref ts1, ref ts2, true);

                    foreach (var pbs in bse.pbsList)
                    {
                        if (pbs.TeamID == t1ID)
                        {
                            splitPlayerStats[pbs.PlayerID]["Losses"].AddBoxScore(pbs);
                        }
                        else
                        {
                            splitPlayerStats[pbs.PlayerID]["Wins"].AddBoxScore(pbs);
                        }
                    }
                }
                foreach (var pbs in bse.pbsList)
                {
                    if (pbs.TeamID == t1ID)
                    {
                        splitPlayerStats[pbs.PlayerID]["Away"].AddBoxScore(pbs);
                        splitPlayerStats[pbs.PlayerID]["vs " + DisplayNames[t2ID]].AddBoxScore(pbs);
                        splitPlayerStats[pbs.PlayerID]["with " + DisplayNames[t1ID]].AddBoxScore(pbs);
                    }
                    else
                    {
                        splitPlayerStats[pbs.PlayerID]["Home"].AddBoxScore(pbs);
                        splitPlayerStats[pbs.PlayerID]["vs " + DisplayNames[t1ID]].AddBoxScore(pbs);
                        splitPlayerStats[pbs.PlayerID]["with " + DisplayNames[t2ID]].AddBoxScore(pbs);
                    }
                    splitPlayerStats[pbs.PlayerID][bs.isPlayoff ? "Playoffs" : "Season"].AddBoxScore(pbs);

                    splitPlayerStats[pbs.PlayerID]["M " + bs.gamedate.Year + " " + bs.gamedate.Month.ToString().PadLeft(2, '0')].AddBoxScore
                        (pbs);
                }
            }

            #endregion

            foreach (var ps in pst)
            {
                ps.Value.CalculateSeasonHighs(bshist);
            }

            teamRankings = new TeamRankings(tst);
            playoffTeamRankings = new TeamRankings(tst, true);
            playerRankings = new PlayerRankings(pst);
            playoffPlayerRankings = new PlayerRankings(pst, true);
        }

        private static void FindTeamByName(string teamName, DateTime startDate, DateTime endDate, out TeamStats ts, out TeamStats tsopp,
                                           out int lastInSeason)
        {
            int maxSeason = getMaxSeason(MainWindow.currentDB);

            string q = "SELECT SeasonNum FROM GameResults";
            q = SQLiteDatabase.AddDateRangeToSQLQuery(q, startDate, endDate, true);
            q += " GROUP BY SeasonNum";
            DataTable res = MainWindow.db.GetDataTable(q);
            List<DataRow> rows = res.Rows.Cast<DataRow>().OrderByDescending(row => row["SeasonNum"]).ToList();

            foreach (var r in rows)
            {
                int curSeason = Tools.getInt(r, "SeasonNum");
                q = "SELECT * FROM Teams" + AddSuffix(curSeason, maxSeason);
                DataTable res2 = MainWindow.db.GetDataTable(q);
                List<DataRow> rows2 = res2.Rows.Cast<DataRow>().ToList();
                try
                {
                    DataRow r2 = rows2.Single(row => row["Name"].ToString() == teamName);
                    GetTeamStatsFromDatabase(MainWindow.currentDB, Tools.getInt(r2, "ID"), curSeason, out ts, out tsopp);
                    lastInSeason = curSeason;
                    return;
                }
                catch (Exception)
                {
                }
            }
            ts = null;
            tsopp = null;
            lastInSeason = 0;
        }

        public static string AddSuffix(int curSeason, int maxSeason)
        {
            return (curSeason != maxSeason ? "S" + curSeason : "");
        }
    }
}