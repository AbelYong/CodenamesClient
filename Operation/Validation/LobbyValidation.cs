using System;
using System.Text.RegularExpressions;

namespace CodenamesClient.Operation.Validation
{
    public static class LobbyValidation
    {
        private readonly static Regex _emailRegex = 
            new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100));

        public static bool ValidateEmailAddress(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }
            else
            {
                return _emailRegex.IsMatch(email);
            }
        }
    }
}
