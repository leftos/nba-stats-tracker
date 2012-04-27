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
using System.Windows.Shapes;

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
