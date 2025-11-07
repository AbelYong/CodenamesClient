using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodenamesGame.Domain.POCO
{
    public class MatchDM
    {
        public GamemodeDM Rules { get; set; }
        public PlayerDM Player { get; set; }
        public PlayerDM Companion { get; set; }

        public MatchDM()
        {

        }
    }
}
