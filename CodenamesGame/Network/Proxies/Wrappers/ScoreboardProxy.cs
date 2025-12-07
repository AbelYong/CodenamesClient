using CodenamesGame.Domain.POCO;
using CodenamesGame.Network.Proxies.CallbackHandlers;
using CodenamesGame.ScoreboardService;
using System;
using System.ServiceModel;
using System.Threading.Tasks;

namespace CodenamesGame.Network.Proxies.Wrappers
{
    public class ScoreboardProxy : DuplexClientBase<IScoreboardManager>, IScoreboardManager
    {
        public ScoreboardProxy(ScoreboardCallbackHandler callbackInstance)
            : base(new InstanceContext(callbackInstance), "NetTcpBinding_IScoreboardManager")
        {
        }

        public void SubscribeToScoreboardUpdates(Guid playerID)
        {
            Channel.SubscribeToScoreboardUpdates(playerID);
        }

        public void UnsubscribeFromScoreboardUpdates(Guid playerID)
        {
            Channel.UnsubscribeFromScoreboardUpdates(playerID);
        }

        public Scoreboard GetMyScore(Guid playerID)
        {
            return Channel.GetMyScore(playerID);
        }

        public Task SubscribeToScoreboardUpdatesAsync(Guid playerID)
        {
            return Channel.SubscribeToScoreboardUpdatesAsync(playerID);
        }

        public Task UnsubscribeFromScoreboardUpdatesAsync(Guid playerID)
        {
            return Channel.UnsubscribeFromScoreboardUpdatesAsync(playerID);
        }

        public Task<Scoreboard> GetMyScoreAsync(Guid playerID)
        {
            return Channel.GetMyScoreAsync(playerID);
        }
    }
}