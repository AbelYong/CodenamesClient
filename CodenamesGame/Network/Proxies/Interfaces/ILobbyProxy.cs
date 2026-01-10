using CodenamesGame.Domain.POCO;
using CodenamesGame.LobbyService;
using System;

namespace CodenamesGame.Network.Proxies.Interfaces
{
    public interface ILobbyProxy
    {
        CommunicationRequest Initialize(Guid playerID);
        void Disconnect();
        CreateLobbyRequest CreateLobby(PlayerDM player);
        CommunicationRequest SendEmailInvitation(string toAddress);
        CommunicationRequest InviteToParty(PlayerDM hostPlayer, Guid friendToInviteID, string lobbyCode);
        JoinPartyRequest JoinParty(PlayerDM joiningPlayer, string lobbyCode);
    }
}
