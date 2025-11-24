using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodenamesGame.Domain.POCO
{
    public class MatchRulesDM
    {
        public const int MAX_TIMER_TOKENS = 15;
        public const int MAX_TURN_TIMER = 180;
        public const int MAX_BYSTANDER_TOKENS = 10;

        public GamemodeDM gamemode { get; set; }
        public int TurnTimer { get; set; }
        public int TimerTokens { get; set; }
        public int BystanderTokens { get; set; }

        public static MatchRulesDM AssembleRules(MatchmakingService.MatchRules incomingRules)
        {
            return new MatchRulesDM
            {
                gamemode = AssembleGamemode(incomingRules.Gamemode),
                TurnTimer = incomingRules.TurnTimer,
                TimerTokens = incomingRules.TimerTokens,
                BystanderTokens = incomingRules.BystanderTokens,
            };
        }

        //Even if rules are set, rules for non-custom modes are set in the server.
        //So only the type of gamemode needs to be specified
        public static MatchmakingService.MatchRules AssembleSvMachtmakingMatchRules(MatchRulesDM rules)
        {
            MatchmakingService.MatchRules matchRules = new MatchmakingService.MatchRules();
            switch (rules.gamemode)
            {
                case GamemodeDM.NORMAL:
                    matchRules.Gamemode = AssembleMatchmakingSvGamemode(GamemodeDM.NORMAL);
                    return matchRules;
                case GamemodeDM.CUSTOM:
                    matchRules.Gamemode = AssembleMatchmakingSvGamemode(GamemodeDM.CUSTOM);
                    matchRules.TurnTimer = rules.TurnTimer;
                    matchRules.TimerTokens = rules.TimerTokens;
                    matchRules.BystanderTokens = rules.BystanderTokens;
                    return matchRules;
                case GamemodeDM.COUNTERINTELLIGENCE:
                    matchRules.Gamemode = AssembleMatchmakingSvGamemode(GamemodeDM.COUNTERINTELLIGENCE);
                    return matchRules;
                default:
                    matchRules.Gamemode = AssembleMatchmakingSvGamemode(GamemodeDM.NORMAL);
                    return matchRules;
            }
        }

        public static GamemodeDM AssembleGamemode(MatchmakingService.Gamemode mode)
        {
            switch(mode)
            {
                case MatchmakingService.Gamemode.NORMAL:
                    return GamemodeDM.NORMAL;
                case MatchmakingService.Gamemode.CUSTOM:
                    return GamemodeDM.CUSTOM;
                case MatchmakingService.Gamemode.COUNTERINTELLIGENCE:
                    return GamemodeDM.COUNTERINTELLIGENCE;
                default:
                    return GamemodeDM.NORMAL;
            }
        }

        public static MatchmakingService.Gamemode AssembleMatchmakingSvGamemode(GamemodeDM mode)
        {
            switch(mode)
            {
                case GamemodeDM.NORMAL:
                    return MatchmakingService.Gamemode.NORMAL;
                case GamemodeDM.CUSTOM:
                    return MatchmakingService.Gamemode.CUSTOM;
                case GamemodeDM.COUNTERINTELLIGENCE:
                    return MatchmakingService.Gamemode.COUNTERINTELLIGENCE;
                default:
                    return MatchmakingService.Gamemode.NORMAL;
            }
        }

        public static MatchService.Gamemode AssembleMatchSvGamemode(GamemodeDM mode)
        {
            switch (mode)
            {
                case GamemodeDM.NORMAL:
                    return MatchService.Gamemode.NORMAL;
                case GamemodeDM.CUSTOM:
                    return MatchService.Gamemode.CUSTOM;
                case GamemodeDM.COUNTERINTELLIGENCE:
                    return MatchService.Gamemode.COUNTERINTELLIGENCE;
                default:
                    return MatchService.Gamemode.NORMAL;
            }
        }
    }
}
