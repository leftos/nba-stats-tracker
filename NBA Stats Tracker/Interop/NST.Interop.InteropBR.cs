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
using System.Data;
using System.Linq;
using HtmlAgilityPack;
using LeftosCommonLibrary;
using NBA_Stats_Tracker.Data;

#endregion

namespace NBA_Stats_Tracker.Interop
{
    public static class InteropBR
    {
        private const int tMINS = 0,
                          tPF = 1,
                          tPA = 2,
                          tFGM = 4,
                          tFGA = 5,
                          tTPM = 6,
                          tTPA = 7,
                          tFTM = 8,
                          tFTA = 9,
                          tOREB = 10,
                          tDREB = 11,
                          tSTL = 12,
                          tTO = 13,
                          tBLK = 14,
                          tAST = 15,
                          tFOUL = 16;

        private const int pGP = 0,
                          pGS = 1,
                          pMINS = 2,
                          pPTS = 3,
                          pDREB = 4,
                          pOREB = 5,
                          pAST = 6,
                          pSTL = 7,
                          pBLK = 8,
                          pTO = 9,
                          pFOUL = 10,
                          pFGM = 11,
                          pFGA = 12,
                          pTPM = 13,
                          pTPA = 14,
                          pFTM = 15,
                          pFTA = 16;

        private static DataSet GetBoxScore(string url)
        {
            var dataset = new DataSet();

            var htmlweb = new HtmlWeb();
            HtmlDocument doc = htmlweb.Load(url);

            HtmlNodeCollection tables = doc.DocumentNode.SelectNodes("//table");
            foreach (HtmlNode cur in tables)
            {
                try
                {
                    if (!cur.Attributes["id"].Value.EndsWith("_basic")) continue;
                }
                catch (Exception)
                {
                    continue;
                }
                var table = new DataTable(cur.Attributes["id"].Value);

                HtmlNode thead = cur.SelectSingleNode("thead");
                HtmlNodeCollection theadrows = thead.SelectNodes("tr");
                HtmlNodeCollection header = theadrows[1].SelectNodes("th");

                IEnumerable<string> headers = theadrows[1]
                    .Elements("th")
                    .Select(th => th.InnerText.Trim());
                foreach (string colheader in headers)
                {
                    table.Columns.Add(colheader);
                }

                HtmlNode tbody = cur.SelectSingleNode("tbody");
                HtmlNodeCollection tbodyrows = tbody.SelectNodes("tr");
                IEnumerable<string[]> rows = tbodyrows.Select(tr => tr.Elements("td")
                                                                        .Select(td => td.InnerText.Trim())
                                                                        .ToArray());
                foreach (string[] row in rows)
                {
                    table.Rows.Add(row);
                }

                dataset.Tables.Add(table);
            }
            return dataset;
        }

        private static DataSet GetSeasonTeamStats(string url, out string[] recordparts)
        {
            var dataset = new DataSet();

            var htmlweb = new HtmlWeb();
            HtmlDocument doc = htmlweb.Load(url);

            HtmlNode infobox = doc.DocumentNode.SelectSingleNode("//div[@id='info_box']");
            HtmlNodeCollection infoboxps = infobox.SelectNodes("p");
            HtmlNode infoboxp = infoboxps[2];
            HtmlNode infoboxpstrong = infoboxp.NextSibling;
            string record = infoboxpstrong.InnerText;
            recordparts = record.Split('-');

            HtmlNodeCollection tables = doc.DocumentNode.SelectNodes("//table");
            foreach (HtmlNode cur in tables)
            {
                try
                {
                    if (cur.Attributes["id"].Value != "team") continue;
                }
                catch (Exception)
                {
                    continue;
                }
                var table = new DataTable(cur.Attributes["id"].Value);

                HtmlNode thead = cur.SelectSingleNode("thead");
                HtmlNodeCollection theadrows = thead.SelectNodes("tr");

                IEnumerable<string> headers = theadrows[0]
                    .Elements("th")
                    .Select(th => th.InnerText.Trim());
                foreach (string colheader in headers)
                {
                    table.Columns.Add(colheader);
                }

                HtmlNode tbody = cur.SelectSingleNode("tbody");
                HtmlNodeCollection tbodyrows = tbody.SelectNodes("tr");
                IEnumerable<string[]> rows = tbodyrows.Select(tr => tr.Elements("td")
                                                                        .Select(td => td.InnerText.Trim())
                                                                        .ToArray());
                foreach (string[] row in rows)
                {
                    table.Rows.Add(row);
                }

                dataset.Tables.Add(table);
            }
            return dataset;
        }

