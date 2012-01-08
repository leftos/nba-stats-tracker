using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Net;
using System.ComponentModel;
using System.Reflection;
using System.Diagnostics;

namespace NBA_2K12_Correct_Team_Stats
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static string AppDocsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\NBA 2K12 Correct Team Stats\";
        public static string AppTempPath = AppDocsPath + @"Temp\";
        public static string SavesPath = "";

        public const int M = 0, PF = 1, PA = 2, FGM = 4, FGA = 5, TPM = 6, TPA = 7,
            FTM = 8, FTA = 9, OREB = 10, DREB = 11, STL = 12, TO = 13, BLK = 14, AST = 15,
            FOUL = 16;
        public const int PPG = 0, PAPG = 1, FGp = 2, FGeff = 3, TPp = 4, TPeff = 5,
            FTp = 6, FTeff = 7, RPG = 8, ORPG = 9, DRPG = 10, SPG = 11, BPG = 12,
            TPG = 13, APG = 14, FPG = 15;

        /// <summary>
        /// TeamStats array.
        /// 0: Nuggets, 1: Trail Blazers, 2: Pacers, 3: Pistons, 4: Heat,
        /// 5: Knicks, 6: Grizzlies, 7: Clippers, 8: Warriors, 9: Bucks,
        /// 10: Spurs, 11: Cavaliers, 12: Celtcs, 13: Kings, 14: Suns,
        /// 15:Hornets, 16: Hawks, 17: Timberwolves, 18: nets, 19: Wizards,
        /// 20: 76ers, 21: Raptors, 22: Bobcats, 23: Magic, 24: Thunder,
        /// 25: Lakers, 26: Rockets, 27: Jazz, 28: Bulls, 29: Mavericks
        /// </summary>
        public static TeamStats[] tst = new TeamStats[30];
        public static BoxScore bs;
        public static PlayoffTree pt;
        public static string ext;

        public static Dictionary<string, int> TeamNames = new Dictionary<string, int>
        {
            {"76ers", 20},
            {"Bobcats", 22},
            {"Bucks", 9},
            {"Bulls", 28},
            {"Cavaliers", 11},
            {"Celtics", 12},
            {"Clippers", 7},
            {"Grizzlies", 6},
            {"Hawks", 16},
            {"Heat", 4},
            {"Hornets", 15},
            {"Jazz", 27},
            {"Kings", 13},
            {"Knicks", 5},
            {"Lakers", 25},
            {"Magic", 23},
            {"Mavericks", 29},
            {"Nets", 18},
            {"Nuggets", 0},
            {"Pacers", 2},
            {"Pistons", 3},
            {"Raptors", 21},
            {"Rockets", 26},
            {"Spurs", 10},
            {"Suns", 14},
            {"Thunder", 24},
            {"Timberwolves", 17},
            {"Trail Blazers", 1},
            {"Warriors", 8},
            {"Wizards", 19}
        };

        public static List<string> West = new List<string>
        {
            "Thunder", "Spurs", "Trail Blazers",
            "Clippers", "Nuggets", "Jazz",
            "Lakers", "Mavericks", "Suns",
            "Grizzlies", "Kings", "Timberwolves",
            "Rockets", "Hornets", "Warriors"
        };

        public MainWindow()
        {
            InitializeComponent();

            btnSave.Visibility = Visibility.Hidden;
            btnCRC.Visibility = Visibility.Hidden;

            if (Directory.Exists(AppDocsPath) == false) Directory.CreateDirectory(AppDocsPath);
            if (Directory.Exists(AppTempPath) == false) Directory.CreateDirectory(AppTempPath);

            for (int i = 0; i < 30; i++)
            {
                tst[i] = new TeamStats();
            }

            foreach (KeyValuePair<string, int> kvp in TeamNames)
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

            if ((rk = rk.OpenSubKey(@"SOFTWARE\2K Sports\NBA 2K12")) == null)
            {
                SavesPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\2K Sports\NBA 2K12\Saves\";
            }
            else
            {
                SavesPath = rk.GetValue("Saves").ToString();
            }

            checkForRedundantSettings();

            checkForUpdates();
        }

        private void checkForRedundantSettings()
        {
            string[] stgFiles = Directory.GetFiles(AppDocsPath, "*.cfg");
            if (Directory.Exists(SavesPath))
            {
                foreach (string file in stgFiles)
                {
                    string realfile = file.Substring(0, file.Length - 4);
                    if (File.Exists(SavesPath + getSafeFilename(realfile)) == false)
                        File.Delete(file);
                }
            }
        }

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Please select the Career file you're playing...";
            ofd.Filter = "All NBA 2K12 Career Files (*.FXG; *.CMG; *.RFG; *.PMG; *.SMG)|*.FXG;*.CMG;*.RFG;*.PMG;*.SMG|"
            + "Association files (*.FXG)|*.FXG|My Player files (*.CMG)|*.CMG|Season files (*.RFG)|*.RFG|Playoff files (*.PMG)|*.PMG|" +
                "Create A Legend files (*.SMG)|*.SMG";
            if (Directory.Exists(SavesPath)) ofd.InitialDirectory = SavesPath;
            ofd.ShowDialog();
            if (ofd.FileName == "") return;
            txtFile.Text = ofd.FileName;

            GetStats(txtFile.Text);

            cmbTeam1.SelectedIndex = 0;
        }

        private void GetStats(string fn, bool havePT = false)
        {
            if (!havePT) pt = null;
            if (ext == "PMG")
            {
                if (!havePT)
                {
                    pt = new PlayoffTree();
                    MessageBoxResult r = MessageBox.Show("Do you have a saved Playoff Tree you want to load for this save file?", "NBA 2K12 Correct Team Stats", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                    if (r == MessageBoxResult.No)
                    {
                        playoffTreeW ptw = new playoffTreeW();
                        ptw.ShowDialog();
                        if (!pt.done) return;

                        SaveFileDialog spt = new SaveFileDialog();
                        spt.Title = "Please select a file to save the Playoff Tree to...";
                        spt.InitialDirectory = AppDocsPath;
                        spt.Filter = "Playoff Tree files (*.ptr)|*.ptr";
                        spt.ShowDialog();

                        if (spt.FileName == "") return;

                        try
                        {
                            FileStream stream = File.Open(spt.FileName, FileMode.Create);
                            BinaryFormatter bf = new BinaryFormatter();

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
                        OpenFileDialog ofd = new OpenFileDialog();
                        ofd.Filter = "Playoff Tree files (*.ptr)|*.ptr";
                        ofd.InitialDirectory = AppDocsPath;
                        ofd.Title = "Please select the file you saved the Playoff Tree to for " + getSafeFilename(fn) + "...";
                        ofd.ShowDialog();

                        if (ofd.FileName == "") return;

                        FileStream stream = File.Open(ofd.FileName, FileMode.Open);
                        BinaryFormatter bf = new BinaryFormatter();

                        pt = (PlayoffTree)bf.Deserialize(stream);
                        stream.Close();
                    }
                    else return;
                }
            }
            prepareOffsets(fn);

            BinaryReader br = new BinaryReader(File.OpenRead(fn));
            MemoryStream ms = new MemoryStream(br.ReadBytes(Convert.ToInt32(br.BaseStream.Length)), true);
            br.Close();
            byte[] buf = new byte[2];

            if ((pt == null) || (pt.teams[0] == "Invalid"))
            {
                for (int i = 0; i < 30; i++)
                {
                    ms.Seek(tst[i].offset, SeekOrigin.Begin);
                    ms.Read(buf, 0, 2);
                    tst[i].winloss[0] = buf[0];
                    tst[i].winloss[1] = buf[1];
                    for (int j = 0; j < 18; j++)
                    {
                        ms.Read(buf, 0, 2);
                        tst[i].stats[j] = BitConverter.ToUInt16(buf, 0);
                    }
                }
            }
            else
            {
                cmbTeam1.Items.Clear();
                List<string> newteams = new List<string>();
                for (int i = 0; i < 16; i++)
                {
                    newteams.Add(pt.teams[i]);
                    int id = TeamNames[pt.teams[i]];
                    ms.Seek(tst[id].offset, SeekOrigin.Begin);
                    ms.Read(buf, 0, 2);
                    tst[id].winloss[0] = buf[0];
                    tst[id].winloss[1] = buf[1];
                    for (int j = 0; j < 18; j++)
                    {
                        ms.Read(buf, 0, 2);
                        tst[id].stats[j] = BitConverter.ToUInt16(buf, 0);
                    }
                }
                newteams.Sort();
                foreach (string team in newteams)
                    cmbTeam1.Items.Add(team);
            }
        }

        private string getExtension(string fn)
        {
            string[] parts = fn.Split('.');
            return parts[parts.Length - 1];
        }

        private int checkIfIntoPlayoffs(string fn)
        {
            int gamesInSeason = -1;
            string ptFile = "";
            string safefn = getSafeFilename(fn);
            string SettingsFile = AppDocsPath + safefn + ".cfg";

            if (File.Exists(SettingsFile))
            {
                StreamReader sr = new StreamReader(SettingsFile);
                while (sr.Peek() > -1)
                {
                    string line = sr.ReadLine();
                    string[] parts = line.Split('\t');
                    if (parts[0] == fn) 
                    {
                        gamesInSeason = Convert.ToInt32(parts[1]);
                        ptFile = parts[2];
                        break;
                    }
                }
                sr.Close();
            }
            if (gamesInSeason == -1)
            {
                MessageBoxResult r = MessageBox.Show("How many games does each season have in this save?\n\n82 Games: Yes\n58 Games: No\n29 Games: Cancel", "", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (r == MessageBoxResult.Yes) gamesInSeason = 82;
                else if (r == MessageBoxResult.No) gamesInSeason = 58;
                else if (r == MessageBoxResult.Cancel) gamesInSeason = 28;

                StreamWriter sw = new StreamWriter(SettingsFile, true);
                sw.WriteLine("{0}\t{1}\t{2}", fn, gamesInSeason, "");
                sw.Close();
            }

            BinaryReader br = new BinaryReader(File.OpenRead(fn));
            MemoryStream ms = new MemoryStream(br.ReadBytes(Convert.ToInt32(br.BaseStream.Length)), true);
            br.Close();

            bool done = true;

            for (int i = 0; i < 30; i++)
            {
                ms.Seek(tst[i].offset, SeekOrigin.Begin);
                byte w = (byte)ms.ReadByte();
                byte l = (byte)ms.ReadByte();
                uint total = Convert.ToUInt32(w + l);
                if (total < gamesInSeason)
                {
                    done = false;
                    break;
                }
            }

            if (done == true)
            {
                if (ptFile == "")
                {
                    pt = null;
                    pt = new PlayoffTree();
                    playoffTreeW ptW = new playoffTreeW();
                    ptW.ShowDialog();

                    if (!pt.done) return -1;

                    SaveFileDialog spt = new SaveFileDialog();
                    spt.Title = "Please select a file to save the Playoff Tree to...";
                    spt.InitialDirectory = AppDocsPath;
                    spt.Filter = "Playoff Tree files (*.ptr)|*.ptr";
                    spt.ShowDialog();

                    if (spt.FileName == "") return -1;

                    ptFile = spt.FileName;

                    try
                    {
                        FileStream stream = File.Open(spt.FileName, FileMode.Create);
                        BinaryFormatter bf = new BinaryFormatter();

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
                    BinaryFormatter bf = new BinaryFormatter();

                    pt = (PlayoffTree)bf.Deserialize(stream);
                    stream.Close();
                }
            }

            StreamWriter sw2 = new StreamWriter(SettingsFile, false);
            sw2.WriteLine("{0}\t{1}\t{2}", fn, gamesInSeason, ptFile);
            sw2.Close();

            if (done) return 1;
            else return 0;
        }

        private void prepareOffsets(string fn)
        {
            ext = getExtension(fn);
            if (ext == "FXG" || ext == "RFG")
            {
                tst[0].offset = 3240532;
            }
            else if (ext == "CMG")
            {
                tst[0].offset = 5722996;
            }
            else if (ext == "PMG")
            {
                tst[TeamNames[pt.teams[0]]].offset = 1813028;
            }

            if (ext != "PMG")
            {
                for (int i = 1; i < 30; i++)
                {
                    tst[i].offset = tst[i - 1].offset + 40;
                }
                int inPlayoffs = checkIfIntoPlayoffs(fn);
                if (inPlayoffs == 1)
                {
                    tst[TeamNames[pt.teams[0]]].offset = tst[0].offset - 1440;
                    for (int i = 1; i < 16; i++)
                    {
                        tst[TeamNames[pt.teams[i]]].offset = tst[TeamNames[pt.teams[i - 1]]].offset + 40;
                    }
                }
                else if (inPlayoffs == -1) return;
            }
            else
            {
                for (int i = 1; i < 16; i++)
                {
                    tst[TeamNames[pt.teams[i]]].offset = tst[TeamNames[pt.teams[i-1]]].offset + 40;
                }
            }
        }

        private void btnCRC_Click(object sender, RoutedEventArgs e)
        {
            String hash = getCRC(txtFile.Text);

            MessageBox.Show(hash);
        }

        private String getCRC(string filename)
        {
            Crc32 crc32 = new Crc32();
            String hash = String.Empty;

            using (FileStream fs = File.Open(filename, FileMode.Open))
                foreach (byte b in crc32.ComputeHash(fs))
                    hash += b.ToString("x2").ToLower();
            return hash;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            saveTeamStats(txtFile.Text);
        }

        private void saveTeamStats(string fn)
        {
            BinaryReader br = new BinaryReader(File.OpenRead(fn));
            MemoryStream ms = new MemoryStream(br.ReadBytes(Convert.ToInt32(br.BaseStream.Length)), true);

            if ((pt != null) && (pt.teams[0] != "Invalid"))
            {
                for (int i = 0; i < 16; i++)
                {
                    ms.Seek(tst[TeamNames[pt.teams[i]]].offset, SeekOrigin.Begin);
                    ms.Write(tst[TeamNames[pt.teams[i]]].winloss, 0, 2);
                    for (int j = 0; j < 18; j++)
                    {
                        ms.Write(BitConverter.GetBytes(tst[TeamNames[pt.teams[i]]].stats[j]), 0, 2);
                    }
                }
            }
            else
            {
                for (int i = 0; i < 30; i++)
                {
                    ms.Seek(tst[i].offset, SeekOrigin.Begin);
                    ms.Write(tst[i].winloss, 0, 2);
                    for (int j = 0; j < 18; j++)
                    {
                        ms.Write(BitConverter.GetBytes(tst[i].stats[j]), 0, 2);
                    }
                }
            }

            BinaryWriter bw = new BinaryWriter(File.OpenWrite(AppTempPath + getSafeFilename(fn)));
            ms.Position = 4;
            byte[] t = new byte[1048576];
            int count;
            do
            {
                count = ms.Read(t, 0, 1048576);
                bw.Write(t, 0, count);
            } while (count > 0);

            br.Close();
            bw.Close();

            byte[] crc = ReverseByteOrder(StringToByteArray(getCRC(AppTempPath + getSafeFilename(fn))), 4);

            try
            {
                File.Delete(fn + ".bak");
            }
            catch
            {
            }
            File.Move(fn, fn + ".bak");
            BinaryReader br2 = new BinaryReader(File.OpenRead(AppTempPath + getSafeFilename(fn)));
            BinaryWriter bw2 = new BinaryWriter(File.OpenWrite(fn));
            bw2.Write(crc);
            do
            {
                t = br2.ReadBytes(1048576);
                bw2.Write(t);
            } while (t.Length > 0);
            br2.Close();
            bw2.Close();

            File.Delete(AppTempPath + getSafeFilename(fn));
        }

        public static byte[] ReverseByteOrder(byte[] original, int length)
        {
            byte[] newArr = new byte[length];
            for (int i = 0; i < length; i++)
            {
                newArr[length - i - 1] = original[i];
            }
            return newArr;
        }

        public static string getSafeFilename(string f)
        {
            string[] parts = f.Split('\\');
            string curName = parts[parts.Length - 1];
            return curName;
        }

        public static byte[] StringToByteArray(String hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        private void cmbTeam1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                string team = cmbTeam1.SelectedItem.ToString();
                txtW1.Text = tst[TeamNames[team]].winloss[0].ToString();
                txtL1.Text = tst[TeamNames[team]].winloss[1].ToString();
                txtPF1.Text = tst[TeamNames[team]].stats[PF].ToString();
                txtPA1.Text = tst[TeamNames[team]].stats[PA].ToString();
                txtFGM1.Text = tst[TeamNames[team]].stats[FGM].ToString();
                txtFGA1.Text = tst[TeamNames[team]].stats[FGA].ToString();
                txt3PM1.Text = tst[TeamNames[team]].stats[TPM].ToString();
                txt3PA1.Text = tst[TeamNames[team]].stats[TPA].ToString();
                txtFTM1.Text = tst[TeamNames[team]].stats[FTM].ToString();
                txtFTA1.Text = tst[TeamNames[team]].stats[FTA].ToString();
                txtOREB1.Text = tst[TeamNames[team]].stats[OREB].ToString();
                txtDREB1.Text = tst[TeamNames[team]].stats[DREB].ToString();
                txtSTL1.Text = tst[TeamNames[team]].stats[STL].ToString();
                txtTO1.Text = tst[TeamNames[team]].stats[TO].ToString();
                txtBLK1.Text = tst[TeamNames[team]].stats[BLK].ToString();
                txtAST1.Text = tst[TeamNames[team]].stats[AST].ToString();
                txtFOUL1.Text = tst[TeamNames[team]].stats[FOUL].ToString();
            }
            catch
            { }
        }

        private void btnSaveTS_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Team Stats Table (*.tst)|*.tst";
            sfd.InitialDirectory = AppDocsPath;
            sfd.ShowDialog();

            if (sfd.FileName == "") return;

            try
            {
                FileStream stream = File.Open(sfd.FileName, FileMode.Create);
                BinaryFormatter bf = new BinaryFormatter();

                for (int i = 0; i < 30; i++)
                    bf.Serialize(stream, tst[i]);

                if (pt != null)
                {
                    bf.Serialize(stream, pt);
                }
                else
                {
                    pt = new PlayoffTree();
                    pt.teams[0] = "Invalid";
                    bf.Serialize(stream, pt);
                    pt = null;
                }
                stream.Close();
            }
            catch (Exception ex)
            {
                App.errorReport(ex, "Trying to save team stats");
            }
        }

        private void btnLoadUpdate_Click(object sender, RoutedEventArgs e)
        {
            TeamStats[] temptst = new TeamStats[30];
            bool havePT = false;

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Team Stats Table (*.tst)|*.tst";
            ofd.InitialDirectory = AppDocsPath;
            ofd.Title = "Please select the TST file that you saved before the game...";
            ofd.ShowDialog();

            if (ofd.FileName == "") return;

            FileStream stream = File.Open(ofd.FileName, FileMode.Open);
            BinaryFormatter bf = new BinaryFormatter();

            for (int i = 0; i < 30; i++)
            {
                temptst[i] = new TeamStats();
                temptst[i] = (TeamStats)bf.Deserialize(stream);
            }
            pt = (PlayoffTree)bf.Deserialize(stream);
            stream.Close();

            bs = new BoxScore();
            boxScoreW bsW = new boxScoreW();
            bsW.ShowDialog();

            if (bs.done == false) return;

            int id1 = -1;
            int id2 = -1;

            if (pt.teams[0] == "Invalid")
            {
                id1 = TeamNames[bs.Team1];
                id2 = TeamNames[bs.Team2];
                havePT = false;
            }
            else
            {
                for (int i = 0; i < 16; i++)
                {
                    if (pt.teams[i] == bs.Team1)
                        id1 = TeamNames[pt.teams[i]];
                    else if (pt.teams[i] == bs.Team2)
                        id2 = TeamNames[pt.teams[i]];
                }
                havePT = true;
            }

            // Add win & loss
            if (bs.PTS1 > bs.PTS2)
            {
                temptst[id1].winloss[0]++;
                temptst[id2].winloss[1]++;
            }
            else
            {
                temptst[id1].winloss[1]++;
                temptst[id2].winloss[0]++;
            }
            // Add minutes played
            temptst[id1].stats[M] += 48;
            temptst[id2].stats[M] += 48;

            // Add Points For
            temptst[id1].stats[PF] += bs.PTS1;
            temptst[id2].stats[PF] += bs.PTS2;

            // Add Points Against
            temptst[id1].stats[PA] += bs.PTS2;
            temptst[id2].stats[PA] += bs.PTS1;

            //
            temptst[id1].stats[FGM] += bs.FGM1;
            temptst[id2].stats[FGM] += bs.FGM2;

            temptst[id1].stats[FGA] += bs.FGA1;
            temptst[id2].stats[FGA] += bs.FGA2;

            //
            temptst[id1].stats[TPM] += bs.TPM1;
            temptst[id2].stats[TPM] += bs.TPM2;

            //
            temptst[id1].stats[TPA] += bs.TPA1;
            temptst[id2].stats[TPA] += bs.TPA2;

            //
            temptst[id1].stats[FTM] += bs.FTM1;
            temptst[id2].stats[FTM] += bs.FTM2;

            //
            temptst[id1].stats[FTA] += bs.FTA1;
            temptst[id2].stats[FTA] += bs.FTA2;

            //
            temptst[id1].stats[OREB] += bs.OFF1;
            temptst[id2].stats[OREB] += bs.OFF2;

            //
            temptst[id1].stats[DREB] += Convert.ToUInt16(bs.REB1 - bs.OFF1);
            temptst[id2].stats[DREB] += Convert.ToUInt16(bs.REB2 - bs.OFF2);

            //
            temptst[id1].stats[STL] += bs.STL1;
            temptst[id2].stats[STL] += bs.STL2;

            //
            temptst[id1].stats[TO] += bs.TO1;
            temptst[id2].stats[TO] += bs.TO2;

            //
            temptst[id1].stats[BLK] += bs.BLK1;
            temptst[id2].stats[BLK] += bs.BLK2;

            //
            temptst[id1].stats[AST] += bs.AST1;
            temptst[id2].stats[AST] += bs.AST2;

            //
            temptst[id1].stats[FOUL] += bs.PF1;
            temptst[id2].stats[FOUL] += bs.PF2;

            ofd = new OpenFileDialog();
            ofd.Title = "Please select the Career file you want to update...";
            ofd.Filter = "All NBA 2K12 Career Files (*.FXG; *.CMG; *.RFG; *.PMG; *.SMG)|*.FXG;*.CMG;*.RFG;*.PMG;*.SMG|"
            + "Association files (*.FXG)|*.FXG|My Player files (*.CMG)|*.CMG|Season files (*.RFG)|*.RFG|Playoff files (*.PMG)|*.PMG|" +
                "Create A Legend files (*.SMG)|*.SMG";
            if (Directory.Exists(SavesPath)) ofd.InitialDirectory = SavesPath;
            ofd.ShowDialog();

            if (ofd.FileName == "") return;
            string fn = ofd.FileName;
            GetStats(fn, havePT);

            tst[id1].winloss = temptst[id1].winloss;
            tst[id2].winloss = temptst[id2].winloss;
            tst[id1].stats = temptst[id1].stats;
            tst[id2].stats = temptst[id2].stats;

            saveTeamStats(fn);

            cmbTeam1.SelectedIndex = 0;
            txtFile.Text = ofd.FileName;

            MessageBox.Show("Team Stats updated in " + getSafeFilename(fn) + " succesfully!");
        }

        private static void checkForUpdates()
        {
            try
            {
                WebClient webClient = new WebClient();
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
                //webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
                webClient.DownloadFileAsync(new Uri("http://students.ceid.upatras.gr/~aslanoglou/ctsversion.txt"), AppDocsPath + @"ctsversion.txt");
            }
            catch (Exception ex)
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
            string[] updateInfo = File.ReadAllLines(AppDocsPath + @"ctsversion.txt");
            string[] versionParts = updateInfo[0].Split('.');
            string[] curVersionParts = Assembly.GetExecutingAssembly().GetName().Version.ToString().Split('.');
            int[] iVP = new int[versionParts.Length];
            int[] iCVP = new int[versionParts.Length];
            bool found = false;
            for (int i = 0; i < versionParts.Length; i++)
            {
                iVP[i] = Convert.ToInt32(versionParts[i]);
                iCVP[i] = Convert.ToInt32(curVersionParts[i]);
                if (iCVP[i] > iVP[i]) break;
                if (iVP[i] > iCVP[i])
                {
                    MessageBoxResult mbr = MessageBox.Show("A new version is available! Would you like to download it?", "NBA 2K12 Correct Team Stats", MessageBoxButton.YesNo, MessageBoxImage.Information);
                    found = true;
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
            string safefn = getSafeFilename(txtFile.Text);
            string SettingsFile = AppDocsPath + safefn + ".cfg";
            if (File.Exists(SettingsFile)) File.Delete(SettingsFile);
        }

        private void btnShowAvg_Click(object sender, RoutedEventArgs e)
        {
            int id = TeamNames[cmbTeam1.SelectedItem.ToString()];
            tst[id].calcAvg();
            string text = String.Format("PPG: {0:F2}\nPAPG: {1:F2}\nFG%: {2:F3}\nFGeff: {3:F1}", tst[id].averages[PPG], tst[id].averages[PAPG], tst[id].averages[FGp],
                tst[id].averages[FGeff]);
            MessageBox.Show(text);
        }
    }

    [Serializable()]
    public class TeamStats : ISerializable
    {
        public Int32 offset = 0;
        public const int M = 0, PF = 1, PA = 2, FGM = 4, FGA = 5, TPM = 6, TPA = 7,
            FTM = 8, FTA = 9, OREB = 10, DREB = 11, STL = 12, TO = 13, BLK = 14, AST = 15,
            FOUL = 16;
        public const int PPG = 0, PAPG = 1, FGp = 2, FGeff = 3, TPp = 4, TPeff = 5,
            FTp = 6, FTeff = 7, RPG = 8, ORPG = 9, DRPG = 10, SPG = 11, BPG = 12,
            TPG = 13, APG = 14, FPG = 15;
        /// <summary>
        /// Stats for each team.
        /// 0: M, 1: PF, 2: PA, 3: 0x0000, 4: FGM, 5: FGA, 6: 3PM, 7: 3PA, 8: FTM, 9: FTA,
        /// 10: OREB, 11: DREB, 12: STL, 13: TO, 14: BLK, 15: AST,
        /// 16: FOUL
        /// </summary>
        public UInt16[] stats = new UInt16[18];
        /// <summary>
        /// Averages for each team.
        /// 0: PPG, 1: PAPG, 2: FG%, 3: FGEff, 4: 3P%, 5: 3PEff, 6: FT%, 7:FTEff,
        /// 8: RPG, 9: ORPG, 10: DRPG, 11: SPG, 12: BPG, 13: TPG, 14: APG, 15: FPG
        /// </summary>
        public float[] averages = new float[16];
        public byte[] winloss = new byte[2];

        public TeamStats()
        {
        }

        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("stats", stats);
            info.AddValue("winloss", winloss);
        }

        public TeamStats(SerializationInfo info, StreamingContext ctxt)
        {
            stats = (UInt16[])info.GetValue("stats", typeof(UInt16[]));
            winloss = (byte[])info.GetValue("winloss", typeof(byte[]));
        }

        public void calcAvg()
        {
            int games = winloss[0] + winloss[1];
            averages[PPG] = (float)stats[PF] / games;
            averages[PAPG] = (float)stats[PA] / games;
            averages[FGp] = (float)stats[FGM] / stats[FGA];
            averages[FGeff] = averages[FGp] * ((float)stats[FGM] / games);
            averages[TPp] = (float)stats[TPM] / stats[TPA];
            averages[TPeff] = averages[TPp] * ((float)stats[TPM] / games);
            averages[FTp] = (float)stats[FTM] / stats[FTA];
            averages[FTeff] = averages[FTp] * ((float)stats[FTM] / games);
            averages[RPG] = (float)(stats[OREB] + stats[DREB]) / games;
            averages[ORPG] = (float)stats[OREB] / games;
            averages[DRPG] = (float)stats[DREB] / games;
            averages[SPG] = (float)stats[STL] / games;
            averages[BPG] = (float)stats[BLK] / games;
            averages[TPG] = (float)stats[TO] / games;
            averages[APG] = (float)stats[AST] / games;
            averages[FPG] = (float)stats[FOUL] / games;
        }
    }

    public class BoxScore
    {
        public string Team1;
        public string Team2;
        public UInt16 PTS1, REB1, AST1, STL1, BLK1, TO1, FGM1, FGA1, TPM1, TPA1, FTM1, FTA1, OFF1, PF1;
        public UInt16 PTS2, REB2, AST2, STL2, BLK2, TO2, FGM2, FGA2, TPM2, TPA2, FTM2, FTA2, OFF2, PF2;
        public bool done = false;
    }

    [Serializable()]
    public class PlayoffTree : ISerializable
    {
        public string[] teams = new string[16];
        public bool done = false;

        public PlayoffTree() { }

        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("teams", teams);
            info.AddValue("done", done);
        }

        public PlayoffTree(SerializationInfo info, StreamingContext ctxt)
        {
            teams = (string[])info.GetValue("teams", typeof(string[]));
            done = (bool)info.GetValue("done", typeof(bool));
        }
    }
}
