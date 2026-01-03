using CodenamesGame.Domain.POCO;
using CodenamesGame.Network;
using CodenamesGame.Network.Proxies.Interfaces;
using CodenamesGame.Network.Proxies.Wrappers;
using CodenamesGame.SessionService;
using Moq;
using NUnit.Framework;
using System;
using System.ServiceModel;
using System.Threading.Tasks;

namespace CodenamesGame.Tests.ServiceTests
{
    [TestFixture]
    public class SessionProxyTest
    {
        private Mock<ISessionManager> _mockSessionManager;
        private Mock<ICommunicationObject> _mockCommunicationObject;
        private SessionProxy _sessionProxy;

        [SetUp]
        public void Setup()
        {
            _mockSessionManager = new Mock<ISessionManager>();
            _mockCommunicationObject = _mockSessionManager.As<ICommunicationObject>();
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Created);
            _sessionProxy = new SessionProxy((context, endpoint) => _mockSessionManager.Object);
        }

        #region Initialize Tests

        [Test]
        public void Initialize_ValidPlayer_ReturnsSuccessRequest()
        {
            // Arrange
            var player = new PlayerDM { PlayerID = Guid.NewGuid(), Username = "TestUser" };
            var expectedRequest = new CommunicationRequest { IsSuccess = true };

            // Setup Connect to return success
            _mockSessionManager
                .Setup(m => m.Connect(It.IsAny<Player>()))
                .Returns(expectedRequest);

            // Setup Open to verify it is called
            _mockCommunicationObject.Setup(m => m.Open());

            // Act
            var result = _sessionProxy.Initialize(player);

            // Assert
            Assert.That(result.IsSuccess);
            _mockCommunicationObject.Verify(m => m.Open(), Times.Once);
            _mockSessionManager.Verify(m => m.Connect(It.IsAny<Player>()), Times.Once);
        }

        [Test]
        public void Initialize_AlreadyConnected_ReturnsUnauthorized()
        {
            // Arrange
            var player = new PlayerDM { PlayerID = Guid.NewGuid() };

            _mockSessionManager
                .Setup(m => m.Connect(It.IsAny<Player>()))
                .Returns(new CommunicationRequest { IsSuccess = true });

            _sessionProxy.Initialize(player);

            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);

            // Act
            var result = _sessionProxy.Initialize(player);

            // Assert
            Assert.That(result.IsSuccess); //If connection was already established, we consider it success
            Assert.That(StatusCode.UNAUTHORIZED.Equals(result.StatusCode));
            _mockSessionManager.Verify(m => m.Connect(It.IsAny<Player>()), Times.Once);
        }

        [Test]
        public void Initialize_ServerTimeout_ReturnsTimeoutStatusCode()
        {
            // Arrange
            _mockSessionManager
                .Setup(m => m.Connect(It.IsAny<Player>()))
                .Throws(new TimeoutException());

            // Act
            var result = _sessionProxy.Initialize(new PlayerDM { PlayerID = Guid.NewGuid() });

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(StatusCode.SERVER_TIMEOUT.Equals(result.StatusCode));
            _mockCommunicationObject.Verify(m => m.Close(), Times.Once);
        }

        [Test]
        public void Initialize_EndpointNotFound_ReturnsServerUnreachableStatusCode()
        {
            // Arrange
            _mockSessionManager
                .Setup(m => m.Connect(It.IsAny<Player>()))
                .Throws(new EndpointNotFoundException());

            // Act
            var result = _sessionProxy.Initialize(new PlayerDM { PlayerID = Guid.NewGuid() });

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(StatusCode.SERVER_UNREACHABLE.Equals(result.StatusCode));
            _mockCommunicationObject.Verify(m => m.Close(), Times.Once);
        }

        #endregion

        #region Disconnect Tests

        [Test]
        public void Disconnect_StateOpened_CallsDisconnectAsyncAndCloses()
        {
            // Arrange
            var player = new PlayerDM { PlayerID = Guid.NewGuid() };

            _mockSessionManager.Setup(m => m.Connect(It.IsAny<Player>())).Returns(new CommunicationRequest { IsSuccess = true });
            _sessionProxy.Initialize(player);

            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);
            _mockSessionManager.Setup(m => m.DisconnectAsync(It.IsAny<Player>())).Returns(Task.CompletedTask);

            // Act
            _sessionProxy.Disconnect();

            // Assert
            _mockSessionManager.Verify(m => m.DisconnectAsync(It.IsAny<Player>()), Times.Once);
            _mockCommunicationObject.Verify(m => m.Close(), Times.Once);
        }

        [Test]
        public void Disconnect_StateNotOpened_DoesNotCallDisconnectAsync()
        {
            // Arrange
            var player = new PlayerDM { PlayerID = Guid.NewGuid() };
            _mockSessionManager.Setup(m => m.Connect(It.IsAny<Player>())).Returns(new CommunicationRequest { IsSuccess = true });
            _sessionProxy.Initialize(player);

            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Closed);

            // Act
            _sessionProxy.Disconnect();

            // Assert
            _mockSessionManager.Verify(m => m.DisconnectAsync(It.IsAny<Player>()), Times.Never);
        }

        [Test]
        public void Disconnect_Exception_ClosesProxy()
        {
            // Arrange
            var player = new PlayerDM { PlayerID = Guid.NewGuid() };
            _mockSessionManager.Setup(m => m.Connect(It.IsAny<Player>())).Returns(new CommunicationRequest { IsSuccess = true });
            _sessionProxy.Initialize(player);

            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);

            _mockSessionManager
                .Setup(m => m.DisconnectAsync(It.IsAny<Player>()))
                .Throws(new CommunicationException());

            // Act
            _sessionProxy.Disconnect();

            // Assert
            _mockCommunicationObject.Verify(m => m.Close(), Times.Once);
        }

        #endregion

        #region ConnectionLost Event Tests

        [Test]
        public void ConnectionLost_ChannelFaulted_RaisesEvent()
        {
            // Arrange
            bool eventRaised = false;
            _sessionProxy.ConnectionLost += (s, e) => eventRaised = true;
            _sessionProxy.Initialize(new PlayerDM { PlayerID = Guid.NewGuid() });

            // Act
            // Simulate the channel faulting event
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Faulted);
            _mockCommunicationObject.Raise(m => m.Faulted += null, EventArgs.Empty);

            // Assert
            Assert.That(eventRaised, "ConnectionLost event should be raised when the channel faults.");
            _mockCommunicationObject.Verify(m => m.Abort(), Times.Once);
        }

        #endregion
    }
}
