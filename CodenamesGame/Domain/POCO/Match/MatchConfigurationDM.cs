using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodenamesGame.Domain.POCO.Match
{
    public class MatchConfigurationDM
    {
        public PlayerDM Requester { get; set; }
        public PlayerDM Companion { get; set; }
        public MatchRulesDM Rules { get; set; }

        public MatchConfigurationDM()
        {
            Rules = new MatchRulesDM();
        }

        public static MatchmakingService.MatchConfiguration AssembleMatchmakingSvMatchConfig(MatchConfigurationDM config)
        {
            return new MatchmakingService.MatchConfiguration
            {
                Requester = PlayerDM.AssembleMatchmakingSvPlayer(config.Requester),
                Companion = PlayerDM.AssembleMatchmakingSvPlayer(config.Companion),
                MatchRules = MatchRulesDM.AssembleSvMachtmakingMatchRules(config.Rules)
            };
        }
    }
}
