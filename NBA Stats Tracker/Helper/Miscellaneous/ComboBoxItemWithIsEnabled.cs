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

namespace NBA_Stats_Tracker.Helper.Miscellaneous
{
    /// <summary>
    ///     Implements a generic combo-box item with an IsEnabled property.
    ///     Used to create items in combo-boxes that can't be selected (e.g. group headers).
    /// </summary>
    public class ComboBoxItemWithIsEnabled
    {
        public ComboBoxItemWithIsEnabled(string item, bool isEnabled = true)
        {
            Item = item;
            IsEnabled = isEnabled;
        }

        public string Item { get; set; }
        public bool IsEnabled { get; set; }
    }
}