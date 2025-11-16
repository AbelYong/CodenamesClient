using CodenamesGame.Domain.POCO;
using CodenamesGame.LobbyService;
using CodenamesGame.Network.EventArguments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace CodenamesGame.Network
{
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class LobbyCallbackHandler : ILobbyManagerCallback
    {
        public static event EventHandler<PlayerEventArgs> OnPlayerJoined;
        public static event EventHandler<InvitationReceivedEventArgs> OnInvitationReceived;
        public static event EventHandler<Guid> OnPlayerLeft;

        public void NotifyMatchInvitationAccepted(Player byPlayer)
        {
            if (byPlayer != null)
            {
                PlayerDM player = PlayerDM.AssemblePlayer(byPlayer);
                OnPlayerJoined.Invoke(null, 
                    new PlayerEventArgs { Player = player });
            }
        }

        public void NotifyMatchInvitationReceived(Player fromPlayer, string lobbyCode)
        {
            if (fromPlayer != null)
            {
                PlayerDM player = PlayerDM.AssemblePlayer(fromPlayer);
                OnInvitationReceived.Invoke(null, 
                    new InvitationReceivedEventArgs { Player = player, LobbyCode = lobbyCode });
            }
        }

        public void NotifyPartyAbandoned(Guid leavingPlayerID)
        {
            OnPlayerLeft.Invoke(null, leavingPlayerID);
        }
    }
}
