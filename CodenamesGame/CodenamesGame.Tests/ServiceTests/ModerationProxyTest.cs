using CodenamesGame.ModerationService;
using CodenamesGame.Network.Proxies.Wrappers;
using Moq;
using NUnit.Framework;
using System;
using System.ServiceModel;

namespace CodenamesGame.Tests.ServiceTests
{
    [TestFixture]
    public class ModerationProxyTest
    {
        private Mock<IModerationManager> _mockModerationManager;
        private ModerationProxy _moderationProxy;

        [SetUp]
        public void Setup()
        {
            _mockModerationManager = new Mock<IModerationManager>();
            _moderationProxy = new ModerationProxy(() => _mockModerationManager.Object);
        }

        [Test]
        public void ReportPlayer_ValidData_ReturnsSuccessRequest()
        {
            // Arrange
            Guid reporterId = Guid.NewGuid();
            Guid reportedId = Guid.NewGuid();
            string reason = "Abusive chat";

            var expectedRequest = new CommunicationRequest
            {
                IsSuccess = true,
                StatusCode = StatusCode.OK
            };

            _mockModerationManager
                .Setup(m => m.ReportPlayer(reporterId, reportedId, reason))
                .Returns(expectedRequest);

            // Act
            var result = _moderationProxy.ReportPlayer(reporterId, reportedId, reason);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
        }

        [Test]
        public void ReportPlayer_ServerTimeout_ReturnsServerTimeoutStatusCode()
        {
            // Arrange
            _mockModerationManager
                .Setup(m => m.ReportPlayer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()))
                .Throws(new TimeoutException());

            // Act
            var result = _moderationProxy.ReportPlayer(Guid.NewGuid(), Guid.NewGuid(), "reason");

            // Assert
            Assert.That(result.StatusCode, Is.EqualTo(StatusCode.SERVER_TIMEOUT));
        }

        [Test]
        public void ReportPlayer_EndpointNotFound_ReturnsServerUnreachableStatusCode()
        {
            // Arrange
            _mockModerationManager
                .Setup(m => m.ReportPlayer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()))
                .Throws(new EndpointNotFoundException());

            // Act
            var result = _moderationProxy.ReportPlayer(Guid.NewGuid(), Guid.NewGuid(), "reason");

            // Assert
            Assert.That(result.StatusCode, Is.EqualTo(StatusCode.SERVER_UNREACHABLE));
        }

        [Test]
        public void ReportPlayer_CommunicationException_ReturnsServerUnavailableStatusCode()
        {
            // Arrange
            _mockModerationManager
                .Setup(m => m.ReportPlayer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()))
                .Throws(new CommunicationException());

            // Act
            var result = _moderationProxy.ReportPlayer(Guid.NewGuid(), Guid.NewGuid(), "reason");

            // Assert
            Assert.That(result.StatusCode, Is.EqualTo(StatusCode.SERVER_UNAVAIBLE));
        }

        [Test]
        public void ReportPlayer_GeneralException_ReturnsServerErrorStatusCode()
        {
            // Arrange
            _mockModerationManager
                .Setup(m => m.ReportPlayer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()))
                .Throws(new Exception("Unexpected error"));

            // Act
            var result = _moderationProxy.ReportPlayer(Guid.NewGuid(), Guid.NewGuid(), "reason");

            // Assert
            Assert.That(result.StatusCode, Is.EqualTo(StatusCode.SERVER_ERROR));
        }
    }
}