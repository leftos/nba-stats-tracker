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
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using NBA_Stats_Tracker.Data.Other;
using NBA_Stats_Tracker.Data.Players;
using NBA_Stats_Tracker.Data.Players.Injuries;

#endregion

namespace NBA_Stats_Tracker.Windows
{
    public partial class PlayerInjuryWindow
    {
        public static int InjuryType;
        public static string CustomInjuryName;
        public static int InjuryDaysLeft;

        public PlayerInjuryWindow()
        {
            InitializeComponent();

            cmbInjuryType.ItemsSource = PlayerInjury.InjuryTypes.Values.ToList();

            List<string> approx = PlayerInjury.ApproximateDurations.Keys.Skip(2).ToList();
            approx.AddRange(PlayerInjury.ApproximateDurations.Keys.Take(2).ToList());
            
            cmbTFApproximateAmount.ItemsSource = approx;
            cmbTFExactType.ItemsSource = new List<string>{"Days", "Weeks", "Months"};

            rbTFApproximate.IsChecked = true;
        }

        public PlayerInjuryWindow(PlayerInjury injury) : this()
        {
            if (injury.InjuryType != -1)
            {
                cmbInjuryType.SelectedItem = injury.InjuryName;
            }
            else
            {
                cmbInjuryType.SelectedItem = "Custom";
                txtCustomInjuryName.Text = injury.CustomInjuryName;
            }
            cmbTFApproximateAmount.SelectedItem = injury.ApproximateDays;
            cmbTFExactType.SelectedItem = "Days";
            txtTFExactAmount.Text = injury.InjuryDaysLeft.ToString();
        }
        
        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            InjuryType = PlayerInjury.InjuryTypes.Single(it => it.Value == cmbInjuryType.SelectedItem.ToString()).Key;
            CustomInjuryName = InjuryType == -1 ? txtCustomInjuryName.Text : "";
            if (InjuryType != 0)
            {
                if (rbTFApproximate.IsChecked.GetValueOrDefault())
                {
                    InjuryDaysLeft = PlayerInjury.ApproximateDurations[cmbTFApproximateAmount.SelectedItem.ToString()];
                }
                else
                {
                    switch (cmbTFExactType.SelectedItem.ToString())
                    {
                        case "Days":
                            InjuryDaysLeft = Convert.ToInt32(txtTFExactAmount.Text);
                            break;
                        case "Weeks":
                            InjuryDaysLeft = Convert.ToInt32(txtTFExactAmount.Text)*7;
                            break;
                        case "Months":
                            InjuryDaysLeft = Convert.ToInt32(txtTFExactAmount.Text)*30;
                            break;
                    }
                }
            }
            else
            {
                InjuryDaysLeft = 0;
            }
            DialogResult = true;
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void cmbInjuryType_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            switch (cmbInjuryType.SelectedItem.ToString())
            {
                case "Healthy":
                    grpTF.IsEnabled = false;
                    txtCustomInjuryName.IsEnabled = false;
                    break;
                case "Custom":
                    grpTF.IsEnabled = true;
                    txtCustomInjuryName.IsEnabled = true;
                    break;
                default:
                    grpTF.IsEnabled = true;
                    txtCustomInjuryName.IsEnabled = false;
                    break;

            }
        }
    }
}