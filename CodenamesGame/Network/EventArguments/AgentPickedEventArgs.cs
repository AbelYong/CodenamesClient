using CodenamesGame.Domain.POCO.Match;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodenamesGame.Network.EventArguments
{
    public class AgentPickedEventArgs
    {
        public BoardCoordinatesDM Coordinates {  get; set; }
        public int NewTurnLength { get; set; }
    }
}
