using CodenamesGame.Domain.POCO;
using CodenamesGame.LobbyService;
using CodenamesGame.Network.Proxies.Wrappers;
using Moq;
using NUnit.Framework;
using System;
using System.ServiceModel;

namespace CodenamesGame.Tests.ServiceTests
{
    [TestFixture]
    public class LobbyProxyTest
    {
        private Mock<ILobbyManager> _mockLobbyManager;
        private Mock<ICommunicationObject> _mockCommunicationObject;
        private LobbyProxy _lobbyProxy;

        [SetUp]
        public void Setup()
        {
            _mockLobbyManager = new Mock<ILobbyManager>();
            _mockCommunicationObject = _mockLobbyManager.As<ICommunicationObject>();
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

            _lobbyProxy = new LobbyProxy((context, endpoint) => _mockLobbyManager.Object);
        }

        [Test]
        public void Initialize_ValidData_ReturnsSuccessRequestAndConnects()
        {
            Guid playerId = Guid.NewGuid();
            var expectedRequest = new CommunicationRequest { IsSuccess = true };
            _mockLobbyManager.Setup(m => m.Connect(playerId)).Returns(expectedRequest);

            var result = _lobbyProxy.Initialize(playerId);

            Assert.That(result.IsSuccess &&
                _mockCommunicationObject.Object.State.Equals(CommunicationState.Opened));
            _mockLobbyManager.Verify(m => m.Connect(playerId), Times.Once);
        }

        [Test]
        public void Initialize_AlreadyConnected_ReturnsUnauthorized()
        {
            Guid playerId = Guid.NewGuid();
            _lobbyProxy.Initialize(playerId);

            var result = _lobbyProxy.Initialize(playerId);

            Assert.That(StatusCode.UNAUTHORIZED.Equals(result.StatusCode));
            _mockLobbyManager.Verify(m => m.Connect(It.IsAny<Guid>()), Times.Once);
        }

        [Test]
        public void Initialize_EmptyGuid_ReturnsMissingDataDoesNotConnect()
        {
            var result = _lobbyProxy.Initialize(Guid.Empty);

            Assert.That(StatusCode.MISSING_DATA.Equals(result.StatusCode));
            _mockLobbyManager.Verify(m => m.Connect(It.IsAny<Guid>()), Times.Never);
        }

        [Test]
        public void Initialize_TimeoutException_ReturnsServerTimeoutAndClosesProxy()
        {
            _mockLobbyManager.Setup(m => m.Connect(It.IsAny<Guid>()))
                .Throws(new TimeoutException());

            var result = _lobbyProxy.Initialize(Guid.NewGuid());

            Assert.That(StatusCode.SERVER_TIMEOUT.Equals(result.StatusCode) &&
                _mockCommunicationObject.Object.State.Equals(CommunicationState.Closed));
        }

        [Test]
        public void Initialize_EnpointNotFound_ReturnsServerUnreachableAndClosesProxy()
        {
            _mockLobbyManager.Setup(m => m.Connect(It.IsAny<Guid>()))
                .Throws(new EndpointNotFoundException());

            var result = _lobbyProxy.Initialize(Guid.NewGuid());

            Assert.That(StatusCode.SERVER_UNREACHABLE.Equals(result.StatusCode) &&
                _mockCommunicationObject.Object.State.Equals(CommunicationState.Closed));
        }

        [Test]
        public void Initialize_CommunicationException_ReturnsServerUnavaibleAndClosesProxy()
        {
            _mockLobbyManager.Setup(m => m.Connect(It.IsAny<Guid>()))
                .Throws(new CommunicationException());

            var result = _lobbyProxy.Initialize(Guid.NewGuid());

            Assert.That(StatusCode.SERVER_UNAVAIBLE.Equals(result.StatusCode) &&
                _mockCommunicationObject.Object.State.Equals(CommunicationState.Closed));
        }

        [Test]
        public void Initialize_GeneralException_ReturnsServerClientErrorAndClosesProxy()
        {
            _mockLobbyManager.Setup(m => m.Connect(It.IsAny<Guid>()))
                .Throws(new Exception());

            var result = _lobbyProxy.Initialize(Guid.NewGuid());

            Assert.That(StatusCode.CLIENT_ERROR.Equals(result.StatusCode) &&
                _mockCommunicationObject.Object.State.Equals(CommunicationState.Closed));
        }

        [Test]
        public void Disconnect_ClientOpen_CallsDisconnectAndClosesProxy()
        {
            Guid playerId = Guid.NewGuid();
            _lobbyProxy.Initialize(playerId);

            _lobbyProxy.Disconnect();

            _mockLobbyManager.Verify(m => m.DisconnectAsync(playerId), Times.Once);
            Assert.That(_mockCommunicationObject.Object.State.Equals(CommunicationState.Closed));
        }

        [Test]
        public void Disconnect_ClientNotOpen_DoesNothing()
        {
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Closed);

            _lobbyProxy.Disconnect();

            _mockLobbyManager.Verify(m => m.DisconnectAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Test]
        public void Disconnect_TimeoutException_CallsDisconnectAndClosesProxy()
        {
            Guid playerId = Guid.NewGuid();
            _lobbyProxy.Initialize(playerId);
            _mockLobbyManager.Setup(m => m.DisconnectAsync(It.IsAny<Guid>()))
                .Throws(new TimeoutException());

            _lobbyProxy.Disconnect();

            _mockLobbyManager.Verify(m => m.DisconnectAsync(playerId), Times.Once);
            Assert.That(_mockCommunicationObject.Object.State.Equals(CommunicationState.Closed));
        }

