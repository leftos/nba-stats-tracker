using System.Windows;

namespace NBA_2K12_Correct_Team_Stats
{
    /// <summary>
    /// Interaction logic for addW.xaml
    /// </summary>
    public partial class addW : Window
    {
        public addW()
        {
            InitializeComponent();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (tbcAdd.SelectedItem == tabTeams)
            {
                MainWindow.addInfo = txtTeams.Text;
            }

            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.addInfo = "";
            this.Close();
        }
    }
}
