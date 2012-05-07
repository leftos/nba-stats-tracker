using System.Diagnostics;
using System.Reflection;
using System.Windows;

namespace NBA_Stats_Tracker
{
    /// <summary>
    /// Interaction logic for aboutW.xaml
    /// </summary>
    public partial class aboutW : Window
    {
        public aboutW()
        {
            InitializeComponent();

            lblVersion.Content = "version " + Assembly.GetExecutingAssembly().GetName().Version;

            txbThanks.Text =
                "- JaoSming, for his roster editing tutorial\n" +
                "- Onisak, for his help with debugging\n" +
                "- Vl@d Zola Jr, for helping make NBA Stats Tracker what it is and will be\n" +
                "- albidnis, for his idea to export to CSV\n" +
                "- jrlocke, for being the first donator, and a generous one\n" +
                "- zizoux, for his idea to inject real stats, which ended up being the Custom Leagues and Real NBA Stats features\n" +
                "- AreaOfEffect, for his help with debugging\n" +
                "- Everyone at the NLSC community, for their continued support";
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnDonate_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("http://students.ceid.upatras.gr/~aslanoglou/donate.html");
        }
    }
}