using CodenamesGame.Domain.POCO;
using System;
using System.Collections.Generic;

namespace CodenamesGame.Network.EventArguments
{
    public class ScoreboardEventArgs : EventArgs
    {
        public List<ScoreboardDM> Leaderboard { get; set; }
    }
}