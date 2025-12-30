using CodenamesClient.Operation.Network.Duplex;
using CodenamesClient.Operation.Network.Oneway;
using CodenamesClient.Operation.Validation;
using CodenamesClient.Properties.Langs;
using CodenamesClient.Validation;
using CodenamesGame.Domain.POCO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CodenamesClient.GameUI.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        private static readonly Random _random = new Random();
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
        private readonly Dictionary<string, List<string>> _errors = new Dictionary<string, List<string>>();
        public event EventHandler<PlayerDM> NavigateToMainMenu;
        public event Action RaiseError;
        private bool _hasPlayerConnection;
        private bool _btnLoginEnabled;
        private bool _btnSignInGuestEnabled;
        private string _requestErrorMessage;
        private string _usernameErrorMessage;
        private string _passwordErrorMessage;
        private string _newPassword;
        private string _confirmPassword;

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

        public string NewPassword
        {
            get => _newPassword;
            set
            {
                var trimmedPassword = value?.Trim() ?? string.Empty;

                if (Set(ref _newPassword, trimmedPassword))
                {
                    OnPropertyChanged(nameof(PwHasMinLength));
                    OnPropertyChanged(nameof(PwWithinMaxLength));
                    OnPropertyChanged(nameof(PwHasUpper));
                    OnPropertyChanged(nameof(PwHasLower));
                    OnPropertyChanged(nameof(PwHasDigit));
                    OnPropertyChanged(nameof(PwHasSpecial));
                    OnPropertyChanged(nameof(IsPasswordValid));
                    ValidateProperty(nameof(ConfirmPassword));
                    OnPropertyChanged(nameof(CanSubmit));
                }
                else if (value != trimmedPassword)
                {
                    OnPropertyChanged(nameof(NewPassword));
                }
            }
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                if (Set(ref _confirmPassword, value))
                {
                    ValidateProperty(nameof(ConfirmPassword));
                    OnPropertyChanged(nameof(CanSubmit));
                }
            }
        }

        public bool CanSubmit
        {
            get => (!HasErrors && IsPasswordValid && PasswordsMatch);
        }

        public bool HasErrors
        {
            get => (_errors.Any(kv => kv.Value?.Count > 0));
        }

        public bool IsPasswordValid
        {
            get => (PwHasMinLength && PwWithinMaxLength && PwHasUpper && PwHasLower && PwHasDigit && PwHasSpecial);
        }

        private bool PasswordsMatch
        {
            get => (!string.IsNullOrEmpty(ConfirmPassword) && ConfirmPassword == NewPassword);
        }

        public static string PwMinLengthText
        {
            get => string.Format(Lang.signInPasswordMinLength, PasswordValidation.PASSWORD_MIN_LENGTH);
        }

        public static string PwMaxLengthText
        {
            get => string.Format(Lang.signInPasswordMaxLength, PasswordValidation.PASSWORD_MAX_LENGTH);
        }

        public bool PwHasMinLength
        {
            get => PasswordValidation.MeetsMinLength(NewPassword);
        }

        public bool PwWithinMaxLength
        {
            get => PasswordValidation.WithinMaxLength(NewPassword);
        }

        public bool PwHasUpper
        {
            get => PasswordValidation.HasUpper(NewPassword);
        }

        public bool PwHasLower
        {
            get => PasswordValidation.HasLower(NewPassword);
        }

        public bool PwHasDigit
        {
            get => PasswordValidation.HasDigit(NewPassword);
        }

        public bool PwHasSpecial
        {
            get => PasswordValidation.HasSpecial(NewPassword);
        }

        public void TriggerPasswordValidation()
        {
            ValidateProperty(nameof(NewPassword));
        }

        private void ValidateProperty(string propertyName)
        {
            IEnumerable<string> errors = Enumerable.Empty<string>();

            switch (propertyName)
            {
                case nameof(NewPassword):
                    if (!IsPasswordValid)
                    {
                        errors = new[] { Lang.signInInvalidPassword };
                    }
                    break;

                case nameof(ConfirmPassword):
                    if (IsPasswordValid && !string.IsNullOrWhiteSpace(ConfirmPassword) && ConfirmPassword != NewPassword)
                    {
                        errors = new[] { Lang.signInPasswordsDoNotMatch };
                    }
                    break;
            }

            SetErrors(propertyName, errors);
            OnPropertyChanged(nameof(CanSubmit));
        }

        private void SetErrors(string propertyName, IEnumerable<string> errors)
        {
            var list = errors?.Distinct().ToList() ?? new List<string>();

            if (list.Count == 0)
            {
                if (_errors.Remove(propertyName))
                {
                    ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
                }
                return;
            }

            _errors[propertyName] = list;
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        private bool Set<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        public IEnumerable GetErrors(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                return _errors.SelectMany(kv => kv.Value);
            }
            return _errors.TryGetValue(propertyName, out var list) ? list : Enumerable.Empty<string>();
        }

        public async Task Login(string username, string password)
        {
            CodenamesGame.AuthenticationService.AuthenticationRequest request = 
                OnewayNetworkManager.Instance.Authenticate(username, password);
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
                        _requestErrorMessage = Util.StatusToMessageMapper.GetAuthServiceMessage(Util.AuthOperationType.AUTHENTICATION, request.StatusCode);
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

        public static CodenamesGame.EmailService.CommunicationRequest SendPasswordResetEmail (string email)
        {
            return OnewayNetworkManager.Instance.SendVerificationEmail(email, CodenamesGame.EmailService.EmailType.PASSWORD_RESET);
        }

        public static CodenamesGame.AuthenticationService.PasswordResetRequest CompletePasswordReset(string email, string code, string password)
        {
            return OnewayNetworkManager.Instance.CompletePasswordReset(email, code, password);
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
