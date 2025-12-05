using CodenamesGame.Domain.POCO.Match;
using CodenamesGame.MatchService;
using CodenamesGame.Network.Proxies.Interfaces;
using CodenamesGame.Network.Proxies.Wrappers;
using System;

namespace CodenamesGame.Network
{
    public class MatchOperation
    {
        private readonly IMatchProxy _proxy;

        public MatchOperation() : this(MatchProxy.Instance) { }

        public MatchOperation(IMatchProxy proxy)
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

        public CommunicationRequest JoinMatch(MatchDM match)
        {
            return _proxy.JoinMatch(match);
        }

        public void SendClue(string clue)
        {
            _proxy.SendClue(clue);
        }

        public void NotifyTurnTimeout(MatchRoleType currentRole)
        {
            _proxy.NotifyTurnTimeout(currentRole);
        }

        public void NotifyPickedAgent(BoardCoordinatesDM coordinates, int newTurnLength)
        {
            _proxy.NotifyPickedAgent(coordinates, newTurnLength);
        }
        public void NotifyPickedBystander(BoardCoordinatesDM coordinates)
        {
            _proxy.NotifyPickedBystander(coordinates);
        }

        public void NotifyPickedAssassin(BoardCoordinatesDM coordinates)
        {
            _proxy.NotifyPickedAssassin(coordinates);
        }
    }
}
