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

using System.Collections.Generic;
using System.Windows;
using NBA_Stats_Tracker.Data.Misc;

#endregion

namespace NBA_Stats_Tracker.Windows
{
    /// <summary>
    ///     Implements a multi-function combo-box choice window.
    /// </summary>
    public partial class ComboChoiceWindow
    {
        #region Mode enum

        /// <summary>
        ///     Used to determine what choices the window should offer, and its functions
        /// </summary>
        public enum Mode
        {
            OneTeam,
            Versus,
            Division,
            Generic
        }

        #endregion

        private readonly Mode _mode;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ComboChoiceWindow" /> class.
        /// </summary>
        /// <param name="mode">The Mode enum instance which determines what choices should be offered.</param>
        /// <param name="index">The default choice.</param>
        public ComboChoiceWindow(Mode mode, int index = 0)
        {
            InitializeComponent();
            this._mode = mode;
            
            if (mode == Mode.Versus)
            {
                label1.Content = "Pick the two teams";
                cmbSelection2.Visibility = Visibility.Visible;
                foreach (var kvp in MainWindow.TeamOrder)
                {
                    cmbSelection1.Items.Add(kvp.Key);
                    cmbSelection2.Items.Add(kvp.Key);
                }
            }
            else if (mode == Mode.Division)
            {
                label1.Content = "Pick the new division for the team:";
                cmbSelection2.Visibility = Visibility.Hidden;
                foreach (Division div in MainWindow.Divisions)
                {
                    Conference conf = MainWindow.Conferences.Find(conference => conference.ID == div.ConferenceID);
                    cmbSelection1.Items.Add(string.Format("{0}: {1}", conf.Name, div.Name));
                }
            }
            cmbSelection1.SelectedIndex = index;
            cmbSelection2.SelectedIndex = index != 0 ? 0 : 1;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ComboChoiceWindow" /> class.
        ///     Used for when a player is set to active while previously inactive.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="items"></param>
        /// <param name="mode"></param>
        /// <param name="teams">The available teams to sign the player to.</param>
        public ComboChoiceWindow(string message, IEnumerable<string> items, Mode mode = Mode.Generic)
        {
            InitializeComponent();

            _mode = mode;

            label1.Content = message;
            cmbSelection1.ItemsSource = items;
            cmbSelection2.Visibility = Visibility.Hidden;

            if (cmbSelection1.Items.Count > 0)
            {
                cmbSelection1.SelectedIndex = 0;
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (_mode == Mode.OneTeam)
            {
                if (cmbSelection1.SelectedIndex == -1)
                    return;
                PlayerOverviewWindow.askedTeam = cmbSelection1.SelectedItem.ToString();
            }
            else if (_mode == Mode.Division || _mode == Mode.Generic)
            {
                MainWindow.input = cmbSelection1.SelectedItem.ToString();
            }
            DialogResult = true;
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}