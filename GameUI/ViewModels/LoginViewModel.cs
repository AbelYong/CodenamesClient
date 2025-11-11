using CodenamesClient.Properties.Langs;
using CodenamesGame.Domain.POCO;
using CodenamesGame.Network;
using CodenamesGame.SessionService;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CodenamesClient.GameUI.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event Action ConnectionLost;
        private readonly SessionOperation _session;
        private bool _hasPlayerConnection;
        private string _requestErrorMessage;

        public LoginViewModel()
        {
            _session = new SessionOperation();
        }

        public SessionOperation Session
        {
            get => _session;
        }

        public bool HasPlayerConnection
        {
            get => _hasPlayerConnection;
        }

        public string RequestErrorMessage
        {
            get => _requestErrorMessage;
        }

        public static PlayerDM AssembleGuest()
        {
            const int DEFAULT_AVATAR = 0;
            PlayerDM guest = new PlayerDM();
            guest.PlayerID = Guid.NewGuid();
            guest.Username = Lang.globalGuest;
            guest.AvatarID = DEFAULT_AVATAR;
            return guest;
        }

        public async Task Connect(PlayerDM player)
        {
            if (player != null)
            {
                CommunicationRequest request = await Task.Run(() => _session.Connect(player));
                if (request.IsSuccess)
                {
                    _hasPlayerConnection = true;
                }
                else
                {
                    _hasPlayerConnection = false;
                    _requestErrorMessage = Util.StatusToMessageMapper.GetSessionServiceMessage(request.StatusCode);
                    ConnectionLost?.Invoke();
                }
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
