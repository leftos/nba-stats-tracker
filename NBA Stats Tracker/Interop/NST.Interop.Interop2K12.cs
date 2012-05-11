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
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using LeftosCommonLibrary;
using Microsoft.Win32;
using NBA_Stats_Tracker.Data;
using NBA_Stats_Tracker.Windows;

#endregion

namespace NBA_Stats_Tracker.Interop
{
    public static class Interop2K12
    {
        public static void GetStatsFrom2K12Save(string fn, ref TeamStats[] tst, ref TeamStats[] tstopp,
                                                ref SortedDictionary<string, int> TeamOrder, ref PlayoffTree pt,
                                                bool havePT = false)
        {
            var _teamStats = new TeamStats[30];
            for (int i = 0; i < 30; i++)
            {
                _teamStats[i] = new TeamStats();
            }
            if (!havePT) pt = null;

            string ext = Tools.getExtension(fn);

            if (ext.ToUpperInvariant() == "PMG")
            {
                if (!havePT)
                {
                    pt = new PlayoffTree();
                    MessageBoxResult r =
                        MessageBox.Show("Do you have a saved Playoff Tree you want to load for this save file?",
                                        "NBA Stats Tracker", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                    if (r == MessageBoxResult.No)
                    {
                        var ptw = new playoffTreeW();
                        ptw.ShowDialog();
                        if (!pt.done)
                        {
                            tst = new TeamStats[1];
                            return;
                        }

                        var spt = new SaveFileDialog();
                        spt.Title = "Please select a file to save the Playoff Tree to...";
                        spt.InitialDirectory = Helper.AppDocsPath;
                        spt.Filter = "Playoff Tree files (*.ptr)|*.ptr";
                        spt.ShowDialog();

                        if (spt.FileName == "")
                        {
                            tst = new TeamStats[1];
                            return;
                        }

                        try
                        {
                            FileStream stream = File.Open(spt.FileName, FileMode.Create);
                            var bf = new BinaryFormatter();
                            bf.AssemblyFormat = FormatterAssemblyStyle.Simple;

                            bf.Serialize(stream, pt);
                            stream.Close();
                        }
                        catch (Exception ex)
                        {
                            App.errorReport(ex, "Trying to save playoff tree");
                        }
                    }
                    else if (r == MessageBoxResult.Yes)
                    {
                        var ofd = new OpenFileDialog();
                        ofd.Filter = "Playoff Tree files (*.ptr)|*.ptr";
                        ofd.InitialDirectory = Helper.AppDocsPath;
                        ofd.Title = "Please select the file you saved the Playoff Tree to for " +
                                    Tools.getSafeFilename(fn) + "...";
                        ofd.ShowDialog();

                        if (ofd.FileName == "")
                        {
                            tst = new TeamStats[1];
                            return;
                        }

                        FileStream stream = File.Open(ofd.FileName, FileMode.Open);
                        var bf = new BinaryFormatter();
                        bf.AssemblyFormat = FormatterAssemblyStyle.Simple;

                        pt = (PlayoffTree) bf.Deserialize(stream);
                        stream.Close();
                    }
                    else
                    {
                        tst = new TeamStats[1];
                        return;
                    }
                }
            }
            prepareOffsets(fn, _teamStats, ref TeamOrder, ref pt);

            var br = new BinaryReader(File.OpenRead(fn));
            var ms = new MemoryStream(br.ReadBytes(Convert.ToInt32(br.BaseStream.Length)), true);
            br.Close();
            var buf = new byte[2];

            foreach (KeyValuePair<string, int> kvp in TeamOrder)
            {
                if (kvp.Key != "")
                {
                    _teamStats[kvp.Value].name = kvp.Key;
                }
            }
            for (int i = 0; i < 30; i++)
            {
                ms.Seek(_teamStats[i].offset, SeekOrigin.Begin);
                ms.Read(buf, 0, 2);
                _teamStats[i].winloss[0] = buf[0];
                _teamStats[i].winloss[1] = buf[1];
                for (int j = 0; j < 18; j++)
                {
                    ms.Read(buf, 0, 2);
                    _teamStats[i].stats[j] = BitConverter.ToUInt16(buf, 0);
                }
            }

            if (pt != null && pt.teams[0] != "Invalid")
            {
                for (int i = 0; i < 16; i++)
                {
                    int id = TeamOrder[pt.teams[i]];
                    ms.Seek(_teamStats[id].pl_offset, SeekOrigin.Begin);
                    ms.Read(buf, 0, 2);
                    _teamStats[id].name = pt.teams[i];
                    _teamStats[id].pl_winloss[0] = buf[0];
                    _teamStats[id].pl_winloss[1] = buf[1];
                    for (int j = 0; j < 18; j++)
                    {
                        ms.Read(buf, 0, 2);
                        _teamStats[id].pl_stats[j] = BitConverter.ToUInt16(buf, 0);
                    }
                }
            }

            for (int i = 0; i < _teamStats.Length; i++)
            {
                _teamStats[i].calcAvg();
            }

            tst = _teamStats;

            //TODO: Implement loading opponents stats from 2K12 save here
            /*
            tstopp = new TeamStats[tst.Length];
            for (int i = 0; i < tst.Length; i++)
            {
                tstopp[i] = new TeamStats(tst[i].name);
            }
            */
        }

        public static void prepareOffsets(string fn, TeamStats[] _teamStats, ref SortedDictionary<string, int> TeamOrder,
                                          ref PlayoffTree pt)
        {
            // Stage 1
            string ext = Tools.getExtension(fn);
            if (ext.ToUpperInvariant() == "FXG" || ext.ToUpperInvariant() == "RFG")
            {
                _teamStats[0].offset = 3240532;
            }
            else if (ext.ToUpperInvariant() == "CMG")
            {
                _teamStats[0].offset = 5722996;
            }
            else if (ext.ToUpperInvariant() == "PMG")
            {
                _teamStats[TeamOrder[pt.teams[0]]].offset = 1813028;
            }

            // Stage 2
            if (ext.ToUpperInvariant() != "PMG")
            {
                for (int i = 1; i < 30; i++)
                {
                    _teamStats[i].offset = _teamStats[i - 1].offset + 40;
                }
                int inPlayoffs = checkIfIntoPlayoffs(fn, _teamStats, ref TeamOrder, ref pt);
                if (inPlayoffs == 1)
                {
                    _teamStats[TeamOrder[pt.teams[0]]].pl_offset = _teamStats[0].offset - 1440;
                    for (int i = 1; i < 16; i++)
                    {
                        _teamStats[TeamOrder[pt.teams[i]]].pl_offset =
                            _teamStats[TeamOrder[pt.teams[i - 1]]].pl_offset + 40;
                    }
                }
                else if (inPlayoffs == -1) return;
            }
            else
            {
                for (int i = 1; i < 16; i++)
                {
                    _teamStats[TeamOrder[pt.teams[i]]].pl_offset = _teamStats[TeamOrder[pt.teams[i - 1]]].pl_offset + 40;
                }
            }
        }

        public static int checkIfIntoPlayoffs(string fn, TeamStats[] _teamStats,
                                              ref SortedDictionary<string, int> TeamOrder, ref PlayoffTree pt)
        {
            int gamesInSeason = -1;
            string ptFile = "";
            string safefn = Tools.getSafeFilename(fn);
            string SettingsFile = Helper.AppDocsPath + safefn + ".cfg";
            string mode = "";

            if (File.Exists(SettingsFile))
            {
                var sr = new StreamReader(SettingsFile);
                while (sr.Peek() > -1)
                {
                    string line = sr.ReadLine();
                    string[] parts = line.Split('\t');
                    if (parts[0] == fn)
                    {
                        try
                        {
                            gamesInSeason = Convert.ToInt32(parts[1]);
                            ptFile = parts[2];
                            mode = parts[3];

                            TeamOrder = Helper.setTeamOrder(mode);
                        }
                        catch
                        {
                            gamesInSeason = -1;
                        }
                        break;
                    }
                }
                sr.Close();
            }
            if (gamesInSeason == -1)
            {
                gamesInSeason = Helper.askGamesInSeason(gamesInSeason);

                mode = askMode();

                TeamOrder = Helper.setTeamOrder(mode);

                saveSettingsForFile(fn, gamesInSeason, "", mode, SettingsFile);
            }

            var br = new BinaryReader(File.OpenRead(fn));
            var ms = new MemoryStream(br.ReadBytes(Convert.ToInt32(br.BaseStream.Length)), true);
            br.Close();

            bool done = true;

            if (ptFile == "")
            {
                for (int i = 0; i < 30; i++)
                {
                    ms.Seek(_teamStats[i].offset, SeekOrigin.Begin);
                    var w = (byte) ms.ReadByte();
                    var l = (byte) ms.ReadByte();
                    uint total = Convert.ToUInt32(w + l);
                    if (total < gamesInSeason)
                    {
                        done = false;
                        break;
                    }
                }
            }

            if (done)
            {
                if (ptFile == "")
                {
                    pt = null;
                    pt = new PlayoffTree();
                    Helper.tempPT = new PlayoffTree();
                    var ptW = new playoffTreeW();
                    ptW.ShowDialog();
                    pt = Helper.tempPT;

                    if (!pt.done) return -1;

                    var spt = new SaveFileDialog();
                    spt.Title = "Please select a file to save the Playoff Tree to...";
                    spt.InitialDirectory = Helper.AppDocsPath;
                    spt.Filter = "Playoff Tree files (*.ptr)|*.ptr";
                    spt.ShowDialog();

                    if (spt.FileName == "") return -1;

                    ptFile = spt.FileName;

                    try
                    {
                        FileStream stream = File.Open(spt.FileName, FileMode.Create);
                        var bf = new BinaryFormatter();
                        bf.AssemblyFormat = FormatterAssemblyStyle.Simple;

                        bf.Serialize(stream, pt);
                        stream.Close();
                    }
                    catch (Exception ex)
                    {
                        App.errorReport(ex, "Trying to save playoff tree");
                    }
                }
                else
                {
                    FileStream stream = File.Open(ptFile, FileMode.Open);
                    var bf = new BinaryFormatter();
                    bf.AssemblyFormat = FormatterAssemblyStyle.Simple;

                    pt = (PlayoffTree) bf.Deserialize(stream);
                    stream.Close();
                }
            }

            saveSettingsForFile(fn, gamesInSeason, ptFile, mode, SettingsFile);

            if (done) return 1;
            else return 0;
        }

        private static string askMode()
        {
            var at = new askTeamW(false);
            at.ShowDialog();
            return Helper.mode;
        }

        public static void saveSettingsForFile(string fn, int gamesInSeason, string ptFile, string mode,
                                               string SettingsFile)
        {
            var sw2 = new StreamWriter(SettingsFile, false);
            sw2.WriteLine("{0}\t{1}\t{2}\t{3}", fn, gamesInSeason, ptFile, mode);
            sw2.Close();
        }

        public static void updateSavegame(string fn, TeamStats[] tst, SortedDictionary<string, int> TeamOrder,
                                          PlayoffTree pt)
        {
            var br = new BinaryReader(File.OpenRead(fn));
            var ms = new MemoryStream(br.ReadBytes(Convert.ToInt32(br.BaseStream.Length)), true);

            if ((pt != null) && (pt.teams[0] != "Invalid"))
            {
                for (int i = 0; i < 16; i++)
                {
                    ms.Seek(tst[TeamOrder[pt.teams[i]]].pl_offset, SeekOrigin.Begin);
                    ms.Write(tst[TeamOrder[pt.teams[i]]].pl_winloss, 0, 2);
                    for (int j = 0; j < 18; j++)
                    {
                        ms.Write(BitConverter.GetBytes(tst[TeamOrder[pt.teams[i]]].pl_stats[j]), 0, 2);
                    }
                }
            }

            for (int i = 0; i < 30; i++)
            {
                ms.Seek(tst[i].offset, SeekOrigin.Begin);
                ms.Write(tst[i].winloss, 0, 2);
                for (int j = 0; j < 18; j++)
                {
                    ms.Write(BitConverter.GetBytes(tst[i].stats[j]), 0, 2);
                }
            }

            var bw = new BinaryWriter(File.OpenWrite(Helper.AppTempPath + Tools.getSafeFilename(fn)));
            ms.Position = 4;
            var t = new byte[1048576];
            int count;
            do
            {
                count = ms.Read(t, 0, 1048576);
                bw.Write(t, 0, count);
            } while (count > 0);

            br.Close();
            bw.Close();

            byte[] crc =
                Tools.ReverseByteOrder(
                    Tools.StringToByteArray(Tools.getCRC(Helper.AppTempPath + Tools.getSafeFilename(fn))), 4);

            try
            {
                File.Delete(fn + ".bak");
            }
            catch
            {
            }
            File.Move(fn, fn + ".bak");
            var br2 = new BinaryReader(File.OpenRead(Helper.AppTempPath + Tools.getSafeFilename(fn)));
            var bw2 = new BinaryWriter(File.OpenWrite(fn));
            bw2.Write(crc);
            do
            {
                t = br2.ReadBytes(1048576);
                bw2.Write(t);
            } while (t.Length > 0);
            br2.Close();
            bw2.Close();

            File.Delete(Helper.AppTempPath + Tools.getSafeFilename(fn));
        }
    }

    [Serializable]
    public class PlayoffTree : ISerializable
    {
        public bool done;
        public string[] teams = new string[16];

        public PlayoffTree()
        {
            teams[0] = "Invalid";
        }

        public PlayoffTree(SerializationInfo info, StreamingContext ctxt)
        {
            teams = (string[]) info.GetValue("teams", typeof (string[]));
            done = (bool) info.GetValue("done", typeof (bool));
        }

        #region ISerializable Members

        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("teams", teams);
            info.AddValue("done", done);
        }

        #endregion
    }
}