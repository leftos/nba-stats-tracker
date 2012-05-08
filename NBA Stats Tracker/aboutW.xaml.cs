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
                "I want to thank everyone that took the time to give me suggestions, feedback, and bug reports.\n" +
                "I also want to thank my family and friends for their support, as well as my professor Mr. " +
                "Tsakalidis for letting NBA Stats Tracker be the thesis for my Computer Engineering degree.\n" +
                "Special thanks goes to the NLSC community and specific members which I've named in the Readme.\n" +
                "\nThanks for all your support, enjoy!";
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnDonate_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("http://students.ceid.upatras.gr/~aslanoglou/donate.html");
        }

        private void btnWebsite_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("http://forums.nba-live.com/viewtopic.php?f=143&t=84110");
        }
    }
}