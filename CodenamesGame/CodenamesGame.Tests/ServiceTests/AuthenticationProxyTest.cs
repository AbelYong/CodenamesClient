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
            // Arrange
            string username = "ValidUser";
            string password = "ValidPassword";
            var expectedRequest = new AuthenticationRequest
            {
                IsSuccess = true,
                UserID = Guid.NewGuid(),
                StatusCode = StatusCode.OK
            };

            _mockAuthenticationManager
                .Setup(m => m.Authenticate(username, password))
                .Returns(expectedRequest);

            // Act
            var result = _authenticationProxy.Authenticate(username, password);

            // Assert
            Assert.That(result.IsSuccess);
            Assert.That(expectedRequest.UserID.Equals(result.UserID));
            Assert.That(StatusCode.OK.Equals(result.StatusCode));
        }

        [Test]
        public void Authenticate_ServerTimeout_ReturnsTimeoutStatusCode()
        {
            // Arrange
            _mockAuthenticationManager
                .Setup(m => m.Authenticate(It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new TimeoutException());

            // Act
            var result = _authenticationProxy.Authenticate("user", "pass");

            // Assert
            Assert.That(StatusCode.SERVER_TIMEOUT.Equals(result.StatusCode));
            Assert.That(result.IsSuccess, Is.False);
        }

        [Test]
        public void Authenticate_EndpointNotFound_ReturnsServerUnreachableStatusCode()
        {
            // Arrange
            _mockAuthenticationManager
                .Setup(m => m.Authenticate(It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new EndpointNotFoundException());

            // Act
            var result = _authenticationProxy.Authenticate("user", "pass");

            // Assert
            Assert.That(StatusCode.SERVER_UNREACHABLE.Equals(result.StatusCode));
            Assert.That(result.IsSuccess, Is.False);
        }

        [Test]
        public void Authenticate_CommunicationException_ReturnsServerUnavailableStatusCode()
        {
            // Arrange
            _mockAuthenticationManager
                .Setup(m => m.Authenticate(It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new CommunicationException());

            // Act
            var result = _authenticationProxy.Authenticate("user", "pass");

            // Assert
            Assert.That(StatusCode.SERVER_UNAVAIBLE.Equals(result.StatusCode));
            Assert.That(result.IsSuccess, Is.False);
        }

        [Test]
        public void Authenticate_GeneralException_ReturnsClientErrorStatusCode()
        {
            // Arrange
            _mockAuthenticationManager
                .Setup(m => m.Authenticate(It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new Exception("Unexpected error"));

            // Act
            var result = _authenticationProxy.Authenticate("user", "pass");

            // Assert
            Assert.That(StatusCode.CLIENT_ERROR.Equals(result.StatusCode));
            Assert.That(result.IsSuccess, Is.False);
        }

        [Test]
        public void CompletePasswordReset_ValidData_ReturnsSuccessRequest()
        {
            // Arrange
            string email = "test@test.com";
            string code = "123456";
            string newPass = "NewPass123";
            var expectedRequest = new PasswordResetRequest
            {
                IsSuccess = true,
                StatusCode = StatusCode.OK
            };

            _mockAuthenticationManager
                .Setup(m => m.CompletePasswordReset(email, code, newPass))
                .Returns(expectedRequest);

            // Act
            var result = _authenticationProxy.CompletePasswordReset(email, code, newPass);

            // Assert
            Assert.That(result.IsSuccess);
            Assert.That(StatusCode.OK.Equals(result.StatusCode));
        }

        [Test]
        public void CompletePasswordReset_ServerTimeout_ReturnsTimeoutStatusCode()
        {
            // Arrange
            _mockAuthenticationManager
                .Setup(m => m.CompletePasswordReset(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new TimeoutException());

            // Act
            var result = _authenticationProxy.CompletePasswordReset("email", "123456", "pass");

            // Assert
            Assert.That(StatusCode.SERVER_TIMEOUT.Equals(result.StatusCode));
        }

        [Test]
        public void CompletePasswordReset_EndpointNotFound_ReturnsServerUnreachableStatusCode()
        {
            // Arrange
            _mockAuthenticationManager
                .Setup(m => m.CompletePasswordReset(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new EndpointNotFoundException());

            // Act
            var result = _authenticationProxy.CompletePasswordReset("email", "123456", "pass");

            // Assert
            Assert.That(StatusCode.SERVER_UNREACHABLE.Equals(result.StatusCode));
        }

        [Test]
        public void CompletePasswordReset_CommunicationException_ReturnsServerUnavailableStatusCode()
        {
            // Arrange
            _mockAuthenticationManager
                .Setup(m => m.CompletePasswordReset(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new CommunicationException());

            // Act
            var result = _authenticationProxy.CompletePasswordReset("email", "123456", "pass");

            // Assert
            Assert.That(StatusCode.SERVER_UNAVAIBLE.Equals(result.StatusCode));
        }

        [Test]
        public void CompletePasswordReset_GeneralException_ReturnsClientErrorStatusCode()
        {
            // Arrange
            _mockAuthenticationManager
                .Setup(m => m.CompletePasswordReset(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new Exception());

            // Act
            var result = _authenticationProxy.CompletePasswordReset("email", "123456", "pass");

            // Assert
            Assert.That(StatusCode.CLIENT_ERROR.Equals(result.StatusCode));
        }

        [Test]
        public void UpdatePassword_ValidData_ReturnsSuccessRequest()
        {
            // Arrange
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

            // Act
            var result = _authenticationProxy.UpdatePassword(username, currentPass, newPass);

            // Assert
            Assert.That(result.IsSuccess);
            Assert.That(StatusCode.OK.Equals(result.StatusCode));
        }

        [Test]
        public void UpdatePassword_EndpointNotFound_ReturnsServerUnreachableStatusCode()
        {
            // Arrange
            _mockAuthenticationManager
                .Setup(m => m.UpdatePassword(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new EndpointNotFoundException());

            // Act
            var result = _authenticationProxy.UpdatePassword("user", "old", "new");

            // Assert
            Assert.That(StatusCode.SERVER_UNREACHABLE.Equals(result.StatusCode));
        }

        [Test]
        public void UpdatePassword_GeneralException_ReturnsClientErrorStatusCode()
        {
            // Arrange
            _mockAuthenticationManager
                .Setup(m => m.UpdatePassword(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new Exception());

            // Act
            var result = _authenticationProxy.UpdatePassword("user", "old", "new");

            // Assert
            Assert.That(StatusCode.CLIENT_ERROR.Equals(result.StatusCode));
        }
    }
}
