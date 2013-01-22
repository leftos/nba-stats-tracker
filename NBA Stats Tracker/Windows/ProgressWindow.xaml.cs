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

namespace NBA_Stats_Tracker.Windows
{
    /// <summary>
    /// Interaction logic for ProgressWindow.xaml
    /// </summary>
    public partial class ProgressWindow : Window
    {
        public bool CanClose = false;

        public ProgressWindow()
        {
            InitializeComponent();
        }

        public ProgressWindow(string message) : this()
        {
            txbProgress.Text = message;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!CanClose)
                e.Cancel = true;
        }

        private void pb_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Title = "Please wait (" + pb.Value + "% completed)...";
        }
    }
}
