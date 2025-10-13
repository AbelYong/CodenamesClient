using CodenamesClient.Properties.Langs;
using CodenamesGame.Domain.POCO;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;

namespace CodenamesClient.Validation
{
    public static class SignInValidation
    {
        public const int PASSWORD_MIN_LENGTH = 10;
        public const int PASSWORD_MAX_LENGTH = 16;

        private static readonly Regex GmailRegex =
            new Regex(@"^[A-Za-z0-9.!#$%&'*+/=?^_`{|}~-]+@gmail\.com$",
                      RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        public static IEnumerable<string> ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                yield return Lang.signInEmailRequired;
                yield break;
            }

            if (!GmailRegex.IsMatch(email))
                yield return Lang.signInEmailInvalidFormat;
        }

        public static IEnumerable<string> ValidateUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                yield return Lang.signInUsernameRequired;
                yield break;
            }
        }

        public static bool MeetsMinLength(string s) => !string.IsNullOrEmpty(s) && s.Length >= PASSWORD_MIN_LENGTH;
        public static bool WithinMaxLength(string s) => s != null && s.Length <= PASSWORD_MAX_LENGTH;
        public static bool HasUpper(string s) => !string.IsNullOrEmpty(s) && s.Any(char.IsUpper);
        public static bool HasLower(string s) => !string.IsNullOrEmpty(s) && s.Any(char.IsLower);
        public static bool HasDigit(string s) => !string.IsNullOrEmpty(s) && s.Any(char.IsDigit);
        public static bool HasSpecial(string s) => !string.IsNullOrEmpty(s) && s.Any(c => !char.IsLetterOrDigit(c));
        public static bool NoConsecutiveRun(string s) => string.IsNullOrEmpty(s) || !HasRunOfConsecutiveDigits(s, 2);

        private static bool IsLettersAndSpacesOnly(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return true;

            string normalized = input.Normalize(NormalizationForm.FormC).Trim();

            foreach (char c in normalized)
            {
                if (!(char.IsLetter(c) || c == ' '))
                    return false;
            }

            return true;
        }
        public static IEnumerable<string> ValidateFirstName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                yield break;

            if (!IsLettersAndSpacesOnly(name))
                yield return Lang.signInFirstNameOnlyLettersSpacesAccents;

            if (name.Length > PlayerPOCO.NAME_MAX_LENGTH)
                yield return Lang.signInFirstNameMaxLength;
        }

        public static IEnumerable<string> ValidateLastName(string lastName)
        {
            if (string.IsNullOrWhiteSpace(lastName))
                yield break;

            if (!IsLettersAndSpacesOnly(lastName))
                yield return Lang.signInLastNameOnlyLettersSpacesAccents;

            if (lastName.Length > PlayerPOCO.LASTNAME_MAX_LENGTH)
                yield return Lang.signInLastNameMaxLength;
        }


        private static bool HasRunOfConsecutiveDigits(string s, int allowedRunLength)
        {
            int incRun = 0;
            int decRun = 0;
            bool prevWasDigit = false;
            char prev = '\0';

            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (char.IsDigit(c))
                {
                    if (prevWasDigit)
                    {
                        incRun = (c == prev + 1) ? incRun + 1 : 1;
                        decRun = (c == prev - 1) ? decRun + 1 : 1;
                    }
                    else
                    {
                        incRun = 1;
                        decRun = 1;
                    }

                    if (incRun > allowedRunLength || decRun > allowedRunLength)
                        return true;

                    prevWasDigit = true;
                    prev = c;
                }
                else
                {
                    prevWasDigit = false;
                    incRun = 0;
                    decRun = 0;
                    prev = '\0';
                }
            }

            return false;
        }
    }
}