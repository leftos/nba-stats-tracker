#region Copyright Notice

// Created by Lefteris Aslanoglou, (c) 2011-2012
// 
// Implementation of thesis
// "Application Development for Basketball Statistical Analysis in Natural Language"
// under the supervision of Prof. Athanasios Tsakalidis & MSc Alexandros Georgiou,
// Computer Engineering & Informatics Department, University of Patras, Greece.
// 
// All rights reserved. Unless specifically stated otherwise, the code in this file should 
// not be reproduced, edited and/or republished without explicit permission from the 
// author.

#endregion

#region Using Directives

using System.Windows;

#endregion

namespace NBA_Stats_Tracker.Windows
{
    /// <summary>
    /// Implements a general-purpose Input-box Window.
    /// </summary>
    public partial class InputBoxWindow
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InputBoxWindow" /> class.
        /// </summary>
        /// <param name="message">The prompt to display.</param>
        public InputBoxWindow(string message)
        {
            InitializeComponent();

            lblMessage.Content = message;

            txtInput.Focus();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InputBoxWindow" /> class.
        /// </summary>
        /// <param name="message">The prompt to display.</param>
        /// <param name="defaultValue">The default value.</param>
        public InputBoxWindow(string message, string defaultValue) : this(message)
        {
            txtInput.Text = defaultValue;
            txtInput.SelectAll();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.input = txtInput.Text;
            DialogResult = true;
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.input = "";
            DialogResult = false;
            Close();
        }
    }
}