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
            ID = ParseCell.GetInt32(dataRow, "ID");
            Team = ParseCell.GetInt32(dataRow, "TeamFin");
            LastName = ParseCell.GetString(dataRow, "LastName");
            FirstName = ParseCell.GetString(dataRow, "FirstName");
            string p1 = ParseCell.GetString(dataRow, "Position1");
            if (String.IsNullOrWhiteSpace(p1))
            {
                Position1 = Position.None;
            }
            else
            {
                Position1 = (Position) Enum.Parse(typeof(Position), p1);
            }
            string p2 = ParseCell.GetString(dataRow, "Position2");
            if (String.IsNullOrWhiteSpace(p2))
            {
                Position1 = Position.None;
            }
            else
            {
                Position1 = (Position) Enum.Parse(typeof(Position), p2);
            }
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