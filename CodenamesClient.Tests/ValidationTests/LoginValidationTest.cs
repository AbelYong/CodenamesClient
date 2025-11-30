using CodenamesClient.Validation;
using NUnit.Framework;
using CodenamesClient.Properties.Langs;
using System;

namespace CodenamesClient.Tests
{
    [TestFixture]
    public class LoginValidationTest
    {
        [Test]
        [TestCase("", TestName = "Username empty")]
        [TestCase(null, TestName = "Username null")]
        public void ValidateUsername_UsernameEmpty_ReturnsUsernameRequired(string username)
        {
            string result = LoginValidation.ValidateUsername(username);
            Assert.That(result, Is.EqualTo(Lang.loginUsernameRequired));
        }

        [Test]
        public void ValidateUsername_UsernameTooLong_ReturnsUsernameTooLong()
        {
            string username = "paricularly long user"; //21 characters
            string result = LoginValidation.ValidateUsername(username);
            Assert.That(result, Is.EqualTo(Lang.loginUsernameTooLong));
        }

        [Test]
        [TestCase("", TestName = "Empty password")]
        [TestCase(null, TestName = "Null password")]
        public void ValidatePassword_PasswordEmpty_ReturnsPasswordRequired(string password)
        {
            string result = LoginValidation.ValidatePassword(password);
            Assert.That(result, Is.EqualTo(Lang.loginPasswordRequired));
        }

        [Test]
        public void ValidatePassword_PasswordTooLong_ReturnsPasswordTooLong()
        {
            string password = "long_____password"; //17 characters
            string result = LoginValidation.ValidatePassword(password);
            Assert.That(result, Is.EqualTo(Lang.loginPasswordTooLong));
        }

        [Test]
        public void ValidatePassword_ValidUsername_ReturnsOK()
        {
            string username = "valid username"; //14 characters
            string result = LoginValidation.ValidateUsername(username);
            Assert.That(result, Is.EqualTo(string.Empty));
        }

        [Test]
        public void ValidatePassword_ValidPassword_ReturnsOK()
        {
            string password = "valid password"; //14 characters
            string result = LoginValidation.ValidatePassword(password);
            Assert.That(result, Is.EqualTo(string.Empty));
        }
    }
}
