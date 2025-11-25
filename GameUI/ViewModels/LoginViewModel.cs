using CodenamesClient.Properties.Langs;
using CodenamesGame.Domain.POCO;
using CodenamesGame.Network;
using CodenamesGame.SessionService;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CodenamesClient.GameUI.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private static readonly Random _random = new Random();
        public event PropertyChangedEventHandler PropertyChanged;
        public event Action ConnectionLost;
        private bool _hasPlayerConnection;
        private bool _btnLoginEnabled;
        private bool _btnSignInGuestEnabled;
        private string _requestErrorMessage;

        public LoginViewModel()
        {
            BtnLoginEnabled = true;
            BtnSignInGuestEnabled = true;
        }

        public bool HasPlayerConnection
        {
            get => _hasPlayerConnection;
        }

        public bool BtnLoginEnabled
        {
            get => _btnLoginEnabled;
            set
            {
                _btnLoginEnabled = value;
                OnPropertyChanged();
            }
        }

        public bool BtnSignInGuestEnabled
        {
            get => _btnSignInGuestEnabled;
            set
            {
                _btnSignInGuestEnabled = value;
                OnPropertyChanged();
            }
        }

        public string RequestErrorMessage
        {
            get => _requestErrorMessage;
        }

        public async Task Connect(PlayerDM player)
        {
            if (player != null)
            {
                BtnLoginEnabled = false;
                BtnSignInGuestEnabled = false;
                CommunicationRequest request = await Task.Run(() => SessionOperation.Instance.Initialize(player));
                if (request.IsSuccess)
                {
                    _hasPlayerConnection = true;
                    BtnLoginEnabled = true;
                    BtnSignInGuestEnabled = true;
                }
                else
                {
                    _hasPlayerConnection = false;
                    _requestErrorMessage = Util.StatusToMessageMapper.GetSessionServiceMessage(request.StatusCode);
                    BtnLoginEnabled = true;
                    BtnSignInGuestEnabled = true;
                    ConnectionLost?.Invoke();
                }
            }
        }

        public static PlayerDM AssembleGuest()
        {
            const int DEFAULT_AVATAR = 0;
            PlayerDM guest = new PlayerDM();
            guest.PlayerID = Guid.NewGuid();
            guest.Username = string.Format("{0}{1}", Lang.globalGuest, GenerateGuestSuffix());
            guest.AvatarID = DEFAULT_AVATAR;
            return guest;
        }

        private static string GenerateGuestSuffix()
        {
            const string chars = "0123456789";
            const int suffixLength = 4;
            StringBuilder sequence = new StringBuilder(suffixLength);

            for (int i = 0; i < suffixLength; i++)
            {
                int index = _random.Next(chars.Length);
                sequence.Append(chars[index]);
            }
            return string.Format("#{0}", sequence.ToString());
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
