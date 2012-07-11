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
using System.Diagnostics;
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
        public static void GetStatsFrom2K12Save(string fn, out Dictionary<int, TeamStats> tst, ref Dictionary<int, TeamStats> tstopp,
                                                ref SortedDictionary<string, int> TeamOrder, ref PlayoffTree pt,
                                                bool havePT = false)
        {
            var _teamStats = new Dictionary<int, TeamStats>();
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
                        var ptw = new PlayoffTreeWindow();
                        ptw.ShowDialog();
                        if (!pt.done)
                        {
                            tst = new Dictionary<int, TeamStats>();
                            return;
                        }

                        var spt = new SaveFileDialog
                                      {
                                          Title = "Please select a file to save the Playoff Tree to...",
                                          InitialDirectory = Helper.AppDocsPath,
                                          Filter = "Playoff Tree files (*.ptr)|*.ptr"
                                      };
                        spt.ShowDialog();

                        if (spt.FileName == "")
                        {
                            tst = new Dictionary<int, TeamStats>();
                            return;
                        }

                        try
                        {
                            using (FileStream stream = File.Open(spt.FileName, FileMode.Create))
                            {
                                var bf = new BinaryFormatter {AssemblyFormat = FormatterAssemblyStyle.Simple};

                                bf.Serialize(stream, pt);
                            }
                        }
                        catch (Exception ex)
                        {
                            App.errorReport(ex, "Trying to save playoff tree");
                        }
                    }
                    else if (r == MessageBoxResult.Yes)
                    {
                        var ofd = new OpenFileDialog
                                      {
                                          Filter = "Playoff Tree files (*.ptr)|*.ptr",
                                          InitialDirectory = Helper.AppDocsPath,
                                          Title = "Please select the file you saved the Playoff Tree to for " +
                                                  Tools.getSafeFilename(fn) + "..."
                                      };
                        ofd.ShowDialog();

                        if (ofd.FileName == "")
                        {
                            tst = new Dictionary<int, TeamStats>();
                            return;
                        }

                        using (FileStream stream = File.Open(ofd.FileName, FileMode.Open))
                        {
                            var bf = new BinaryFormatter {AssemblyFormat = FormatterAssemblyStyle.Simple};

                            pt = (PlayoffTree) bf.Deserialize(stream);
                        }
                    }
                    else
                    {
                        tst = new Dictionary<int, TeamStats>();
                        return;
                    }
                }
            }
            prepareOffsets(fn, _teamStats, ref TeamOrder, ref pt);

            using (var fileStream = File.OpenRead(fn))
            {
                var br = new BinaryReader(fileStream);
                
                using (var ms = new MemoryStream(br.ReadBytes(Convert.ToInt32(br.BaseStream.Length)), true))
                {
                    var buf = new byte[2];

                    foreach (KeyValuePair<string, int> kvp in TeamOrder)
                    {
                        if (kvp.Key != "")
                        {
                            _teamStats[kvp.Value].name = kvp.Key;
                            _teamStats[kvp.Value].displayName = kvp.Key;
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
                }
                
            }

            foreach (var key in _teamStats.Keys)
            {
                _teamStats[key].calcAvg();
            }

            tst = new Dictionary<int, TeamStats>(_teamStats);

            //TODO: Implement loading opponents stats from 2K12 save here
            /*
            tstopp = new TeamStats[tst.Count];
            for (int i = 0; i < tst.Count; i++)
            {
                tstopp[i] = new TeamStats(tst[i].name);
            }
            */
        }

        public static void prepareOffsets(string fn, Dictionary<int, TeamStats> _teamStats, ref SortedDictionary<string, int> TeamOrder,
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
                //else if (inPlayoffs == -1) return;
            }
            else
            {
                for (int i = 1; i < 16; i++)
                {
                    _teamStats[TeamOrder[pt.teams[i]]].pl_offset = _teamStats[TeamOrder[pt.teams[i - 1]]].pl_offset + 40;
                }
            }
        }

        public static int checkIfIntoPlayoffs(string fn, Dictionary<int, TeamStats> _teamStats,
                                              ref SortedDictionary<string, int> TeamOrder, ref PlayoffTree pt)
        {
            int gamesInSeason = -1;
            string ptFile = "";
            string safefn = Tools.getSafeFilename(fn);
            string SettingsFile = Helper.AppDocsPath + safefn + ".cfg";
            string mode = "";

            if (File.Exists(SettingsFile))
            {
                using (var sr = new StreamReader(SettingsFile))
                {
                    while (sr.Peek() > -1)
                    {
                        string line = sr.ReadLine();
                        Debug.Assert(line != null, "line != null");
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
                }
            }
            if (gamesInSeason == -1)
            {
                gamesInSeason = Helper.askGamesInSeason(gamesInSeason);

                mode = askMode();

                TeamOrder = Helper.setTeamOrder(mode);

                saveSettingsForFile(fn, gamesInSeason, "", mode, SettingsFile);
            }

            bool done;
            using (var fileStream = File.OpenRead(fn))
            {
                var br = new BinaryReader(fileStream);
                MemoryStream ms;
                using (ms = new MemoryStream(br.ReadBytes(Convert.ToInt32(br.BaseStream.Length)), true))
                {
                    done = true;

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
                }
            }

            if (done)
            {
                if (ptFile == "")
                {
/*
                    pt = null;
                    pt = new PlayoffTree();
*/
                    Helper.tempPT = new PlayoffTree();
                    var ptW = new PlayoffTreeWindow();
                    ptW.ShowDialog();
                    pt = Helper.tempPT;

                    if (!pt.done) return -1;

                    var spt = new SaveFileDialog
                                  {
                                      Title = "Please select a file to save the Playoff Tree to...",
                                      InitialDirectory = Helper.AppDocsPath,
                                      Filter = "Playoff Tree files (*.ptr)|*.ptr"
                                  };
                    spt.ShowDialog();

                    if (spt.FileName == "") return -1;

                    ptFile = spt.FileName;

                    try
                    {
                        using (FileStream stream = File.Open(spt.FileName, FileMode.Create))
                        {
                            var bf = new BinaryFormatter {AssemblyFormat = FormatterAssemblyStyle.Simple};

                            bf.Serialize(stream, pt);
                        }
                    }
                    catch (Exception ex)
                    {
                        App.errorReport(ex, "Trying to save playoff tree");
                    }
                }
                else
                {
                    using (FileStream stream = File.Open(ptFile, FileMode.Open))
                    {
                        var bf = new BinaryFormatter {AssemblyFormat = FormatterAssemblyStyle.Simple};

                        pt = (PlayoffTree) bf.Deserialize(stream);
                    }
                }
            }

            saveSettingsForFile(fn, gamesInSeason, ptFile, mode, SettingsFile);

            if (done) return 1;
            
            return 0;
        }

        private static string askMode()
        {
            var at = new ComboChoiceWindow(false);
            at.ShowDialog();
            return Helper.mode;
        }

        public static void saveSettingsForFile(string fn, int gamesInSeason, string ptFile, string mode,
                                               string SettingsFile)
        {
            using (var sw2 = new StreamWriter(SettingsFile, false))
            {
                sw2.WriteLine("{0}\t{1}\t{2}\t{3}", fn, gamesInSeason, ptFile, mode);
            }
        }

        public static void updateSavegame(string fn, Dictionary<int, TeamStats> tst, SortedDictionary<string, int> TeamOrder,
                                          PlayoffTree pt)
        {
            using (var openRead = File.OpenRead(fn))
            {
                var br = new BinaryReader(openRead);
                using (var ms = new MemoryStream(br.ReadBytes(Convert.ToInt32(br.BaseStream.Length)), true))
                {
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

                    using (var fileStream = File.OpenWrite(Helper.AppTempPath + Tools.getSafeFilename(fn)))
                    {
                        var bw = new BinaryWriter(fileStream);

                        ms.Position = 4;
                        var t = new byte[1048576];
                        int count;
                        do
                        {
                            count = ms.Read(t, 0, 1048576);
                            bw.Write(t, 0, count);
                        } while (count > 0);
                    }
                }
            }
            
            byte[] crc =
                Tools.ReverseByteOrder(
                    Tools.StringToByteArray(Tools.getCRC(Helper.AppTempPath + Tools.getSafeFilename(fn))), 4);

            File.Delete(fn + ".bak");
            
            File.Move(fn, fn + ".bak");
            using (var fileStream = File.OpenRead(Helper.AppTempPath + Tools.getSafeFilename(fn)))
            {
                var br2 = new BinaryReader(fileStream);
                using (var openWrite = File.OpenWrite(fn))
                {
                    var bw2 = new BinaryWriter(openWrite); bw2.Write(crc);
                    byte[] readBytes;
                    do
                    {
                        readBytes = br2.ReadBytes(1048576);
                        bw2.Write(readBytes);
                    } while (readBytes.Length > 0);
                }
            }
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

        protected PlayoffTree(SerializationInfo info, StreamingContext ctxt)
        {
            teams = (string[]) info.GetValue("teams", typeof (string[]));
            done = (bool) info.GetValue("done", typeof (bool));
        }

        #region ISerializable Members

        protected virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("teams", teams);
            info.AddValue("done", done);
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException("info");

            GetObjectData(info, context);
        }


        #endregion
    }
}