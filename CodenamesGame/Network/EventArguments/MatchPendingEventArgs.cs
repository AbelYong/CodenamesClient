using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodenamesGame.Network.EventArguments
{
    public class MatchPendingEventArgs
    {
        public Guid RequesterID { get; set; }
        public Guid CompanionID { get; set; }
    }
}
