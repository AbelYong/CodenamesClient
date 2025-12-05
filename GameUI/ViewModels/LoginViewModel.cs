using CodenamesClient.Operation.Network.Oneway;
using CodenamesClient.Operation.Network.Duplex;
using CodenamesClient.Properties.Langs;
using CodenamesClient.Validation;
using CodenamesGame.Domain.POCO;
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
        public event EventHandler<PlayerDM> NavigateToMainMenu;
        public event Action RaiseError;
        private bool _hasPlayerConnection;
        private bool _btnLoginEnabled;
        private bool _btnSignInGuestEnabled;
        private string _requestErrorMessage;
        private string _usernameErrorMessage;
        private string _passwordErrorMessage;

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

        public string UsernameErrorMessage
        {
            get => _usernameErrorMessage;
            set
            {
                _usernameErrorMessage = value;
                OnPropertyChanged();
            }
        }

        public string PasswordErrorMessage
        {
            get => _passwordErrorMessage;
            set
            {
                _passwordErrorMessage = value;
                OnPropertyChanged();
            }
        }

        public async Task Login(string username, string password)
        {
            CodenamesGame.AuthenticationService.LoginRequest request = OnewayNetworkManager.Instance.Authenticate(username, password);
            if (ValidateLoginData(username, password))
            {
                if (request.IsSuccess)
                {
                    await BeginSession(request.UserID);
                }
                else
                {
                    if (request.StatusCode == CodenamesGame.AuthenticationService.StatusCode.UNAUTHORIZED)
                    {
                        ShowFailedLoginMessage();
                    }
                    else
                    {
                        _requestErrorMessage = Util.StatusToMessageMapper.GetAuthServiceMessage(request.StatusCode);
                        RaiseError?.Invoke();
                    }
                }
            }
        }

        private bool ValidateLoginData(string username, string password)
        {
            ClearFields();
            UsernameErrorMessage = LoginValidation.ValidateUsername(username);

            PasswordErrorMessage = LoginValidation.ValidatePassword(password);

            return string.IsNullOrEmpty(UsernameErrorMessage) && string.IsNullOrEmpty(PasswordErrorMessage);
        }

        private void ClearFields()
        {
            UsernameErrorMessage = string.Empty;
            PasswordErrorMessage = string.Empty;
        }

        private void ShowFailedLoginMessage()
        {
            UsernameErrorMessage = Lang.loginWrongCredentials;
            PasswordErrorMessage = Lang.loginWrongCredentials;
        }

        public static void BeginPasswordReset(string user, string email)
        {
            OnewayNetworkManager.Instance.BeginPasswordReset(user, email);
        }

        public static CodenamesGame.AuthenticationService.ResetResult CompletePasswordReset(string user, string code, string password)
        {
            return OnewayNetworkManager.Instance.CompletePasswordReset(user, code, password);
        }

        public async Task BeginSession(Guid? userID)
        {
            if (userID != null)
            {
                Guid auxUserID = userID.Value;
                PlayerDM player = OnewayNetworkManager.Instance.GetPlayer(auxUserID);
                if (player != null && player.PlayerID != Guid.Empty)
                {
                    await Connect(player);
                    NavigateToMainMenu?.Invoke(this, player);
                }
                else
                {
                    _requestErrorMessage = Lang.globalErrorProfileNotFound;
                    RaiseError?.Invoke();
                }
            }
            else
            {
                PlayerDM guest = LoginViewModel.AssembleGuest();
                await Connect(guest);
                NavigateToMainMenu?.Invoke(this, guest);
            }
        }

        private async Task Connect(PlayerDM player)
        {
            if (player != null)
            {
                BtnLoginEnabled = false;
                BtnSignInGuestEnabled = false;
                CodenamesGame.SessionService.CommunicationRequest request = await Task.Run(() => DuplexNetworkManager.Instance.ConnectToSessionService(player));
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
                    RaiseError?.Invoke();
                }
            }
        }

        public static PlayerDM AssembleGuest()
        {
            const int DEFAULT_AVATAR = 0;
            PlayerDM guest = new PlayerDM();
            guest.IsGuest = true;
            guest.PlayerID = Guid.NewGuid();
            guest.Username = string.Format("{0}{1}", Lang.globalGuest, GenerateGuestSuffix());
            guest.AvatarID = DEFAULT_AVATAR;
            return guest;
        }

        private static string GenerateGuestSuffix()
        {
            const string numbers = "0123456789";
            const string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const int suffixLength = 6;

            StringBuilder sequence = new StringBuilder(suffixLength);
            for (int i = 0; i < suffixLength; i++)
            {
                int index = i % 2 == 0 ? _random.Next(numbers.Length) : _random.Next(letters.Length);
                char auxChar = i % 2 == 0 ? numbers[index] : letters[index]; 
                sequence.Append(auxChar);
            }
            return string.Format("#{0}", sequence.ToString());
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
