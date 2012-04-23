using System.Windows;

namespace NBA_2K12_Correct_Team_Stats
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class customLeagueW : Window
    {
        public customLeagueW()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.tst[0].name = txtTeam1.Text;
            MainWindow.tst[1].name = txtTeam2.Text;
            MainWindow.tst[2].name = txtTeam3.Text;
            MainWindow.tst[3].name = txtTeam4.Text;
            MainWindow.tst[4].name = txtTeam5.Text;
            MainWindow.tst[5].name = txtTeam6.Text;
            MainWindow.tst[6].name = txtTeam7.Text;
            MainWindow.tst[7].name = txtTeam8.Text;
            MainWindow.tst[8].name = txtTeam9.Text;
            MainWindow.tst[9].name = txtTeam10.Text;
            MainWindow.tst[10].name = txtTeam11.Text;
            MainWindow.tst[11].name = txtTeam12.Text;
            MainWindow.tst[12].name = txtTeam13.Text;
            MainWindow.tst[13].name = txtTeam14.Text;
            MainWindow.tst[14].name = txtTeam15.Text;
            MainWindow.tst[15].name = txtTeam16.Text;
            MainWindow.tst[16].name = txtTeam17.Text;
            MainWindow.tst[17].name = txtTeam18.Text;
            MainWindow.tst[18].name = txtTeam19.Text;
            MainWindow.tst[19].name = txtTeam20.Text;
            MainWindow.tst[20].name = txtTeam21.Text;
            MainWindow.tst[21].name = txtTeam22.Text;
            MainWindow.tst[22].name = txtTeam23.Text;
            MainWindow.tst[23].name = txtTeam24.Text;
            MainWindow.tst[24].name = txtTeam25.Text;
            MainWindow.tst[25].name = txtTeam26.Text;
            MainWindow.tst[26].name = txtTeam27.Text;
            MainWindow.tst[27].name = txtTeam28.Text;
            MainWindow.tst[28].name = txtTeam29.Text;
            MainWindow.tst[29].name = txtTeam30.Text;
            this.Close();
        }
    }
}
