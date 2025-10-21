using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodenamesGame.Domain.POCO
{
    public class Gamemode
    {
        public int timerTokens {  get; set; }
        public int bystanderTokens { get; set; }
        public int turnTimer {  get; set; }
        public GamemodeName name { get; set; }

        public Gamemode(GamemodeName name)
        {
            this.name = name;
        }
        public enum GamemodeName
        {
            NORMAL = 0,
            CUSTOM = 1,
            COUNTERINTELLIGENCE = 2
        }
    }
}
