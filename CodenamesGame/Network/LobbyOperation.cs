using CodenamesGame.Domain.POCO;
using CodenamesGame.LobbyService;
using CodenamesGame.Network.Proxies.Interfaces;
using CodenamesGame.Network.Proxies.Wrappers;
using System;

namespace CodenamesGame.Network
{
    public class LobbyOperation
    {
        private readonly ILobbyProxy _proxy;

        public LobbyOperation() : this (LobbyProxy.Instance) { }

        public LobbyOperation(ILobbyProxy proxy)
        {
            _proxy = proxy;
        }

        public CommunicationRequest Initialize(Guid playerID)
        {
            return _proxy.Initialize(playerID);
        }

        public void Disconnect()
        {
            _proxy.Disconnect();
        }

        public CreateLobbyRequest CreateLobby(PlayerDM player)
        {
            return _proxy.CreateLobby(player);
        }

        public CommunicationRequest InviteToParty(PlayerDM hostPlayer, Guid friendToInviteID, string lobbyCode)
        {
            return _proxy.InviteToParty(hostPlayer, friendToInviteID, lobbyCode);
        }

        public JoinPartyRequest JoinParty(PlayerDM joiningPlayer, string lobbyCode)
        {
            return _proxy.JoinParty(joiningPlayer, lobbyCode);
        }
    }
}
