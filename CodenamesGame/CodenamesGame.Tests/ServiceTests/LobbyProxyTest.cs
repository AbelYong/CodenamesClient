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
            _lobbyProxy = new LobbyProxy((context, endpoint) => _mockLobbyManager.Object);
        }

        [Test]
        public void Initialize_ValidData_ReturnsSuccessRequest()
        {
            // Arrange
            Guid playerId = Guid.NewGuid();
            var expectedRequest = new CommunicationRequest { IsSuccess = true };

            _mockLobbyManager
                .Setup(m => m.Connect(playerId))
                .Returns(expectedRequest);

            // Simulate the channel opening
            _mockCommunicationObject.Setup(m => m.Open()).Callback(() =>
                _mockCommunicationObject.Setup(s => s.State).Returns(CommunicationState.Opened));

            // Act
            var result = _lobbyProxy.Initialize(playerId);

            // Assert
            Assert.That(result.IsSuccess);
            _mockCommunicationObject.Verify(m => m.Open(), Times.Once);
            _mockLobbyManager.Verify(m => m.Connect(playerId), Times.Once);
        }

        [Test]
        public void Initialize_AlreadyConnected_ReturnsUnauthorized()
        {
            // Arrange
            Guid playerId = Guid.NewGuid();

            _mockLobbyManager.Setup(m => m.Connect(playerId)).Returns(new CommunicationRequest { IsSuccess = true });
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);
            
            _lobbyProxy.Initialize(playerId); //Perform a successful connect first to populate internal `_client`.

            // Act
            var result = _lobbyProxy.Initialize(playerId);

            // Assert
            Assert.That(StatusCode.UNAUTHORIZED.Equals(result.StatusCode));
            _mockLobbyManager.Verify(m => m.Connect(It.IsAny<Guid>()), Times.Exactly(1));
        }

        [Test]
        public void Initialize_EmptyGuid_ReturnsMissingData()
        {
            // Act
            var result = _lobbyProxy.Initialize(Guid.Empty);

            // Assert
            Assert.That(StatusCode.MISSING_DATA.Equals(result.StatusCode));
            _mockLobbyManager.Verify(m => m.Connect(It.IsAny<Guid>()), Times.Never);
        }

        [Test]
        public void Initialize_TimeoutException_ReturnsServerTimeoutAndAborts()
        {
            // Arrange
            _mockLobbyManager
                .Setup(m => m.Connect(It.IsAny<Guid>()))
                .Throws(new TimeoutException());
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Faulted);

            // Act
            var result = _lobbyProxy.Initialize(Guid.NewGuid());

            // Assert
            Assert.That(StatusCode.SERVER_TIMEOUT.Equals(result.StatusCode));
            _mockCommunicationObject.Verify(m => m.Abort(), Times.Once);
        }

        [Test]
        public void Initialize_EnpointNotFound_ReturnsServerUnreachableAndAborts()
        {
            // Arrange
            _mockLobbyManager
                .Setup(m => m.Connect(It.IsAny<Guid>()))
                .Throws(new EndpointNotFoundException());
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Faulted);

            // Act
            var result = _lobbyProxy.Initialize(Guid.NewGuid());

            // Assert
            Assert.That(StatusCode.SERVER_UNREACHABLE.Equals(result.StatusCode));
            _mockCommunicationObject.Verify(m => m.Abort(), Times.Once);
        }

        [Test]
        public void Initialize_CommunicationException_ReturnsServerUnavaibleAndAborts()
        {
            // Arrange
            _mockLobbyManager
                .Setup(m => m.Connect(It.IsAny<Guid>()))
                .Throws(new CommunicationException());
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Faulted);

            // Act
            var result = _lobbyProxy.Initialize(Guid.NewGuid());

            // Assert
            Assert.That(StatusCode.SERVER_UNAVAIBLE.Equals(result.StatusCode));
            _mockCommunicationObject.Verify(m => m.Abort(), Times.Once);
        }

        [Test]
        public void Initialize_GeneralException_ReturnsServerClientErrorAndAborts()
        {
            // Arrange
            _mockLobbyManager
                .Setup(m => m.Connect(It.IsAny<Guid>()))
                .Throws(new Exception());
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Faulted);

            // Act
            var result = _lobbyProxy.Initialize(Guid.NewGuid());

            // Assert
            Assert.That(StatusCode.CLIENT_ERROR.Equals(result.StatusCode));
            _mockCommunicationObject.Verify(m => m.Abort(), Times.Once);
        }

        [Test]
        public void Disconnect_ClientOpen_CallsDisconnectAsyncAndCloses()
        {
            // Arrange
            Guid playerId = Guid.NewGuid();
            _mockLobbyManager.Setup(m => m.Connect(playerId)).Returns(new CommunicationRequest { IsSuccess = true });
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);
            _lobbyProxy.Initialize(playerId);

            // Act
            _lobbyProxy.Disconnect();

            // Assert
            _mockLobbyManager.Verify(m => m.DisconnectAsync(playerId), Times.Once);
            _mockCommunicationObject.Verify(m => m.Close(), Times.Once);
        }

        [Test]
        public void Disconnect_ClientNotOpen_DoesNothing()
        {
            // Arrange
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Closed);

            // Act
            _lobbyProxy.Disconnect();

            // Assert
            _mockLobbyManager.Verify(m => m.DisconnectAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Test]
        public void Disconnect_TimeoutException_CallsDisconnectAsyncAndAborts()
        {
            // Arrange
            Guid playerId = Guid.NewGuid();
            _mockLobbyManager.Setup(m => m.Connect(playerId)).Returns(new CommunicationRequest { IsSuccess = true });
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);
            _lobbyProxy.Initialize(playerId);

            _mockCommunicationObject.SetupSequence(m => m.State)
                .Returns(CommunicationState.Opened).Returns(CommunicationState.Faulted);
            _mockLobbyManager.Setup(m => m.DisconnectAsync(It.IsAny<Guid>()))
                .Throws(new TimeoutException());

            // Act
            _lobbyProxy.Disconnect();

            // Assert
            _mockLobbyManager.Verify(m => m.DisconnectAsync(playerId), Times.Once);
            _mockCommunicationObject.Verify(m => m.Abort(), Times.Once);
        }

        [Test]
        public void CreateLobby_ValidPlayerAndConnected_ReturnsSuccess()
        {
            // Arrange
            var player = new PlayerDM { PlayerID = Guid.NewGuid() };
            var expectedRequest = new CreateLobbyRequest { IsSuccess = true };

            // Setup connection
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);

            _mockLobbyManager.Setup(m => m.Connect(It.IsAny<Guid>())).Returns(new CommunicationRequest { IsSuccess = true });
            _lobbyProxy.Initialize(player.PlayerID.Value);

            _mockLobbyManager
                .Setup(m => m.CreateParty(It.IsAny<Player>()))
                .Returns(expectedRequest);

            // Act
            var result = _lobbyProxy.CreateLobby(player);

            // Assert
            Assert.That(result.IsSuccess);
            _mockLobbyManager.Verify(m => m.CreateParty(It.IsAny<Player>()), Times.Once);
        }

        [Test]
        public void CreateLobby_InvalidPlayer_ReturnsMissingData()
        {
            // Arrange
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);
            _lobbyProxy.Initialize(Guid.NewGuid());

            // Act
            var result = _lobbyProxy.CreateLobby(new PlayerDM { PlayerID = null });

            // Assert
            Assert.That(StatusCode.MISSING_DATA.Equals(result.StatusCode));
            _mockLobbyManager.Verify(m => m.CreateParty(It.IsAny<Player>()), Times.Never);
        }

        [Test]
        public void CreateLobby_NotConnectedAndReconnectFails_ReturnsServerUnavailable()
        {
            // Arrange
            // Proxy is not initialized, so _client is null. 
            // TryReconnect calls Initialize(Empty), which fails.

            // Act
            var result = _lobbyProxy.CreateLobby(new PlayerDM { PlayerID = Guid.NewGuid() });

            // Assert
            Assert.That(StatusCode.SERVER_UNAVAIBLE.Equals(result.StatusCode));
        }

        [Test]
        public void CreateLobby_EndpointNotFoundException_ReturnsUnreachableAndAborts()
        {
            // Arrange
            var player = new PlayerDM { PlayerID = Guid.NewGuid() };
            _mockCommunicationObject.SetupSequence(s => s.State)
                .Returns(CommunicationState.Opened).Returns(CommunicationState.Opened);
            _lobbyProxy.Initialize(player.PlayerID.Value);

            _mockLobbyManager
                .Setup(m => m.CreateParty(It.IsAny<Player>()))
                .Throws(new EndpointNotFoundException());
            _mockCommunicationObject.SetupSequence(m => m.State)
                .Returns(CommunicationState.Opened).Returns(CommunicationState.Opened).Returns(CommunicationState.Faulted);

            // Act
            var result = _lobbyProxy.CreateLobby(player);

            // Assert
            Assert.That(StatusCode.SERVER_UNREACHABLE.Equals(result.StatusCode));
            _mockCommunicationObject.Verify(m => m.Abort(), Times.Once);
        }

        [Test]
        public void InviteToParty_ValidData_ReturnsSuccess()
        {
            // Arrange
            var host = new PlayerDM { PlayerID = Guid.NewGuid() };
            Guid friendId = Guid.NewGuid();
            string code = "ABC123";

            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);
            _lobbyProxy.Initialize(host.PlayerID.Value);

            _mockLobbyManager
                .Setup(m => m.InviteToParty(It.IsAny<Player>(), friendId, code))
                .Returns(new CommunicationRequest { IsSuccess = true });

            // Act
            var result = _lobbyProxy.InviteToParty(host, friendId, code);

            // Assert
            Assert.That(result.IsSuccess);
        }

        [Test]
        public void InviteToParty_ReconnectsIfClosed_ReturnsSuccess()
        {
            // Arrange
            var host = new PlayerDM { PlayerID = Guid.NewGuid() };
            Guid friendId = Guid.NewGuid();
            string code = "ABC123";

            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);
            _lobbyProxy.Initialize(host.PlayerID.Value);

            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Closed);
            _mockLobbyManager
                .Setup(m => m.Connect(host.PlayerID.Value))
                .Returns(new CommunicationRequest { IsSuccess = true })
                .Callback(() => _mockCommunicationObject.Setup(s => s.State).Returns(CommunicationState.Opened));

            _mockLobbyManager
                .Setup(m => m.InviteToParty(It.IsAny<Player>(), friendId, code))
                .Returns(new CommunicationRequest { IsSuccess = true });

            // Act
            var result = _lobbyProxy.InviteToParty(host, friendId, code);

            // Assert
            Assert.That(result.IsSuccess);
            _mockLobbyManager.Verify(m => m.Connect(host.PlayerID.Value), Times.Exactly(2)); //once for initial setup, once for reconnect
        }

        [Test]
        public void InviteToParty_CommunicationException_ReturnsUnavailableAndAborts()
        {
            // Arrange
            var host = new PlayerDM { PlayerID = Guid.NewGuid() };
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);
            _lobbyProxy.Initialize(host.PlayerID.Value);

            _mockLobbyManager
                .Setup(m => m.InviteToParty(It.IsAny<Player>(), It.IsAny<Guid>(), It.IsAny<string>()))
                .Throws(new CommunicationException());
            _mockCommunicationObject.SetupSequence(m => m.State)
                .Returns(CommunicationState.Opened).Returns(CommunicationState.Opened).Returns(CommunicationState.Faulted);

            // Act
            var result = _lobbyProxy.InviteToParty(host, Guid.NewGuid(), "ABC123");

            // Assert
            Assert.That(StatusCode.SERVER_UNAVAIBLE.Equals(result.StatusCode));
            _mockCommunicationObject.Verify(m => m.Abort(), Times.Once);
        }

        [Test]
        public void JoinParty_ValidData_ReturnsSuccess()
        {
            // Arrange
            var joiner = new PlayerDM { PlayerID = Guid.NewGuid() };
            string code = "ABC123";

            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);
            _lobbyProxy.Initialize(joiner.PlayerID.Value);

            _mockLobbyManager
                .Setup(m => m.JoinParty(It.IsAny<Player>(), code))
                .Returns(new JoinPartyRequest { IsSuccess = true });

            // Act
            var result = _lobbyProxy.JoinParty(joiner, code);

            // Assert
            Assert.That(result.IsSuccess);
        }

        [Test]
        public void JoinParty_MissingData_ReturnsMissingData()
        {
            // Arrange
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);
            _lobbyProxy.Initialize(Guid.NewGuid());

            // Act
            var result = _lobbyProxy.JoinParty(new PlayerDM { PlayerID = null }, "ABC123");

            // Assert
            Assert.That(StatusCode.MISSING_DATA.Equals(result.StatusCode));
        }

        [Test]
        public void JoinParty_GeneralException_ReturnsClientErrorAndCloses()
        {
            // Arrange
            var joiner = new PlayerDM { PlayerID = Guid.NewGuid() };
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);
            _lobbyProxy.Initialize(joiner.PlayerID.Value);

            _mockLobbyManager
                .Setup(m => m.JoinParty(It.IsAny<Player>(), It.IsAny<string>()))
                .Throws(new Exception());

            // Act
            var result = _lobbyProxy.JoinParty(joiner, "ABC123");

            // Assert
            Assert.That(StatusCode.CLIENT_ERROR.Equals(result.StatusCode));
            _mockCommunicationObject.Verify(m => m.Close(), Times.Once);
        }
    }
}
