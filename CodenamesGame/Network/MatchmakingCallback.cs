using CodenamesGame.MatchmakingService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodenamesGame.Network
{
    public class MatchmakingCallback : IMatchmakingManagerCallback
    {
        public void NotifyMatchCanceled()
        {
            throw new NotImplementedException();
        }

        public void ReceiveMatch(Match match)
        {
            throw new NotImplementedException();
        }
    }
}
