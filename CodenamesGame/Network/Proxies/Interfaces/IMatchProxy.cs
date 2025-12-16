using CodenamesGame.Domain.POCO.Match;
using CodenamesGame.MatchService;
using System;
using System.Threading.Tasks;

namespace CodenamesGame.Network.Proxies.Interfaces
{
    public interface IMatchProxy
    {
        CommunicationRequest Initialize(Guid playerID);
        void Disconnect();
        CommunicationRequest JoinMatch(MatchDM match);
        Task SendClue(string clue);
        Task NotifyTurnTimeout(MatchRoleType currentRole);
        Task NotifyPickedAgent(BoardCoordinatesDM coordinates, int newTurnLength);
        Task NotifyPickedBystander(BoardCoordinatesDM coordinates);
        Task NotifyPickedAssassin(BoardCoordinatesDM coordinates);
    }
}
