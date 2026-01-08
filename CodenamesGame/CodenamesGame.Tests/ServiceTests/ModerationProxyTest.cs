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
            Guid reporterId = Guid.NewGuid();
            Guid reportedId = Guid.NewGuid();
            string reason = "Insults in chat";
            var expectedRequest = new CommunicationRequest
            {
                IsSuccess = true,
                StatusCode = StatusCode.OK
            };

            _mockModerationManager
                .Setup(m => m.ReportPlayer(reporterId, reportedId, reason))
                .Returns(expectedRequest);

            var result = _moderationProxy.ReportPlayer(reporterId, reportedId, reason);

            Assert.That(result.IsSuccess);
        }

        [Test]
        public void ReportPlayer_ServerTimeout_ReturnsServerTimeoutStatusCode()
        {
            _mockModerationManager
                .Setup(m => m.ReportPlayer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()))
                .Throws(new TimeoutException());

            var result = _moderationProxy.ReportPlayer(Guid.NewGuid(), Guid.NewGuid(), "reason");

            Assert.That(result.StatusCode.Equals(StatusCode.SERVER_TIMEOUT));
        }

        [Test]
        public void ReportPlayer_EndpointNotFound_ReturnsServerUnreachableStatusCode()
        {
            _mockModerationManager
                .Setup(m => m.ReportPlayer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()))
                .Throws(new EndpointNotFoundException());

            var result = _moderationProxy.ReportPlayer(Guid.NewGuid(), Guid.NewGuid(), "reason");

            Assert.That(result.StatusCode.Equals(StatusCode.SERVER_UNREACHABLE));
        }

        [Test]
        public void ReportPlayer_CommunicationException_ReturnsServerUnavailableStatusCode()
        {
            _mockModerationManager
                .Setup(m => m.ReportPlayer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()))
                .Throws(new CommunicationException());

            var result = _moderationProxy.ReportPlayer(Guid.NewGuid(), Guid.NewGuid(), "reason");

            Assert.That(result.StatusCode.Equals(StatusCode.SERVER_UNAVAIBLE));
        }

        [Test]
        public void ReportPlayer_GeneralException_ReturnsServerErrorStatusCode()
        {
            _mockModerationManager
                .Setup(m => m.ReportPlayer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()))
                .Throws(new Exception("Unexpected error"));

            var result = _moderationProxy.ReportPlayer(Guid.NewGuid(), Guid.NewGuid(), "reason");

            Assert.That(result.StatusCode.Equals(StatusCode.SERVER_ERROR));
        }
    }
}