        [Test]
        public void CreateLobby_ValidPlayerAndConnected_ReturnsSuccess()
        {
            var player = new PlayerDM { PlayerID = Guid.NewGuid() };
            var expectedRequest = new CreateLobbyRequest { IsSuccess = true };
            _lobbyProxy.Initialize(player.PlayerID.Value);
            _mockLobbyManager.Setup(m => m.CreateParty(It.IsAny<Player>()))
                .Returns(expectedRequest);

            var result = _lobbyProxy.CreateLobby(player);

            Assert.That(result.IsSuccess);
            _mockLobbyManager.Verify(m => m.CreateParty(It.IsAny<Player>()), Times.Once);
        }

        [Test]
        public void CreateLobby_InvalidPlayer_ReturnsMissingData()
        {
            _lobbyProxy.Initialize(Guid.NewGuid());

            var result = _lobbyProxy.CreateLobby(new PlayerDM { PlayerID = null });

            Assert.That(StatusCode.MISSING_DATA.Equals(result.StatusCode));
            _mockLobbyManager.Verify(m => m.CreateParty(It.IsAny<Player>()), Times.Never);
        }

        [Test]
        public void CreateLobby_NotConnectedAndReconnectFails_ReturnsServerUnavailable()
        {
            var result = _lobbyProxy.CreateLobby(new PlayerDM { PlayerID = Guid.NewGuid() });

            Assert.That(StatusCode.SERVER_UNAVAIBLE.Equals(result.StatusCode));
        }

        [Test]
        public void CreateLobby_EndpointNotFoundException_ReturnsUnreachableAndClosesProxy()
        {
            var player = new PlayerDM { PlayerID = Guid.NewGuid() };
            _lobbyProxy.Initialize(player.PlayerID.Value);
            _mockLobbyManager.Setup(m => m.CreateParty(It.IsAny<Player>()))
                .Throws(new EndpointNotFoundException());

            var result = _lobbyProxy.CreateLobby(player);

            Assert.That(StatusCode.SERVER_UNREACHABLE.Equals(result.StatusCode) &&
                _mockCommunicationObject.Object.State.Equals(CommunicationState.Closed));
        }

        [Test]
        public void InviteToParty_InvitationSuccess_ReturnsSuccess()
        {
            var host = new PlayerDM { PlayerID = Guid.NewGuid() };
            Guid friendId = Guid.NewGuid();
            string code = "ABC123";
            _lobbyProxy.Initialize(host.PlayerID.Value);
            _mockLobbyManager.Setup(m => m.InviteToParty(It.IsAny<Player>(), friendId, code))
                .Returns(new CommunicationRequest { IsSuccess = true });

            var result = _lobbyProxy.InviteToParty(host, friendId, code);

            Assert.That(result.IsSuccess);
        }

        [Test]
        public void InviteToParty_ReconnectsIfClosed_ReturnsSuccess()
        {
            var host = new PlayerDM { PlayerID = Guid.NewGuid() };
            Guid friendId = Guid.NewGuid();
            string code = "ABC123";
            _lobbyProxy.Initialize(host.PlayerID.Value);
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Closed);
            _mockLobbyManager.Setup(m => m.InviteToParty(It.IsAny<Player>(), friendId, code))
                .Returns(new CommunicationRequest { IsSuccess = true });

            var result = _lobbyProxy.InviteToParty(host, friendId, code);

            Assert.That(result.IsSuccess);
            _mockLobbyManager.Verify(m => m.Connect(host.PlayerID.Value), Times.Exactly(2));
        }

        [Test]
        public void InviteToParty_CommunicationException_ReturnsUnavailableAndClosesProxy()
        {
            var host = new PlayerDM { PlayerID = Guid.NewGuid() };
            _lobbyProxy.Initialize(host.PlayerID.Value);
            _mockLobbyManager.Setup(m => m.InviteToParty(It.IsAny<Player>(), It.IsAny<Guid>(), It.IsAny<string>()))
                .Throws(new CommunicationException());

            var result = _lobbyProxy.InviteToParty(host, Guid.NewGuid(), "ABC123");

            Assert.That(StatusCode.SERVER_UNAVAIBLE.Equals(result.StatusCode) &&
                _mockCommunicationObject.Object.State.Equals(CommunicationState.Closed));
        }

        [Test]
        public void JoinParty_JoinSuccess_ReturnsSuccess()
        {
            var joiner = new PlayerDM { PlayerID = Guid.NewGuid() };
            string code = "ABC123";
            _lobbyProxy.Initialize(joiner.PlayerID.Value);
            _mockLobbyManager.Setup(m => m.JoinParty(It.IsAny<Player>(), code))
                .Returns(new JoinPartyRequest { IsSuccess = true });

            var result = _lobbyProxy.JoinParty(joiner, code);

            Assert.That(result.IsSuccess);
        }

        [Test]
        public void JoinParty_MissingPlayerID_ReturnsMissingData()
        {
            _lobbyProxy.Initialize(Guid.NewGuid());

            var result = _lobbyProxy.JoinParty(new PlayerDM { PlayerID = null }, "ABC123");

            Assert.That(StatusCode.MISSING_DATA.Equals(result.StatusCode));
        }

        [Test]
        public void JoinParty_GeneralException_ReturnsClientErrorAndClosesProxy()
        {
            var joiner = new PlayerDM { PlayerID = Guid.NewGuid() };
            _lobbyProxy.Initialize(joiner.PlayerID.Value);
            _mockLobbyManager.Setup(m => m.JoinParty(It.IsAny<Player>(), It.IsAny<string>()))
                .Throws(new Exception());

            var result = _lobbyProxy.JoinParty(joiner, "ABC123");

            Assert.That(StatusCode.CLIENT_ERROR.Equals(result.StatusCode) &&
                _mockCommunicationObject.Object.State.Equals(CommunicationState.Closed));
        }
    }
}
