using CodenamesGame.Domain.POCO;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CodenamesClient.Properties.Langs;
using System.Windows.Input;

namespace CodenamesClient.GameUI.ViewModels
{
    public class LobbyViewModel : INotifyPropertyChanged
    {
        private string _gamemodeName;
        private int _timerTokens;
        private int _bystanderTokens;
        private int _turnTimer;
        private bool _isCustomGame;

        public string GamemodeName
        {
            get
            {
                return _gamemodeName;
            }
            set
            {
                _gamemodeName = value;
                OnPropertyChanged();
            }
        }

        public int TimerTokens
        {
            get
            {
                return _timerTokens;
            }
            set
            {
                _timerTokens = value;
                OnPropertyChanged();
            }
        }
        
        public int BystanderTokens
        {
            get
            {
                return _bystanderTokens;
            }
            set
            {
                _bystanderTokens = value;
                OnPropertyChanged();
            }
        }
        
        public int TurnTimer
        {
            get
            {
                return _turnTimer;
            }
            set
            {
                _turnTimer = value;
                OnPropertyChanged();
            }
        }

        public bool IsCustomGame
        {
            get
            {
                return _isCustomGame;
            }
            set
            {
                _isCustomGame = value;
                OnPropertyChanged();
            }
        }

        public LobbyViewModel(Gamemode gamemode)
        {
            switch (gamemode.name)
            {
                case (Gamemode.GamemodeName.NORMAL):
                    GamemodeName = Lang.gamemodeNormalGame;
                    LoadDefaultRules();
                    break;
                case (Gamemode.GamemodeName.CUSTOM):
                    GamemodeName = Lang.gamemodeCustomGame;
                    LoadDefaultRules();
                    break;
                case (Gamemode.GamemodeName.COUNTERINTELLIGENCE):
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
