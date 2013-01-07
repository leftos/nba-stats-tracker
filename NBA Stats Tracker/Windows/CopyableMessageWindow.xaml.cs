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

using System;
using System.Windows;

#endregion

namespace NBA_Stats_Tracker.Windows
{
    /// <summary>
    ///     Implements a window to display a message to the user that can be copied to the clipboard.
    ///     Window size adjusts depending on the contents.
    /// </summary>
    public partial class CopyableMessageWindow
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CopyableMessageWindow" /> class.
        /// </summary>
        /// <param name="msg">The message to display.</param>
        /// <param name="title">The title of the window.</param>
        /// <param name="align">The text alignment to be used for the message.</param>
        public CopyableMessageWindow(String msg, String title, TextAlignment align)
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

        /// <summary>
        ///     Handles the Click event of the btnCopyToClip control.
        ///     Copies the message displayed to the clipboard.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void btnCopyToClip_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(txbMsg.Text);
            Title += " (copied to clipboard)";
        }
    }
}