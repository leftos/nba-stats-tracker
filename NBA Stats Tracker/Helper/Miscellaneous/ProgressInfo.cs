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

#region Using Directives

using System.Diagnostics;
using LeftosCommonLibrary;

#endregion

namespace NBA_Stats_Tracker.Helper.Miscellaneous
{
    public class ProgressInfo
    {
        public int CurrentStage;
        public int MaxStage;
        public string Message;
        public int Percentage;
        public Stopwatch Timing;

        public ProgressInfo(int curStage, int maxStage, string message, int percentage = 0) : this(curStage, message, percentage)
        {
            MaxStage = maxStage;
        }

        public ProgressInfo(int curStage, string message, int percentage = 0) : this(message, percentage)
        {
            CurrentStage = curStage;
            Timing = new Stopwatch();
            Timing.Start();
        }

        public ProgressInfo(string message, int percentage = 0) : this(percentage)
        {
            Message = message;
        }

        public ProgressInfo(ProgressInfo progress, string message, int percentage = 0) : this(message, percentage)
        {
            Tools.WriteToTrace(string.Format("Stage {0} ({1}): {2}", progress.CurrentStage, progress.Message,
                                             progress.Timing.ElapsedMilliseconds));
            CurrentStage = progress.CurrentStage + 1;
            MaxStage = progress.MaxStage;
            Timing = new Stopwatch();
            Timing.Start();
        }

        public ProgressInfo(int percentage)
        {
            Percentage = percentage;
        }

        public ProgressInfo(ProgressInfo progress, int percentage)
        {
            CurrentStage = progress.CurrentStage;
            MaxStage = progress.MaxStage;
            Message = progress.Message;
            Percentage = percentage;
            Timing = progress.Timing;
        }
    }
}