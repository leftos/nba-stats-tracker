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

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LeftosCommonLibrary;
using NBA_Stats_Tracker.Interop;
using NBA_Stats_Tracker.Windows;
using SQLite_Database;

namespace NBA_Stats_Tracker.Data
{
    internal static class SQLiteIO
    {
        public static void saveAllSeasons(string file)
        {
            string oldDB = MainWindow.currentDB;
            int oldSeason = MainWindow.curSeason;

            int maxSeason = getMaxSeason(oldDB);

            MainWindow.bshist = GetAllBoxScoresFromDatabase(oldDB);
            saveSeasonToDatabase(file, MainWindow.tst, MainWindow.tstopp, MainWindow.pst, MainWindow.curSeason,
                                 maxSeason);

            for (int i = 1; i <= maxSeason; i++)
            {
                if (i != oldSeason)
                {
                    LoadSeason(oldDB, out MainWindow.tst, out MainWindow.tstopp, out MainWindow.pst,
                               out MainWindow.TeamOrder, ref MainWindow.pt, ref MainWindow.bshist, _curSeason: i,
                               doNotLoadBoxScores: true);
                    saveSeasonToDatabase(file, MainWindow.tst, MainWindow.tstopp, MainWindow.pst, MainWindow.curSeason,
                                         maxSeason, doNotSaveBoxScores: true);
                }
            }
            LoadSeason(file, out MainWindow.tst, out MainWindow.tstopp, out MainWindow.pst, out MainWindow.TeamOrder,
                       ref MainWindow.pt, ref MainWindow.bshist, true, oldSeason,
                       doNotLoadBoxScores: true);
        }

        public static void saveSeasonToDatabase()
        {
            saveSeasonToDatabase(MainWindow.currentDB, MainWindow.tst, MainWindow.tstopp, MainWindow.pst,
                                 MainWindow.curSeason, getMaxSeason(MainWindow.currentDB));
        }

        public static void saveSeasonToDatabase(string file, Dictionary<int, TeamStats> tstToSave,
                                                Dictionary<int, TeamStats> tstoppToSave,
                                                Dictionary<int, PlayerStats> pstToSave,
                                                int season, int maxSeason, bool doNotSaveBoxScores = false)
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
            if (!FileExists) prepareNewDB(MainWindow.db, season, maxSeason);

            SaveSeasonName(season);

            SaveTeamsToDatabase(file, tstToSave, tstoppToSave, season, maxSeason);

            #region Save Player Stats

            savePlayersToDatabase(file, pstToSave, season, maxSeason);

            #endregion

            #region Save Box Scores

            if (!doNotSaveBoxScores)
            {
                const string q = "select GameID from GameResults;";
                DataTable res = MainWindow.db.GetDataTable(q);
                List<int> idList = (from DataRow r in res.Rows select Convert.ToInt32(r[0].ToString())).ToList();

                foreach (BoxScoreEntry bse in MainWindow.bshist)
                {
                    string md5 = Tools.GetMD5(DateTime.Now.ToString(CultureInfo.InvariantCulture));
                    if ((!FileExists) || (bse.bs.id == -1) || (!idList.Contains(bse.bs.id)) || (bse.mustUpdate))
                    {
                        var dict2 = new Dictionary<string, string>
                                        {
                                            {"T1Name", bse.bs.Team1},
                                            {"T2Name", bse.bs.Team2},
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
                                    MainWindow.db.GetDataTable("select GameID from GameResults where HASH LIKE \"" + md5 +
                                                               "\"").
                                        Rows
                                        [0][
                                            "GameID"].ToString());
                            bse.bs.id = lastid;
                        }
                        MainWindow.db.Delete("PlayerResults", "GameID = " + bse.bs.id);

                        var sqlinsert = new List<Dictionary<string, string>>(500);
                        var used = new List<int>();
                        foreach (PlayerBoxScore pbs in bse.pbsList)
                        {
                            int id = GetFreePlayerResultID(file, used);
                            used.Add(id);
                            dict2 = new Dictionary<string, string>
                                        {
                                            {"ID", id.ToString()},
                                            {"GameID", bse.bs.id.ToString()},
                                            {"PlayerID", pbs.PlayerID.ToString()},
                                            {"Team", pbs.Team},
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

                            if (sqlinsert.Count == 500)
                            {
                                MainWindow.db.InsertMany("PlayerResults", sqlinsert);
                                sqlinsert.Clear();
                            }

                            //db.Insert("PlayerResults", dict2);
                        }

                        if (sqlinsert.Count > 0)
                        {
                            MainWindow.db.InsertMany("PlayerResults", sqlinsert);
                        }
                    }
                }
            }

