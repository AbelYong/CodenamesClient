using CodenamesClient.Properties.Langs;
using CodenamesGame.Domain.POCO;

namespace CodenamesClient.Validation
{
    public static class LoginValidation
    {
        public static string ValidateUsername(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return Lang.loginUsernameRequired;
            }
            if (username.Length > PlayerDM.USERNAME_MAX_LENGTH)
            {
                return Lang.loginUsernameTooLong;
            }
            return string.Empty;
        }

        public static string ValidatePassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                return Lang.loginPasswordRequired;
            }
            if (password.Length > UserDM.PASSWORD_MAX_LENGTH)
            {
                return Lang.loginPasswordTooLong;
            }
            return string.Empty;
        }
    }
}
