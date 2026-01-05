using CodenamesGame.Domain.POCO;
using System;

namespace CodenamesGame.Network.EventArguments
{
    public class InvitationReceivedEventArgs : EventArgs
    {
        public PlayerDM Player { get; set; }

        public string LobbyCode { get; set; }
    }
}
