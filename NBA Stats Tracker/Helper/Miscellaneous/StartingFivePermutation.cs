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

using System.Collections.Generic;

#endregion

namespace NBA_Stats_Tracker.Helper.Miscellaneous
{
    /// <summary>
    ///     Implements a list of five players. Used in determining the best starting five in a specific scope.
    /// </summary>
    public class StartingFivePermutation
    {
        public List<int> IDList = new List<int>(5);
        public int PlayersInPrimaryPosition = 0;
        public double Sum = 0;
        public int BestPermCount = 0;
    }
}