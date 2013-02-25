using System;
using System.Threading;
using System.Threading.Tasks;

namespace NBA_Stats_Tracker.Helper.Miscellaneous
{
    static internal class ProgressHelper
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

        public static void DoInScheduler(Action a, TaskScheduler scheduler)
        {
            Task.Factory.StartNew(() => a, CancellationToken.None, TaskCreationOptions.None, scheduler).Wait();
        }
    }
}