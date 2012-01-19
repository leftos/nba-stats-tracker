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
using System.Reflection;

namespace NBA_2K12_Correct_Team_Stats
{
    /// <summary>
    /// Interaction logic for aboutW.xaml
    /// </summary>
    public partial class aboutW : Window
    {
        public aboutW()
        {
            InitializeComponent();

            lblVersion.Content = "version " + Assembly.GetExecutingAssembly().GetName().Version.ToString();

            txbThanks.Text =
                "- JaoSming, for his roster editing tutorial\n" +
                "- Onisak, for his help with debugging\n" +
                "- Vl@d Zola Jr, for helping make NBA Stats Tracker what it is and will be\n" +
                "- albidnis, for his idea to export to CSV\n" +
                "- jrlocke, for being the first donator, and a generous one\n" +
                "- zizoux, for his idea to inject real stats, which ended up being the Custom Leagues and Real NBA Stats features\n" +
                "- Everyone at the NLSC community, for their continued support";
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
