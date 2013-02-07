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

using System;
using System.Data;
using LeftosCommonLibrary;

#endregion

namespace NBA_Stats_Tracker.Data.Players
{
    /// <summary>
    ///     Contains the basic information required to initially create a player.
    /// </summary>
    public class Player
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Player" /> class.
        /// </summary>
        public Player()
        {
            ID = -1;
            Team = -1;
            LastName = "";
            FirstName = "";
            Position1 = Position.None;
            Position2 = Position.None;
            Height = "0";
            Weight = 0;
            YearOfBirth = 0;
            YearsPro = 0;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Player" /> class.
        /// </summary>
        /// <param name="id">The ID.</param>
        /// <param name="team">The team.</param>
        /// <param name="lastName">The last name.</param>
        /// <param name="firstName">The first name.</param>
        /// <param name="position1">The primary position.</param>
        /// <param name="position2">The secondary position.</param>
        public Player(int id, int team, string lastName, string firstName, Position position1, Position position2)
        {
            ID = id;
            Team = team;
            LastName = lastName;
            FirstName = firstName;
            Position1 = position1;
            Position2 = position2;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Player" /> class.
        /// </summary>
        /// <param name="dataRow">The data row containing the player information.</param>
        public Player(DataRow dataRow)
        {
            ID = DataRowCellParsers.GetInt32(dataRow, "ID");
            Team = DataRowCellParsers.GetInt32(dataRow, "TeamFin");
            LastName = DataRowCellParsers.GetString(dataRow, "LastName");
            FirstName = DataRowCellParsers.GetString(dataRow, "FirstName");
            string p1 = DataRowCellParsers.GetString(dataRow, "Position1");
            if (String.IsNullOrWhiteSpace(p1))
                Position1 = Position.None;
            else
                Position1 = (Position) Enum.Parse(typeof (Position), p1);
            string p2 = DataRowCellParsers.GetString(dataRow, "Position2");
            if (String.IsNullOrWhiteSpace(p2))
                Position1 = Position.None;
            else
                Position1 = (Position) Enum.Parse(typeof (Position), p2);
        }

        public int ID { get; set; }
        public int Team { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public Position Position1 { get; set; }
        public Position Position2 { get; set; }
        public string Height { get; set; }
        public double Weight { get; set; }
        public int YearsPro { get; set; }
        public int YearOfBirth { get; set; }
        public bool AddToAll { get; set; }
    }
}