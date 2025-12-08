using CodenamesGame.Domain.POCO;
using CodenamesGame.Network.Proxies.Interfaces;
using CodenamesGame.Network.Proxies.Wrappers;
using System;

namespace CodenamesGame.Network
{
    public class ScoreboardOperation
    {
        private readonly IScoreboardProxy _proxy;

        public ScoreboardOperation() : this (ScoreboardProxy.Instance) { }

        public ScoreboardOperation(IScoreboardProxy proxy)
        {
            _proxy = proxy;
        }

        public void Initialize(Guid playerID)
        {
            _proxy.Initialize(playerID);
        }

        public void Disconnect()
        {
            _proxy.Disconnect();
        }

        public ScoreboardDM GetMyScore(Guid playerID)
        {
            return _proxy.GetMyScore(playerID);
        }
    }
}