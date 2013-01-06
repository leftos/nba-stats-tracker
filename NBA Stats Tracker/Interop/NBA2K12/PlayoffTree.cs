using System;
using System.Runtime.Serialization;

namespace NBA_Stats_Tracker.Interop.NBA2K12
{
    /// <summary>
    /// Implements the Playoff Tree structure, containing the 16 teams participating in the playoffs.
    /// </summary>
    [Serializable]
    public class PlayoffTree : ISerializable
    {
        public readonly string[] teams = new string[16];
        public bool done;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayoffTree" /> class.
        /// </summary>
        public PlayoffTree()
        {
            teams[0] = "Invalid";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayoffTree" /> class. Used for serialization.
        /// </summary>
        /// <param name="info">The info.</param>
        /// <param name="ctxt">The CTXT.</param>
        protected PlayoffTree(SerializationInfo info, StreamingContext ctxt)
        {
            teams = (string[]) info.GetValue("teams", typeof (string[]));
            done = (bool) info.GetValue("done", typeof (bool));
        }

        #region ISerializable Members

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException("info");

            GetObjectData(info, context);
        }

        #endregion

        protected virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("teams", teams);
            info.AddValue("done", done);
        }
    }
}