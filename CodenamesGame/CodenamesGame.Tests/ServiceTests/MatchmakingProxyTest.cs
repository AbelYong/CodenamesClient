using CodenamesGame.Domain.POCO;
using CodenamesGame.Domain.POCO.Match;
using CodenamesGame.MatchmakingService;
using CodenamesGame.Network.Proxies.Wrappers;
using Moq;
using NUnit.Framework;
using System;
using System.ServiceModel;
using System.Threading.Tasks;

namespace CodenamesGame.Tests.ServiceTests
{
    [TestFixture]
    public class MatchmakingProxyTest
    {
        private Mock<IMatchmakingManager> _mockMatchmakingManager;
        private Mock<ICommunicationObject> _mockCommunicationObject;
        private MatchmakingProxy _matchmakingProxy;

        [SetUp]
        public void Setup()
        {
            _mockMatchmakingManager = new Mock<IMatchmakingManager>();
            _mockCommunicationObject = _mockMatchmakingManager.As<ICommunicationObject>();
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Created);
            _mockCommunicationObject.Setup(m => m.Open()).Callback(() =>
            {
                _mockCommunicationObject.Setup(s => s.State).Returns(CommunicationState.Opened);
            });
            _mockCommunicationObject.Setup(m => m.Close()).Callback(() =>
            {
                _mockCommunicationObject.Setup(s => s.State).Returns(CommunicationState.Closed);
            });
            _mockCommunicationObject.Setup(m => m.Abort()).Callback(() =>
            {
                _mockCommunicationObject.Setup(s => s.State).Returns(CommunicationState.Closed);
            });

            _matchmakingProxy = new MatchmakingProxy((context, endpoint) => _mockMatchmakingManager.Object);
        }

        [Test]
        public void Initialize_ValidPlayerID_ReturnsSuccessRequestAndConnects()
        {
            Guid playerId = Guid.NewGuid();
            var expectedRequest = new CommunicationRequest { IsSuccess = true };
            _mockMatchmakingManager.Setup(m => m.Connect(playerId))
                .Returns(expectedRequest);

            var result = _matchmakingProxy.Initialize(playerId);

            Assert.That(result.IsSuccess &&
                _mockCommunicationObject.Object.State.Equals(CommunicationState.Opened));
            _mockMatchmakingManager.Verify(m => m.Connect(playerId), Times.Once);
        }

        [Test]
        public void Initialize_AlreadyConnected_ReturnsUnauthorized()
        {
            Guid playerId = Guid.NewGuid();
            _mockMatchmakingManager.Setup(m => m.Connect(playerId))
                .Returns(new CommunicationRequest { IsSuccess = true });
            _matchmakingProxy.Initialize(playerId);

            var result = _matchmakingProxy.Initialize(playerId);

            Assert.That(StatusCode.UNAUTHORIZED.Equals(result.StatusCode));
            _mockMatchmakingManager.Verify(m => m.Connect(It.IsAny<Guid>()), Times.Once);
        }

        [Test]
        public void Initialize_EmptyPlayerID_ReturnsMissingDataDoesNotConnect()
        {
            var result = _matchmakingProxy.Initialize(Guid.Empty);

            Assert.That(StatusCode.MISSING_DATA.Equals(result.StatusCode));
            _mockMatchmakingManager.Verify(m => m.Connect(It.IsAny<Guid>()), Times.Never);
        }

        [Test]
        public void Initialize_TimeoutException_ReturnsServerTimeoutAndClosesProxy()
        {
            _mockMatchmakingManager.Setup(m => m.Connect(It.IsAny<Guid>()))
                .Throws(new TimeoutException());

            var result = _matchmakingProxy.Initialize(Guid.NewGuid());

            Assert.That(result.StatusCode.Equals(StatusCode.SERVER_TIMEOUT) &&
                _mockCommunicationObject.Object.State.Equals(CommunicationState.Closed));
        }

        [Test]
        public void Disconnect_ClientOpen_CallsDisconnectAndClosesProxy()
        {
            Guid playerId = Guid.NewGuid();
            _matchmakingProxy.Initialize(playerId);

            _matchmakingProxy.Disconnect();

            _mockMatchmakingManager.Verify(m => m.DisconnectAsync(playerId), Times.Once);
            Assert.That(_mockCommunicationObject.Object.State.Equals(CommunicationState.Closed));
        }

