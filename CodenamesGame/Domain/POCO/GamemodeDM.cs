
namespace CodenamesGame.Domain.POCO
{
    public class GamemodeDM
    {
        public int timerTokens {  get; set; }
        public int bystanderTokens { get; set; }
        public int turnTimer {  get; set; }
        public GamemodeName name { get; set; }

        public GamemodeDM(GamemodeName name)
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
