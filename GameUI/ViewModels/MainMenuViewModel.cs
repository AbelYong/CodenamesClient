using CodenamesClient.Properties.Langs;
using CodenamesGame.Domain.POCO;
using CodenamesGame.Network;
using CodenamesGame.SessionService;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace CodenamesClient.GameUI.ViewModels
{
    public class MainMenuViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private readonly SessionOperation _session;
        private PlayerDM _player;
        private string _username;
        private bool _isPlayerGuest;

        public MainMenuViewModel(PlayerDM player, SessionOperation session, bool isGuest)
        {
            _session = session;
            IsPlayerGuest = isGuest;
            Player = player;
            Username = Player.Username;
        }

        public PlayerDM Player
        {
            get => _player;
            set
            {
                _player = value;
                OnPropertyChanged();
            }
        }

        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged();
            }
        }

        public bool IsPlayerGuest
        {
            get => _isPlayerGuest;
            set
            {
                _isPlayerGuest = value;
                OnPropertyChanged();
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void Disconnect(PlayerDM player)
        {
            if (_player != null)
            {
                _session.Disconnect(player);
            }
        }
    }
}
