using System.Linq;

namespace CodenamesClient.Operation.Validation
{
    public static class PasswordValidation
    {
        public const int PASSWORD_MIN_LENGTH = 10;
        public const int PASSWORD_MAX_LENGTH = 16;

        public static bool MeetsMinLength(string password)
        {
            return !string.IsNullOrEmpty(password) && password.Length >= PASSWORD_MIN_LENGTH;
        }
        public static bool WithinMaxLength(string password)
        {
            return !string.IsNullOrEmpty(password) && password.Length <= PASSWORD_MAX_LENGTH;
        }
        public static bool HasUpper(string password)
        {
            return !string.IsNullOrEmpty(password) && password.Any(char.IsUpper);
        }

        public static bool HasLower(string password)
        {
            return !string.IsNullOrEmpty(password) && password.Any(char.IsLower);
        }
        public static bool HasDigit(string password)
        {
            return !string.IsNullOrEmpty(password) && password.Any(char.IsDigit);
        }
        public static bool HasSpecial(string password)
        {
            return !string.IsNullOrEmpty(password) && password.Any(c => !char.IsLetterOrDigit(c));
        }
    }
}
