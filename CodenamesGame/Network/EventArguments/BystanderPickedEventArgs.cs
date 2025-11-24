using CodenamesGame.Domain.POCO.Match;
using CodenamesGame.MatchService;

namespace CodenamesGame.Network.EventArguments
{
    public class BystanderPickedEventArgs
    {
        public BoardCoordinatesDM Coordinates { get; set; }
        public TokenType TokenToUpdate { get; set; }
        public int RemainingTokens { get; set; }
    }
}
