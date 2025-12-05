using CodenamesGame.Domain.POCO.Match;
using CodenamesGame.MatchmakingService;
using CodenamesGame.Network.EventArguments;
using System;
using System.ServiceModel;

namespace CodenamesGame.Network.Proxies.CallbackHandlers
{
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class MatchmakingCallbackHandler : IMatchmakingManagerCallback
    {
        public static event EventHandler<MatchCanceledEventArgs> OnMatchCanceled;
        public static event EventHandler<MatchPendingEventArgs> OnMatchPending;
        public static event EventHandler<MatchDM> OnMatchReady;
        public static event EventHandler<Guid> OnPlayersReady;
        private Guid _currentPlayerID;

        public MatchmakingCallbackHandler(Guid playerID)
        {
            if (playerID != Guid.Empty)
            {
                _currentPlayerID = playerID;
            }
        }

        public void NotifyMatchCanceled(Guid matchID, StatusCode reason)
        {
            if (matchID != Guid.Empty)
            {
                OnMatchCanceled.Invoke(null, new MatchCanceledEventArgs { MatchID = matchID, Reason = reason });
            }
        }

        public void NotifyMatchReady(Match match)
        {
            if (match != null)
            {
                MatchDM auxMatch = MatchDM.AssembleMatch(match, _currentPlayerID);
                OnMatchReady.Invoke(null, auxMatch);
            }
        }

        public void NotifyPlayersReady(Guid matchID)
        {
            OnPlayersReady.Invoke(null, matchID);
        }

        public void NotifyRequestPending(Guid requesterID, Guid companionID)
        {
            OnMatchPending.Invoke(null, new MatchPendingEventArgs { RequesterID = requesterID, CompanionID = companionID });
        }
    }
}