        [Test]
        public void Disconnect_ClientNotOpen_DoesNothing()
        {
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Closed);

            _matchmakingProxy.Disconnect();

            _mockMatchmakingManager.Verify(m => m.DisconnectAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Test]
        public async Task RequestArrangedMatch_ValidConfigAndConnected_ReturnsSuccess()
        {
            var expectedRequest = new CommunicationRequest { IsSuccess = true };
            Guid playerId = Guid.NewGuid();
            _matchmakingProxy.Initialize(playerId);
            _mockMatchmakingManager.Setup(m => m.RequestArrangedMatchAsync(It.IsAny<MatchConfiguration>()))
                .ReturnsAsync(expectedRequest);

            var result = await _matchmakingProxy.RequestArrangedMatch(GenerateDummyConfig());

            Assert.That(result.IsSuccess);
            _mockMatchmakingManager.Verify(m => m.RequestArrangedMatchAsync(It.IsAny<MatchConfiguration>()), Times.Once);
        }

        [Test]
        public async Task RequestArrangedMatch_ReconnectFails_ReturnsServerUnavailable()
        {
            var result = await _matchmakingProxy.RequestArrangedMatch(GenerateDummyConfig());

            Assert.That(StatusCode.SERVER_UNAVAIBLE.Equals(result.StatusCode));
        }

        [Test]
        public async Task RequestArrangedMatch_CommunicationException_ReturnsUnavailableAndClosesProxy()
        {
            _matchmakingProxy.Initialize(Guid.NewGuid());
            _mockMatchmakingManager.Setup(m => m.RequestArrangedMatchAsync(It.IsAny<MatchConfiguration>()))
                .ThrowsAsync(new CommunicationException());
            
            var result = await _matchmakingProxy.RequestArrangedMatch(GenerateDummyConfig());

            Assert.That(StatusCode.SERVER_UNAVAIBLE.Equals(result.StatusCode) &&
                _mockCommunicationObject.Object.State.Equals(CommunicationState.Closed));
        }

        [Test]
        public void ConfirmMatch_Connected_CallsService()
        {
            Guid matchId = Guid.NewGuid();
            Guid playerId = Guid.NewGuid();
            _matchmakingProxy.Initialize(playerId);
            _mockMatchmakingManager.Setup(m => m.ConfirmMatchReceivedAsync(playerId, matchId))
                .Returns(Task.CompletedTask);

            _matchmakingProxy.ConfirmMatch(matchId);

            _mockMatchmakingManager.Verify(m => m.ConfirmMatchReceivedAsync(playerId, matchId), Times.Once);
        }

        [Test]
        public void ConfirmMatch_GeneralException_ClosesProxy()
        {
            _matchmakingProxy.Initialize(Guid.NewGuid());
            _mockMatchmakingManager
                .Setup(m => m.ConfirmMatchReceivedAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Throws(new Exception());
            
            _matchmakingProxy.ConfirmMatch(Guid.NewGuid());

            Assert.That(_mockCommunicationObject.Object.State.Equals(CommunicationState.Closed));
        }

        [Test]
        public void CancelMatch_Connected_CallsService()
        {
            Guid playerId = Guid.NewGuid();
            _matchmakingProxy.Initialize(playerId);
            _mockMatchmakingManager.Setup(m => m.RequestMatchCancelAsync(playerId))
                .Returns(Task.CompletedTask);

            _matchmakingProxy.CancelMatch();

            _mockMatchmakingManager.Verify(m => m.RequestMatchCancelAsync(playerId), Times.Once);
        }

        [Test]
        public void CancelMatch_EndpointNotFound_AbortsProxy()
        {
            _matchmakingProxy.Initialize(Guid.NewGuid());
            _mockMatchmakingManager.Setup(m => m.RequestMatchCancelAsync(It.IsAny<Guid>()))
                .Throws(new EndpointNotFoundException());

            _matchmakingProxy.CancelMatch();

            Assert.That(_mockCommunicationObject.Object.State.Equals(CommunicationState.Closed));
        }

        private MatchConfigurationDM GenerateDummyConfig()
        {
            return new MatchConfigurationDM
            {
                Requester = new PlayerDM(),
                Companion = new PlayerDM(),
                Rules = new MatchRulesDM { Gamemode = GamemodeDM.NORMAL }
            };
        }
    }
}
