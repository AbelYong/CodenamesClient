using CodenamesGame.Domain.POCO.Match;
using CodenamesGame.MatchService;
using CodenamesGame.Network.Proxies.Interfaces;
using CodenamesGame.Network.Proxies.Wrappers;
using System;
using System.Threading.Tasks;

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

        public async Task SendClue(string clue)
        {
            await _proxy.SendClue(clue);
        }

        public async Task NotifyTurnTimeout(MatchRoleType currentRole)
        {
            await _proxy.NotifyTurnTimeout(currentRole);
        }

        public async Task NotifyPickedAgent(BoardCoordinatesDM coordinates, int newTurnLength)
        {
            await _proxy.NotifyPickedAgent(coordinates, newTurnLength);
        }
        public async Task NotifyPickedBystander(BoardCoordinatesDM coordinates)
        {
            await _proxy.NotifyPickedBystander(coordinates);
        }

        public async Task NotifyPickedAssassin(BoardCoordinatesDM coordinates)
        {
            await _proxy.NotifyPickedAssassin(coordinates);
        }

        public async Task<bool> CheckCompanionStatus()
        {
            return await _proxy.CheckCompanionStatus();
        }
    }
}
