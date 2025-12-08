using System;
using CodenamesGame.Domain.POCO;

namespace CodenamesGame.Network.Proxies.Interfaces
{
    public interface IScoreboardProxy
    {
        void Initialize(Guid playerID);
        void Disconnect();
        ScoreboardDM GetMyScore(Guid playerID);
    }
}
