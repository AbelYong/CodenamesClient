using CodenamesGame.EmailService;
using CodenamesGame.Network.Proxies.Wrappers;
using Moq;
using NUnit.Framework;
using System;
using System.ServiceModel;

namespace CodenamesGame.Tests.ServiceTests
{
    [TestFixture]
    public class EmailProxyTest
    {
        //Note: EmailType is indistinct for the proxy, as same procedure's are followed regardless of type
        private Mock<IEmailManager> _mockEmailManager;
        private EmailProxy _emailProxy;

        [SetUp]
        public void Setup()
        {
            _mockEmailManager = new Mock<IEmailManager>();
            _emailProxy = new EmailProxy(() => _mockEmailManager.Object);
        }

        [Test]
        public void SendVerificationEmail_ValidData_ReturnsSuccessRequest()
        {
            // Arrange
            string email = "test@test.com";
            EmailType type = EmailType.EMAIL_VERIFICATION;
            var expectedRequest = new CommunicationRequest
            {
                IsSuccess = true,
                StatusCode = StatusCode.OK
            };

            _mockEmailManager
                .Setup(m => m.SendVerificationCode(email, type))
                .Returns(expectedRequest);

            // Act
            var result = _emailProxy.SendVerificationEmail(email, type);

            // Assert
            Assert.That(result.IsSuccess);
            Assert.That(StatusCode.OK.Equals(result.StatusCode));
        }

        [Test]
        public void SendVerificationEmail_ServerTimeout_ReturnsTimeoutStatusCode()
        {
            // Arrange
            _mockEmailManager
                .Setup(m => m.SendVerificationCode(It.IsAny<string>(), It.IsAny<EmailType>()))
                .Throws(new TimeoutException());

            // Act
            var result = _emailProxy.SendVerificationEmail("email", EmailType.EMAIL_VERIFICATION);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(StatusCode.SERVER_TIMEOUT.Equals(result.StatusCode));
        }

        [Test]
        public void SendVerificationEmail_EndpointNotFound_ReturnsServerUnreachableStatusCode()
        {
            // Arrange
            _mockEmailManager
                .Setup(m => m.SendVerificationCode(It.IsAny<string>(), It.IsAny<EmailType>()))
                .Throws(new EndpointNotFoundException());

            // Act
            var result = _emailProxy.SendVerificationEmail("email", EmailType.EMAIL_VERIFICATION);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(StatusCode.SERVER_UNREACHABLE.Equals(result.StatusCode));
        }

        [Test]
        public void SendVerificationEmail_CommunicationException_ReturnsServerUnavailableStatusCode()
        {
            // Arrange
            _mockEmailManager
                .Setup(m => m.SendVerificationCode(It.IsAny<string>(), It.IsAny<EmailType>()))
                .Throws(new CommunicationException());

            // Act
            var result = _emailProxy.SendVerificationEmail("email", EmailType.EMAIL_VERIFICATION);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(StatusCode.SERVER_UNAVAIBLE.Equals(result.StatusCode));
        }

        [Test]
        public void SendVerificationEmail_GeneralException_ReturnsClientErrorStatusCode()
        {
            // Arrange
            _mockEmailManager
                .Setup(m => m.SendVerificationCode(It.IsAny<string>(), It.IsAny<EmailType>()))
                .Throws(new Exception("Unexpected error"));

            // Act
            var result = _emailProxy.SendVerificationEmail("email", EmailType.EMAIL_VERIFICATION);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(StatusCode.CLIENT_ERROR.Equals(result.StatusCode));
        }

        [Test]
        public void SendVerificationCode_ValidCode_ReturnsSuccessRequest()
        {
            // Arrange
            string email = "test@test.com";
            string code = "123456";
            EmailType type = EmailType.PASSWORD_RESET;
            var expectedRequest = new ConfirmEmailRequest
            {
                IsSuccess = true,
                StatusCode = StatusCode.OK
            };

            // Note: Proxy.SendVerificationCode calls Client.ValidateVerificationCode
            _mockEmailManager
                .Setup(m => m.ValidateVerificationCode(email, code, type))
                .Returns(expectedRequest);

            // Act
            var result = _emailProxy.SendVerificationCode(email, code, type);

            // Assert
            Assert.That(result.IsSuccess);
            Assert.That(StatusCode.OK.Equals(result.StatusCode));
        }

        [Test]
        public void SendVerificationCode_ServerTimeout_ReturnsTimeoutStatusCode()
        {
            // Arrange
            _mockEmailManager
                .Setup(m => m.ValidateVerificationCode(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<EmailType>()))
                .Throws(new TimeoutException());

            // Act
            var result = _emailProxy.SendVerificationCode("email", "123456", EmailType.PASSWORD_RESET);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(StatusCode.SERVER_TIMEOUT.Equals(result.StatusCode));
        }

        [Test]
        public void SendVerificationCode_EndpointNotFound_ReturnsServerUnreachableStatusCode()
        {
            // Arrange
            _mockEmailManager
                .Setup(m => m.ValidateVerificationCode(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<EmailType>()))
                .Throws(new EndpointNotFoundException());

            // Act
            var result = _emailProxy.SendVerificationCode("email", "123456", EmailType.PASSWORD_RESET);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(StatusCode.SERVER_UNREACHABLE.Equals(result.StatusCode));
        }

        [Test]
        public void SendVerificationCode_CommunicationException_ReturnsServerUnavailableStatusCode()
        {
            // Arrange
            _mockEmailManager
                .Setup(m => m.ValidateVerificationCode(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<EmailType>()))
                .Throws(new CommunicationException());

            // Act
            var result = _emailProxy.SendVerificationCode("email", "123456", EmailType.PASSWORD_RESET);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(StatusCode.SERVER_UNAVAIBLE.Equals(result.StatusCode));
        }

        [Test]
        public void SendVerificationCode_GeneralException_ReturnsClientErrorStatusCode()
        {
            // Arrange
            _mockEmailManager
                .Setup(m => m.ValidateVerificationCode(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<EmailType>()))
                .Throws(new Exception("Unexpected error"));

            // Act
            var result = _emailProxy.SendVerificationCode("email", "123456", EmailType.PASSWORD_RESET);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(StatusCode.CLIENT_ERROR.Equals(result.StatusCode));
        }
    }
}
