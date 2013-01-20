using System.Collections.Generic;

namespace NBA_Stats_Tracker.Helper.Miscellaneous
{
    /// <summary>
    ///     Implements a list of five players. Used in determining the best starting five in a specific scope.
    /// </summary>
    public class StartingFivePermutation
    {
        public int PlayersInPrimaryPosition = 0;
        public double Sum = 0;
        public List<int> IDList = new List<int>(5);
    }
}