            #endregion

            MainWindow.mwInstance.txtFile.Text = file;
            MainWindow.currentDB = file;
            MainWindow.isCustom = true;

            //}
            //catch (Exception ex)
            //{
            //App.errorReport(ex, "Trying to save team stats - SQLite");
            //}
        }

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
                if (result < 1) throw (new Exception());
            }
            catch (Exception)
            {
                MainWindow.db.Insert("SeasonNames", dict);
            }
        }

        private static void SaveTeamsToDatabase(string file, Dictionary<int, TeamStats> tstToSave,
                                                Dictionary<int, TeamStats> tstoppToSave, int season,
                                                int maxSeason)
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
            foreach (int key in tstToSave.Keys)
            {
                //bool found = false;

                var dict = new Dictionary<string, string>
                               {
                                   {"ID", tstToSave[key].ID.ToString()},
                                   {"Name", tstToSave[key].name},
                                   {"DisplayName", tstToSave[key].displayName},
                                   {"isHidden", tstToSave[key].isHidden.ToString()},
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
                                   {"TOS", tstToSave[key].stats[t.TO].ToString()},
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
                                      {"TOS", tstToSave[key].pl_stats[t.TO].ToString()},
                                      {"BLK", tstToSave[key].pl_stats[t.BLK].ToString()},
                                      {"AST", tstToSave[key].pl_stats[t.AST].ToString()},
                                      {"FOUL", tstToSave[key].pl_stats[t.FOUL].ToString()},
                                      {"OFFSET", tstToSave[key].pl_offset.ToString()}
                                  };

                playoffList.Add(pl_dict);

                i++;

                if (i == 500)
                {
                    _db.InsertMany(teamsT, seasonList);
                    _db.InsertMany(pl_teamsT, playoffList);
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
                _db.InsertMany(teamsT, seasonList);
                _db.InsertMany(pl_teamsT, playoffList);
            }

            seasonList = new List<Dictionary<string, string>>(500);
            playoffList = new List<Dictionary<string, string>>(500);
            i = 0;
            foreach (int key in tstoppToSave.Keys)
            {
                //bool found = false;

                var dict = new Dictionary<string, string>
                               {
                                   {"ID", MainWindow.TeamOrder[tstToSave[key].name].ToString()},
                                   {"Name", tstToSave[key].name},
                                   {"DisplayName", tstToSave[key].displayName},
                                   {"isHidden", tstToSave[key].isHidden.ToString()},
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
                                   {"TOS", tstToSave[key].stats[t.TO].ToString()},
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
                                      {"TOS", tstToSave[key].pl_stats[t.TO].ToString()},
                                      {"BLK", tstToSave[key].pl_stats[t.BLK].ToString()},
                                      {"AST", tstToSave[key].pl_stats[t.AST].ToString()},
                                      {"FOUL", tstToSave[key].pl_stats[t.FOUL].ToString()},
                                      {"OFFSET", tstToSave[key].pl_offset.ToString()}
                                  };

                playoffList.Add(pl_dict);

                i++;

                if (i == 500)
                {
                    _db.InsertMany(oppT, seasonList);
                    _db.InsertMany(pl_oppT, playoffList);
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
                _db.InsertMany(oppT, seasonList);
                _db.InsertMany(pl_oppT, playoffList);
            }
        }

        public static void savePlayersToDatabase(string file, Dictionary<int, PlayerStats> playerStats, int season,
                                                 int maxSeason, bool partialUpdate = false)
        {
            var _db = new SQLiteDatabase(file);

            string playersT = "Players";

            if (season != maxSeason)
            {
                playersT += "S" + season.ToString();
            }

            if (!partialUpdate) MainWindow.db.ClearTable(playersT);
            string q = "select ID from " + playersT + ";";
            DataTable res = MainWindow.db.GetDataTable(q);

            //var idList = (from DataRow dr in res.Rows select Convert.ToInt32(dr["ID"].ToString())).ToList();

            var sqlinsert = new List<Dictionary<string, string>>(500);
            int i = 0;

            foreach (var kvp in playerStats)
            {
                PlayerStats ps = kvp.Value;
                if (partialUpdate) _db.Delete(playersT, "ID = " + ps.ID);
                var dict = new Dictionary<string, string>
                               {
                                   {"ID", ps.ID.ToString()},
                                   {"LastName", ps.LastName},
                                   {"FirstName", ps.FirstName},
                                   {"Position1", ps.Position1},
                                   {"Position2", ps.Position2},
                                   {"isActive", ps.isActive.ToString()},
                                   {"isInjured", ps.isInjured.ToString()},
                                   {"TeamFin", ps.TeamF},
                                   {"TeamSta", ps.TeamS},
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
                                   {"TOS", ps.stats[p.TO].ToString()},
                                   {"BLK", ps.stats[p.BLK].ToString()},
                                   {"AST", ps.stats[p.AST].ToString()},
                                   {"FOUL", ps.stats[p.FOUL].ToString()},
                                   {"isAllStar", ps.isAllStar.ToString()},
                                   {"isNBAChampion", ps.isNBAChampion.ToString()}
                               };

                sqlinsert.Add(dict);
                i++;

                /*
                if (idList.Contains(ps.ID))
                {
                    dict.Remove("ID");
                    _db.Update(playersT, dict, "ID = " + ps.ID.ToString());
                }
                else
                {
                    _db.Insert(playersT, dict);
                }
                */

                if (i == 500)
                {
                    _db.InsertMany(playersT, sqlinsert);
                    i = 0;
                    sqlinsert.Clear();
                }
            }

            if (i > 0)
            {
                _db.InsertMany(playersT, sqlinsert);
            }
        }

        public static void prepareNewDB(SQLiteDatabase sqldb, int curSeason, int maxSeason, bool onlyNewSeason = false)
        {
            try
            {
                String qr;

                if (!onlyNewSeason)
                {
                    qr = "DROP TABLE IF EXISTS \"GameResults\"";
                    sqldb.ExecuteNonQuery(qr);
                    qr =
                        "CREATE TABLE \"GameResults\" (\"GameID\" INTEGER PRIMARY KEY  NOT NULL ,\"T1Name\" TEXT NOT NULL ,\"T2Name\" TEXT NOT NULL ,\"Date\" DATE NOT NULL ,\"SeasonNum\" INTEGER NOT NULL ,\"IsPlayoff\" TEXT NOT NULL  DEFAULT ('FALSE') ,\"T1PTS\" INTEGER NOT NULL ,\"T1REB\" INTEGER NOT NULL ,\"T1AST\" INTEGER NOT NULL ,\"T1STL\" INTEGER NOT NULL ,\"T1BLK\" INTEGER NOT NULL ,\"T1TOS\" INTEGER NOT NULL ,\"T1FGM\" INTEGER NOT NULL ,\"T1FGA\" INTEGER NOT NULL ,\"T13PM\" INTEGER NOT NULL ,\"T13PA\" INTEGER NOT NULL ,\"T1FTM\" INTEGER NOT NULL ,\"T1FTA\" INTEGER NOT NULL ,\"T1OREB\" INTEGER NOT NULL ,\"T1FOUL\" INTEGER NOT NULL,\"T1MINS\" INTEGER NOT NULL ,\"T2PTS\" INTEGER NOT NULL ,\"T2REB\" INTEGER NOT NULL ,\"T2AST\" INTEGER NOT NULL ,\"T2STL\" INTEGER NOT NULL ,\"T2BLK\" INTEGER NOT NULL ,\"T2TOS\" INTEGER NOT NULL ,\"T2FGM\" INTEGER NOT NULL ,\"T2FGA\" INTEGER NOT NULL ,\"T23PM\" INTEGER NOT NULL ,\"T23PA\" INTEGER NOT NULL ,\"T2FTM\" INTEGER NOT NULL ,\"T2FTA\" INTEGER NOT NULL ,\"T2OREB\" INTEGER NOT NULL ,\"T2FOUL\" INTEGER NOT NULL,\"T2MINS\" INTEGER NOT NULL, \"HASH\" TEXT )";
                    sqldb.ExecuteNonQuery(qr);
                    qr = "DROP TABLE IF EXISTS \"PlayerResults\"";
                    sqldb.ExecuteNonQuery(qr);
                    qr =
                        "CREATE TABLE \"PlayerResults\" (\"ID\" INTEGER PRIMARY KEY  NOT NULL ,\"GameID\" INTEGER NOT NULL ,\"PlayerID\" INTEGER NOT NULL ,\"Team\" TEXT NOT NULL ,\"isStarter\" TEXT, \"playedInjured\" TEXT, \"isOut\" TEXT, \"MINS\" INTEGER NOT NULL  DEFAULT (0), \"PTS\" INTEGER NOT NULL ,\"REB\" INTEGER NOT NULL ,\"AST\" INTEGER NOT NULL ,\"STL\" INTEGER NOT NULL ,\"BLK\" INTEGER NOT NULL ,\"TOS\" INTEGER NOT NULL ,\"FGM\" INTEGER NOT NULL ,\"FGA\" INTEGER NOT NULL ,\"TPM\" INTEGER NOT NULL ,\"TPA\" INTEGER NOT NULL ,\"FTM\" INTEGER NOT NULL ,\"FTA\" INTEGER NOT NULL ,\"OREB\" INTEGER NOT NULL ,\"FOUL\" INTEGER NOT NULL  DEFAULT (0) )";
                    sqldb.ExecuteNonQuery(qr);
                    qr = "DROP TABLE IF EXISTS \"Misc\"";
                    sqldb.ExecuteNonQuery(qr);
                    qr = "CREATE TABLE \"Misc\" (\"CurSeason\" INTEGER);";
                    sqldb.ExecuteNonQuery(qr);
                    qr = "DROP TABLE IF EXISTS \"SeasonNames\"";
                    sqldb.ExecuteNonQuery(qr);
                    qr = "CREATE TABLE \"SeasonNames\" (\"ID\" INTEGER PRIMARY KEY  NOT NULL , \"Name\" TEXT)";
                    sqldb.ExecuteNonQuery(qr);
                    sqldb.Insert("SeasonNames",
                                 new Dictionary<string, string>
                                     {{"ID", curSeason.ToString()}, {"Name", curSeason.ToString()}});
                }
                string teamsT = "Teams";
                string pl_teamsT = "PlayoffTeams";
                string oppT = "Opponents";
                string pl_oppT = "PlayoffOpponents";
                string playersT = "Players";
                if (curSeason != maxSeason)
                {
                    string s = "S" + curSeason.ToString();
                    teamsT += s;
                    pl_teamsT += s;
                    oppT += s;
                    pl_oppT += s;
                    playersT += s;
                }
                qr = "DROP TABLE IF EXISTS \"" + pl_teamsT + "\"";
                sqldb.ExecuteNonQuery(qr);
                qr = "CREATE TABLE \"" + pl_teamsT +
                     "\" (\"ID\" INTEGER PRIMARY KEY  NOT NULL ,\"Name\" TEXT NOT NULL ,\"DisplayName\" TEXT NOT NULL,\"isHidden\" TEXT NOT NULL, \"WIN\" INTEGER NOT NULL ,\"LOSS\" INTEGER NOT NULL ,\"MINS\" INTEGER, \"PF\" INTEGER NOT NULL ,\"PA\" INTEGER NOT NULL ,\"FGM\" INTEGER NOT NULL ,\"FGA\" INTEGER NOT NULL ,\"TPM\" INTEGER NOT NULL ,\"TPA\" INTEGER NOT NULL ,\"FTM\" INTEGER NOT NULL ,\"FTA\" INTEGER NOT NULL ,\"OREB\" INTEGER NOT NULL ,\"DREB\" INTEGER NOT NULL ,\"STL\" INTEGER NOT NULL ,\"TOS\" INTEGER NOT NULL ,\"BLK\" INTEGER NOT NULL ,\"AST\" INTEGER NOT NULL ,\"FOUL\" INTEGER NOT NULL ,\"OFFSET\" INTEGER)";
                sqldb.ExecuteNonQuery(qr);

                qr = "DROP TABLE IF EXISTS \"" + teamsT + "\"";
                sqldb.ExecuteNonQuery(qr);
                qr = "CREATE TABLE \"" + teamsT +
                     "\" (\"ID\" INTEGER PRIMARY KEY  NOT NULL ,\"Name\" TEXT NOT NULL ,\"DisplayName\" TEXT NOT NULL,\"isHidden\" TEXT NOT NULL, \"WIN\" INTEGER NOT NULL ,\"LOSS\" INTEGER NOT NULL ,\"MINS\" INTEGER, \"PF\" INTEGER NOT NULL ,\"PA\" INTEGER NOT NULL ,\"FGM\" INTEGER NOT NULL ,\"FGA\" INTEGER NOT NULL ,\"TPM\" INTEGER NOT NULL ,\"TPA\" INTEGER NOT NULL ,\"FTM\" INTEGER NOT NULL ,\"FTA\" INTEGER NOT NULL ,\"OREB\" INTEGER NOT NULL ,\"DREB\" INTEGER NOT NULL ,\"STL\" INTEGER NOT NULL ,\"TOS\" INTEGER NOT NULL ,\"BLK\" INTEGER NOT NULL ,\"AST\" INTEGER NOT NULL ,\"FOUL\" INTEGER NOT NULL ,\"OFFSET\" INTEGER)";
                sqldb.ExecuteNonQuery(qr);

                qr = "DROP TABLE IF EXISTS \"" + pl_oppT + "\"";
                sqldb.ExecuteNonQuery(qr);
                qr = "CREATE TABLE \"" + pl_oppT +
                     "\" (\"ID\" INTEGER PRIMARY KEY  NOT NULL ,\"Name\" TEXT NOT NULL ,\"DisplayName\" TEXT NOT NULL,\"isHidden\" TEXT NOT NULL, \"WIN\" INTEGER NOT NULL ,\"LOSS\" INTEGER NOT NULL ,\"MINS\" INTEGER, \"PF\" INTEGER NOT NULL ,\"PA\" INTEGER NOT NULL ,\"FGM\" INTEGER NOT NULL ,\"FGA\" INTEGER NOT NULL ,\"TPM\" INTEGER NOT NULL ,\"TPA\" INTEGER NOT NULL ,\"FTM\" INTEGER NOT NULL ,\"FTA\" INTEGER NOT NULL ,\"OREB\" INTEGER NOT NULL ,\"DREB\" INTEGER NOT NULL ,\"STL\" INTEGER NOT NULL ,\"TOS\" INTEGER NOT NULL ,\"BLK\" INTEGER NOT NULL ,\"AST\" INTEGER NOT NULL ,\"FOUL\" INTEGER NOT NULL ,\"OFFSET\" INTEGER)";
                sqldb.ExecuteNonQuery(qr);

                qr = "DROP TABLE IF EXISTS \"" + oppT + "\"";
                sqldb.ExecuteNonQuery(qr);
                qr = "CREATE TABLE \"" + oppT +
                     "\" (\"ID\" INTEGER PRIMARY KEY  NOT NULL ,\"Name\" TEXT NOT NULL ,\"DisplayName\" TEXT NOT NULL,\"isHidden\" TEXT NOT NULL, \"WIN\" INTEGER NOT NULL ,\"LOSS\" INTEGER NOT NULL ,\"MINS\" INTEGER, \"PF\" INTEGER NOT NULL ,\"PA\" INTEGER NOT NULL ,\"FGM\" INTEGER NOT NULL ,\"FGA\" INTEGER NOT NULL ,\"TPM\" INTEGER NOT NULL ,\"TPA\" INTEGER NOT NULL ,\"FTM\" INTEGER NOT NULL ,\"FTA\" INTEGER NOT NULL ,\"OREB\" INTEGER NOT NULL ,\"DREB\" INTEGER NOT NULL ,\"STL\" INTEGER NOT NULL ,\"TOS\" INTEGER NOT NULL ,\"BLK\" INTEGER NOT NULL ,\"AST\" INTEGER NOT NULL ,\"FOUL\" INTEGER NOT NULL ,\"OFFSET\" INTEGER)";
                sqldb.ExecuteNonQuery(qr);

                qr = "DROP TABLE IF EXISTS \"" + playersT + "\"";
                sqldb.ExecuteNonQuery(qr);
                qr = "CREATE TABLE \"" + playersT +
                     "\" (\"ID\" INTEGER PRIMARY KEY  NOT NULL ,\"LastName\" TEXT NOT NULL ,\"FirstName\" TEXT NOT NULL ,\"Position1\" TEXT,\"Position2\" TEXT,\"isActive\" TEXT,\"isInjured\" TEXT,\"TeamFin\" TEXT,\"TeamSta\" TEXT,\"GP\" INTEGER,\"GS\" INTEGER,\"MINS\" INTEGER NOT NULL  DEFAULT (0) ,\"PTS\" INTEGER NOT NULL ,\"FGM\" INTEGER NOT NULL ,\"FGA\" INTEGER NOT NULL ,\"TPM\" INTEGER NOT NULL ,\"TPA\" INTEGER NOT NULL ,\"FTM\" INTEGER NOT NULL ,\"FTA\" INTEGER NOT NULL ,\"OREB\" INTEGER NOT NULL ,\"DREB\" INTEGER NOT NULL ,\"STL\" INTEGER NOT NULL ,\"TOS\" INTEGER NOT NULL ,\"BLK\" INTEGER NOT NULL ,\"AST\" INTEGER NOT NULL ,\"FOUL\" INTEGER NOT NULL ,\"isAllStar\" TEXT,\"isNBAChampion\" TEXT)";
                sqldb.ExecuteNonQuery(qr);
            }
            catch
            {
            }
        }

        public static int getMaxSeason(string file)
        {
            try
            {
                if (!File.Exists(file)) throw (new Exception());

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

        public static void GetTeamStatsFromDatabase(string file, string team, int season, out TeamStats ts,
                                                    out TeamStats tsopp)
        {
            var _db = new SQLiteDatabase(file);

            String q;
            int maxSeason = getMaxSeason(file);

            if (season == 0) season = maxSeason;

            if (maxSeason == season)
            {
                q = "select * from Teams where Name LIKE \"" + team + "\"";
            }
            else
            {
                q = "select * from TeamsS" + season.ToString() + " where Name LIKE \"" + team + "'";
            }

            DataTable res = _db.GetDataTable(q);

            ts = new TeamStats();

            foreach (DataRow r in res.Rows)
            {
                ts = new TeamStats(r["Name"].ToString());

                // For compatibility reasons, if properties added after v0.10.5.1 don't exist,
                // we create them without error.
                try
                {
                    ts.displayName = r["DisplayName"].ToString();
                    ts.isHidden = Tools.getBoolean(r, "isHidden");
                }
                catch (Exception)
                {
                    ts.displayName = ts.name;
                    ts.isHidden = false;
                }

                ts.ID = Convert.ToInt32(r["ID"].ToString());
                ts.offset = Convert.ToInt32(r["OFFSET"].ToString());
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
                ts.stats[t.TO] = Convert.ToUInt16(r["TOS"].ToString());
                ts.stats[t.BLK] = Convert.ToUInt16(r["BLK"].ToString());
                ts.stats[t.AST] = Convert.ToUInt16(r["AST"].ToString());
                ts.stats[t.FOUL] = Convert.ToUInt16(r["FOUL"].ToString());
            }

            if (maxSeason == season)
            {
                q = "select * from PlayoffTeams where Name LIKE \"" + team + "\"";
            }
            else
            {
                q = "select * from PlayoffTeamsS" + season.ToString() + " where Name LIKE \"" + team + "\"";
            }
            res = _db.GetDataTable(q);

            foreach (DataRow r in res.Rows)
            {
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
                ts.pl_stats[t.TO] = Convert.ToUInt16(r["TOS"].ToString());
                ts.pl_stats[t.BLK] = Convert.ToUInt16(r["BLK"].ToString());
                ts.pl_stats[t.AST] = Convert.ToUInt16(r["AST"].ToString());
                ts.pl_stats[t.FOUL] = Convert.ToUInt16(r["FOUL"].ToString());

                ts.calcAvg();
            }

            if (maxSeason == season)
            {
                q = "select * from Opponents where Name LIKE \"" + team + "\"";
            }
            else
            {
                q = "select * from OpponentsS" + season.ToString() + " where Name LIKE \"" + team + "\"";
            }

            res = _db.GetDataTable(q);

            tsopp = new TeamStats();

            foreach (DataRow r in res.Rows)
            {
                tsopp = new TeamStats(r["Name"].ToString());

                // For compatibility reasons, if properties added after v0.10.5.1 don't exist,
                // we create them without error.
                try
                {
                    tsopp.displayName = r["DisplayName"].ToString();
                    tsopp.isHidden = Tools.getBoolean(r, "isHidden");
                }
                catch (Exception)
                {
                    tsopp.displayName = tsopp.name;
                    tsopp.isHidden = false;
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
                tsopp.stats[t.TO] = Convert.ToUInt16(r["TOS"].ToString());
                tsopp.stats[t.BLK] = Convert.ToUInt16(r["BLK"].ToString());
                tsopp.stats[t.AST] = Convert.ToUInt16(r["AST"].ToString());
                tsopp.stats[t.FOUL] = Convert.ToUInt16(r["FOUL"].ToString());
            }

            if (maxSeason == season)
            {
                q = "select * from PlayoffOpponents where Name LIKE \"" + team + "\"";
            }
            else
            {
                q = "select * from PlayoffOpponentsS" + season.ToString() + " where Name LIKE \"" + team + "\"";
            }
            res = _db.GetDataTable(q);

            foreach (DataRow r in res.Rows)
            {
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
                tsopp.pl_stats[t.TO] = Convert.ToUInt16(r["TOS"].ToString());
                tsopp.pl_stats[t.BLK] = Convert.ToUInt16(r["BLK"].ToString());
                tsopp.pl_stats[t.AST] = Convert.ToUInt16(r["AST"].ToString());
                tsopp.pl_stats[t.FOUL] = Convert.ToUInt16(r["FOUL"].ToString());

                tsopp.calcAvg();
            }
        }

        public static void GetAllTeamStatsFromDatabase(string file, int season, out Dictionary<int, TeamStats> _tst,
                                                       out Dictionary<int, TeamStats> _tstopp,
                                                       out SortedDictionary<string, int> TeamOrder)
        {
            var _db = new SQLiteDatabase(file);

            String q;
            int maxSeason = getMaxSeason(file);

            if (season == 0) season = maxSeason;

            if (maxSeason == season)
            {
                q = "select Name from Teams;";
            }
            else
            {
                q = "select Name from TeamsS" + season.ToString() + ";";
            }

            DataTable res = _db.GetDataTable(q);

            _tst = new Dictionary<int, TeamStats>();
            _tstopp = new Dictionary<int, TeamStats>();
            TeamOrder = new SortedDictionary<string, int>();
            int i = 0;

            foreach (DataRow r in res.Rows)
            {
                string name = r["Name"].ToString();
                var ts = new TeamStats(name);
                var tsopp = new TeamStats(name);
                GetTeamStatsFromDatabase(file, name, season, out ts, out tsopp);
                _tst[i] = ts;
                _tstopp[i] = tsopp;
                TeamOrder.Add(name, i);
                i++;
            }
        }

        /// <summary>
        /// Default implementation of LoadSeason; calls LoadSeason to update MainWindow data
        /// </summary>
        public static void LoadSeason()
        {
            LoadSeason(MainWindow.currentDB, out MainWindow.tst, out MainWindow.tstopp, out MainWindow.pst,
                       out MainWindow.TeamOrder, ref MainWindow.pt, ref MainWindow.bshist,
                       _curSeason: MainWindow.curSeason,
                       doNotLoadBoxScores: true);
        }

        public static void LoadSeason(string file, out Dictionary<int, TeamStats> _tst,
                                      out Dictionary<int, TeamStats> _tstopp,
                                      out Dictionary<int, PlayerStats> pst,
                                      out SortedDictionary<string, int> _TeamOrder, ref PlayoffTree _pt,
                                      ref IList<BoxScoreEntry> _bshist, bool updateCombo = true,
                                      int _curSeason = 0, bool doNotLoadBoxScores = false)
        {
            MainWindow.loadingSeason = true;

            UpgradeDB(file);

            int maxSeason = getMaxSeason(file);

            if (_curSeason == 0) _curSeason = maxSeason;

            GetAllTeamStatsFromDatabase(file, _curSeason, out _tst, out _tstopp, out _TeamOrder);

            pst = GetPlayersFromDatabase(file, _tst, _tstopp, _TeamOrder, _curSeason, maxSeason);

            if (!doNotLoadBoxScores) _bshist = GetSeasonBoxScoresFromDatabase(file, _curSeason, maxSeason);

            /*
            try
            {
                q = "select CurSeason from Misc limit 1;";
                res = _db.GetDataTable(q);
                curSeason = Convert.ToInt32(res.Rows[0]["CurSeason"].ToString());
            }
            catch
            {
                curSeason = 1;
            }
            */
            MainWindow.ChangeSeason(_curSeason, maxSeason);

            /*
            if (updateCombo)
            {
                mwInstance.cmbTeam1.Items.Clear();
                foreach (KeyValuePair<string, int> kvp in _TeamOrder)
                {
                    mwInstance.cmbTeam1.Items.Add(kvp.Key);
                }
            }
            */

            MainWindow.loadingSeason = false;
        }

        /// <summary>
        /// Checks for missing and changed fields in older databases and upgrades them to the current format.
        /// </summary>
        /// <param name="file">The path to the database.</param>
        private static void UpgradeDB(string file)
        {
            var db = new SQLiteDatabase(file);

            // Check for missing SeasonNames table (v0.11)

            #region SeasonNames

            string qr = "SELECT * FROM SeasonNames";
            try
            {
                DataTable dt = db.GetDataTable(qr);
            }
            catch (Exception)
            {
                int maxSeason = getMaxSeason(file);
                qr = "CREATE TABLE \"SeasonNames\" (\"ID\" INTEGER PRIMARY KEY  NOT NULL , \"Name\" TEXT)";
                db.ExecuteNonQuery(qr);

                for (int i = 1; i <= maxSeason; i++)
                {
                    db.Insert("SeasonNames",
                              new Dictionary<string, string> {{"ID", i.ToString()}, {"Name", i.ToString()}});
                }
            }

            #endregion
        }

        public static IList<BoxScoreEntry> GetAllBoxScoresFromDatabase(string file)
        {
            int maxSeason = getMaxSeason(file);

            IList<BoxScoreEntry> bshist = new List<BoxScoreEntry>();

            for (int i = maxSeason; i >= 1; i--)
            {
                IList<BoxScoreEntry> temp = GetSeasonBoxScoresFromDatabase(MainWindow.currentDB, i, maxSeason);

                foreach (BoxScoreEntry bse in temp)
                {
                    bshist.Add(bse);
                }
            }

            return bshist;
        }

        public static IList<BoxScoreEntry> GetSeasonBoxScoresFromDatabase(string file, int curSeason, int maxSeason)
        {
            var _db = new SQLiteDatabase(file);

            string q = "select * from GameResults WHERE SeasonNum = " + curSeason + " ORDER BY Date DESC;";
            DataTable res2 = _db.GetDataTable(q);

            string teamsT = "Teams";
            if (curSeason != maxSeason) teamsT += "S" + curSeason;

            DataTable res;
            var DisplayNames = new Dictionary<string, string>();
            try
            {
                q = "select Name, DisplayName from " + teamsT;
                res = _db.GetDataTable(q);

                foreach (DataRow r in res.Rows)
                {
                    DisplayNames.Add(r["Name"].ToString(), r["DisplayName"].ToString());
                }
            }
            catch
            {
                q = "select Name from " + teamsT;
                res = _db.GetDataTable(q);

                foreach (DataRow r in res.Rows)
                {
                    DisplayNames.Add(r["Name"].ToString(), r["Name"].ToString());
                }
            }

            IList<BoxScoreEntry> _bshist = new List<BoxScoreEntry>(res2.Rows.Count);
            Parallel.ForEach(res2.Rows.Cast<DataRow>(), r =>
                                                            {
                                                                var bs = new BoxScore(r);

                                                                var bse = new BoxScoreEntry(bs)
                                                                              {
                                                                                  date = bs.gamedate,
                                                                                  Team1Display = DisplayNames[bs.Team1],
                                                                                  Team2Display = DisplayNames[bs.Team2]
                                                                              };

                                                                string q2 =
                                                                    "select * from PlayerResults WHERE GameID = " +
                                                                    bs.id.ToString();
                                                                DataTable res3 = _db.GetDataTable(q2);
                                                                bse.pbsList = new List<PlayerBoxScore>(res3.Rows.Count);

                                                                Parallel.ForEach(res3.Rows.Cast<DataRow>(),
                                                                                 r3 =>
                                                                                 bse.pbsList.Add(new PlayerBoxScore(r3)));

                                                                _bshist.Add(bse);
                                                            });
            return _bshist;
        }

        public static Dictionary<int, PlayerStats> GetPlayersFromDatabase(string file, Dictionary<int, TeamStats> _tst,
                                                                          Dictionary<int, TeamStats> _tstopp,
                                                                          SortedDictionary<string, int> _TeamOrder,
                                                                          int curSeason,
                                                                          int maxSeason)
        {
            string q;

            if (curSeason == maxSeason)
            {
                q = "select * from Players;";
            }
            else
            {
                q = "select * from PlayersS" + curSeason.ToString() + ";";
            }

            var _db = new SQLiteDatabase(file);
            DataTable res = _db.GetDataTable(q);

            Dictionary<int, PlayerStats> _pst =
                (from DataRow r in res.Rows.AsParallel() select new PlayerStats(r)).ToDictionary(ps => ps.ID);

            PlayerStats.CalculateAllMetrics(ref _pst, _tst, _tstopp, _TeamOrder);

            return _pst;
        }

        public static bool isTSTEmpty()
        {
            if (String.IsNullOrWhiteSpace(MainWindow.currentDB)) return true;

            MainWindow.db = new SQLiteDatabase(MainWindow.currentDB);

            string teamsT = "Teams";
            if (MainWindow.curSeason != getMaxSeason(MainWindow.currentDB)) teamsT += "S" + MainWindow.curSeason;
            string q = "select Name from " + teamsT;
            DataTable res = MainWindow.db.GetDataTable(q);

            try
            {
                if (res.Rows[0]["Name"].ToString() == "$$NewDB") return true;

                return false;
            }
            catch
            {
                return true;
            }
        }

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
                if (!used.Contains(i)) return i;

                i++;
            }
        }

        /// <summary>
        /// Saves teams to the current database using current MainWindow.tst and MainWindow.tstopp
        /// </summary>
        public static void SaveTeamsToDatabase()
        {
            SaveTeamsToDatabase(MainWindow.currentDB, MainWindow.tst, MainWindow.tstopp, MainWindow.curSeason,
                                getMaxSeason(MainWindow.currentDB));
        }
    }
}