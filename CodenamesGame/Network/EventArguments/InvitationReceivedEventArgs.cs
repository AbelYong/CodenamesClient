using CodenamesGame.Domain.POCO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodenamesGame.Network.EventArguments
{
    public class InvitationReceivedEventArgs : EventArgs
    {
        public PlayerDM Player { get; set; }

        public string LobbyCode { get; set; }
    }
}
