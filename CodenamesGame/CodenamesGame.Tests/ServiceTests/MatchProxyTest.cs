using CodenamesGame.Domain.POCO.Match;
using CodenamesGame.MatchService;
using CodenamesGame.Network.Proxies.Wrappers;
using Moq;
using NUnit.Framework;
using System;
using System.ServiceModel;
using System.Threading.Tasks;

namespace CodenamesGame.Tests.ServiceTests
{
    [TestFixture]
    public class MatchProxyTest
    {
        private Mock<IMatchManager> _mockMatchManager;
        private Mock<ICommunicationObject> _mockCommunicationObject;
        private MatchProxy _matchProxy;

        [SetUp]
        public void Setup()
        {
            _mockMatchManager = new Mock<IMatchManager>();
            _mockCommunicationObject = _mockMatchManager.As<ICommunicationObject>();
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Created);
            _matchProxy = new MatchProxy((context, endpoint) => _mockMatchManager.Object);
        }

        #region Initialize Tests

        [Test]
        public void Initialize_ValidPlayerID_ReturnsSuccessRequest()
        {
            // Arrange
            Guid playerId = Guid.NewGuid();
            var expectedRequest = new CommunicationRequest { IsSuccess = true };

            _mockMatchManager
                .Setup(m => m.Connect(playerId))
                .Returns(expectedRequest);

            _mockCommunicationObject.Setup(m => m.Open()).Callback(() =>
                _mockCommunicationObject.Setup(s => s.State).Returns(CommunicationState.Opened));

            // Act
            var result = _matchProxy.Initialize(playerId);

            // Assert
            Assert.That(result.IsSuccess);
            _mockCommunicationObject.Verify(m => m.Open(), Times.Once);
            _mockMatchManager.Verify(m => m.Connect(playerId), Times.Once);
        }

        [Test]
        public void Initialize_AlreadyConnected_ReturnsUnauthorized()
        {
            // Arrange
            Guid playerId = Guid.NewGuid();

            // 1. Initialize successfully first
            _mockMatchManager.Setup(m => m.Connect(playerId)).Returns(new CommunicationRequest { IsSuccess = true });
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);
            _matchProxy.Initialize(playerId);

            // Act
            var result = _matchProxy.Initialize(playerId);

