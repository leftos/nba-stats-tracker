using System.Windows;

namespace NBA_Stats_Tracker
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class inputBoxW : Window
    {
        public inputBoxW(string message)
        {
            InitializeComponent();

            lblMessage.Content = message;

            txtInput.Focus();
        }

        public inputBoxW(string message, string defaultValue) : this(message)
        {
            txtInput.Text = defaultValue;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.input = txtInput.Text;
            Close();
        }
    }
}
