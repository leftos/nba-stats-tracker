namespace NBA_Stats_Tracker.Helper.Miscellaneous
{
    #region Using Directives

    using System;
    using System.Threading;

    #endregion

    internal static class ProgressHelper
    {
        public static ProgressInfo Progress = new ProgressInfo("");

        public static void UpdateProgress(double percentage)
        {
            UpdateProgress(Convert.ToInt32(percentage));
        }

        public static void UpdateProgress(int percentage)
        {
            Interlocked.Exchange(ref Progress, new ProgressInfo(Progress, percentage));
        }

        public static void UpdateProgress(string message)
        {
            Interlocked.Exchange(ref Progress, new ProgressInfo(Progress, message));
        }
    }
}