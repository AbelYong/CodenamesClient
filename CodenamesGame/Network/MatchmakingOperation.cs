using CodenamesGame.Domain.POCO.Match;
using CodenamesGame.MatchmakingService;
using CodenamesGame.Network.Proxies.CallbackHandlers;
using CodenamesGame.Network.Proxies.Interfaces;
using CodenamesGame.Network.Proxies.Wrappers;
using System;
using System.Threading.Tasks;

namespace CodenamesGame.Network
{
    public class MatchmakingOperation
    {
        private readonly IMatchmakingProxy _proxy;

        public MatchmakingOperation() : this(MatchmakingProxy.Instance)
        {

        }

        public MatchmakingOperation(IMatchmakingProxy proxy)
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

        public Task<CommunicationRequest> RequestArrangedMatch(MatchConfigurationDM matchConfig)
        {
            return _proxy.RequestArrangedMatch(matchConfig);
        }

        public void ConfirmMatch(Guid matchID)
        {
            _proxy.ConfirmMatch(matchID);
        }

        public void CancelMatch()
        {
            _proxy.CancelMatch();
        }
    }
}
