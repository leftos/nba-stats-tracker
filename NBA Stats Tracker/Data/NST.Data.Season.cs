#region Copyright Notice

// Created by Lefteris Aslanoglou, (c) 2011-2012
// 
// Implementation of thesis
// "Application Development for Basketball Statistical Analysis in Natural Language"
// under the supervision of Prof. Athanasios Tsakalidis & MSc Alexandros Georgiou
// 
// All rights reserved. Unless specifically stated otherwise, the code in this file should 
// not be reproduced, edited and/or republished without explicit permission from the 
// author.

#endregion

namespace NBA_Stats_Tracker.Data
{
    public class Season
    {
        public int ID { get; set; }
        public string Name { get; set; }
    }

    public class Division
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int ConferenceID { get; set; }
    }

    public class Conference
    {
        public int ID { get; set; }
        public string Name { get; set; }
    }

    public enum Span
    {
        Season,
        Playoffs,
        SeasonAndPlayoffs
    }
}