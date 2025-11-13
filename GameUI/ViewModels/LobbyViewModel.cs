using CodenamesGame.Domain.POCO;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CodenamesClient.Properties.Langs;

namespace CodenamesClient.GameUI.ViewModels
{
    public class LobbyViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private string _gamemodeName;
        private int _timerTokens;
        private int _bystanderTokens;
        private int _turnTimer;
        private bool _isCustomGame;

        public string GamemodeName
        {
            get => _gamemodeName;
            set
            {
                _gamemodeName = value;
                OnPropertyChanged();
            }
        }

        public int TimerTokens
        {
            get => _timerTokens;
            set
            {
                _timerTokens = value;
                OnPropertyChanged();
            }
        }
        
        public int BystanderTokens
        {
            get => _bystanderTokens;
            set
            {
                _bystanderTokens = value;
                OnPropertyChanged();
            }
        }
        
        public int TurnTimer
        {
            get => _turnTimer;
            set
            {
                _turnTimer = value;
                OnPropertyChanged();
            }
        }

        public bool IsCustomGame
        {
            get => _isCustomGame;
            set
            {
                _isCustomGame = value;
                OnPropertyChanged();
            }
        }

        public LobbyViewModel(GamemodeDM gamemode)
        {
            switch (gamemode)
            {
                case (GamemodeDM.NORMAL):
                    GamemodeName = Lang.gamemodeNormalGame;
                    LoadDefaultRules();
                    break;
                case (GamemodeDM.CUSTOM):
                    GamemodeName = Lang.gamemodeCustomGame;
                    LoadDefaultRules();
                    break;
                case (GamemodeDM.COUNTERINTELLIGENCE):
                    GamemodeName = Lang.gamemodeCounterintelligenceMode;
                    LoadDefaultRules();
                    break;
                default:
                    GamemodeName = "Gamemode";
                    LoadDefaultRules();
                    break;
            }
        }

        private void LoadDefaultRules()
        {
            const int NORMAL_TIMER_TOKENS = 9;
            const int NORMAL_BYSTANDER_TOKENS = 0;
            const int NORMAL_TIMER = 30;
            TimerTokens = NORMAL_TIMER_TOKENS;
            BystanderTokens = NORMAL_BYSTANDER_TOKENS;
            TurnTimer = NORMAL_TIMER;
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
