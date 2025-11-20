using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using CodenamesClient.Properties.Langs;
using CodenamesClient.Validation;

namespace CodenamesClient.GameUI.ViewModels
{
    public class SignInViewModel : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        private string _email = string.Empty;
        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _confirmPassword = string.Empty;
        private string _firstName = string.Empty;
        private string _lastName = string.Empty;

        private readonly Dictionary<string, List<string>> _errors = new Dictionary<string, List<string>>();

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public string Email { get => _email; set { if (Set(ref _email, value)) ValidateProperty(nameof(Email)); } }
        public string Username { get => _username; set { if (Set(ref _username, value)) ValidateProperty(nameof(Username)); } }
        public string FirstName { get => _firstName; set { if (Set(ref _firstName, value)) ValidateProperty(nameof(FirstName)); } }
        public string LastName { get => _lastName; set { if (Set(ref _lastName, value)) ValidateProperty(nameof(LastName)); } }

        public string Password
        {
            get => _password;
            set
            {
                if (Set(ref _password, value))
                {
                    OnPropertyChanged(nameof(PwHasMinLength));
                    OnPropertyChanged(nameof(PwWithinMaxLength));
                    OnPropertyChanged(nameof(PwHasUpper));
                    OnPropertyChanged(nameof(PwHasLower));
                    OnPropertyChanged(nameof(PwHasDigit));
                    OnPropertyChanged(nameof(PwHasSpecial));
                    OnPropertyChanged(nameof(PwNoConsecutiveRun));
                    OnPropertyChanged(nameof(IsPasswordValid));
                    ValidateProperty(nameof(ConfirmPassword));
                    OnPropertyChanged(nameof(CanSubmit));
                }
            }
        }

        public void TriggerPasswordValidation()
        {
            ValidateProperty(nameof(Password));
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

        public string PwMinLengthText => string.Format(Lang.signInPasswordMinLength, SignInValidation.PASSWORD_MIN_LENGTH);
        public string PwMaxLengthText => string.Format(Lang.signInPasswordMaxLength, SignInValidation.PASSWORD_MAX_LENGTH);

        public bool PwHasMinLength => SignInValidation.MeetsMinLength(Password);
        public bool PwWithinMaxLength => SignInValidation.WithinMaxLength(Password);
        public bool PwHasUpper => SignInValidation.HasUpper(Password);
        public bool PwHasLower => SignInValidation.HasLower(Password);
        public bool PwHasDigit => SignInValidation.HasDigit(Password);
        public bool PwHasSpecial => SignInValidation.HasSpecial(Password);
        public bool PwNoConsecutiveRun => SignInValidation.NoConsecutiveRun(Password);
        public bool IsPasswordValid => PwHasMinLength && PwWithinMaxLength && PwHasUpper && PwHasLower && PwHasDigit && PwHasSpecial && PwNoConsecutiveRun;

        public bool CanSubmit => !HasErrors && IsPasswordValid && PasswordsMatch;

        private bool PasswordsMatch => !string.IsNullOrEmpty(ConfirmPassword) && ConfirmPassword == Password;

        public bool HasErrors => _errors.Any(kv => kv.Value?.Count > 0);
        public IEnumerable GetErrors(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName)) return _errors.SelectMany(kv => kv.Value);
            return _errors.TryGetValue(propertyName, out var list) ? list : Enumerable.Empty<string>();
        }

        public void ValidateAll()
        {
            ValidateProperty(nameof(Email));
            ValidateProperty(nameof(Username));
            ValidateProperty(nameof(FirstName));
            ValidateProperty(nameof(LastName));
            ValidateProperty(nameof(Password));
            ValidateProperty(nameof(ConfirmPassword));
            OnPropertyChanged(nameof(CanSubmit));
        }

        private void ValidateProperty(string propertyName)
        {
            IEnumerable<string> errors = Enumerable.Empty<string>();

            switch (propertyName)
            {
                case nameof(Email):
                    errors = SignInValidation.ValidateEmail(Email);
                    break;

                case nameof(Username):
                    errors = SignInValidation.ValidateUsername(Username);
                    break;

                case nameof(FirstName):
                    errors = SignInValidation.ValidateFirstName(FirstName);
                    break;

                case nameof(LastName):
                    errors = SignInValidation.ValidateLastName(LastName);
                    break;

                case nameof(Password):
                    if (!IsPasswordValid)
                    {
                        errors = new[] { Lang.signInInvalidPassword };
                    }
                    break;

                case nameof(ConfirmPassword):
                    if (IsPasswordValid && !string.IsNullOrWhiteSpace(ConfirmPassword) && ConfirmPassword != Password)
                        errors = new[] { Lang.signInPasswordsDoNotMatch };
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
                    ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
                return;
            }

            _errors[propertyName] = list;
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        private bool Set<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}