            // Assert
            Assert.That(StatusCode.UNAUTHORIZED.Equals(result.StatusCode));
            _mockMatchManager.Verify(m => m.Connect(It.IsAny<Guid>()), Times.Once); //Only first connect
        }

        [Test]
        public void Initialize_TimeoutException_ReturnsServerTimeoutAndAbortsProxy()
        {
            // Arrange
            _mockCommunicationObject.Setup(m => m.State)
                .Returns(CommunicationState.Faulted);
            _mockMatchManager
                .Setup(m => m.Connect(It.IsAny<Guid>()))
                .Throws(new TimeoutException());

            // Act
            var result = _matchProxy.Initialize(Guid.NewGuid());

            // Assert
            Assert.That(StatusCode.SERVER_TIMEOUT.Equals(result.StatusCode));
            _mockCommunicationObject.Verify(m => m.Abort(), Times.Once);
        }

        #endregion

        #region Disconnect Tests

        [Test]
        public void Disconnect_Connected_CallsDisconnectAsyncAndClosesProxy()
        {
            // Arrange
            Guid playerId = Guid.NewGuid();

            // Initialize to set _currentPlayerID and _client
            _mockMatchManager.Setup(m => m.Connect(playerId)).Returns(new CommunicationRequest { IsSuccess = true });
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);
            _matchProxy.Initialize(playerId);

            // Act
            _matchProxy.Disconnect();

            // Assert
            _mockMatchManager.Verify(m => m.DisconnectAsync(playerId), Times.Once);
            _mockCommunicationObject.Verify(m => m.Close(), Times.Once);
        }

        [Test]
        public void Disconnect_NotConnected_DoesNotCallService()
        {
            // Arrange
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Closed);

            // Act
            _matchProxy.Disconnect();

            // Assert
            _mockMatchManager.Verify(m => m.DisconnectAsync(It.IsAny<Guid>()), Times.Never);
        }

        #endregion

        #region JoinMatch Tests

        [Test]
        public void JoinMatch_ConnectedAndValid_ReturnsSuccess()
        {
            // Arrange
            Guid playerId = Guid.NewGuid();
            var expectedRequest = new CommunicationRequest { IsSuccess = true };

            // Initialize
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);
            _mockMatchManager.Setup(m => m.Connect(playerId)).Returns(new CommunicationRequest { IsSuccess = true });
            _matchProxy.Initialize(playerId);

            _mockMatchManager
                .Setup(m => m.JoinMatch(It.IsAny<MatchService.Match>(), playerId))
                .Returns(expectedRequest);

            // Act
            var result = _matchProxy.JoinMatch(GenerateDummyMatch());

            // Assert
            Assert.That(result.IsSuccess);
            _mockMatchManager.Verify(m => m.JoinMatch(It.IsAny<MatchService.Match>(), playerId), Times.Once);
        }

        [Test]
        public void JoinMatch_NotConnected_ReturnsServerUnavailable()
        {
            // Arrange
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Closed);

            // Act
            var result = _matchProxy.JoinMatch(GenerateDummyMatch());

            // Assert
            Assert.That(StatusCode.SERVER_UNAVAIBLE.Equals(result.StatusCode));
            _mockMatchManager.Verify(m => m.JoinMatch(It.IsAny<MatchService.Match>(), It.IsAny<Guid>()), Times.Never);
        }

        [Test]
        public void JoinMatch_CommunicationException_ReturnsServerUnavailableAndAbortsProxy()
        {
            // Arrange
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);
            _matchProxy.Initialize(Guid.NewGuid());

            _mockCommunicationObject.SetupSequence(m => m.State)
                .Returns(CommunicationState.Opened).Returns(CommunicationState.Faulted);
            _mockMatchManager
                .Setup(m => m.JoinMatch(It.IsAny<MatchService.Match>(), It.IsAny<Guid>()))
                .Throws(new CommunicationException());

            // Act
            var result = _matchProxy.JoinMatch(GenerateDummyMatch());

            // Assert
            Assert.That(StatusCode.SERVER_UNAVAIBLE.Equals(result.StatusCode));
            _mockCommunicationObject.Verify(m => m.Abort(), Times.Once);
        }

        #endregion

        #region SendClue Tests

        [Test]
        public async Task SendClue_Connected_CallsService()
        {
            // Arrange
            string clue = "TestClue";
            Guid playerId = Guid.NewGuid();

            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);
            _matchProxy.Initialize(playerId);

            _mockMatchManager
                .Setup(m => m.SendClueAsync(playerId, clue))
                .Returns(Task.CompletedTask);

            // Act
            await _matchProxy.SendClue(clue);

            // Assert
            _mockMatchManager.Verify(m => m.SendClueAsync(playerId, clue), Times.Once);
        }

        #endregion

        #region NotifyTurnTimeout Tests

        [Test]
        public async Task NotifyTurnTimeout_Connected_CallsService()
        {
            // Arrange
            Guid playerId = Guid.NewGuid();
            MatchRoleType role = MatchRoleType.SPYMASTER; // Assuming enum exists

            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);
            _matchProxy.Initialize(playerId);

            _mockMatchManager
                .Setup(m => m.NotifyTurnTimeoutAsync(playerId, role))
                .Returns(Task.CompletedTask);

            // Act
            await _matchProxy.NotifyTurnTimeout(role);

            // Assert
            _mockMatchManager.Verify(m => m.NotifyTurnTimeoutAsync(playerId, role), Times.Once);
        }

        #endregion

        #region NotifyPickedAgent Tests

        [Test]
        public async Task NotifyPickedAgent_Connected_CallsService()
        {
            // Arrange
            var coords = new BoardCoordinatesDM(1, 1);
            int newLength = 30;

            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);
            _matchProxy.Initialize(Guid.NewGuid());

            _mockMatchManager
                .Setup(m => m.NotifyPickedAgentAsync(It.IsAny<AgentPickedNotification>()))
                .Returns(Task.CompletedTask);

            // Act
            await _matchProxy.NotifyPickedAgent(coords, newLength);

            // Assert
            _mockMatchManager.Verify(m => m.NotifyPickedAgentAsync(It.IsAny<AgentPickedNotification>()), Times.Once);
        }

        [Test]
        public async Task NotifyPickedAgent_CommunicationException_AbortsProxy()
        {
            // Arrange
            var coords = new BoardCoordinatesDM(1, 1);
            int newTurnLength = 30;

            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);
            _matchProxy.Initialize(Guid.NewGuid());

            _mockCommunicationObject.SetupSequence(m => m.State)
                .Returns(CommunicationState.Opened).Returns(CommunicationState.Faulted);
            _mockMatchManager
                .Setup(m => m.NotifyPickedAgentAsync(It.IsAny<AgentPickedNotification>()))
                .ThrowsAsync(new CommunicationException());

            // Act
            await _matchProxy.NotifyPickedAgent(coords, newTurnLength);

            // Assert
            _mockCommunicationObject.Verify(m => m.Abort(), Times.Once);
        }

        #endregion

        #region NotifyPickedBystander Tests

        [Test]
        public async Task NotifyPickedBystander_Connected_CallsService()
        {
            // Arrange
            var coords = new BoardCoordinatesDM(1, 1);

            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);
            _matchProxy.Initialize(Guid.NewGuid());

            _mockMatchManager
                .Setup(m => m.NotifyPickedBystanderAsync(It.IsAny<BystanderPickedNotification>()))
                .Returns(Task.CompletedTask);

            // Act
            await _matchProxy.NotifyPickedBystander(coords);

            // Assert
            _mockMatchManager.Verify(m => m.NotifyPickedBystanderAsync(It.IsAny<BystanderPickedNotification>()), Times.Once);
        }

        [Test]
        public async Task NotifyPickedBystander_EndpointNotFound_AbortsProxy()
        {
            // Arrange
            var coords = new BoardCoordinatesDM(1, 1);

            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);
            _matchProxy.Initialize(Guid.NewGuid());

            _mockCommunicationObject.SetupSequence(m => m.State)
                .Returns(CommunicationState.Opened).Returns(CommunicationState.Faulted);
            _mockMatchManager
                .Setup(m => m.NotifyPickedBystanderAsync(It.IsAny<BystanderPickedNotification>()))
                .ThrowsAsync(new EndpointNotFoundException());

            // Act
            await _matchProxy.NotifyPickedBystander(coords);

            // Assert
            _mockCommunicationObject.Verify(m => m.Abort(), Times.Once);
        }

        #endregion

        #region NotifyPickedAssassin Tests

        [Test]
        public async Task NotifyPickedAssassin_Connected_CallsService()
        {
            // Arrange
            var coords = new BoardCoordinatesDM(1, 1);

            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);
            _matchProxy.Initialize(Guid.NewGuid());

            _mockMatchManager
                .Setup(m => m.NotifyPickedAssassinAsync(It.IsAny<AssassinPickedNotification>()))
                .Returns(Task.CompletedTask);

            // Act
            await _matchProxy.NotifyPickedAssassin(coords);

            // Assert
            _mockMatchManager.Verify(m => m.NotifyPickedAssassinAsync(It.IsAny<AssassinPickedNotification>()), Times.Once);
        }

        [Test]
        public async Task NotifyPickedAssassin_TimeoutException_AbortsProxy()
        {
            // Arrange
            var coords = new BoardCoordinatesDM(1, 1);

            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);
            _matchProxy.Initialize(Guid.NewGuid());

            _mockCommunicationObject.SetupSequence(m => m.State)
                .Returns(CommunicationState.Opened).Returns(CommunicationState.Faulted);
            _mockMatchManager
                .Setup(m => m.NotifyPickedAssassinAsync(It.IsAny<AssassinPickedNotification>()))
                .ThrowsAsync(new TimeoutException());

            // Act
            await _matchProxy.NotifyPickedAssassin(coords);

            // Assert
            _mockCommunicationObject.Verify(m => m.Abort(), Times.Once);
        }

        #endregion

        private MatchDM GenerateDummyMatch()
        {
            return new MatchDM
            {
                MatchID = Guid.NewGuid(),
                Requester = new Domain.POCO.PlayerDM(),
                Companion = new Domain.POCO.PlayerDM()
            };
        }
    }
}
