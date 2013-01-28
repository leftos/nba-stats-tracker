#region Copyright Notice

// Created by Lefteris Aslanoglou, (c) 2011-2013
// 
// Initial development until v1.0 done as part of the implementation of thesis
// "Application Development for Basketball Statistical Analysis in Natural Language"
// under the supervision of Prof. Athanasios Tsakalidis & MSc Alexandros Georgiou
// 
// All rights reserved. Unless specifically stated otherwise, the code in this file should 
// not be reproduced, edited and/or republished without explicit permission from the 
// author.

#endregion

#region Using Directives

using System.ComponentModel;
using System.Windows;

#endregion

namespace NBA_Stats_Tracker.Windows
{
    /// <summary>
    ///     Interaction logic for ProgressWindow.xaml
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

        private void Window_Closing(object sender, CancelEventArgs e)
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