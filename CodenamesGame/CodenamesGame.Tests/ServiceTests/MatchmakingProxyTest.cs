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
            _matchmakingProxy = new MatchmakingProxy((context, endpoint) => _mockMatchmakingManager.Object);
        }

        [Test]
        public void Initialize_ValidPlayerID_ReturnsSuccessRequestAndConnects()
        {
            Guid playerId = Guid.NewGuid();
            var expectedRequest = new CommunicationRequest { IsSuccess = true };
            _mockMatchmakingManager.Setup(m => m.Connect(playerId))
                .Returns(expectedRequest);
            _mockCommunicationObject.Setup(m => m.Open()).Callback(() =>
                _mockCommunicationObject.Setup(s => s.State).Returns(CommunicationState.Opened));

            var result = _matchmakingProxy.Initialize(playerId);

            Assert.That(result.IsSuccess);
            _mockCommunicationObject.Verify(m => m.Open(), Times.Once);
            _mockMatchmakingManager.Verify(m => m.Connect(playerId), Times.Once);
        }

        [Test]
        public void Initialize_AlreadyConnected_ReturnsUnauthorized()
        {
            Guid playerId = Guid.NewGuid();
            _mockMatchmakingManager.Setup(m => m.Connect(playerId))
                .Returns(new CommunicationRequest { IsSuccess = true });
            _mockCommunicationObject.Setup(m => m.State)
                .Returns(CommunicationState.Opened);
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
        public void Initialize_TimeoutException_ReturnsServerTimeoutAndAborts()
        {
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Faulted);
            _mockMatchmakingManager.Setup(m => m.Connect(It.IsAny<Guid>()))
                .Throws(new TimeoutException());

            var result = _matchmakingProxy.Initialize(Guid.NewGuid());

            Assert.That(StatusCode.SERVER_TIMEOUT.Equals(result.StatusCode));
            _mockCommunicationObject.Verify(m => m.Abort(), Times.Once);
        }

        [Test]
        public void Disconnect_ClientOpen_CallsDisconnectAndCloses()
        {
            Guid playerId = Guid.NewGuid();
            _mockMatchmakingManager.Setup(m => m.Connect(playerId))
                .Returns(new CommunicationRequest { IsSuccess = true });
            _mockCommunicationObject.Setup(m => m.State)
                .Returns(CommunicationState.Opened);
            _matchmakingProxy.Initialize(playerId);

            _matchmakingProxy.Disconnect();

            _mockMatchmakingManager.Verify(m => m.DisconnectAsync(playerId), Times.Once);
            _mockCommunicationObject.Verify(m => m.Close(), Times.Once);
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
            _mockCommunicationObject.Setup(m => m.State)
                .Returns(CommunicationState.Opened);
            _mockMatchmakingManager.Setup(m => m.Connect(playerId))
                .Returns(new CommunicationRequest { IsSuccess = true });
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
        public async Task RequestArrangedMatch_CommunicationException_ReturnsUnavailableAndAborts()
        {
            _mockCommunicationObject.Setup(m => m.State)
                .Returns(CommunicationState.Opened);
            _matchmakingProxy.Initialize(Guid.NewGuid());
            _mockCommunicationObject.SetupSequence(m => m.State)
                .Returns(CommunicationState.Opened)
                .Returns(CommunicationState.Opened)
                .Returns(CommunicationState.Faulted);
            _mockMatchmakingManager.Setup(m => m.RequestArrangedMatchAsync(It.IsAny<MatchConfiguration>()))
                .ThrowsAsync(new CommunicationException());
            
            var result = await _matchmakingProxy.RequestArrangedMatch(GenerateDummyConfig());

            Assert.That(StatusCode.SERVER_UNAVAIBLE.Equals(result.StatusCode));
            _mockCommunicationObject.Verify(m => m.Abort(), Times.Once);
        }

        [Test]
        public void ConfirmMatch_Connected_CallsService()
        {
            Guid matchId = Guid.NewGuid();
            Guid playerId = Guid.NewGuid();
            _mockCommunicationObject.Setup(m => m.State)
                .Returns(CommunicationState.Opened);
            _mockMatchmakingManager.Setup(m => m.Connect(playerId))
                .Returns(new CommunicationRequest { IsSuccess = true });
            _matchmakingProxy.Initialize(playerId);
            _mockMatchmakingManager.Setup(m => m.ConfirmMatchReceivedAsync(playerId, matchId))
                .Returns(Task.CompletedTask);

            _matchmakingProxy.ConfirmMatch(matchId);

            _mockMatchmakingManager.Verify(m => m.ConfirmMatchReceivedAsync(playerId, matchId), Times.Once);
        }

        [Test]
        public void ConfirmMatch_GeneralException_AbortsProxy()
        {
            _mockCommunicationObject.Setup(m => m.State)
                .Returns(CommunicationState.Opened);
            _matchmakingProxy.Initialize(Guid.NewGuid());
            _mockCommunicationObject.SetupSequence(m => m.State)
                .Returns(CommunicationState.Opened)
                .Returns(CommunicationState.Opened)
                .Returns(CommunicationState.Faulted);
            _mockMatchmakingManager
                .Setup(m => m.ConfirmMatchReceivedAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Throws(new Exception());
            
            _matchmakingProxy.ConfirmMatch(Guid.NewGuid());

            _mockCommunicationObject.Verify(m => m.Abort(), Times.Once);
        }

        [Test]
        public void CancelMatch_Connected_CallsService()
        {
            Guid playerId = Guid.NewGuid();
            _mockCommunicationObject.Setup(m => m.State)
                .Returns(CommunicationState.Opened);
            _mockMatchmakingManager.Setup(m => m.Connect(playerId))
                .Returns(new CommunicationRequest { IsSuccess = true });
            _matchmakingProxy.Initialize(playerId);
            _mockMatchmakingManager.Setup(m => m.RequestMatchCancelAsync(playerId))
                .Returns(Task.CompletedTask);

            _matchmakingProxy.CancelMatch();

            _mockMatchmakingManager.Verify(m => m.RequestMatchCancelAsync(playerId), Times.Once);
        }

        [Test]
        public void CancelMatch_EndpointNotFound_AbortsProxy()
        {
            _mockCommunicationObject.Setup(m => m.State)
                .Returns(CommunicationState.Opened);
            _matchmakingProxy.Initialize(Guid.NewGuid());
            _mockCommunicationObject.SetupSequence(m => m.State)
                .Returns(CommunicationState.Opened)
                .Returns(CommunicationState.Opened)
                .Returns(CommunicationState.Faulted);
            _mockMatchmakingManager.Setup(m => m.RequestMatchCancelAsync(It.IsAny<Guid>()))
                .Throws(new EndpointNotFoundException());

            _matchmakingProxy.CancelMatch();

            _mockCommunicationObject.Verify(m => m.Abort(), Times.Once);
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
