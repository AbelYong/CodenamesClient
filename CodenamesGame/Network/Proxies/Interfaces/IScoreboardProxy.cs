using System;
using CodenamesGame.Domain.POCO;
using CodenamesGame.ScoreboardService;

namespace CodenamesGame.Network.Proxies.Interfaces
{
    public interface IScoreboardProxy
    {
        void Initialize(Guid playerID);
        void Disconnect();
        ScoreboardRequest GetMyScore(Guid playerID);
        ScoreboardRequest GetTopPlayers();
    }
}
