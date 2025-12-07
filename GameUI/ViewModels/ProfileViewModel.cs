using CodenamesClient.Properties.Langs;
using CodenamesClient.Validation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace CodenamesClient.GameUI.ViewModels
{
    public class ProfileViewModel : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
        private readonly Dictionary<string, List<string>> _errors = new Dictionary<string, List<string>>();
        private string _emailVerification;
        private string _currentPassword;
        private string _newPassword;
        private string _confirmPassword;

        public string EmailVerification
        {
            get => _emailVerification;
            set
            {
                _emailVerification = value;
                OnPropertyChanged(nameof(EmailVerification));
            }
        }

        public bool CanSubmit
        {
            get => (!HasErrors && IsPasswordValid && PasswordsMatch);
        }

        public static string PwMinLengthText
        {
            get => string.Format(Lang.signInPasswordMinLength, SignInValidation.PASSWORD_MIN_LENGTH);
        }

        public static string PwMaxLengthText
        {
            get => string.Format(Lang.signInPasswordMaxLength, SignInValidation.PASSWORD_MAX_LENGTH);
        }

        public string CurrentPassword
        {
            get => _currentPassword;
            set
            {
                _currentPassword = value;
                OnPropertyChanged(nameof(CurrentPassword));
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
                    OnPropertyChanged(nameof(PwNoConsecutiveRun));
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

        public bool PwHasMinLength
        {
            get => SignInValidation.MeetsMinLength(NewPassword);
        }

        public bool PwWithinMaxLength
        {
            get => SignInValidation.WithinMaxLength(NewPassword);
        }

        public bool PwHasUpper
        {
            get => SignInValidation.HasUpper(NewPassword);
        }

        public bool PwHasLower
        {
            get => SignInValidation.HasLower(NewPassword);
        }

        public bool PwHasDigit
        {
            get => SignInValidation.HasDigit(NewPassword);
        }

        public bool PwHasSpecial
        {
            get => SignInValidation.HasSpecial(NewPassword);
        }

        public bool PwNoConsecutiveRun
        {
            get => SignInValidation.NoConsecutiveRun(NewPassword);
        }

        public bool IsPasswordValid
        {
            get => (PwHasMinLength && PwWithinMaxLength && PwHasUpper && PwHasLower && PwHasDigit && PwHasSpecial && PwNoConsecutiveRun);
        }

        private bool PasswordsMatch
        {
            get => (!string.IsNullOrEmpty(ConfirmPassword) && ConfirmPassword == NewPassword);
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
                    ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
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

        public bool HasErrors
        {
            get => (_errors.Any(kv => kv.Value?.Count > 0));
        }

        public IEnumerable GetErrors(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName)) return _errors.SelectMany(kv => kv.Value);
            return _errors.TryGetValue(propertyName, out var list) ? list : Enumerable.Empty<string>();
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
