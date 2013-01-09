using System;

namespace NBA_Stats_Tracker.Data.Players
{
    [Serializable]
    public enum PlayerContractOption
    {
        None = 0,
        Team = 1,
        Player = 2,
        Team2Yr = 3
    }
}