using System;

namespace NBA_Stats_Tracker.Data.Players.Contracts
{
    [Serializable]
    public enum PlayerContractOption : byte
    {
        None = 0,
        Team = 1,
        Player = 2,
        Team2Yr = 3
    }
}