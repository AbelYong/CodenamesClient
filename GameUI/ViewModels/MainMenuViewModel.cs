using CodenamesClient.Properties.Langs;
using CodenamesGame.Domain.POCO;
using CodenamesGame.Network;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
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

        public MainMenuViewModel(PlayerDM player)
        {
            _session = new SessionOperation();
            if (player != null)
            {
                Player = player;
            }
            else
            {
                Player = AssembleGuest();
                IsPlayerGuest = true;
            }
            Username = Player.Username;
            Connect(Player);
        }

        public PlayerDM Player
        {
            get
            {
                return _player;
            }
            set
            {
                _player = value;
                OnPropertyChanged();
            }
        }

        public string Username
        {
            get
            {
                return _username;
            }
            set
            {
                _username = value;
                OnPropertyChanged();
            }
        }

        public bool IsPlayerGuest
        {
            get
            {
                return _isPlayerGuest;
            }
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

        private void Connect(PlayerDM player)
        {
            if (player != null)
            {
                _session.Connect(player);
            }
        }

        public void Disconnect(PlayerDM player)
        {
            if (_player != null)
            {
                _session.Disconnect(player);
            }
        }

        private static PlayerDM AssembleGuest()
        {
            const int DEFAULT_AVATAR = 0;
            PlayerDM guest = new PlayerDM();
            guest.PlayerID = Guid.NewGuid();
            guest.Username = Lang.globalGuest;
            guest.AvatarID = DEFAULT_AVATAR;
            return guest;
        }
    }
}
