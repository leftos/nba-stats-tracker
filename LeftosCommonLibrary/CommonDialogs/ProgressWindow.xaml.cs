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

namespace LeftosCommonLibrary.CommonDialogs
{
    /// <summary>
    ///     Interaction logic for ProgressWindow.xaml
    /// </summary>
    public partial class ProgressWindow : Window
    {
        /// <summary>
        ///     A static copy of the ProgressWindow instance to be used while the window is opened.
        /// </summary>
        public static ProgressWindow PwInstance;

        /// <summary>
        ///     Whether the window can be closed either by code or by user input.
        /// </summary>
        public bool CanClose = false;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ProgressWindow" /> class.
        /// </summary>
        public ProgressWindow()
        {
            InitializeComponent();

            PwInstance = this;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ProgressWindow" /> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="pbVisible">
        ///     if set to <c>true</c>, the progress bar is visible.
        /// </param>
        /// <param name="updateTitleOnPBValueChange">
        ///     if set to <c>true</c>, the window title is updated to include the progress bar value when the latter one is changed.
        /// </param>
        public ProgressWindow(string message, bool pbVisible = true, bool updateTitleOnPBValueChange = true) : this()
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

            if (updateTitleOnPBValueChange)
            {
                pb.ValueChanged += pb_ValueChanged;
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (!CanClose)
            {
                e.Cancel = true;
            }
        }

        /// <summary>
        ///     Sets the progress bar value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        public void SetProgressBarValue<T>(T value)
        {
            pb.Value = value.ToInt32();
        }

        /// <summary>
        ///     Sets the message.
        /// </summary>
        /// <param name="message">The message.</param>
        public void SetMessage(string message)
        {
            txbProgress.Text = message;
        }

        /// <summary>
        ///     Sets the state (i.e. message shown, progress bar value, etc.)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message">The message.</param>
        /// <param name="value">The progress bar value.</param>
        public void SetState<T>(string message, T value)
        {
            SetProgressBarValue(value);
            SetMessage(message);
        }

        private void pb_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Title = "Please wait (" + pb.Value + "% completed)...";
        }
    }
}