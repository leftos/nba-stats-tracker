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

namespace NBA_Stats_Tracker.Data.Players.Injuries
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Linq;

    #endregion

    [Serializable]
    public class PlayerInjury
    {
        public static readonly Dictionary<string, int> ApproximateDurations = new Dictionary<string, int>
            {
                { "Career-Ending", -2 },
                { "Unknown", -1 },
                { "Active", 0 },
                { "Day-To-Day", 6 },
                { "1-2 weeks", 14 },
                { "2-4 weeks", 28 },
                { "4-6 weeks", 42 },
                { "6-8 weeks", 56 },
                { "2-4 months", 120 },
                { "4-6 months", 180 },
                { "6-8 months", 240 },
                { "8-10 months", 300 },
                { "10-12 months", 360 },
                { "More than a year", int.MaxValue }
            };

        public static readonly Dictionary<int, string> InjuryTypes = new Dictionary<int, string>
            {
                { -1, "Custom" },
                { 0, "Healthy" },
                { 1, "Appendectomy" },
                { 2, "Arthroscopic Surgery" },
                { 3, "Back Spasms" },
                { 4, "Bone Bruise" },
                { 5, "Bone Spurs" },
                { 6, "Broken Ankle" },
                { 7, "Broken Arm" },
                { 8, "Broken Back" },
                { 9, "Broken Finger" },
                { 10, "Broken Foot" },
                { 11, "Broken Hand" },
                { 12, "Broken Hip" },
                { 13, "Broken Jaw" },
                { 14, "Broken Patella" },
                { 15, "Broken Nose" },
                { 16, "Broken Rib" },
                { 17, "Broken Toe" },
                { 18, "Broken Wrist" },
                { 19, "Bruised Heel" },
                { 20, "Bruised Hip" },
                { 21, "Bruised Knee" },
                { 22, "Bruised Rib" },
                { 23, "Bruised Spinal Cord" },
                { 24, "Bruised Sternum" },
                { 25, "Bruised Tailbone" },
                { 26, "Bruised Thigh" },
                { 27, "Concussion" },
                { 28, "Dislocated Finger" },
                { 29, "Dislocated Patella" },
                { 30, "Elbow Surgery" },
                { 31, "Eye Surgery" },
                { 32, "Fatigue" },
                { 33, "Flu" },
                { 34, "Foot Surgery" },
                { 35, "Fractured Eye Socket" },
                { 36, "Hand Surgery" },
                { 37, "Hernia" },
                { 38, "High Ankle Sprain" },
                { 39, "Hip Surgery" },
                { 40, "Hyperextended Knee" },
                { 41, "Inner Ear Infection" },
                { 42, "Knee Surgery" },
                { 43, "Knee Tendinitis" },
                { 44, "Lower Back Strain" },
                { 45, "Microfracture Surgery" },
                { 46, "Migraine Headache" },
                { 47, "Plantar Fasciitis" },
                { 48, "Personal Reason" },
                { 49, "Separated Shoulder" },
                { 50, "Severe Ankle Sprain" },
                { 51, "Shin Splints" },
                { 52, "Sore Ankle" },
                { 53, "Sore Back" },
                { 54, "Sore Foot" },
                { 55, "Sore Handt" },
                { 56, "Sore Hamstring" },
                { 57, "Sore Knee" },
                { 58, "Sore Wrist" },
                { 59, "Sprained Ankle" },
                { 60, "Sprained Finger" },
                { 61, "Sprained Foot" },
                { 62, "Sprained Knee" },
                { 63, "Sprained Shoulder" },
                { 64, "Sprained Toe" },
                { 65, "Sprained Wrist" },
                { 66, "Strained Abdomen" },
                { 67, "Strained Achilles" },
                { 68, "Strained Calf" },
                { 69, "Strained Elbow" },
                { 70, "Strained Groin" },
                { 71, "Strained Hamstring" },
                { 72, "Strained Hip Flexor" },
                { 73, "Strained Knee" },
                { 74, "Strained MCL" },
                { 75, "Sprained Neck" },
                { 76, "Strained Oblique" },
                { 77, "Strained Quad" },
                { 78, "Stress Fracture" },
                { 79, "Suspended" },
                { 80, "Torn Achilles" },
                { 81, "Torn ACL" },
                { 82, "Torn Bicep" },
                { 83, "Torn Ligament Foot" },
                { 84, "Torn Hamstring" },
                { 85, "Torn Hip Flexor" },
                { 86, "Torn Labrum" },
                { 87, "Torn Ligament Elbow" },
                { 88, "Torn Hand Ligament" },
                { 89, "Torn MCL" },
                { 90, "Torn Meniscus" },
                { 91, "Torn Patellar Tendon" },
                { 92, "Torn Tricep" },
            };

        public PlayerInjury()
        {
            InjuryType = 0;
            InjuryDaysLeft = 0;
            CustomInjuryName = "";
        }

        public PlayerInjury(int type, int days)
        {
            InjuryType = type;
            InjuryDaysLeft = days;

            CustomInjuryName = InjuryType == -1 ? "Unknown" : "";
        }

        public PlayerInjury(string customName, int days)
            : this(-1, days)
        {
            CustomInjuryName = customName;
        }

        public int InjuryType { get; private set; }
        public string CustomInjuryName { get; private set; }
        public int InjuryDaysLeft { get; private set; }

        public bool IsInjured
        {
            get { return InjuryType != 0; }
        }

        public string InjuryName
        {
            get { return InjuryType == -1 ? CustomInjuryName : InjuryTypes[InjuryType]; }
        }

        public string ApproximateDays
        {
            get
            {
                return (from dur in ApproximateDurations where InjuryDaysLeft <= dur.Value select dur.Key).FirstOrDefault();

                #region Old matching code

                /*
                if (InjuryDaysLeft == -1)
                    return "Unknown";
                else if (InjuryDaysLeft == 0)
                    return "Healthy";
                else if (InjuryDaysLeft <= 6)
                    return "Day-To-Day";
                else if (InjuryDaysLeft <= 14)
                    return "1-2 weeks";
                else if (InjuryDaysLeft <= 28)
                    return "3-4 weeks";
                else if (InjuryDaysLeft <= 60)
                    return "1-2 months";
                else if (InjuryDaysLeft <= 120)
                    return "3-4 months";
                else
                {
                    var approxMonth = 2*Math.Floor(0.0167*InjuryDaysLeft);
                    return string.Format("{0}-{1} months", approxMonth, approxMonth + 2);
                }
                */

                #endregion
            }
        }

        public string Status
        {
            get
            {
                if (InjuryType != 0)
                {
                    return string.Format("{0} ({1})", InjuryName, ApproximateDays);
                }
                else
                {
                    return "Healthy";
                }
            }
        }

        public new string ToString()
        {
            return Status;
        }
    }
}