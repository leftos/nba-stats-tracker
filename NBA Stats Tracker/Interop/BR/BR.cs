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
using System.Data;
using System.Linq;
using HtmlAgilityPack;
using LeftosCommonLibrary;
using NBA_Stats_Tracker.Data.BoxScores;
using NBA_Stats_Tracker.Data.Players;
using NBA_Stats_Tracker.Data.Teams;
using NBA_Stats_Tracker.Windows;

#endregion

namespace NBA_Stats_Tracker.Interop.BR
{
    /// <summary>
    ///     Used to download and import real NBA stats from the Basketball-Reference.com website.
    /// </summary>
    public static class BR
    {
        /// <summary>
        ///     Downloads a box score from the specified URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="parts">The resulting date parts.</param>
        /// <returns></returns>
        private static DataSet GetBoxScore(string url, out string[] parts)
        {
            parts = new string[1];
            using (var dataset = new DataSet())
            {
                var htmlweb = new HtmlWeb();
                HtmlDocument doc = htmlweb.Load(url);

                HtmlNodeCollection divs = doc.DocumentNode.SelectNodes("//div");
                foreach (HtmlNode cur in divs)
                {
                    try
                    {
                        if (cur.Attributes["id"].Value != ("page_content"))
                            continue;
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                    HtmlNode h1 = doc.DocumentNode.SelectSingleNode("id('page_content')/table/tr/td/h1");
                    string name = h1.InnerText;
                    parts = name.Split(new[] {" at ", " Box Score, ", ", "}, 4, StringSplitOptions.None);
                    for (int i = 0; i < parts.Count(); i++)
                    {
                        parts[i] = parts[i].Replace("\n", "");
                    }
                }

                HtmlNodeCollection tables = doc.DocumentNode.SelectNodes("//table");
                foreach (HtmlNode cur in tables)
                {
                    try
                    {
                        if (!cur.Attributes["id"].Value.EndsWith("_basic"))
                            continue;
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                    using (var table = new DataTable(cur.Attributes["id"].Value))
                    {
                        HtmlNode thead = cur.SelectSingleNode("thead");
                        HtmlNodeCollection theadrows = thead.SelectNodes("tr");

                        IEnumerable<string> headers = theadrows[1].Elements("th").Select(th => th.InnerText.Trim());
                        foreach (string colheader in headers)
                        {
                            table.Columns.Add(colheader);
                        }

                        HtmlNode tbody = cur.SelectSingleNode("tbody");
                        HtmlNodeCollection tbodyrows = tbody.SelectNodes("tr");
                        IEnumerable<string[]> rows = tbodyrows.Select(tr => tr.Elements("td").Select(td => td.InnerText.Trim()).ToArray());
                        foreach (object[] row in rows)
                        {
                            table.Rows.Add(row);
                        }
                        HtmlNode tfoot = cur.SelectSingleNode("tfoot");
                        HtmlNode frow = tfoot.SelectSingleNode("tr");
                        IEnumerable<HtmlNode> elements = frow.Elements("td");
                        var erow = new string[elements.Count()];
                        for (int i = 0; i < elements.Count(); i++)
                        {
                            erow[i] = elements.ElementAt(i).InnerText;
                        }
                        table.Rows.Add(erow);

                        dataset.Tables.Add(table);
                    }
                }
                return dataset;
            }
        }

        /// <summary>
        ///     Downloads the season team stats for a team.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="recordparts">The parts of the team's record string.</param>
        /// <returns></returns>
        private static DataSet GetSeasonTeamStats(string url, out string[] recordparts)
        {
            using (var dataset = new DataSet())
            {
                var htmlweb = new HtmlWeb();
                HtmlDocument doc = htmlweb.Load(url);

                HtmlNode infobox = doc.DocumentNode.SelectSingleNode("//*[@id='info_box']");
                HtmlNodeCollection infoboxps = infobox.SelectNodes("p");
                HtmlNode infoboxp = infoboxps[1].NextSibling.NextSibling;
                string record = infoboxp.InnerText;

                recordparts = record.Split('-');
                recordparts[0] = recordparts[0].TrimStart(new[] {' '});
                recordparts[1] = recordparts[1].Split(',')[0];

                HtmlNodeCollection tables = doc.DocumentNode.SelectNodes("//table");
                foreach (HtmlNode cur in tables)
                {
                    try
                    {
                        if (cur.Attributes["id"].Value != "team_stats")
                            continue;
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                    using (var table = new DataTable(cur.Attributes["id"].Value))
                    {
                        HtmlNode thead = cur.SelectSingleNode("thead");
                        HtmlNodeCollection theadrows = thead.SelectNodes("tr");

                        IEnumerable<string> headers = theadrows[0].Elements("th").Select(th => th.InnerText.Trim());
                        foreach (string colheader in headers)
                        {
                            table.Columns.Add(colheader);
                        }

                        HtmlNode tbody = cur.SelectSingleNode("tbody");
                        HtmlNodeCollection tbodyrows = tbody.SelectNodes("tr");
                        IEnumerable<string[]> rows = tbodyrows.Select(tr => tr.Elements("td").Select(td => td.InnerText.Trim()).ToArray());
                        foreach (object[] row in rows)
                        {
                            table.Rows.Add(row);
                        }

                        dataset.Tables.Add(table);
                    }
                }
                return dataset;
            }
        }

        /// <summary>
        ///     Gets the player stats for a specific player.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns></returns>
        private static DataSet GetPlayerStats(string url)
        {
            using (var dataset = new DataSet())
            {
                var htmlweb = new HtmlWeb();
                HtmlDocument doc = htmlweb.Load(url);

                HtmlNodeCollection tables = doc.DocumentNode.SelectNodes("//table");
                foreach (HtmlNode cur in tables)
                {
                    try
                    {
                        if (
                            !(cur.Attributes["id"].Value == "totals" || cur.Attributes["id"].Value == "playoffs" ||
                              cur.Attributes["id"].Value == "roster"))
                            continue;
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                    using (var table = new DataTable(cur.Attributes["id"].Value))
                    {
                        HtmlNode thead = cur.SelectSingleNode("thead");
                        HtmlNodeCollection theadrows = thead.SelectNodes("tr");

                        HtmlNode theadrow = cur.Attributes["id"].Value == "playoffs" ? theadrows[1] : theadrows[0];

                        IEnumerable<string> headers = theadrow.Elements("th").Select(th => th.InnerText.Trim());
                        foreach (string colheader in headers)
                        {
                            try
                            {
                                table.Columns.Add(colheader);
                            }
                            catch (Exception)
                            {
                                table.Columns.Add(colheader + "2");
                            }
                        }

                        HtmlNode tbody = cur.SelectSingleNode("tbody");
                        HtmlNodeCollection tbodyrows = tbody.SelectNodes("tr");
                        IEnumerable<string[]> rows = tbodyrows.Select(tr => tr.Elements("td").Select(td => td.InnerText.Trim()).ToArray());
                        foreach (object[] row in rows)
                        {
                            table.Rows.Add(row);
                        }

                        dataset.Tables.Add(table);
                    }
                }
                return dataset;
            }
        }

        /// <summary>
        ///     Gets the playoff team stats for a specific team.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns></returns>
        private static DataSet GetPlayoffTeamStats(string url)
        {
            using (var dataset = new DataSet())
            {
                var htmlweb = new HtmlWeb();
                HtmlDocument doc = htmlweb.Load(url);

                HtmlNodeCollection tables = doc.DocumentNode.SelectNodes("//table");
                foreach (HtmlNode cur in tables)
                {
                    try
                    {
                        if (
                            !(cur.Attributes["id"].Value == "team" || cur.Attributes["id"].Value == "opponent" ||
                              cur.Attributes["id"].Value == "misc"))
                            continue;
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                    using (var table = new DataTable(cur.Attributes["id"].Value))
                    {
                        HtmlNode thead = cur.SelectSingleNode("thead");
                        HtmlNodeCollection theadrows = thead.SelectNodes("tr");

                        HtmlNode theadrow = cur.Attributes["id"].Value == "misc" ? theadrows[1] : theadrows[0];

                        IEnumerable<string> headers = theadrow.Elements("th").Select(th => th.InnerText.Trim());
                        foreach (string colheader in headers)
                        {
                            try
                            {
                                table.Columns.Add(colheader);
                            }
                            catch (Exception)
                            {
                                table.Columns.Add(colheader + "2");
                            }
                        }

                        HtmlNode tbody = cur.SelectSingleNode("tbody");
                        HtmlNodeCollection tbodyrows = tbody.SelectNodes("tr");
                        IEnumerable<string[]> rows = tbodyrows.Select(tr => tr.Elements("td").Select(td => td.InnerText.Trim()).ToArray());
                        foreach (object[] row in rows)
                        {
                            table.Rows.Add(row);
                        }

                        dataset.Tables.Add(table);
                    }
                }
                return dataset;
            }
        }

        /// <summary>
        ///     Creates the team and opposing team stats instances using data from the downloaded DataTable.
        /// </summary>
        /// <param name="dt">The DataTable.</param>
        /// <param name="name">The name of the team.</param>
        /// <param name="recordparts">The parts of the team's record string.</param>
        /// <param name="ts">The resulting team stats instance.</param>
        /// <param name="tsopp">The resulting opposing team stats instance.</param>
        private static void TeamStatsFromDataTable(DataTable dt, string name, string[] recordparts, out TeamStats ts, out TeamStats tsopp)
        {
            ts = new TeamStats(name);
            tsopp = new TeamStats(name);

            tsopp.winloss[1] = ts.winloss[0] = Convert.ToByte(recordparts[0]);
            tsopp.winloss[0] = ts.winloss[1] = Convert.ToByte(recordparts[1]);

            DataRow tr = dt.Rows[0];
            DataRow toppr = dt.Rows[2];

            ts.stats[t.MINS] = (ushort) (Tools.getUInt16(tr, "MP")/5);
            ts.stats[t.FGM] = Tools.getUInt16(tr, "FG");
            ts.stats[t.FGA] = Tools.getUInt16(tr, "FGA");
            ts.stats[t.TPM] = Tools.getUInt16(tr, "3P");
            ts.stats[t.TPA] = Tools.getUInt16(tr, "3PA");
            ts.stats[t.FTM] = Tools.getUInt16(tr, "FT");
            ts.stats[t.FTA] = Tools.getUInt16(tr, "FTA");
            ts.stats[t.OREB] = Tools.getUInt16(tr, "ORB");
            ts.stats[t.DREB] = Tools.getUInt16(tr, "DRB");
            ts.stats[t.AST] = Tools.getUInt16(tr, "AST");
            ts.stats[t.STL] = Tools.getUInt16(tr, "STL");
            ts.stats[t.BLK] = Tools.getUInt16(tr, "BLK");
            ts.stats[t.TO] = Tools.getUInt16(tr, "TOV");
            ts.stats[t.FOUL] = Tools.getUInt16(tr, "PF");
            ts.stats[t.PF] = Tools.getUInt16(tr, "PTS");
            ts.stats[t.PA] = Tools.getUInt16(toppr, "PTS");

            ts.CalcAvg();

            tsopp.stats[t.MINS] = (ushort) (Tools.getUInt16(toppr, "MP")/5);
            tsopp.stats[t.FGM] = Tools.getUInt16(toppr, "FG");
            tsopp.stats[t.FGA] = Tools.getUInt16(toppr, "FGA");
            tsopp.stats[t.TPM] = Tools.getUInt16(toppr, "3P");
            tsopp.stats[t.TPA] = Tools.getUInt16(toppr, "3PA");
            tsopp.stats[t.FTM] = Tools.getUInt16(toppr, "FT");
            tsopp.stats[t.FTA] = Tools.getUInt16(toppr, "FTA");
            tsopp.stats[t.OREB] = Tools.getUInt16(toppr, "ORB");
            tsopp.stats[t.DREB] = Tools.getUInt16(toppr, "DRB");
            tsopp.stats[t.AST] = Tools.getUInt16(toppr, "AST");
            tsopp.stats[t.STL] = Tools.getUInt16(toppr, "STL");
            tsopp.stats[t.BLK] = Tools.getUInt16(toppr, "BLK");
            tsopp.stats[t.TO] = Tools.getUInt16(toppr, "TOV");
            tsopp.stats[t.FOUL] = Tools.getUInt16(toppr, "PF");
            tsopp.stats[t.PF] = Tools.getUInt16(toppr, "PTS");
            tsopp.stats[t.PA] = Tools.getUInt16(tr, "PTS");

            tsopp.CalcAvg();
        }

        /// <summary>
        ///     Creates the playoff team and opposing playoff team stats instances using data from the downloaded DataSet.
        /// </summary>
        /// <param name="ds">The dataset.</param>
        /// <param name="tst">The team stats dictionary.</param>
        /// <param name="tstopp">The opposing team stats dictionary.</param>
        private static void PlayoffTeamStatsFromDataSet(DataSet ds, ref Dictionary<int, TeamStats> tst,
                                                        ref Dictionary<int, TeamStats> tstopp)
        {
            DataTable dt = ds.Tables["team"];
            DataTable dtopp = ds.Tables["opponent"];
            DataTable dtmisc = ds.Tables["misc"];

            for (int i = 0; i < tst.Count; i++)
            {
                DataRow tr = dt.Rows[0];
                DataRow toppr = dtopp.Rows[0];
                DataRow tmiscr = dtmisc.Rows[0];

                bool found = false;

                for (int j = 0; j < dt.Rows.Count; j++)
                {
                    if (dt.Rows[j]["Team"].ToString().EndsWith(tst[i].name))
                    {
                        tr = dt.Rows[j];
                        found = true;
                        break;
                    }
                }

                if (!found)
                    continue;

                for (int j = 0; j < dtopp.Rows.Count; j++)
                {
                    if (dtopp.Rows[j]["Team"].ToString().EndsWith(tst[i].name))
                    {
                        toppr = dtopp.Rows[j];
                        break;
                    }
                }

                for (int j = 0; j < dtmisc.Rows.Count; j++)
                {
                    if (dtmisc.Rows[j]["Team"].ToString().EndsWith(tst[i].name))
                    {
                        tmiscr = dtmisc.Rows[j];
                        break;
                    }
                }

                tst[i].pl_winloss[0] = (byte) Tools.getUInt16(tmiscr, "W");
                tst[i].pl_winloss[1] = (byte) Tools.getUInt16(tmiscr, "L");
                tst[i].pl_stats[t.MINS] = (ushort) (Tools.getUInt16(tr, "MP")/5);
                tst[i].pl_stats[t.FGM] = Tools.getUInt16(tr, "FG");
                tst[i].pl_stats[t.FGA] = Tools.getUInt16(tr, "FGA");
                tst[i].pl_stats[t.TPM] = Tools.getUInt16(tr, "3P");
                tst[i].pl_stats[t.TPA] = Tools.getUInt16(tr, "3PA");
                tst[i].pl_stats[t.FTM] = Tools.getUInt16(tr, "FT");
                tst[i].pl_stats[t.FTA] = Tools.getUInt16(tr, "FTA");
                tst[i].pl_stats[t.OREB] = Tools.getUInt16(tr, "ORB");
                tst[i].pl_stats[t.DREB] = Tools.getUInt16(tr, "DRB");
                tst[i].pl_stats[t.AST] = Tools.getUInt16(tr, "AST");
                tst[i].pl_stats[t.STL] = Tools.getUInt16(tr, "STL");
                tst[i].pl_stats[t.BLK] = Tools.getUInt16(tr, "BLK");
                tst[i].pl_stats[t.TO] = Tools.getUInt16(tr, "TOV");
                tst[i].pl_stats[t.FOUL] = Tools.getUInt16(tr, "PF");
                tst[i].pl_stats[t.PF] = Tools.getUInt16(tr, "PTS");
                tst[i].pl_stats[t.PA] = Tools.getUInt16(toppr, "PTS");

                tstopp[i].pl_winloss[0] = (byte) Tools.getUInt16(tmiscr, "L");
                tstopp[i].pl_winloss[1] = (byte) Tools.getUInt16(tmiscr, "W");
                tstopp[i].pl_stats[t.MINS] = (ushort) (Tools.getUInt16(toppr, "MP")/5);
                tstopp[i].pl_stats[t.FGM] = Tools.getUInt16(toppr, "FG");
                tstopp[i].pl_stats[t.FGA] = Tools.getUInt16(toppr, "FGA");
                tstopp[i].pl_stats[t.TPM] = Tools.getUInt16(toppr, "3P");
                tstopp[i].pl_stats[t.TPA] = Tools.getUInt16(toppr, "3PA");
                tstopp[i].pl_stats[t.FTM] = Tools.getUInt16(toppr, "FT");
                tstopp[i].pl_stats[t.FTA] = Tools.getUInt16(toppr, "FTA");
                tstopp[i].pl_stats[t.OREB] = Tools.getUInt16(toppr, "ORB");
                tstopp[i].pl_stats[t.DREB] = Tools.getUInt16(toppr, "DRB");
                tstopp[i].pl_stats[t.AST] = Tools.getUInt16(toppr, "AST");
                tstopp[i].pl_stats[t.STL] = Tools.getUInt16(toppr, "STL");
                tstopp[i].pl_stats[t.BLK] = Tools.getUInt16(toppr, "BLK");
                tstopp[i].pl_stats[t.TO] = Tools.getUInt16(toppr, "TOV");
                tstopp[i].pl_stats[t.FOUL] = Tools.getUInt16(toppr, "PF");
                tstopp[i].pl_stats[t.PF] = Tools.getUInt16(toppr, "PTS");
                tstopp[i].pl_stats[t.PA] = Tools.getUInt16(tr, "PTS");
            }
        }

        /// <summary>
        ///     Creates the player stats instances using data from the downloaded DataSet.
        /// </summary>
        /// <param name="ds">The DataSet.</param>
        /// <param name="team">The player's team.</param>
        /// <param name="pst">The player stats dictionary.</param>
        /// <exception cref="System.Exception">Don't recognize the position </exception>
        private static void PlayerStatsFromDataSet(DataSet ds, string team, out Dictionary<int, PlayerStats> pst)
        {
            var pstnames = new Dictionary<string, PlayerStats>();

            DataTable dt = ds.Tables["roster"];

            foreach (DataRow r in dt.Rows)
            {
                Position Position1;
                Position Position2;
                switch (r["Pos"].ToString())
                {
                    case "C":
                        Position1 = Position.C;
                        Position2 = Position.None;
                        break;

                    case "G":
                        Position1 = Position.PG;
                        Position2 = Position.SG;
                        break;

                    case "F":
                        Position1 = Position.SF;
                        Position2 = Position.PF;
                        break;

                    case "G-F":
                        Position1 = Position.SG;
                        Position2 = Position.SF;
                        break;

                    case "F-G":
                        Position1 = Position.SF;
                        Position2 = Position.SG;
                        break;

                    case "F-C":
                        Position1 = Position.PF;
                        Position2 = Position.C;
                        break;

                    case "C-F":
                        Position1 = Position.C;
                        Position2 = Position.PF;
                        break;

                    default:
                        throw (new Exception("Don't recognize the position " + r["Pos"]));
                }
                var ps =
                    new PlayerStats(new Player(pstnames.Count, team, r["Player"].ToString().Split(' ')[1],
                                               r["Player"].ToString().Split(' ')[0], Position1, Position2));

                pstnames.Add(r["Player"].ToString(), ps);
            }

            dt = ds.Tables["totals"];

            foreach (DataRow r in dt.Rows)
            {
                string name = r["Player"].ToString();
                pstnames[name].stats[p.GP] = Tools.getUInt16(r, "G");
                pstnames[name].stats[p.GS] = Tools.getUInt16(r, "GS");
                pstnames[name].stats[p.MINS] = Tools.getUInt16(r, "MP");
                pstnames[name].stats[p.FGM] = Tools.getUInt16(r, "FG");
                pstnames[name].stats[p.FGA] = Tools.getUInt16(r, "FGA");
                pstnames[name].stats[p.TPM] = Tools.getUInt16(r, "3P");
                pstnames[name].stats[p.TPA] = Tools.getUInt16(r, "3PA");
                pstnames[name].stats[p.FTM] = Tools.getUInt16(r, "FT");
                pstnames[name].stats[p.FTA] = Tools.getUInt16(r, "FTA");
                pstnames[name].stats[p.OREB] = Tools.getUInt16(r, "ORB");
                pstnames[name].stats[p.DREB] = Tools.getUInt16(r, "DRB");
                pstnames[name].stats[p.AST] = Tools.getUInt16(r, "AST");
                pstnames[name].stats[p.STL] = Tools.getUInt16(r, "STL");
                pstnames[name].stats[p.BLK] = Tools.getUInt16(r, "BLK");
                pstnames[name].stats[p.TO] = Tools.getUInt16(r, "TOV");
                pstnames[name].stats[p.FOUL] = Tools.getUInt16(r, "PF");
                pstnames[name].stats[p.PTS] = Tools.getUInt16(r, "PTS");
            }

            dt = ds.Tables["playoffs"];

            try
            {
                foreach (DataRow r in dt.Rows)
                {
                    string name = r["Player"].ToString();
                    pstnames[name].pl_stats[p.GP] += Tools.getUInt16(r, "G");
                    //pstnames[name].pl_stats[p.GS] += NSTHelper.getUShort(r, "GS");
                    pstnames[name].pl_stats[p.MINS] += Tools.getUInt16(r, "MP");
                    pstnames[name].pl_stats[p.FGM] += Tools.getUInt16(r, "FG");
                    pstnames[name].pl_stats[p.FGA] += Tools.getUInt16(r, "FGA");
                    pstnames[name].pl_stats[p.TPM] += Tools.getUInt16(r, "3P");
                    pstnames[name].pl_stats[p.TPA] += Tools.getUInt16(r, "3PA");
                    pstnames[name].pl_stats[p.FTM] += Tools.getUInt16(r, "FT");
                    pstnames[name].pl_stats[p.FTA] += Tools.getUInt16(r, "FTA");
                    pstnames[name].pl_stats[p.OREB] += Tools.getUInt16(r, "ORB");
                    pstnames[name].pl_stats[p.DREB] += (ushort) (Tools.getUInt16(r, "TRB") - Tools.getUInt16(r, "ORB"));
                    pstnames[name].pl_stats[p.AST] += Tools.getUInt16(r, "AST");
                    pstnames[name].pl_stats[p.STL] += Tools.getUInt16(r, "STL");
                    pstnames[name].pl_stats[p.BLK] += Tools.getUInt16(r, "BLK");
                    pstnames[name].pl_stats[p.TO] += Tools.getUInt16(r, "TOV");
                    pstnames[name].pl_stats[p.FOUL] += Tools.getUInt16(r, "PF");
                    pstnames[name].pl_stats[p.PTS] += Tools.getUInt16(r, "PTS");

                    pstnames[name].CalcAvg();
                }
            }
            catch (Exception)
            {
            }

            pst = new Dictionary<int, PlayerStats>();
            foreach (var kvp in pstnames)
            {
                kvp.Value.ID = pst.Count;
                pst.Add(pst.Count, kvp.Value);
            }
        }

        /// <summary>
        ///     Creates a team box score and all the required player box score instances using data from the downloaded DataSet.
        /// </summary>
        /// <param name="ds">The DataSet.</param>
        /// <param name="parts">The parts of the split date string.</param>
        /// <param name="bse">The resulting BoxScoreEntry.</param>
        /// <returns>0 if every required player was found in the database; otherwise, -1.</returns>
        private static int BoxScoreFromDataSet(DataSet ds, string[] parts, out BoxScoreEntry bse)
        {
            DataTable awayDT = ds.Tables[0];
            DataTable homeDT = ds.Tables[1];

            var bs = new TeamBoxScore(ds, parts);
            bse = new BoxScoreEntry(bs);
            bse.date = bs.gamedate;
            bse.pbsList = new List<PlayerBoxScore>();
            int result = 0;
            for (int i = 0; i < awayDT.Rows.Count - 1; i++)
            {
                if (i == 5)
                    continue;
                var pbs = new PlayerBoxScore(awayDT.Rows[i], bs.Team1, bs.id, (i < 5), MainWindow.pst);
                if (pbs.PlayerID == -1)
                {
                    result = -1;
                    continue;
                }
                bse.pbsList.Add(pbs);
            }
            for (int i = 0; i < homeDT.Rows.Count - 1; i++)
            {
                if (i == 5)
                    continue;
                var pbs = new PlayerBoxScore(homeDT.Rows[i], bs.Team2, bs.id, (i < 5), MainWindow.pst);
                if (pbs.PlayerID == -1)
                {
                    result = -1;
                    continue;
                }
                bse.pbsList.Add(pbs);
            }
            return result;
        }

        /// <summary>
        ///     Downloads and imports the real NBA stats of a specific team and its players.
        /// </summary>
        /// <param name="teamAbbr">The team name-abbreviation KeyValuePair.</param>
        /// <param name="ts">The resulting team stats instance.</param>
        /// <param name="tsopp">The opposing team stats instance.</param>
        /// <param name="pst">The resulting player stats dictionary.</param>
        public static void ImportRealStats(KeyValuePair<string, string> teamAbbr, out TeamStats ts, out TeamStats tsopp,
                                           out Dictionary<int, PlayerStats> pst)
        {
            string[] recordparts;
            DataSet ds = GetSeasonTeamStats(@"http://www.basketball-reference.com/teams/" + teamAbbr.Value + @"/2013.html", out recordparts);
            TeamStatsFromDataTable(ds.Tables[0], teamAbbr.Key, recordparts, out ts, out tsopp);

            ds = GetPlayerStats(@"http://www.basketball-reference.com/teams/" + teamAbbr.Value + @"/2013.html");
            PlayerStatsFromDataSet(ds, teamAbbr.Key, out pst);
        }

        /// <summary>
        ///     Downloads and imports a box score.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns></returns>
        public static int ImportBoxScore(string url)
        {
            string[] parts;
            DataSet ds = GetBoxScore(url, out parts);
            BoxScoreEntry bse;
            int result = BoxScoreFromDataSet(ds, parts, out bse);

            MainWindow.bshist.Add(bse);

            return result;
        }

        /// <summary>
        ///     Adds the playoff team stats to the current database.
        /// </summary>
        /// <param name="tst">The team stats dictionary.</param>
        /// <param name="tstopp">The opposing team stats dictionary.</param>
        public static void AddPlayoffTeamStats(ref Dictionary<int, TeamStats> tst, ref Dictionary<int, TeamStats> tstopp)
        {
            DataSet ds = GetPlayoffTeamStats("http://www.basketball-reference.com/playoffs/NBA_2013.html");
            PlayoffTeamStatsFromDataSet(ds, ref tst, ref tstopp);
        }
    }
}