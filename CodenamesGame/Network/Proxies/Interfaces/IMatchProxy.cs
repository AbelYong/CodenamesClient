using CodenamesGame.Domain.POCO.Match;
using CodenamesGame.MatchService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodenamesGame.Network.Proxies.Interfaces
{
    public interface IMatchProxy
    {
        CommunicationRequest Initialize(Guid playerID);
        void Disconnect();
        CommunicationRequest JoinMatch(MatchDM match);
        void SendClue(string clue);
        void NotifyTurnTimeout(MatchRoleType currentRole);
        void NotifyPickedAgent(BoardCoordinatesDM coordinates, int newTurnLength);
        void NotifyPickedBystander(BoardCoordinatesDM coordinates);
        void NotifyPickedAssassin(BoardCoordinatesDM coordinates);
    }
}
