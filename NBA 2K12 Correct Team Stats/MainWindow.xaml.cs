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
using System.Globalization;

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
        public static bool isCustom = false;

        public const int M = 0, PF = 1, PA = 2, FGM = 4, FGA = 5, TPM = 6, TPA = 7,
            FTM = 8, FTA = 9, OREB = 10, DREB = 11, STL = 12, TO = 13, BLK = 14, AST = 15,
            FOUL = 16;
        public const int PPG = 0, PAPG = 1, FGp = 2, FGeff = 3, TPp = 4, TPeff = 5,
            FTp = 6, FTeff = 7, RPG = 8, ORPG = 9, DRPG = 10, SPG = 11, BPG = 12,
            TPG = 13, APG = 14, FPG = 15, Wp = 16, Weff = 17;

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

        public static SortedDictionary<string, int> TeamNames; 

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

            System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-US");

            btnSave.Visibility = Visibility.Hidden;
            btnCRC.Visibility = Visibility.Hidden;
            btnSaveCustomTeam.Visibility = Visibility.Hidden;
            btnInject.Visibility = Visibility.Hidden;

            if (Directory.Exists(AppDocsPath) == false) Directory.CreateDirectory(AppDocsPath);
            if (Directory.Exists(AppTempPath) == false) Directory.CreateDirectory(AppTempPath);

            for (int i = 0; i < 30; i++)
            {
                tst[i] = new TeamStats();
            }

            setRealTeamNames();

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

        private static void setRealTeamNames()
        {
            TeamNames = new SortedDictionary<string, int>
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

            TeamStats[] temp = GetStats(txtFile.Text);
            if (temp.Length > 1) tst = temp;

            isCustom = false;
            setRealTeamNames();

            cmbTeam1.SelectedIndex = 0; 
            txtFile.ScrollToHorizontalOffset(txtFile.GetRectFromCharacterIndex(txtFile.Text.Length).Right);
        }

        private TeamStats[] GetStats(string fn, bool havePT = false)
        {
            TeamStats[] _teamStats = new TeamStats[30];
            for (int i = 0; i < 30; i++)
            {
                _teamStats[i] = new TeamStats();
            }
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
                        if (!pt.done) return new TeamStats[1];

                        SaveFileDialog spt = new SaveFileDialog();
                        spt.Title = "Please select a file to save the Playoff Tree to...";
                        spt.InitialDirectory = AppDocsPath;
                        spt.Filter = "Playoff Tree files (*.ptr)|*.ptr";
                        spt.ShowDialog();

                        if (spt.FileName == "") return new TeamStats[1];

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

                        if (ofd.FileName == "") return new TeamStats[1];

                        FileStream stream = File.Open(ofd.FileName, FileMode.Open);
                        BinaryFormatter bf = new BinaryFormatter();

                        pt = (PlayoffTree)bf.Deserialize(stream);
                        stream.Close();
                    }
                    else return new TeamStats[1];
                }
            }
            prepareOffsets(fn, _teamStats);

            BinaryReader br = new BinaryReader(File.OpenRead(fn));
            MemoryStream ms = new MemoryStream(br.ReadBytes(Convert.ToInt32(br.BaseStream.Length)), true);
            br.Close();
            byte[] buf = new byte[2];

            if ((pt == null) || (pt.teams[0] == "Invalid"))
            {
                cmbTeam1.Items.Clear();
                foreach (KeyValuePair<string, int> kvp in TeamNames)
                {
                    cmbTeam1.Items.Add(kvp.Key);
                    _teamStats[kvp.Value].name = kvp.Key;
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
            }
            else
            {
                cmbTeam1.Items.Clear();
                List<string> newteams = new List<string>();
                for (int i = 0; i < 16; i++)
                {
                    newteams.Add(pt.teams[i]);
                    int id = TeamNames[pt.teams[i]];
                    ms.Seek(_teamStats[id].offset, SeekOrigin.Begin);
                    ms.Read(buf, 0, 2);
                    _teamStats[id].name = pt.teams[i];
                    _teamStats[id].winloss[0] = buf[0];
                    _teamStats[id].winloss[1] = buf[1];
                    for (int j = 0; j < 18; j++)
                    {
                        ms.Read(buf, 0, 2);
                        _teamStats[id].stats[j] = BitConverter.ToUInt16(buf, 0);
                    }
                }
                newteams.Sort();
                foreach (string team in newteams)
                    cmbTeam1.Items.Add(team);
            }
            int temp;
            for (int i = 0; i < 30; i++)
            {
                temp = _teamStats[i].calcAvg();
            }

            return _teamStats;
        }

        private string getExtension(string fn)
        {
            string[] parts = fn.Split('.');
            return parts[parts.Length - 1];
        }

        private int checkIfIntoPlayoffs(string fn, TeamStats[] _teamStats)
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
                ms.Seek(_teamStats[i].offset, SeekOrigin.Begin);
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

        private void prepareOffsets(string fn, TeamStats[] _teamStats)
        {
            ext = getExtension(fn);
            if (ext == "FXG" || ext == "RFG")
            {
                _teamStats[0].offset = 3240532;
            }
            else if (ext == "CMG")
            {
                _teamStats[0].offset = 5722996;
            }
            else if (ext == "PMG")
            {
                _teamStats[TeamNames[pt.teams[0]]].offset = 1813028;
            }

            if (ext != "PMG")
            {
                for (int i = 1; i < 30; i++)
                {
                    _teamStats[i].offset = _teamStats[i - 1].offset + 40;
                }
                int inPlayoffs = checkIfIntoPlayoffs(fn, _teamStats);
                if (inPlayoffs == 1)
                {
                    _teamStats[TeamNames[pt.teams[0]]].offset = _teamStats[0].offset - 1440;
                    for (int i = 1; i < 16; i++)
                    {
                        _teamStats[TeamNames[pt.teams[i]]].offset = _teamStats[TeamNames[pt.teams[i - 1]]].offset + 40;
                    }
                }
                else if (inPlayoffs == -1) return;
            }
            else
            {
                for (int i = 1; i < 16; i++)
                {
                    _teamStats[TeamNames[pt.teams[i]]].offset = _teamStats[TeamNames[pt.teams[i-1]]].offset + 40;
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
                int id = TeamNames[team];
                txtW1.Text = tst[id].winloss[0].ToString();
                txtL1.Text = tst[id].winloss[1].ToString();
                txtPF1.Text = tst[id].stats[PF].ToString();
                txtPA1.Text = tst[id].stats[PA].ToString();
                txtFGM1.Text = tst[id].stats[FGM].ToString();
                txtFGA1.Text = tst[id].stats[FGA].ToString();
                txt3PM1.Text = tst[id].stats[TPM].ToString();
                txt3PA1.Text = tst[id].stats[TPA].ToString();
                txtFTM1.Text = tst[id].stats[FTM].ToString();
                txtFTA1.Text = tst[id].stats[FTA].ToString();
                txtOREB1.Text = tst[id].stats[OREB].ToString();
                txtDREB1.Text = tst[id].stats[DREB].ToString();
                txtSTL1.Text = tst[id].stats[STL].ToString();
                txtTO1.Text = tst[id].stats[TO].ToString();
                txtBLK1.Text = tst[id].stats[BLK].ToString();
                txtAST1.Text = tst[id].stats[AST].ToString();
                txtFOUL1.Text = tst[id].stats[FOUL].ToString();
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

        private void mnuFileOpenCustom_Click(object sender, RoutedEventArgs e)
        {
            tst = new TeamStats[30];
            TeamNames = new SortedDictionary<string,int>();
            bool havePT = false;

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Team Stats Table (*.tst)|*.tst";
            ofd.InitialDirectory = AppDocsPath;
            ofd.Title = "Please select the TST file that you want to edit...";
            ofd.ShowDialog();

            if (ofd.FileName == "") return;

            FileStream stream = File.Open(ofd.FileName, FileMode.Open);
            BinaryFormatter bf = new BinaryFormatter();

            for (int i = 0; i < 30; i++)
            {
                tst[i] = new TeamStats();
                tst[i] = (TeamStats)bf.Deserialize(stream);
                if (tst[i].name == "") continue;
                try
                {
                    TeamNames.Add(tst[i].name, i);
                    tst[i].calcAvg();
                }
                catch
                { }
            }

            cmbTeam1.Items.Clear();
            foreach (KeyValuePair<string, int> kvp in TeamNames)
            {
                cmbTeam1.Items.Add(kvp.Key);
            }
            pt = (PlayoffTree)bf.Deserialize(stream);
            stream.Close();

            isCustom = true;
            prepareWindow(isCustom);

            cmbTeam1.SelectedIndex = -1;
            cmbTeam1.SelectedIndex = 0;
        }

        private void prepareWindow(bool isCustom)
        {
            if (isCustom)
            {
                txt3PA1.IsReadOnly = false;
                txt3PM1.IsReadOnly = false;
                txtAST1.IsReadOnly = false;
                txtBLK1.IsReadOnly = false;
                txtDREB1.IsReadOnly = false;
                txtFGA1.IsReadOnly = false;
                txtFGM1.IsReadOnly = false;
                txtFOUL1.IsReadOnly = false;
                txtFTA1.IsReadOnly = false;
                txtFTM1.IsReadOnly = false;
                txtL1.IsReadOnly = false;
                txtOREB1.IsReadOnly = false;
                txtPA1.IsReadOnly = false;
                txtPF1.IsReadOnly = false;
                txtSTL1.IsReadOnly = false;
                txtTO1.IsReadOnly = false;
                txtW1.IsReadOnly = false;
                btnSaveTS.Content = "Save To Disk";
                btnLoadUpdate.Content = "Update with new Box Score";
                btnSaveCustomTeam.Visibility = Visibility.Visible;
                btnInject.Visibility = Visibility.Visible;
            }
            else
            {
                txt3PA1.IsReadOnly = true;
                txt3PM1.IsReadOnly = true;
                txtAST1.IsReadOnly = true;
                txtBLK1.IsReadOnly = true;
                txtDREB1.IsReadOnly = true;
                txtFGA1.IsReadOnly = true;
                txtFGM1.IsReadOnly = true;
                txtFOUL1.IsReadOnly = true;
                txtFTA1.IsReadOnly = true;
                txtFTM1.IsReadOnly = true;
                txtL1.IsReadOnly = true;
                txtOREB1.IsReadOnly = true;
                txtPA1.IsReadOnly = true;
                txtPF1.IsReadOnly = true;
                txtSTL1.IsReadOnly = true;
                txtTO1.IsReadOnly = true;
                txtW1.IsReadOnly = true;
                btnSaveTS.Content = "Save Team Stats";
                btnLoadUpdate.Content = "Load & Update Team Stats";
                btnSaveCustomTeam.Visibility = Visibility.Hidden;
                btnInject.Visibility = Visibility.Hidden;
            }
        }

        private void btnLoadUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (!isCustom)
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
                TeamStats[] temp = GetStats(fn, havePT);
                if (temp.Length == 1)
                {
                    MessageBox.Show("Couldn't get stats from " + getSafeFilename(fn) + ". Update failed.");
                    return;
                }

                // Check if Win/Loss remain the same
                if ((temp[id1].winloss != temptst[id1].winloss) || (temp[id2].winloss != temptst[id2].winloss))
                {
                    MessageBoxResult r = MessageBox.Show("Your updates to the saved team stats don't seem to be compatible with the save you've selected.\n" +
                        "Making these updates would mean that the Wins/Losses stats would be different than what NBA 2K12 has saved inside the file.\n\n" +
                        "Probable causes:\n\t1. You didn't save your Association and then the team stats in the tool right before the game started.\n" +
                        "\t2. You didn't save your Association right after the game ended.\n\n" +
                        "Make sure you're using the saved Team Stats from right before the game, and an Association save from right after the game ended.\n\n" +
                        "You can continue, but this may cause stat corruption.\nAre you sure you want to continue?",
                        "NBA 2K12 Correct Team Stats", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No);
                    if (r == MessageBoxResult.No) return;
                }

                tst = temp;
                tst[id1].winloss = temptst[id1].winloss;
                tst[id2].winloss = temptst[id2].winloss;
                tst[id1].stats = temptst[id1].stats;
                tst[id2].stats = temptst[id2].stats;

                saveTeamStats(fn);

                cmbTeam1.SelectedIndex = -1;
                cmbTeam1.SelectedIndex = 0;
                txtFile.Text = ofd.FileName;

                MessageBox.Show("Team Stats updated in " + getSafeFilename(fn) + " succesfully!");
            }
            else
            {
                bs = new BoxScore();
                boxScoreW bsW = new boxScoreW();
                bsW.ShowDialog();

                if (bs.done == false) return;

                int id1 = -1;
                int id2 = -1;
                
                id1 = TeamNames[bs.Team1];
                id2 = TeamNames[bs.Team2]; 

                // Add win & loss
                if (bs.PTS1 > bs.PTS2)
                {
                    tst[id1].winloss[0]++;
                    tst[id2].winloss[1]++;
                }
                else
                {
                    tst[id1].winloss[1]++;
                    tst[id2].winloss[0]++;
                }
                // Add minutes played
                tst[id1].stats[M] += 48;
                tst[id2].stats[M] += 48;

                // Add Points For
                tst[id1].stats[PF] += bs.PTS1;
                tst[id2].stats[PF] += bs.PTS2;

                // Add Points Against
                tst[id1].stats[PA] += bs.PTS2;
                tst[id2].stats[PA] += bs.PTS1;

                //
                tst[id1].stats[FGM] += bs.FGM1;
                tst[id2].stats[FGM] += bs.FGM2;

                tst[id1].stats[FGA] += bs.FGA1;
                tst[id2].stats[FGA] += bs.FGA2;

                //
                tst[id1].stats[TPM] += bs.TPM1;
                tst[id2].stats[TPM] += bs.TPM2;

                //
                tst[id1].stats[TPA] += bs.TPA1;
                tst[id2].stats[TPA] += bs.TPA2;

                //
                tst[id1].stats[FTM] += bs.FTM1;
                tst[id2].stats[FTM] += bs.FTM2;

                //
                tst[id1].stats[FTA] += bs.FTA1;
                tst[id2].stats[FTA] += bs.FTA2;

                //
                tst[id1].stats[OREB] += bs.OFF1;
                tst[id2].stats[OREB] += bs.OFF2;

                //
                tst[id1].stats[DREB] += Convert.ToUInt16(bs.REB1 - bs.OFF1);
                tst[id2].stats[DREB] += Convert.ToUInt16(bs.REB2 - bs.OFF2);

                //
                tst[id1].stats[STL] += bs.STL1;
                tst[id2].stats[STL] += bs.STL2;

                //
                tst[id1].stats[TO] += bs.TO1;
                tst[id2].stats[TO] += bs.TO2;

                //
                tst[id1].stats[BLK] += bs.BLK1;
                tst[id2].stats[BLK] += bs.BLK2;

                //
                tst[id1].stats[AST] += bs.AST1;
                tst[id2].stats[AST] += bs.AST2;

                //
                tst[id1].stats[FOUL] += bs.PF1;
                tst[id2].stats[FOUL] += bs.PF2;

                tst[id1].calcAvg();
                tst[id2].calcAvg();

                cmbTeam1.SelectedIndex = -1;
                cmbTeam1.SelectedIndex = 0;
            }
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
            int id = -1;
            try
            {
                id = TeamNames[cmbTeam1.SelectedItem.ToString()];
            }
            catch
            {
                return;
            }
            int[][] rating = calculateRankings(tst);
            string text = String.Format("Win %: {32:F3} ({33})\nWin eff: {34:F1} ({35})\n\nPPG: {0:F2} ({16})\nPAPG: {1:F2} ({17})\n\nFG%: {2:F3} ({18})\nFGeff: {3:F1} ({19})\n3P%: {4:F3} ({20})\n3Peff: {5:F1} ({21})\n"
                    + "FT%: {6:F3} ({22})\nFTeff: {7:F1} ({23})\n\nRPG: {8:F1} ({24})\nORPG: {9:F1} ({25})\nDRPG: {10:F1} ({26})\n\nSPG: {11:F1} ({27})\nBPG: {12:F1} ({28})\n"
                    + "TPG: {13:F1} ({29})\nAPG: {14:F1} ({30})\nFPG: {15:F1} ({31})",
                    tst[id].averages[PPG], tst[id].averages[PAPG], tst[id].averages[FGp],
                    tst[id].averages[FGeff], tst[id].averages[TPp], tst[id].averages[TPeff],
                    tst[id].averages[FTp], tst[id].averages[FTeff], tst[id].averages[RPG], tst[id].averages[ORPG], tst[id].averages[DRPG], tst[id].averages[SPG],
                    tst[id].averages[BPG], tst[id].averages[TPG], tst[id].averages[APG], tst[id].averages[FPG],
                    rating[id][0], 31 - rating[id][1], rating[id][2], rating[id][3], rating[id][4], rating[id][5], rating[id][6], rating[id][7], rating[id][8], rating[id][9],
                    rating[id][10], rating[id][11], rating[id][12], 31 - rating[id][13], rating[id][14], 31 - rating[id][15], tst[id].averages[Wp], rating[id][16], tst[id].averages[Weff], rating[id][Weff]);
            MessageBox.Show(text, cmbTeam1.SelectedItem.ToString());
        }

        private int[][] calculateRankings(TeamStats[] _teamStats)
        {
            int[][] rating = new int[30][];
            for (int i = 0; i < 30; i++)
            {
                rating[i] = new int[19];
            }
            for (int k = 0; k < 30; k++)
            {
                for (int i = 0; i < 18; i++)
                {
                    rating[k][i] = 1;
                    for (int j = 0; j < 30; j++)
                    {
                        if (j != k)
                        {
                            if (_teamStats[j].averages[i] > _teamStats[k].averages[i])
                            {
                                rating[k][i]++;
                            }
                        }
                    }
                }
                rating[k][18] = _teamStats[k].getGames();
            }
            return rating;
        }

        private void scoutReport(int[][] rating, int teamID)
        {
            //public const int PPG = 0, PAPG = 1, FGp = 2, FGeff = 3, TPp = 4, TPeff = 5,
            //FTp = 6, FTeff = 7, RPG = 8, ORPG = 9, DRPG = 10, SPG = 11, BPG = 12,
            //TPG = 13, APG = 14, FPG = 15, Wp = 16, Weff = 17;
            string msg;
            msg = String.Format("{0}, the {1}", cmbTeam1.SelectedItem.ToString(), rating[teamID][17]);
            switch (rating[teamID][17])
            {
                case 1:
                case 21:
                    msg += "st";
                    break;
                case 2:
                case 22:
                    msg += "nd";
                    break;
                case 3:
                case 23:
                    msg += "rd";
                    break;
                default:
                    msg += "th";
                    break;
            }
            msg += " strongest team in the league right now, after having played " + rating[teamID][18].ToString() + " games.\n\n";

            if ((rating[teamID][3] <= 5) && (rating[teamID][5] <= 5))
            {
                if (rating[teamID][7] <= 5)
                {
                    msg += "This team just can't be beaten offensively. One of the strongest in the league in all aspects.";
                }
                else
                {
                    msg += "Great team offensively. Even when they don't get to the line, they know how to raise the bar with "
                        + "efficiency in both 2 and 3 pointers.";
                }
            }
            else if ((rating[teamID][3] <= 10) && (rating[teamID][5] <= 10))
            {
                if (rating[teamID][7] <= 10)
                {
                    msg += "Top 10 in the league in everything offense, and they're one to worry about.";
                }
                else
                {
                    msg += "Although their free throwing is not on par with their other offensive qualities, you can't relax "
                        + "when playing against them. Top 10 in field goals and 3 pointers.";
                }
            }
            else if ((rating[teamID][3] <= 20) && (rating[teamID][5] <= 20))
            {
                if (rating[teamID][7] <= 10)
                {
                    msg += "Although an average offensive team (their field goal efficiency isn't that high in or out of the "
                        + "arc), they can get back at you with their efficiency from the line.";
                }
                else
                {
                    msg += "Average offensive team. Not really efficient in anything they do when they bring the ball down "
                        + "the court.";
                }
            }
            else
            {
                if (rating[teamID][7] <= 10)
                {
                    msg += "They aren't consistent from the floor, but still manage to get to the line enough times and "
                        + "be good enough to make a difference.";
                }
                else
                {
                    msg += "One of the most inconsistent teams at the offensive end, and they aren't efficient enough from "
                        +"the line to make up for it.";
                }
            }
            msg += "\n\n";

            if (rating[teamID][3] <= 5)
                msg += "Top scoring team, one of the top 5 in field goal efficiency.";
            else if (rating[teamID][3] <= 10)
                msg += "You'll have to worry about their scoring efficiency, as they're one of the Top 10 in the league.";
            else if (rating[teamID][3] <= 20)
                msg += "Scoring is not their virtue, but they're not that bad either.";
            else if (rating[teamID][3] <= 30)
                msg += "You won't have to worry about their scoring, one of the least 10 efficient in the league.";

            msg += "\n";

            if (rating[teamID][5] <= 5)
                msg += "You'll need to always have an eye on the perimeter. They can turn a game around with their 3 pointers. "
                    + "They score well, they score a lot.";
            else if (rating[teamID][5] <= 10)
                msg += "Their 3pt shooting is bad news. They're in the top 10, and you can't relax playing against them.";
            else if (rating[teamID][5] <= 20)
                msg += "Not much to say about their 3pt shooting. Average, but it is there.";
            else if (rating[teamID][5] <= 30)
                msg += "Definitely not a threat from 3pt land, one of the worst in the league. They waste too many shots from there.";

            msg += "\n";

            if (rating[teamID][7] <= 5)
                msg += "They tend to attack the lanes hard, getting to the line and making the most of it. They're one of the best "
                    + "teams in the league at it.";
            else if (rating[teamID][7] <= 10)
                msg += "One of the best teams in the league at getting to the line. And they don't miss many. Top 10.";
            else if (rating[teamID][7] <= 20)
                msg += "Average free throw efficiency, you don't have to worry about sending them to the line; at least as much as other aspects of their game.";
            else if (rating[teamID][7] <= 30)
                msg += "A team that you'll enjoy playing hard and aggressively against. They don't know how to go to the line, and when they get a chance, they "
                    + "mostly blow it.";

            msg += "\n";

            if (rating[teamID][14] <= 15)
                msg += "They know how to find the open man, and they get their offense going by getting it around the perimeter until a clean shot is there.";
            else if ((rating[teamID][14] > 15) && (rating[teamID][3] < 10))
                msg += "A team that prefers to run its offense through its core players in isolation. Not very good in assists, but they know how to get the job"
                    + "done more times than not.";
            else
                msg += "A team that seems to have some selfish players around, nobody really that efficient to carry the team into high percentages.";

            msg += "\n\n";

            if ((rating[teamID][9] <= 10) && (rating[teamID][11] <= 10) && (rating[teamID][12] <= 10))
                msg += "Hustle is their middle name. They attack the offensive glass, they block, they steal. Don't even dare to blink or get complacent.\n\n";
            else if ((rating[teamID][9] >= 20) && (rating[teamID][11] >= 20) && (rating[teamID][12] >= 20))
                msg += "This team just doesn't know what hustle means. You'll be doing circles around them if you're careful.\n\n";

            if (rating[teamID][8] <= 5)
                msg += "Sensational rebounding team, everybody jumps for the ball, no missed shot is left loose.";
            else if (rating[teamID][8] <= 10)
                msg += "You can't ignore their rebounding ability, they work together and are in the top 10 in rebounding.";
            else if (rating[teamID][8] <= 20)
                msg += "They crash the boards as much as the next guy, but they won't give up any freebies.";
            else if (rating[teamID][8] <= 30)
                msg += "Second chance points? One of their biggest fears. Low low LOW rebounding numbers; just jump for the ball and you'll keep your score high.";

            msg += " ";

            if ((rating[teamID][9] <= 10) && (rating[teamID][10] <= 10))
                msg += "The work they put on rebounding on both sides of the court is commendable. Both offensive and defensive rebounds, their bread and butter.";

            msg += "\n\n";

            if ((rating[teamID][11] <= 10) && (rating[teamID][12] <= 10))
                msg += "A team that knows how to play defense. They're one of the best in steals and blocks, and they make you work hard on offense.\n";
            else if (rating[teamID][11] <= 10)
                msg += "Be careful dribbling and passing. They won't be much trouble once you shoot the ball, but the trouble is getting there. Great in steals.\n";
            else if (rating[teamID][12] <= 10)
                msg += "Get that thing outta here! Great blocking team, they turn the lights off on any mismatched jumper or drive; sometimes even when you least expect it.\n";

            if ((rating[teamID][13] <= 10) && (rating[teamID][15] <= 10))
                msg += "Clumsy team to say the least. They're not careful with the ball, and they foul too much. Keep your eyes open and play hard.";
            else if (rating[teamID][13] < 10)
                msg += "Not good ball handlers, and that's being polite. Bottom 10 in turnovers, they have work to do until they get their offense going.";
            else if (rating[teamID][12] < 10)
                msg += "A team that's prone to fouling. You better drive the lanes as hard as you can, you'll get to the line a lot.";
            else
                msg += "This team is careful with and without the ball. They're good at keeping their turnovers down, and don't foul too much.\nDon't throw "
                    + "your players into steals or fouls against them, because they play smart, and you're probably going to see the opposite call than the "
                    + "one you expected.";

            MessageBox.Show(msg);
        }

        private void btnScout_Click(object sender, RoutedEventArgs e)
        {
            int id = -1;
            try
            {
                id = TeamNames[cmbTeam1.SelectedItem.ToString()];
            }
            catch
            {
                return;
            }

            int[][] rating = calculateRankings(tst);
            if (rating.Length != 1)
            {
                scoutReport(rating, id);
            }
        }

        private void btnTeamCSV_Click(object sender, RoutedEventArgs e)
        {
            int id = -1;
            try
            {
                id = TeamNames[cmbTeam1.SelectedItem.ToString()];
            }
            catch
            {
                return;
            }

            string header1 = "GP,W,L,PF,PA,FGM,FGA,3PM,3PA,FTM,FTA,OREB,DREB,STL,TO,BLK,AST,FOUL";
            /*
            string data = String.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17}", 
                tst[id].getGames(), tst[id].winloss[0], tst[id].winloss[1], tst[id].stats[FGM], tst[id].stats[FGA], tst[id].stats[TPM], tst[id].stats[TPA],
                tst[id].stats[FTM], tst[id].stats[FTA], tst[
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
            string data2 = String.Format("{0:F3}", tst[id].averages[Wp]) + "," + String.Format("{0:F1}", tst[id].averages[Weff]);
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

            int[][] rankings = calculateRankings(tst);

            string data3 = String.Format("{0:F3}", rankings[id][Wp]) + "," + String.Format("{0:F1}", rankings[id][Weff]);
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

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Comma-Separated Values file (*.csv)|*.csv";
            sfd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            sfd.Title = "Select a file to save the CSV to...";
            sfd.ShowDialog();
            if (sfd.FileName == "") return;

            StreamWriter sw = new StreamWriter(sfd.FileName);
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
                tst[id].getGames(), tst[id].winloss[0], tst[id].winloss[1], tst[id].stats[FGM], tst[id].stats[FGA], tst[id].stats[TPM], tst[id].stats[TPA],
                tst[id].stats[FTM], tst[id].stats[FTA], tst[
             */
            string data1 = "";
            for (int id = 0; id < 30; id++)
            {
                if (tst[id].name == "") continue;

                data1 += (id + 1).ToString() + ",";
                foreach (KeyValuePair<string, int> kvp in TeamNames)
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
                data1 += String.Format("{0:F3}", tst[id].averages[Wp]) + "," + String.Format("{0:F1}", tst[id].averages[Weff]);
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
            
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Comma-Separated Values file (*.csv)|*.csv";
            sfd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            sfd.Title = "Select a file to save the CSV to...";
            sfd.ShowDialog();
            if (sfd.FileName == "") return;

            StreamWriter sw = new StreamWriter(sfd.FileName);
            sw.WriteLine(header1 + header2);
            sw.WriteLine(data1);
            sw.Close();
        }

        private void mnuFileOpen_Click(object sender, RoutedEventArgs e)
        {
            btnSelect_Click(sender, e);
        }

        private void btnSaveCustomTeam_Click(object sender, RoutedEventArgs e)
        {
            int id = TeamNames[cmbTeam1.SelectedItem.ToString()];
            tst[id].winloss[0] = Convert.ToByte(txtW1.Text);
            tst[id].winloss[1] = Convert.ToByte(txtL1.Text);
            tst[id].stats[PF] = Convert.ToUInt16(txtPF1.Text);
            tst[id].stats[PA] = Convert.ToUInt16(txtPA1.Text);
            tst[id].stats[FGM] = Convert.ToUInt16(txtFGM1.Text);
            tst[id].stats[FGA] = Convert.ToUInt16(txtFGA1.Text);
            tst[id].stats[TPM] = Convert.ToUInt16(txt3PM1.Text);
            tst[id].stats[TPA] = Convert.ToUInt16(txt3PA1.Text);
            tst[id].stats[FTM] = Convert.ToUInt16(txtFTM1.Text);
            tst[id].stats[FTA] = Convert.ToUInt16(txtFTA1.Text);
            tst[id].stats[OREB] = Convert.ToUInt16(txtOREB1.Text);
            tst[id].stats[DREB] = Convert.ToUInt16(txtDREB1.Text);
            tst[id].stats[STL] = Convert.ToUInt16(txtSTL1.Text);
            tst[id].stats[TO] = Convert.ToUInt16(txtTO1.Text);
            tst[id].stats[BLK] = Convert.ToUInt16(txtBLK1.Text);
            tst[id].stats[AST] = Convert.ToUInt16(txtAST1.Text);
            tst[id].stats[FOUL] = Convert.ToUInt16(txtFOUL1.Text);

            tst[id].calcAvg();
        }

        private void mnuFileCreateCustom_Click(object sender, RoutedEventArgs e)
        {
            customLeagueW clw = new customLeagueW();
            clw.ShowDialog();

            isCustom = true;
            prepareWindow(isCustom);
            pt = new PlayoffTree();

            cmbTeam1.Items.Clear();
            TeamNames = new SortedDictionary<string, int>();
            for (int i = 0; i < 30; i++)
            {
                if (tst[i].name != "")
                {
                    TeamNames.Add(tst[i].name, i);
                }
            }

            foreach (KeyValuePair<string, int> kvp in TeamNames)
            {
                cmbTeam1.Items.Add(kvp.Key);
            }
        }

        private void mnuExit_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(-1);
        }

        private void btnInject_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Please select the Career file you want to update...";
            ofd.Filter = "All NBA 2K12 Career Files (*.FXG; *.CMG; *.RFG; *.PMG; *.SMG)|*.FXG;*.CMG;*.RFG;*.PMG;*.SMG|"
            + "Association files (*.FXG)|*.FXG|My Player files (*.CMG)|*.CMG|Season files (*.RFG)|*.RFG|Playoff files (*.PMG)|*.PMG|" +
                "Create A Legend files (*.SMG)|*.SMG";
            if (Directory.Exists(SavesPath)) ofd.InitialDirectory = SavesPath;
            ofd.ShowDialog();

            if (ofd.FileName == "") return;
            string fn = ofd.FileName;

            prepareOffsets(fn, tst);

            TeamStats[] temp = GetStats(fn);
            if (temp.Length == 1)
            {
                MessageBox.Show("Couldn't get stats from " + getSafeFilename(fn) + ". Update failed.");
                return;
            }

            saveTeamStats(fn);
            MessageBox.Show("Injected custom Team Stats into " + getSafeFilename(fn) + " successfully!");
            cmbTeam1.SelectedIndex = -1;
            cmbTeam1.SelectedIndex = 0;
        }
    }

    [Serializable()]
    public class TeamStats : ISerializable
    {
        public string name;
        public Int32 offset = 0;
        public const int M = 0, PF = 1, PA = 2, FGM = 4, FGA = 5, TPM = 6, TPA = 7,
            FTM = 8, FTA = 9, OREB = 10, DREB = 11, STL = 12, TO = 13, BLK = 14, AST = 15,
            FOUL = 16;
        public const int PPG = 0, PAPG = 1, FGp = 2, FGeff = 3, TPp = 4, TPeff = 5,
            FTp = 6, FTeff = 7, RPG = 8, ORPG = 9, DRPG = 10, SPG = 11, BPG = 12,
            TPG = 13, APG = 14, FPG = 15, Wp = 16, Weff = 17;
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
        /// 8: RPG, 9: ORPG, 10: DRPG, 11: SPG, 12: BPG, 13: TPG, 14: APG, 15: FPG, 16: W%
        /// </summary>
        public float[] averages = new float[18];
        public byte[] winloss = new byte[2];

        public TeamStats()
        {
        }

        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("name", name);
            info.AddValue("stats", stats);
            info.AddValue("winloss", winloss);
        }

        public TeamStats(SerializationInfo info, StreamingContext ctxt)
        {
            name = (string)info.GetValue("name", typeof(string));
            stats = (UInt16[])info.GetValue("stats", typeof(UInt16[]));
            winloss = (byte[])info.GetValue("winloss", typeof(byte[]));
        }

        public int calcAvg()
        {
            int games = winloss[0] + winloss[1];
            if (games == 0) games = -1;
            averages[Wp] = (float)winloss[0] / games;
            averages[Weff] = averages[Wp] * winloss[0];
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
            return games;
        }

        internal int getGames()
        {
            int games = winloss[0] + winloss[1];
            return games;
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

        public PlayoffTree() 
        {
            this.teams[0] = "Invalid";
        }

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
