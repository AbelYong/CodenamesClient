using CodenamesGame.Domain.POCO;
using CodenamesGame.Network.EventArguments;
using CodenamesGame.ScoreboardService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;

namespace CodenamesGame.Network.Proxies.CallbackHandlers
{
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    public class ScoreboardCallbackHandler : IScoreboardManagerCallback
    {
        public static event EventHandler<ScoreboardEventArgs> OnLeaderboardUpdateReceived;

        public void NotifyLeaderboardUpdate(Scoreboard[] leaderboard)
        {
            if (OnLeaderboardUpdateReceived != null && leaderboard != null)
            {
                var dmList = new List<ScoreboardDM>();
                foreach (var item in leaderboard)
                {
                    dmList.Add(new ScoreboardDM
                    {
                        Username = item.Username,
                        GamesWon = item.GamesWon,
                        FastestMatch = item.FastestMatch,
                        AssassinsRevealed = item.AssassinsRevealed
                    });
                }

                OnLeaderboardUpdateReceived.Invoke(null, new ScoreboardEventArgs { Leaderboard = dmList });
            }
        }
    }
}