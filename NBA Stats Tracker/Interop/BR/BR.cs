#region Copyright Notice

//    Copyright 2011-2013 Eleftherios Aslanoglou
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

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
using NBA_Stats_Tracker.Windows.MainInterface;

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
        private static DataSet getBoxScore(string url, out string[] parts)
        {
            parts = new string[1];
            using (var dataset = new DataSet())
            {
                var htmlweb = new HtmlWeb();
                HtmlDocument doc = htmlweb.Load(url);

                HtmlNodeCollection divs = doc.DocumentNode.SelectNodes("//div");
                foreach (var cur in divs)
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
                foreach (var cur in tables)
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
                        foreach (var colheader in headers)
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
                        IList<HtmlNode> htmlNodes = elements as IList<HtmlNode> ?? elements.ToList();
                        var erow = new object[htmlNodes.Count()];
                        for (int i = 0; i < htmlNodes.Count(); i++)
                        {
                            erow[i] = htmlNodes.ElementAt(i).InnerText;
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
        private static DataSet getSeasonTeamStats(string url, out string[] recordparts)
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
                foreach (var cur in tables)
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
                        foreach (var colheader in headers)
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
        private static DataSet getPlayerStats(string url)
        {
            using (var dataset = new DataSet())
            {
                var htmlweb = new HtmlWeb();
                HtmlDocument doc = htmlweb.Load(url);

                HtmlNodeCollection tables = doc.DocumentNode.SelectNodes("//table");
                foreach (var cur in tables)
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
                        foreach (var colheader in headers)
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
        private static DataSet getPlayoffTeamStats(string url)
        {
            using (var dataset = new DataSet())
            {
                var htmlweb = new HtmlWeb();
                HtmlDocument doc = htmlweb.Load(url);

                HtmlNodeCollection tables = doc.DocumentNode.SelectNodes("//table");
                foreach (var cur in tables)
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
                        foreach (var colheader in headers)
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
        private static void teamStatsFromDataTable(DataTable dt, string name, string[] recordparts, out TeamStats ts, out TeamStats tsopp)
        {
            ts = new TeamStats(MainWindow.TeamOrder[name], name);
            tsopp = new TeamStats(MainWindow.TeamOrder[name], name);

            tsopp.Record[1] = ts.Record[0] = Convert.ToByte(recordparts[0]);
            tsopp.Record[0] = ts.Record[1] = Convert.ToByte(recordparts[1]);

            DataRow tr = dt.Rows[0];
            DataRow toppr = dt.Rows[2];

            ts.Totals[TAbbr.MINS] = (ushort) (ParseCell.GetUInt16(tr, "MP")/5);
            ts.Totals[TAbbr.FGM] = ParseCell.GetUInt16(tr, "FG");
            ts.Totals[TAbbr.FGA] = ParseCell.GetUInt16(tr, "FGA");
            ts.Totals[TAbbr.TPM] = ParseCell.GetUInt16(tr, "3P");
            ts.Totals[TAbbr.TPA] = ParseCell.GetUInt16(tr, "3PA");
            ts.Totals[TAbbr.FTM] = ParseCell.GetUInt16(tr, "FT");
            ts.Totals[TAbbr.FTA] = ParseCell.GetUInt16(tr, "FTA");
            ts.Totals[TAbbr.OREB] = ParseCell.GetUInt16(tr, "ORB");
            ts.Totals[TAbbr.DREB] = ParseCell.GetUInt16(tr, "DRB");
            ts.Totals[TAbbr.AST] = ParseCell.GetUInt16(tr, "AST");
            ts.Totals[TAbbr.STL] = ParseCell.GetUInt16(tr, "STL");
            ts.Totals[TAbbr.BLK] = ParseCell.GetUInt16(tr, "BLK");
            ts.Totals[TAbbr.TOS] = ParseCell.GetUInt16(tr, "TOV");
            ts.Totals[TAbbr.FOUL] = ParseCell.GetUInt16(tr, "PF");
            ts.Totals[TAbbr.PF] = ParseCell.GetUInt16(tr, "PTS");
            ts.Totals[TAbbr.PA] = ParseCell.GetUInt16(toppr, "PTS");

            ts.CalcAvg();

            tsopp.Totals[TAbbr.MINS] = (ushort) (ParseCell.GetUInt16(toppr, "MP")/5);
            tsopp.Totals[TAbbr.FGM] = ParseCell.GetUInt16(toppr, "FG");
            tsopp.Totals[TAbbr.FGA] = ParseCell.GetUInt16(toppr, "FGA");
            tsopp.Totals[TAbbr.TPM] = ParseCell.GetUInt16(toppr, "3P");
            tsopp.Totals[TAbbr.TPA] = ParseCell.GetUInt16(toppr, "3PA");
            tsopp.Totals[TAbbr.FTM] = ParseCell.GetUInt16(toppr, "FT");
            tsopp.Totals[TAbbr.FTA] = ParseCell.GetUInt16(toppr, "FTA");
            tsopp.Totals[TAbbr.OREB] = ParseCell.GetUInt16(toppr, "ORB");
            tsopp.Totals[TAbbr.DREB] = ParseCell.GetUInt16(toppr, "DRB");
            tsopp.Totals[TAbbr.AST] = ParseCell.GetUInt16(toppr, "AST");
            tsopp.Totals[TAbbr.STL] = ParseCell.GetUInt16(toppr, "STL");
            tsopp.Totals[TAbbr.BLK] = ParseCell.GetUInt16(toppr, "BLK");
            tsopp.Totals[TAbbr.TOS] = ParseCell.GetUInt16(toppr, "TOV");
            tsopp.Totals[TAbbr.FOUL] = ParseCell.GetUInt16(toppr, "PF");
            tsopp.Totals[TAbbr.PF] = ParseCell.GetUInt16(toppr, "PTS");
            tsopp.Totals[TAbbr.PA] = ParseCell.GetUInt16(tr, "PTS");

            tsopp.CalcAvg();
        }

        /// <summary>
        ///     Creates the playoff team and opposing playoff team stats instances using data from the downloaded DataSet.
        /// </summary>
        /// <param name="ds">The dataset.</param>
        /// <param name="tst">The team stats dictionary.</param>
        /// <param name="tstOpp">The opposing team stats dictionary.</param>
        private static void playoffTeamStatsFromDataSet(DataSet ds, ref Dictionary<int, TeamStats> tst,
                                                        ref Dictionary<int, TeamStats> tstOpp)
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
                    if (dt.Rows[j]["Team"].ToString().EndsWith(tst[i].Name))
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
                    if (dtopp.Rows[j]["Team"].ToString().EndsWith(tst[i].Name))
                    {
                        toppr = dtopp.Rows[j];
                        break;
                    }
                }

                for (int j = 0; j < dtmisc.Rows.Count; j++)
                {
                    if (dtmisc.Rows[j]["Team"].ToString().EndsWith(tst[i].Name))
                    {
                        tmiscr = dtmisc.Rows[j];
                        break;
                    }
                }

                tst[i].PlRecord[0] = (byte) ParseCell.GetUInt16(tmiscr, "W");
                tst[i].PlRecord[1] = (byte) ParseCell.GetUInt16(tmiscr, "L");
                tst[i].PlTotals[TAbbr.MINS] = (ushort) (ParseCell.GetUInt16(tr, "MP")/5);
                tst[i].PlTotals[TAbbr.FGM] = ParseCell.GetUInt16(tr, "FG");
                tst[i].PlTotals[TAbbr.FGA] = ParseCell.GetUInt16(tr, "FGA");
                tst[i].PlTotals[TAbbr.TPM] = ParseCell.GetUInt16(tr, "3P");
                tst[i].PlTotals[TAbbr.TPA] = ParseCell.GetUInt16(tr, "3PA");
                tst[i].PlTotals[TAbbr.FTM] = ParseCell.GetUInt16(tr, "FT");
                tst[i].PlTotals[TAbbr.FTA] = ParseCell.GetUInt16(tr, "FTA");
                tst[i].PlTotals[TAbbr.OREB] = ParseCell.GetUInt16(tr, "ORB");
                tst[i].PlTotals[TAbbr.DREB] = ParseCell.GetUInt16(tr, "DRB");
                tst[i].PlTotals[TAbbr.AST] = ParseCell.GetUInt16(tr, "AST");
                tst[i].PlTotals[TAbbr.STL] = ParseCell.GetUInt16(tr, "STL");
                tst[i].PlTotals[TAbbr.BLK] = ParseCell.GetUInt16(tr, "BLK");
                tst[i].PlTotals[TAbbr.TOS] = ParseCell.GetUInt16(tr, "TOV");
                tst[i].PlTotals[TAbbr.FOUL] = ParseCell.GetUInt16(tr, "PF");
                tst[i].PlTotals[TAbbr.PF] = ParseCell.GetUInt16(tr, "PTS");
                tst[i].PlTotals[TAbbr.PA] = ParseCell.GetUInt16(toppr, "PTS");

                tstOpp[i].PlRecord[0] = (byte) ParseCell.GetUInt16(tmiscr, "L");
                tstOpp[i].PlRecord[1] = (byte) ParseCell.GetUInt16(tmiscr, "W");
                tstOpp[i].PlTotals[TAbbr.MINS] = (ushort) (ParseCell.GetUInt16(toppr, "MP")/5);
                tstOpp[i].PlTotals[TAbbr.FGM] = ParseCell.GetUInt16(toppr, "FG");
                tstOpp[i].PlTotals[TAbbr.FGA] = ParseCell.GetUInt16(toppr, "FGA");
                tstOpp[i].PlTotals[TAbbr.TPM] = ParseCell.GetUInt16(toppr, "3P");
                tstOpp[i].PlTotals[TAbbr.TPA] = ParseCell.GetUInt16(toppr, "3PA");
                tstOpp[i].PlTotals[TAbbr.FTM] = ParseCell.GetUInt16(toppr, "FT");
                tstOpp[i].PlTotals[TAbbr.FTA] = ParseCell.GetUInt16(toppr, "FTA");
                tstOpp[i].PlTotals[TAbbr.OREB] = ParseCell.GetUInt16(toppr, "ORB");
                tstOpp[i].PlTotals[TAbbr.DREB] = ParseCell.GetUInt16(toppr, "DRB");
                tstOpp[i].PlTotals[TAbbr.AST] = ParseCell.GetUInt16(toppr, "AST");
                tstOpp[i].PlTotals[TAbbr.STL] = ParseCell.GetUInt16(toppr, "STL");
                tstOpp[i].PlTotals[TAbbr.BLK] = ParseCell.GetUInt16(toppr, "BLK");
                tstOpp[i].PlTotals[TAbbr.TOS] = ParseCell.GetUInt16(toppr, "TOV");
                tstOpp[i].PlTotals[TAbbr.FOUL] = ParseCell.GetUInt16(toppr, "PF");
                tstOpp[i].PlTotals[TAbbr.PF] = ParseCell.GetUInt16(toppr, "PTS");
                tstOpp[i].PlTotals[TAbbr.PA] = ParseCell.GetUInt16(tr, "PTS");
            }
        }

        /// <summary>
        ///     Creates the player stats instances using data from the downloaded DataSet.
        /// </summary>
        /// <param name="ds">The DataSet.</param>
        /// <param name="teamID">The player's team.</param>
        /// <param name="pst">The player stats dictionary.</param>
        /// <exception cref="System.Exception">Don't recognize the position </exception>
        private static void playerStatsFromDataSet(DataSet ds, int teamID, out Dictionary<int, PlayerStats> pst)
        {
            var pstnames = new Dictionary<string, PlayerStats>();

            DataTable dt = ds.Tables["roster"];

            foreach (DataRow r in dt.Rows)
            {
                Position position1;
                Position position2;
                switch (r["Pos"].ToString())
                {
                    case "C":
                        position1 = Position.C;
                        position2 = Position.None;
                        break;

                    case "G":
                        position1 = Position.PG;
                        position2 = Position.SG;
                        break;

                    case "F":
                        position1 = Position.SF;
                        position2 = Position.PF;
                        break;

                    case "G-F":
                        position1 = Position.SG;
                        position2 = Position.SF;
                        break;

                    case "F-G":
                        position1 = Position.SF;
                        position2 = Position.SG;
                        break;

                    case "F-C":
                        position1 = Position.PF;
                        position2 = Position.C;
                        break;

                    case "C-F":
                        position1 = Position.C;
                        position2 = Position.PF;
                        break;

                    case "PG":
                        position1 = Position.PG;
                        position2 = Position.None;
                        break;

                    case "SG":
                        position1 = Position.SG;
                        position2 = Position.None;
                        break;

                    case "SF":
                        position1 = Position.SF;
                        position2 = Position.None;
                        break;

                    case "PF":
                        position1 = Position.PF;
                        position2 = Position.None;
                        break;

                    default:
                        throw (new Exception("Don't recognize the position " + r["Pos"]));
                }
                var ps =
                    new PlayerStats(new Player(pstnames.Count, teamID, r["Player"].ToString().Split(' ')[1],
                                               r["Player"].ToString().Split(' ')[0], position1, position2));

                pstnames.Add(r["Player"].ToString(), ps);
            }

            dt = ds.Tables["totals"];

            foreach (DataRow r in dt.Rows)
            {
                string name = r["Player"].ToString();
                pstnames[name].Totals[PAbbr.GP] = ParseCell.GetUInt16(r, "G");
                pstnames[name].Totals[PAbbr.GS] = ParseCell.GetUInt16(r, "GS");
                pstnames[name].Totals[PAbbr.MINS] = ParseCell.GetUInt16(r, "MP");
                pstnames[name].Totals[PAbbr.FGM] = ParseCell.GetUInt16(r, "FG");
                pstnames[name].Totals[PAbbr.FGA] = ParseCell.GetUInt16(r, "FGA");
                pstnames[name].Totals[PAbbr.TPM] = ParseCell.GetUInt16(r, "3P");
                pstnames[name].Totals[PAbbr.TPA] = ParseCell.GetUInt16(r, "3PA");
                pstnames[name].Totals[PAbbr.FTM] = ParseCell.GetUInt16(r, "FT");
                pstnames[name].Totals[PAbbr.FTA] = ParseCell.GetUInt16(r, "FTA");
                pstnames[name].Totals[PAbbr.OREB] = ParseCell.GetUInt16(r, "ORB");
                pstnames[name].Totals[PAbbr.DREB] = ParseCell.GetUInt16(r, "DRB");
                pstnames[name].Totals[PAbbr.AST] = ParseCell.GetUInt16(r, "AST");
                pstnames[name].Totals[PAbbr.STL] = ParseCell.GetUInt16(r, "STL");
                pstnames[name].Totals[PAbbr.BLK] = ParseCell.GetUInt16(r, "BLK");
                pstnames[name].Totals[PAbbr.TOS] = ParseCell.GetUInt16(r, "TOV");
                pstnames[name].Totals[PAbbr.FOUL] = ParseCell.GetUInt16(r, "PF");
                pstnames[name].Totals[PAbbr.PTS] = ParseCell.GetUInt16(r, "PTS");
            }

            dt = ds.Tables["playoffs"];

            try
            {
                foreach (DataRow r in dt.Rows)
                {
                    string name = r["Player"].ToString();
                    pstnames[name].PlTotals[PAbbr.GP] += ParseCell.GetUInt16(r, "G");
                    //pstnames[name].pl_stats[p.GS] += NSTHelper.getUShort(r, "GS");
                    pstnames[name].PlTotals[PAbbr.MINS] += ParseCell.GetUInt16(r, "MP");
                    pstnames[name].PlTotals[PAbbr.FGM] += ParseCell.GetUInt16(r, "FG");
                    pstnames[name].PlTotals[PAbbr.FGA] += ParseCell.GetUInt16(r, "FGA");
                    pstnames[name].PlTotals[PAbbr.TPM] += ParseCell.GetUInt16(r, "3P");
                    pstnames[name].PlTotals[PAbbr.TPA] += ParseCell.GetUInt16(r, "3PA");
                    pstnames[name].PlTotals[PAbbr.FTM] += ParseCell.GetUInt16(r, "FT");
                    pstnames[name].PlTotals[PAbbr.FTA] += ParseCell.GetUInt16(r, "FTA");
                    pstnames[name].PlTotals[PAbbr.OREB] += ParseCell.GetUInt16(r, "ORB");
                    pstnames[name].PlTotals[PAbbr.DREB] +=
                        (ushort) (ParseCell.GetUInt16(r, "TRB") - ParseCell.GetUInt16(r, "ORB"));
                    pstnames[name].PlTotals[PAbbr.AST] += ParseCell.GetUInt16(r, "AST");
                    pstnames[name].PlTotals[PAbbr.STL] += ParseCell.GetUInt16(r, "STL");
                    pstnames[name].PlTotals[PAbbr.BLK] += ParseCell.GetUInt16(r, "BLK");
                    pstnames[name].PlTotals[PAbbr.TOS] += ParseCell.GetUInt16(r, "TOV");
                    pstnames[name].PlTotals[PAbbr.FOUL] += ParseCell.GetUInt16(r, "PF");
                    pstnames[name].PlTotals[PAbbr.PTS] += ParseCell.GetUInt16(r, "PTS");

                    pstnames[name].CalcAvg();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception while trying to parse playoff table: " + ex.Message);
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
        private static int boxScoreFromDataSet(DataSet ds, string[] parts, out BoxScoreEntry bse)
        {
            DataTable awayDT = ds.Tables[0];
            DataTable homeDT = ds.Tables[1];

            var bs = new TeamBoxScore(ds, parts);
            bse = new BoxScoreEntry(bs) {Date = bs.GameDate, PBSList = new List<PlayerBoxScore>()};
            int result = 0;
            for (int i = 0; i < awayDT.Rows.Count - 1; i++)
            {
                if (i == 5)
                    continue;
                var pbs = new PlayerBoxScore(awayDT.Rows[i], bs.Team1ID, bs.ID, (i < 5), MainWindow.PST);
                if (pbs.PlayerID == -1)
                {
                    result = -1;
                    continue;
                }
                bse.PBSList.Add(pbs);
            }
            for (int i = 0; i < homeDT.Rows.Count - 1; i++)
            {
                if (i == 5)
                    continue;
                var pbs = new PlayerBoxScore(homeDT.Rows[i], bs.Team2ID, bs.ID, (i < 5), MainWindow.PST);
                if (pbs.PlayerID == -1)
                {
                    result = -1;
                    continue;
                }
                bse.PBSList.Add(pbs);
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
            DataSet ds = getSeasonTeamStats(@"http://www.basketball-reference.com/teams/" + teamAbbr.Value + @"/2013.html", out recordparts);
            teamStatsFromDataTable(ds.Tables[0], teamAbbr.Key, recordparts, out ts, out tsopp);

            ds = getPlayerStats(@"http://www.basketball-reference.com/teams/" + teamAbbr.Value + @"/2013.html");
            playerStatsFromDataSet(ds, ts.ID, out pst);
        }

        /// <summary>
        ///     Downloads and imports a box score.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns></returns>
        public static int ImportBoxScore(string url)
        {
            string[] parts;
            DataSet ds = getBoxScore(url, out parts);
            BoxScoreEntry bse;
            int result = boxScoreFromDataSet(ds, parts, out bse);

            MainWindow.BSHist.Add(bse);

            return result;
        }

        /// <summary>
        ///     Adds the playoff team stats to the current database.
        /// </summary>
        /// <param name="tst">The team stats dictionary.</param>
        /// <param name="tstOpp">The opposing team stats dictionary.</param>
        public static void AddPlayoffTeamStats(ref Dictionary<int, TeamStats> tst, ref Dictionary<int, TeamStats> tstOpp)
        {
            DataSet ds = getPlayoffTeamStats("http://www.basketball-reference.com/playoffs/NBA_2013.html");
            playoffTeamStatsFromDataSet(ds, ref tst, ref tstOpp);
        }
    }
}