using System;
using System.Windows;

namespace NBA_2K12_Correct_Team_Stats
{
    /// <summary>
    /// Interaction logic for copyableW.xaml
    /// </summary>
    public partial class copyableW : Window
    {
        public copyableW(String msg, String title, TextAlignment align)
        {
            InitializeComponent();

            txbMsg.Text = msg;
            txbMsg.TextAlignment = align;
            this.Title = title;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnCopyToClip_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(txbMsg.Text);
            this.Title += " (copied to clipboard)";
        }
    }
}
