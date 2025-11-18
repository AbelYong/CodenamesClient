using CodenamesGame.MatchmakingService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodenamesGame.Network.EventArguments
{
    public class MatchCanceledEventArgs
    {
        public Guid MatchID { get; set; }
        public StatusCode Reason { get; set; }
    }
}
