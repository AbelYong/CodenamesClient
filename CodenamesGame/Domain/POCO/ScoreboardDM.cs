using System;
using System.Runtime.Serialization;

namespace CodenamesGame.Domain.POCO
{
    public class ScoreboardDM
    {
        public string Username { get; set; }
        public int GamesWon { get; set; }
        public string FastestMatch { get; set; }
        public int AssassinsRevealed { get; set; }
    }
}