using CodenamesGame.Domain.POCO.Match;
using CodenamesGame.MatchmakingService;
using System;
using System.Threading.Tasks;

namespace CodenamesGame.Network.Proxies.Interfaces
{
    public interface IMatchmakingProxy
    {
        CommunicationRequest Initialize(Guid playerID);
        void Disconnect();
        Task<CommunicationRequest> RequestArrangedMatch(MatchConfigurationDM matchConfig);
        void ConfirmMatch(Guid matchID);
        void CancelMatch();

    }
}
