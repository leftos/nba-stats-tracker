#region Copyright Notice

//    Copyright 2011-2013 Eleftherios Aslanoglou
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

#endregion

#region Using Directives

using System.ComponentModel;
using System.Windows;

#endregion

namespace NBA_Stats_Tracker.Windows.MiscTools
{
    /// <summary>
    ///     Interaction logic for ProgressWindow.xaml
    /// </summary>
    public partial class ProgressWindow : Window
    {
        public bool CanClose = false;
        public static ProgressWindow PwInstance;

        public ProgressWindow()
        {
            InitializeComponent();

            PwInstance = this;
        }

        public ProgressWindow(string message, bool pbVisible = true) : this()
        {
            txbProgress.Text = message;
            if (pbVisible)
            {
                pb.Visibility = Visibility.Visible;
            }
            else
            {
                pb.Visibility = Visibility.Collapsed;
                Height = 90;
            }
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