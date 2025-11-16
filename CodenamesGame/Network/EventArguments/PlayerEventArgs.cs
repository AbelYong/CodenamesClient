using CodenamesGame.Domain.POCO;
using System;

namespace CodenamesGame.Network.EventArguments
{
    /// <summary>
    /// Arguments for events involving players.
    /// </summary>
    public class PlayerEventArgs : EventArgs
    {
        public PlayerDM Player { get; set; }
    }
}
