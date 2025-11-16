using CodenamesGame.MatchmakingService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodenamesGame.Network
{
    public class MatchmakingCallbackHandler : IMatchmakingManagerCallback
    {
        public void NotifyMatchCanceled(Guid matchID)
        {
            throw new NotImplementedException();
        }

        public void NotifyMatchReady(Match match)
        {
            throw new NotImplementedException();
        }

        public void NotifyPlayersReady(Guid matchID)
        {
            throw new NotImplementedException();
        }

        public void NotifyRequestPending(Guid RequesterID, Guid CompanionID)
        {
            throw new NotImplementedException();
        }
    }
}
