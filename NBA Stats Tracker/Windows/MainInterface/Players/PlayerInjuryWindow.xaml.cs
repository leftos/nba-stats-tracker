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

namespace NBA_Stats_Tracker.Windows.MainInterface.Players
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;

    using NBA_Stats_Tracker.Data.Players.Injuries;

    #endregion

    public partial class PlayerInjuryWindow
    {
        public static int InjuryType;
        public static string CustomInjuryName;
        public static int InjuryDaysLeft;

        public PlayerInjuryWindow()
        {
            InitializeComponent();

            cmbInjuryType.ItemsSource = PlayerInjury.InjuryTypes.Values.ToList();

            var approx = PlayerInjury.ApproximateDurations.Keys.Skip(2).ToList();
            approx.AddRange(PlayerInjury.ApproximateDurations.Keys.Take(2).ToList());

            cmbTFApproximateAmount.ItemsSource = approx;
            cmbTFExactType.ItemsSource = new List<string> { "Days", "Weeks", "Months" };

            rbTFApproximate.IsChecked = true;
        }

        public PlayerInjuryWindow(PlayerInjury injury)
            : this()
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
                            InjuryDaysLeft = Convert.ToInt32(txtTFExactAmount.Text) * 7;
                            break;
                        case "Months":
                            InjuryDaysLeft = Convert.ToInt32(txtTFExactAmount.Text) * 30;
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

        private void cmbInjuryType_SelectionChanged(object sender, SelectionChangedEventArgs e)
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