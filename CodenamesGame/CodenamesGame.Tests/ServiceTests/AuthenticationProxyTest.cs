using CodenamesGame.AuthenticationService;
using CodenamesGame.Network.Proxies.Wrappers;
using Moq;
using NUnit.Framework;
using System;
using System.ServiceModel;

namespace CodenamesGame.Tests.ServiceTests
{
    [TestFixture]
    public class AuthenticationProxyTest
    {
        private Mock<IAuthenticationManager> _mockAuthenticationManager;
        private AuthenticationProxy _authenticationProxy;

        [SetUp]
        public void Setup()
        {
            _mockAuthenticationManager = new Mock<IAuthenticationManager>();
            _authenticationProxy = new AuthenticationProxy(() => _mockAuthenticationManager.Object);
        }

        [Test]
        public void Authenticate_ValidCredentials_ReturnsSuccessRequest()
        {
            string username = "ValidUser";
            string password = "ValidPassword";
            Guid userID = Guid.NewGuid();
            var expectedRequest = new AuthenticationRequest
            {
                IsSuccess = true,
                UserID = userID,
                StatusCode = StatusCode.OK
            };
            _mockAuthenticationManager
                .Setup(m => m.Authenticate(username, password))
                .Returns(expectedRequest);
            AuthenticationRequest expected = new AuthenticationRequest
            {
                IsSuccess = true,
                UserID = userID,
                StatusCode = StatusCode.OK
            };

            var result = _authenticationProxy.Authenticate(username, password);

            Assert.That(result.IsSuccess && expectedRequest.UserID.Equals(result.UserID));
        }

        [Test]
        public void Authenticate_ServerTimeout_ReturnsTimeoutStatusCode()
        {
            _mockAuthenticationManager
                .Setup(m => m.Authenticate(It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new TimeoutException());

            var result = _authenticationProxy.Authenticate("user", "pass");

            Assert.That(StatusCode.SERVER_TIMEOUT.Equals(result.StatusCode));
        }

        [Test]
        public void Authenticate_EndpointNotFound_ReturnsServerUnreachableStatusCode()
        {
            _mockAuthenticationManager
                .Setup(m => m.Authenticate(It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new EndpointNotFoundException());

            var result = _authenticationProxy.Authenticate("user", "pass");

            Assert.That(StatusCode.SERVER_UNREACHABLE.Equals(result.StatusCode));
        }

        [Test]
        public void Authenticate_CommunicationException_ReturnsServerUnavailableStatusCode()
        {
            _mockAuthenticationManager
                .Setup(m => m.Authenticate(It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new CommunicationException());

            var result = _authenticationProxy.Authenticate("user", "pass");

            Assert.That(StatusCode.SERVER_UNAVAIBLE.Equals(result.StatusCode));
        }

        [Test]
        public void Authenticate_GeneralException_ReturnsClientErrorStatusCode()
        {
            _mockAuthenticationManager
                .Setup(m => m.Authenticate(It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new Exception("Unexpected error"));

            var result = _authenticationProxy.Authenticate("user", "pass");

            Assert.That(StatusCode.CLIENT_ERROR.Equals(result.StatusCode));
        }

        [Test]
        public void CompletePasswordReset_ValidData_ReturnsSuccessRequest()
        {
            string email = "test@gmail.com";
            string code = "123456";
            string newPass = "NewPass123!";
            var expectedRequest = new PasswordResetRequest
            {
                IsSuccess = true,
                StatusCode = StatusCode.OK
            };
            _mockAuthenticationManager
                .Setup(m => m.CompletePasswordReset(email, code, newPass))
                .Returns(expectedRequest);

            var result = _authenticationProxy.CompletePasswordReset(email, code, newPass);

            Assert.That(result.IsSuccess);
        }

        [Test]
        public void CompletePasswordReset_ServerTimeout_ReturnsTimeoutStatusCode()
        {
            _mockAuthenticationManager
                .Setup(m => m.CompletePasswordReset(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new TimeoutException());

            var result = _authenticationProxy.CompletePasswordReset("email", "123456", "pass");

            Assert.That(StatusCode.SERVER_TIMEOUT.Equals(result.StatusCode));
        }

        [Test]
        public void CompletePasswordReset_EndpointNotFound_ReturnsServerUnreachableStatusCode()
        {
            _mockAuthenticationManager
                .Setup(m => m.CompletePasswordReset(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new EndpointNotFoundException());

            var result = _authenticationProxy.CompletePasswordReset("email", "123456", "pass");

            Assert.That(StatusCode.SERVER_UNREACHABLE.Equals(result.StatusCode));
        }

        [Test]
        public void CompletePasswordReset_CommunicationException_ReturnsServerUnavailableStatusCode()
        {
            _mockAuthenticationManager
                .Setup(m => m.CompletePasswordReset(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new CommunicationException());

            var result = _authenticationProxy.CompletePasswordReset("email", "123456", "pass");

            Assert.That(StatusCode.SERVER_UNAVAIBLE.Equals(result.StatusCode));
        }

        [Test]
        public void CompletePasswordReset_GeneralException_ReturnsClientErrorStatusCode()
        {
            _mockAuthenticationManager
                .Setup(m => m.CompletePasswordReset(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new Exception());

            var result = _authenticationProxy.CompletePasswordReset("email", "123456", "pass");

            Assert.That(StatusCode.CLIENT_ERROR.Equals(result.StatusCode));
        }

        [Test]
        public void UpdatePassword_ValidData_ReturnsSuccessRequest()
        {
            string username = "user";
            string currentPass = "oldPass";
            string newPass = "newPass";
            var expectedRequest = new CommunicationRequest
            {
                IsSuccess = true,
                StatusCode = StatusCode.OK
            };
            _mockAuthenticationManager
                .Setup(m => m.UpdatePassword(username, currentPass, newPass))
                .Returns(expectedRequest);

            var result = _authenticationProxy.UpdatePassword(username, currentPass, newPass);

            Assert.That(result.IsSuccess);
        }

        [Test]
        public void UpdatePassword_EndpointNotFound_ReturnsServerUnreachableStatusCode()
        {
            _mockAuthenticationManager
                .Setup(m => m.UpdatePassword(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new EndpointNotFoundException());

            var result = _authenticationProxy.UpdatePassword("user", "old", "new");

            Assert.That(StatusCode.SERVER_UNREACHABLE.Equals(result.StatusCode));
        }

        [Test]
        public void UpdatePassword_GeneralException_ReturnsClientErrorStatusCode()
        {
            _mockAuthenticationManager
                .Setup(m => m.UpdatePassword(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new Exception());

            var result = _authenticationProxy.UpdatePassword("user", "old", "new");

            Assert.That(StatusCode.CLIENT_ERROR.Equals(result.StatusCode));
        }
    }
}
