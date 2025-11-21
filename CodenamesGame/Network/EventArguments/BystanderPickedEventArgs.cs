using CodenamesGame.MatchService;

namespace CodenamesGame.Network.EventArguments
{
    public class BystanderPickedEventArgs
    {
        public TokenType TokenToUpdate { get; set; }
        public int RemainingTokens { get; set; }
    }
}
