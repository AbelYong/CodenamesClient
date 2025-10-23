using CodenamesClient.Properties.Langs;
using CodenamesGame.Domain.POCO;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace CodenamesClient.Validation
{
    public static class ProfileValidation
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

        public static IEnumerable<string> ValidateFacebook(string facebook)
        {
            if (!string.IsNullOrWhiteSpace(facebook) &&
                facebook.Trim().Length > PlayerPOCO.SOCIALMEDIA_MAX_LENGTH)
                yield return Lang.profileSocialNetwork;
        }

        public static IEnumerable<string> ValidateInstagram(string instagram)
        {
            if (!string.IsNullOrWhiteSpace(instagram) &&
                instagram.Trim().Length > PlayerPOCO.SOCIALMEDIA_MAX_LENGTH)
                yield return Lang.profileSocialNetwork;
        }

        public static IEnumerable<string> ValidateDiscord(string discord)
        {
            if (!string.IsNullOrWhiteSpace(discord) &&
                discord.Trim().Length > PlayerPOCO.SOCIALMEDIA_MAX_LENGTH)
                yield return Lang.profileSocialNetwork;
        }

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

        public static IEnumerable<string> ValidateAll(
            string username,
            string email,
            string firstName,
            string lastName,
            string facebook,
            string instagram,
            string discord)
        {
            foreach (var e in ValidateEmail(email)) yield return e;
            foreach (var e in ValidateUsername(username)) yield return e;
            foreach (var e in ValidateFirstName(firstName)) yield return e;
            foreach (var e in ValidateLastName(lastName)) yield return e;
            foreach (var e in ValidateFacebook(facebook)) yield return e;
            foreach (var e in ValidateInstagram(instagram)) yield return e;
            foreach (var e in ValidateDiscord(discord)) yield return e;
        }
    }
}