        private static DataSet GetPlayerStats(string url)
        {
            var dataset = new DataSet();

            var htmlweb = new HtmlWeb();
            HtmlDocument doc = htmlweb.Load(url);

            HtmlNodeCollection tables = doc.DocumentNode.SelectNodes("//table");
            foreach (HtmlNode cur in tables)
            {
                try
                {
                    if (
                        !(cur.Attributes["id"].Value == "totals" || cur.Attributes["id"].Value == "playoffs" ||
                          cur.Attributes["id"].Value == "roster")) continue;
                }
                catch (Exception)
                {
                    continue;
                }
                var table = new DataTable(cur.Attributes["id"].Value);

                HtmlNode thead = cur.SelectSingleNode("thead");
                HtmlNodeCollection theadrows = thead.SelectNodes("tr");

                HtmlNode theadrow;
                if (cur.Attributes["id"].Value == "playoffs") theadrow = theadrows[1];
                else theadrow = theadrows[0];

                IEnumerable<string> headers = theadrow
                    .Elements("th")
                    .Select(th => th.InnerText.Trim());
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
                IEnumerable<string[]> rows = tbodyrows.Select(tr => tr.Elements("td")
                                                                        .Select(td => td.InnerText.Trim())
                                                                        .ToArray());
                foreach (string[] row in rows)
                {
                    table.Rows.Add(row);
                }

                dataset.Tables.Add(table);
            }
            return dataset;
        }

        private static DataSet GetPlayoffTeamStats(string url)
        {
            var dataset = new DataSet();

            var htmlweb = new HtmlWeb();
            HtmlDocument doc = htmlweb.Load(url);

            HtmlNodeCollection tables = doc.DocumentNode.SelectNodes("//table");
            foreach (HtmlNode cur in tables)
            {
                try
                {
                    if (
                        !(cur.Attributes["id"].Value == "team" || cur.Attributes["id"].Value == "opponent" ||
                          cur.Attributes["id"].Value == "misc")) continue;
                }
                catch (Exception)
                {
                    continue;
                }
                var table = new DataTable(cur.Attributes["id"].Value);

                HtmlNode thead = cur.SelectSingleNode("thead");
                HtmlNodeCollection theadrows = thead.SelectNodes("tr");
                HtmlNode theadrow;

                if (cur.Attributes["id"].Value == "misc") theadrow = theadrows[1];
                else theadrow = theadrows[0];

                IEnumerable<string> headers = theadrow
                    .Elements("th")
                    .Select(th => th.InnerText.Trim());
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
                IEnumerable<string[]> rows = tbodyrows.Select(tr => tr.Elements("td")
                                                                        .Select(td => td.InnerText.Trim())
                                                                        .ToArray());
                foreach (string[] row in rows)
                {
                    table.Rows.Add(row);
                }

                dataset.Tables.Add(table);
            }
            return dataset;
        }

        private static void TeamStatsFromDataTable(DataTable dt, string name, string[] recordparts, out TeamStats ts,
                                                   out TeamStats tsopp)
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

            ts.calcAvg();

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

            tsopp.calcAvg();
        }

