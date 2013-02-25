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

using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace NBA_Stats_Tracker.Data.Players.Injuries
{
    [Serializable]
    public class PlayerInjury
    {
        public static readonly Dictionary<string, int> ApproximateDurations = new Dictionary<string, int>
                                                                              {
                                                                                  {"Career-Ending", -2},
                                                                                  {"Unknown", -1},
                                                                                  {"Active", 0},
                                                                                  {"Day-To-Day", 6},
                                                                                  {"1-2 weeks", 14},
                                                                                  {"2-4 weeks", 28},
                                                                                  {"4-6 weeks", 42},
                                                                                  {"6-8 weeks", 56},
                                                                                  {"2-4 months", 120},
                                                                                  {"4-6 months", 180},
                                                                                  {"6-8 months", 240},
                                                                                  {"8-10 months", 300},
                                                                                  {"10-12 months", 360},
                                                                                  {
                                                                                      "More than a year", int.MaxValue
                                                                                  }
                                                                              };

        public static readonly Dictionary<int, string> InjuryTypes = new Dictionary<int, string>
                                                                     {
                                                                         {-1, "Custom"},
                                                                         {0, "Healthy"},
                                                                         {1, "Sore Knee"},
                                                                         {2, "Strained MCL"},
                                                                         {3, "Torn ACL"},
                                                                         {4, "Twisted Ankle"},
                                                                         {5, "Severe Ankle Sprain"},
                                                                         {6, "Broken Ankle"},
                                                                         {7, "Sprained Toe"},
                                                                         {8, "Strained Achilles"},
                                                                         {9, "Torn Achilles"},
                                                                         {10, "Plantar Fasciitis"},
                                                                         {11, "Sore Hamstring"},
                                                                         {12, "Strained Quad"},
                                                                         {13, "Back Spasms"},
                                                                         {14, "Bruised Hip"},
                                                                         {15, "Broken Finger"},
                                                                         {16, "Sprained Wrist"},
                                                                         {17, "Inflamed Elbow"},
                                                                         {18, "Strained Abdomen"},
                                                                         {19, "Strained Hamstring"},
                                                                         {20, "Lower Back Strain"},
                                                                         {21, "Strained Calf"},
                                                                         {22, "Sore Wrist"},
                                                                         {23, "Knee Tendinitis"},
                                                                         {24, "Bone Spurs"},
                                                                         {25, "Broken Wrist"},
                                                                         {26, "Strained Groin"},
                                                                         {27, "Broken Toe"},
                                                                         {28, "Flu"},
                                                                         {29, "Broken Nose"},
                                                                         {30, "Bruised Tailbone"},
                                                                         {31, "Migraine Headache"},
                                                                         {32, "Bruised Heel"},
                                                                         {33, "Broken Patella"},
                                                                         {34, "Shin Splints"},
                                                                         {35, "Separated Shoulder"},
                                                                         {36, "Dislocated Finger"},
                                                                         {37, "Broken Hand"},
                                                                         {38, "Bruised Sternum"},
                                                                         {39, "Torn Patellar Tendon"},
                                                                         {40, "Torn Labrum"},
                                                                         {41, "Sprained Foot"},
                                                                         {42, "Sprained Finger"},
                                                                         {43, "Sprained Knee"},
                                                                         {44, "Sprained Shoulder"},
                                                                         {45, "Sprained Neck"},
                                                                         {46, "Arthroscopic Surgery"},
                                                                         {47, "Microfracture Surgery"},
                                                                         {48, "Sore Ankle"},
                                                                         {49, "Sore Foot"},
                                                                         {50, "Sore Back"},
                                                                         {51, "Torn MCL"},
                                                                         {52, "Torn Meniscus"},
                                                                         {53, "Torn Hand Ligament"},
                                                                         {54, "Torn Hamstring"},
                                                                         {55, "Broken Arm"},
                                                                         {56, "Broken Foot"},
                                                                         {57, "Broken Jaw"},
                                                                         {58, "Broken Back"},
                                                                         {59, "Fractured Eye Socket"},
                                                                         {60, "Hyperextended Knee"},
                                                                         {61, "Concussion"},
                                                                         {62, "Inner Ear Infection"},
                                                                         {63, "Hernia"},
                                                                         {64, "Fatigue"},
                                                                         {65, "Personal Reason"},
                                                                         {66, "Suspended"},
                                                                         {67, "Broken Rib"},
                                                                         {68, "Broken Hip"},
                                                                         {69, "Bruised Rib"},
                                                                         {70, "Bruised Knee"},
                                                                         {71, "Bruised Thigh"},
                                                                         {72, "Bruised Spinal Cord"},
                                                                         {73, "Strained Oblique"},
                                                                         {74, "Bone Bruise"},
                                                                         {75, "High Ankle Sprain"},
                                                                         {76, "Dislocated Patella"},
                                                                         {77, "Eye Surgery"},
                                                                         {78, "Stress Fracture"},
                                                                         {79, "Torn Ligament Foot"}
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

        public PlayerInjury(string customName, int days) : this(-1, days)
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
                return (from dur in ApproximateDurations
                        where InjuryDaysLeft <= dur.Value
                        select dur.Key).FirstOrDefault();

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
                    return string.Format("{0} ({1})", InjuryName, ApproximateDays);
                else
                    return "Healthy";
            }
        }

        public new string ToString()
        {
            return Status;
        }
    }
}