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
        private Mock<IEmailManager> _mockEmailManager;
        private EmailProxy _emailProxy;

        [SetUp]
        public void Setup()
        {
            _mockEmailManager = new Mock<IEmailManager>();
            _emailProxy = new EmailProxy(() => _mockEmailManager.Object);
        }

        [Test]
        public void SendVerificationEmail_CodeSentSuccess_ReturnsSuccessRequest()
        {
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

            var result = _emailProxy.SendVerificationEmail(email, type);

            Assert.That(result.IsSuccess);
        }

        [Test]
        public void SendVerificationEmail_ServerTimeout_ReturnsTimeoutStatusCode()
        {
            _mockEmailManager
                .Setup(m => m.SendVerificationCode(It.IsAny<string>(), It.IsAny<EmailType>()))
                .Throws(new TimeoutException());

            var result = _emailProxy.SendVerificationEmail("email", EmailType.EMAIL_VERIFICATION);

            Assert.That(StatusCode.SERVER_TIMEOUT.Equals(result.StatusCode));
        }

        [Test]
        public void SendVerificationEmail_EndpointNotFound_ReturnsServerUnreachableStatusCode()
        {
            _mockEmailManager
                .Setup(m => m.SendVerificationCode(It.IsAny<string>(), It.IsAny<EmailType>()))
                .Throws(new EndpointNotFoundException());

            var result = _emailProxy.SendVerificationEmail("email", EmailType.EMAIL_VERIFICATION);

            Assert.That(StatusCode.SERVER_UNREACHABLE.Equals(result.StatusCode));
        }

        [Test]
        public void SendVerificationEmail_CommunicationException_ReturnsServerUnavailableStatusCode()
        {
            _mockEmailManager
                .Setup(m => m.SendVerificationCode(It.IsAny<string>(), It.IsAny<EmailType>()))
                .Throws(new CommunicationException());

            var result = _emailProxy.SendVerificationEmail("email", EmailType.EMAIL_VERIFICATION);

            Assert.That(StatusCode.SERVER_UNAVAIBLE.Equals(result.StatusCode));
        }

        [Test]
        public void SendVerificationEmail_GeneralException_ReturnsClientErrorStatusCode()
        {
            _mockEmailManager
                .Setup(m => m.SendVerificationCode(It.IsAny<string>(), It.IsAny<EmailType>()))
                .Throws(new Exception("Unexpected error"));

            var result = _emailProxy.SendVerificationEmail("email", EmailType.EMAIL_VERIFICATION);

            Assert.That(StatusCode.CLIENT_ERROR.Equals(result.StatusCode));
        }

        [Test]
        public void SendVerificationCode_CodeVerificationSuccess_ReturnsSuccessRequest()
        {
            string email = "test@test.com";
            string code = "123456";
            EmailType type = EmailType.PASSWORD_RESET;
            var expectedRequest = new ConfirmEmailRequest
            {
                IsSuccess = true,
                StatusCode = StatusCode.OK
            };
            _mockEmailManager
                .Setup(m => m.ValidateVerificationCode(email, code, type))
                .Returns(expectedRequest);

            var result = _emailProxy.SendVerificationCode(email, code, type);

            Assert.That(result.IsSuccess);
        }

        [Test]
        public void SendVerificationCode_ServerTimeout_ReturnsTimeoutStatusCode()
        {
            _mockEmailManager
                .Setup(m => m.ValidateVerificationCode(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<EmailType>()))
                .Throws(new TimeoutException());

            var result = _emailProxy.SendVerificationCode("email", "123456", EmailType.PASSWORD_RESET);

            Assert.That(StatusCode.SERVER_TIMEOUT.Equals(result.StatusCode));
        }

        [Test]
        public void SendVerificationCode_EndpointNotFound_ReturnsServerUnreachableStatusCode()
        {
            _mockEmailManager
                .Setup(m => m.ValidateVerificationCode(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<EmailType>()))
                .Throws(new EndpointNotFoundException());

            var result = _emailProxy.SendVerificationCode("email", "123456", EmailType.PASSWORD_RESET);

            Assert.That(StatusCode.SERVER_UNREACHABLE.Equals(result.StatusCode));
        }

        [Test]
        public void SendVerificationCode_CommunicationException_ReturnsServerUnavailableStatusCode()
        {
            _mockEmailManager
                .Setup(m => m.ValidateVerificationCode(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<EmailType>()))
                .Throws(new CommunicationException());

            var result = _emailProxy.SendVerificationCode("email", "123456", EmailType.PASSWORD_RESET);

            Assert.That(StatusCode.SERVER_UNAVAIBLE.Equals(result.StatusCode));
        }

        [Test]
        public void SendVerificationCode_GeneralException_ReturnsClientErrorStatusCode()
        {
            _mockEmailManager
                .Setup(m => m.ValidateVerificationCode(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<EmailType>()))
                .Throws(new Exception("Unexpected error"));

            var result = _emailProxy.SendVerificationCode("email", "123456", EmailType.PASSWORD_RESET);

            Assert.That(StatusCode.CLIENT_ERROR.Equals(result.StatusCode));
        }
    }
}
