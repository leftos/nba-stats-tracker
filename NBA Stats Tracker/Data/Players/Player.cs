using System;
using System.Data;
using LeftosCommonLibrary;

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
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Player" /> class.
        /// </summary>
        /// <param name="ID">The ID.</param>
        /// <param name="Team">The team.</param>
        /// <param name="LastName">The last name.</param>
        /// <param name="FirstName">The first name.</param>
        /// <param name="Position1">The primary position.</param>
        /// <param name="Position2">The secondary position.</param>
        public Player(int ID, string Team, string LastName, string FirstName, Position Position1, Position Position2)
        {
            this.ID = ID;
            this.Team = Team;
            this.LastName = LastName;
            this.FirstName = FirstName;
            this.Position1 = Position1;
            this.Position2 = Position2;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Player" /> class.
        /// </summary>
        /// <param name="dataRow">The data row containing the player information.</param>
        public Player(DataRow dataRow)
        {
            ID = Tools.getInt(dataRow, "ID");
            Team = Tools.getString(dataRow, "TeamFin");
            LastName = Tools.getString(dataRow, "LastName");
            FirstName = Tools.getString(dataRow, "FirstName");
            string p1 = Tools.getString(dataRow, "Position1");
            if (String.IsNullOrWhiteSpace(p1))
                Position1 = Position.None;
            else
                Position1 = (Position) Enum.Parse(typeof (Position), p1);
            string p2 = Tools.getString(dataRow, "Position2");
            if (String.IsNullOrWhiteSpace(p2))
                Position1 = Position.None;
            else
                Position1 = (Position) Enum.Parse(typeof (Position), p2);
        }

        public int ID { get; set; }
        public string Team { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public Position Position1 { get; set; }
        public Position Position2 { get; set; }
        public bool AddToAll { get; set; }
    }
}