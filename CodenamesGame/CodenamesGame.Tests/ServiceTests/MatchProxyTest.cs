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

        [Test]
        public void Initialize_ValidPlayerID_ReturnsSuccessRequest()
        {
            Guid playerId = Guid.NewGuid();
            var expectedRequest = new CommunicationRequest { IsSuccess = true };

            _mockMatchManager
                .Setup(m => m.Connect(playerId))
                .Returns(expectedRequest);

            _mockCommunicationObject.Setup(m => m.Open()).Callback(() =>
                _mockCommunicationObject.Setup(s => s.State).Returns(CommunicationState.Opened));

            var result = _matchProxy.Initialize(playerId);

            Assert.That(result.IsSuccess);
            _mockCommunicationObject.Verify(m => m.Open(), Times.Once);
            _mockMatchManager.Verify(m => m.Connect(playerId), Times.Once);
        }

        [Test]
        public void Initialize_AlreadyConnected_ReturnsUnauthorized()
        {
            Guid playerId = Guid.NewGuid();
            _mockMatchManager.Setup(m => m.Connect(playerId)).Returns(new CommunicationRequest { IsSuccess = true });
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);
            _matchProxy.Initialize(playerId);

            var result = _matchProxy.Initialize(playerId);

            Assert.That(StatusCode.UNAUTHORIZED.Equals(result.StatusCode));
            _mockMatchManager.Verify(m => m.Connect(It.IsAny<Guid>()), Times.Once);
        }

        [Test]
        public void Initialize_TimeoutException_ReturnsServerTimeoutAndAbortsProxy()
        {
            _mockCommunicationObject.Setup(m => m.State)
                .Returns(CommunicationState.Faulted);
            _mockMatchManager
                .Setup(m => m.Connect(It.IsAny<Guid>()))
                .Throws(new TimeoutException());

            var result = _matchProxy.Initialize(Guid.NewGuid());

            Assert.That(StatusCode.SERVER_TIMEOUT.Equals(result.StatusCode));
            _mockCommunicationObject.Verify(m => m.Abort(), Times.Once);
        }

        [Test]
        public void Disconnect_Connected_CallsDisconnectAsyncAndClosesProxy()
        {
            Guid playerId = Guid.NewGuid();
            _mockMatchManager.Setup(m => m.Connect(playerId)).Returns(new CommunicationRequest { IsSuccess = true });
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);
            _matchProxy.Initialize(playerId);

            _matchProxy.Disconnect();

            _mockMatchManager.Verify(m => m.DisconnectAsync(playerId), Times.Once);
            _mockCommunicationObject.Verify(m => m.Close(), Times.Once);
        }

        [Test]
        public void Disconnect_NotConnected_DoesNotCallService()
        {
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Closed);

            _matchProxy.Disconnect();

            _mockMatchManager.Verify(m => m.DisconnectAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Test]
        public void JoinMatch_ConnectedAndValid_ReturnsSuccess()
        {
            Guid playerId = Guid.NewGuid();
            var expectedRequest = new CommunicationRequest { IsSuccess = true };
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);
            _mockMatchManager.Setup(m => m.Connect(playerId)).Returns(new CommunicationRequest { IsSuccess = true });
            _matchProxy.Initialize(playerId);
            _mockMatchManager
                .Setup(m => m.JoinMatch(It.IsAny<MatchService.Match>(), playerId))
                .Returns(expectedRequest);

            var result = _matchProxy.JoinMatch(GenerateDummyMatch());

            Assert.That(result.IsSuccess);
            _mockMatchManager.Verify(m => m.JoinMatch(It.IsAny<MatchService.Match>(), playerId), Times.Once);
        }

        [Test]
        public void JoinMatch_NotConnected_ReturnsServerUnavailable()
        {
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Closed);

            var result = _matchProxy.JoinMatch(GenerateDummyMatch());

            Assert.That(StatusCode.SERVER_UNAVAIBLE.Equals(result.StatusCode));
            _mockMatchManager.Verify(m => m.JoinMatch(It.IsAny<MatchService.Match>(), It.IsAny<Guid>()), Times.Never);
        }

        [Test]
        public void JoinMatch_CommunicationException_ReturnsServerUnavailableAndAbortsProxy()
        {
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);
            _matchProxy.Initialize(Guid.NewGuid());
            _mockCommunicationObject.SetupSequence(m => m.State)
                .Returns(CommunicationState.Opened).Returns(CommunicationState.Faulted);
            _mockMatchManager
                .Setup(m => m.JoinMatch(It.IsAny<MatchService.Match>(), It.IsAny<Guid>()))
                .Throws(new CommunicationException());

            var result = _matchProxy.JoinMatch(GenerateDummyMatch());

            Assert.That(StatusCode.SERVER_UNAVAIBLE.Equals(result.StatusCode));
            _mockCommunicationObject.Verify(m => m.Abort(), Times.Once);
        }

        public async Task SendClue_Connected_CallsService()
        {
            string clue = "TestClue";
            Guid playerId = Guid.NewGuid();
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);
            _matchProxy.Initialize(playerId);
            _mockMatchManager
                .Setup(m => m.SendClueAsync(playerId, clue))
                .Returns(Task.CompletedTask);

            await _matchProxy.SendClue(clue);

            _mockMatchManager.Verify(m => m.SendClueAsync(playerId, clue), Times.Once);
        }

        [Test]
        public async Task NotifyTurnTimeout_Connected_CallsService()
        {
            Guid playerId = Guid.NewGuid();
            MatchRoleType role = MatchRoleType.SPYMASTER;
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);
            _matchProxy.Initialize(playerId);
            _mockMatchManager
                .Setup(m => m.NotifyTurnTimeoutAsync(playerId, role))
                .Returns(Task.CompletedTask);

            await _matchProxy.NotifyTurnTimeout(role);

            _mockMatchManager.Verify(m => m.NotifyTurnTimeoutAsync(playerId, role), Times.Once);
        }

        [Test]
        public async Task NotifyPickedAgent_Connected_CallsService()
        {
            var coords = new BoardCoordinatesDM(1, 1);
            int newLength = 30;
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);
            _matchProxy.Initialize(Guid.NewGuid());
            _mockMatchManager
                .Setup(m => m.NotifyPickedAgentAsync(It.IsAny<AgentPickedNotification>()))
                .Returns(Task.CompletedTask);

            await _matchProxy.NotifyPickedAgent(coords, newLength);

            _mockMatchManager.Verify(m => m.NotifyPickedAgentAsync(It.IsAny<AgentPickedNotification>()), Times.Once);
        }

        [Test]
        public async Task NotifyPickedAgent_CommunicationException_AbortsProxy()
        {
            var coords = new BoardCoordinatesDM(1, 1);
            int newTurnLength = 30;
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);
            _matchProxy.Initialize(Guid.NewGuid());
            _mockCommunicationObject.SetupSequence(m => m.State)
                .Returns(CommunicationState.Opened).Returns(CommunicationState.Faulted);
            _mockMatchManager
                .Setup(m => m.NotifyPickedAgentAsync(It.IsAny<AgentPickedNotification>()))
                .ThrowsAsync(new CommunicationException());

            await _matchProxy.NotifyPickedAgent(coords, newTurnLength);

            _mockCommunicationObject.Verify(m => m.Abort(), Times.Once);
        }

        [Test]
        public async Task NotifyPickedBystander_Connected_CallsService()
        {
            var coords = new BoardCoordinatesDM(1, 1);
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);
            _matchProxy.Initialize(Guid.NewGuid());
            _mockMatchManager
                .Setup(m => m.NotifyPickedBystanderAsync(It.IsAny<BystanderPickedNotification>()))
                .Returns(Task.CompletedTask);

            await _matchProxy.NotifyPickedBystander(coords);

            _mockMatchManager.Verify(m => m.NotifyPickedBystanderAsync(It.IsAny<BystanderPickedNotification>()), Times.Once);
        }

        [Test]
        public async Task NotifyPickedBystander_EndpointNotFound_AbortsProxy()
        {
            var coords = new BoardCoordinatesDM(1, 1);
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);
            _matchProxy.Initialize(Guid.NewGuid());
            _mockCommunicationObject.SetupSequence(m => m.State)
                .Returns(CommunicationState.Opened).Returns(CommunicationState.Faulted);
            _mockMatchManager
                .Setup(m => m.NotifyPickedBystanderAsync(It.IsAny<BystanderPickedNotification>()))
                .ThrowsAsync(new EndpointNotFoundException());

            await _matchProxy.NotifyPickedBystander(coords);

            _mockCommunicationObject.Verify(m => m.Abort(), Times.Once);
        }

        [Test]
        public async Task NotifyPickedAssassin_Connected_CallsService()
        {
            var coords = new BoardCoordinatesDM(1, 1);
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);
            _matchProxy.Initialize(Guid.NewGuid());
            _mockMatchManager
                .Setup(m => m.NotifyPickedAssassinAsync(It.IsAny<AssassinPickedNotification>()))
                .Returns(Task.CompletedTask);

            await _matchProxy.NotifyPickedAssassin(coords);

            _mockMatchManager.Verify(m => m.NotifyPickedAssassinAsync(It.IsAny<AssassinPickedNotification>()), Times.Once);
        }

        [Test]
        public async Task NotifyPickedAssassin_TimeoutException_AbortsProxy()
        {
            var coords = new BoardCoordinatesDM(1, 1);
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);
            _matchProxy.Initialize(Guid.NewGuid());
            _mockCommunicationObject.SetupSequence(m => m.State)
                .Returns(CommunicationState.Opened).Returns(CommunicationState.Faulted);
            _mockMatchManager
                .Setup(m => m.NotifyPickedAssassinAsync(It.IsAny<AssassinPickedNotification>()))
                .ThrowsAsync(new TimeoutException());

            await _matchProxy.NotifyPickedAssassin(coords);

            _mockCommunicationObject.Verify(m => m.Abort(), Times.Once);
        }

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
