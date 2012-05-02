using System;
using System.Windows;

namespace NBA_Stats_Tracker
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
            Title = title;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnCopyToClip_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(txbMsg.Text);
            Title += " (copied to clipboard)";
        }
    }
}