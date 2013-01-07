using System;

namespace NBA_Stats_Tracker.Data.Misc
{
    /// <summary>
    ///     Used to pass on all the required information for a specific timeframe.
    /// </summary>
    public class Timeframe
    {
        public Timeframe(int seasonNum)
        {
            isBetween = false;
            SeasonNum = seasonNum;
        }

        public Timeframe(DateTime startDate, DateTime endDate)
        {
            isBetween = true;
            StartDate = startDate;
            EndDate = endDate;
            SeasonNum = 1;
        }

        public bool isBetween { get; set; }
        public int SeasonNum { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}