        private static void PlayoffTeamStatsFromDataSet(DataSet ds, ref TeamStats[] tst, ref TeamStats[] tstopp)
        {
            DataTable dt = ds.Tables["team"];
            DataTable dtopp = ds.Tables["opponent"];
            DataTable dtmisc = ds.Tables["misc"];

            for (int i = 0; i < tst.Length; i++)
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

                if (!found) continue;

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

        private static void PlayerStatsFromDataSet(DataSet ds, string team, out Dictionary<int, PlayerStats> pst)
        {
            var pstnames = new Dictionary<string, PlayerStats>();

            DataTable dt;
            dt = ds.Tables["roster"];

            foreach (DataRow r in dt.Rows)
            {
                string Position1, Position2;
                switch (r["Pos"].ToString())
                {
                    case "C":
                        Position1 = "C";
                        Position2 = " ";
                        break;

                    case "G":
                        Position1 = "PG";
                        Position2 = "SG";
                        break;

                    case "F":
                        Position1 = "SF";
                        Position2 = "PF";
                        break;

                    case "G-F":
                        Position1 = "SG";
                        Position2 = "SF";
                        break;

                    case "F-G":
                        Position1 = "SF";
                        Position2 = "SG";
                        break;

                    case "F-C":
                        Position1 = "PF";
                        Position2 = "C";
                        break;

                    case "C-F":
                        Position1 = "C";
                        Position2 = "PF";
                        break;

                    default:
                        throw (new Exception("Don't recognize the position " + r["Pos"]));
                }
                var ps = new PlayerStats(new Player(pstnames.Count, team, r["Player"].ToString().Split(' ')[1],
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
                pstnames[name].stats[p.TPA] = Tools.getUInt16(r, "3P");
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
                    pstnames[name].stats[p.GP] += Tools.getUInt16(r, "G");
                    //pstnames[name].stats[p.GS] += NSTHelper.getUShort(r, "GS");
                    pstnames[name].stats[p.MINS] += Tools.getUInt16(r, "MP");
                    pstnames[name].stats[p.FGM] += Tools.getUInt16(r, "FG");
                    pstnames[name].stats[p.FGA] += Tools.getUInt16(r, "FGA");
                    pstnames[name].stats[p.TPA] += Tools.getUInt16(r, "3P");
                    pstnames[name].stats[p.TPA] += Tools.getUInt16(r, "3PA");
                    pstnames[name].stats[p.FTM] += Tools.getUInt16(r, "FT");
                    pstnames[name].stats[p.FTA] += Tools.getUInt16(r, "FTA");
                    pstnames[name].stats[p.OREB] += Tools.getUInt16(r, "ORB");
                    pstnames[name].stats[p.DREB] +=
                        (ushort) (Tools.getUInt16(r, "TRB") - Tools.getUInt16(r, "ORB"));
                    pstnames[name].stats[p.AST] += Tools.getUInt16(r, "AST");
                    pstnames[name].stats[p.STL] += Tools.getUInt16(r, "STL");
                    pstnames[name].stats[p.BLK] += Tools.getUInt16(r, "BLK");
                    pstnames[name].stats[p.TO] += Tools.getUInt16(r, "TOV");
                    pstnames[name].stats[p.FOUL] += Tools.getUInt16(r, "PF");
                    pstnames[name].stats[p.PTS] += Tools.getUInt16(r, "PTS");

                    pstnames[name].CalcAvg();
                }
            }
            catch (Exception)
            {
            }

            pst = new Dictionary<int, PlayerStats>();
            foreach (KeyValuePair<string, PlayerStats> kvp in pstnames)
            {
                kvp.Value.ID = pst.Count;
                pst.Add(pst.Count, kvp.Value);
            }
        }

        public static void ImportRealStats(KeyValuePair<string, string> teamAbbr, out TeamStats ts, out TeamStats tsopp,
                                           out Dictionary<int, PlayerStats> pst)
        {
            string[] recordparts;
            DataSet ds =
                GetSeasonTeamStats(@"http://www.basketball-reference.com/teams/" + teamAbbr.Value + @"/2012.html",
                                   out recordparts);
            TeamStatsFromDataTable(ds.Tables[0], teamAbbr.Key, recordparts, out ts, out tsopp);

            ds = GetPlayerStats(@"http://www.basketball-reference.com/teams/" + teamAbbr.Value + @"/2012.html");
            PlayerStatsFromDataSet(ds, teamAbbr.Key, out pst);
        }

        public static void AddPlayoffTeamStats(ref TeamStats[] tst, ref TeamStats[] tstopp)
        {
            DataSet ds = GetPlayoffTeamStats("http://www.basketball-reference.com/playoffs/NBA_2012.html");
            PlayoffTeamStatsFromDataSet(ds, ref tst, ref tstopp);
        }
    }
}