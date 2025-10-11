using CodenamesClient.Properties.Langs;
using CodenamesGame.Domain.POCO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodenamesClient.Validation
{
    public class LoginValidation
    {
        public static String ValidateUsername(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return Lang.loginUsernameRequired;
            }
            if (username.Length > PlayerPOCO.USERNAME_MAX_LENGTH)
            {
                return Lang.loginUsernameTooLong;
            }
            return "OK";
        }

        public static String ValidatePassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                return Lang.loginPasswordRequired;
            }
            if (password.Length > UserPOCO.PASSWORD_MAX_LENGTH)
            {
                return Lang.loginPasswordTooLong;
            }
            return "OK";
        }
    }
}
