using CodenamesGame.Domain.POCO;
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

        [Test]
        public void Initialize_ValidPlayer_ReturnsSuccessRequestAndConnects()
        {
            var player = new PlayerDM { PlayerID = Guid.NewGuid(), Username = "TestUser" };
            var expectedRequest = new CommunicationRequest { IsSuccess = true };
            _mockSessionManager
                .Setup(m => m.Connect(It.IsAny<Player>()))
                .Returns(expectedRequest);
            _mockCommunicationObject.Setup(m => m.Open());

            var result = _sessionProxy.Initialize(player);

            Assert.That(result.IsSuccess);
            _mockCommunicationObject.Verify(m => m.Open(), Times.Once);
            _mockSessionManager.Verify(m => m.Connect(It.IsAny<Player>()), Times.Once);
        }

        [Test]
        public void Initialize_AlreadyConnected_ReturnsUnauthorizedStatusCode()
        {
            var player = new PlayerDM { PlayerID = Guid.NewGuid() };
            _mockSessionManager
                .Setup(m => m.Connect(It.IsAny<Player>()))
                .Returns(new CommunicationRequest { IsSuccess = true });
            _sessionProxy.Initialize(player);
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);

            var result = _sessionProxy.Initialize(player);

            Assert.That(StatusCode.UNAUTHORIZED.Equals(result.StatusCode));
            _mockSessionManager.Verify(m => m.Connect(It.IsAny<Player>()), Times.Once);
        }

        [Test]
        public void Initialize_ServerTimeout_ReturnsTimeoutStatusCode()
        {
            _mockSessionManager
                .Setup(m => m.Connect(It.IsAny<Player>()))
                .Throws(new TimeoutException());

            var result = _sessionProxy.Initialize(new PlayerDM { PlayerID = Guid.NewGuid() });

            Assert.That(StatusCode.SERVER_TIMEOUT.Equals(result.StatusCode));
            _mockCommunicationObject.Verify(m => m.Close(), Times.Once);
        }

        [Test]
        public void Initialize_EndpointNotFound_ReturnsServerUnreachableStatusCode()
        {
            _mockSessionManager
                .Setup(m => m.Connect(It.IsAny<Player>()))
                .Throws(new EndpointNotFoundException());

            var result = _sessionProxy.Initialize(new PlayerDM { PlayerID = Guid.NewGuid() });

            Assert.That(StatusCode.SERVER_UNREACHABLE.Equals(result.StatusCode));
            _mockCommunicationObject.Verify(m => m.Close(), Times.Once);
        }

        [Test]
        public void Disconnect_ClientOpen_CallsDisconnectAndCloses()
        {
            var player = new PlayerDM { PlayerID = Guid.NewGuid() };
            _mockSessionManager.Setup(m => m.Connect(It.IsAny<Player>())).Returns(new CommunicationRequest { IsSuccess = true });
            _sessionProxy.Initialize(player);
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);
            _mockSessionManager.Setup(m => m.DisconnectAsync(It.IsAny<Player>())).Returns(Task.CompletedTask);

            _sessionProxy.Disconnect();

            _mockSessionManager.Verify(m => m.DisconnectAsync(It.IsAny<Player>()), Times.Once);
            _mockCommunicationObject.Verify(m => m.Close(), Times.Once);
        }

        [Test]
        public void Disconnect_ClientNotOpen_DoesNotCallDisconnect()
        {
            var player = new PlayerDM { PlayerID = Guid.NewGuid() };
            _mockSessionManager.Setup(m => m.Connect(It.IsAny<Player>())).Returns(new CommunicationRequest { IsSuccess = true });
            _sessionProxy.Initialize(player);
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Closed);

            _sessionProxy.Disconnect();

            _mockSessionManager.Verify(m => m.DisconnectAsync(It.IsAny<Player>()), Times.Never);
        }

        [Test]
        public void Disconnect_Exception_ClosesProxy()
        {
            var player = new PlayerDM { PlayerID = Guid.NewGuid() };
            _mockSessionManager.Setup(m => m.Connect(It.IsAny<Player>())).Returns(new CommunicationRequest { IsSuccess = true });
            _sessionProxy.Initialize(player);
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);
            _mockSessionManager
                .Setup(m => m.DisconnectAsync(It.IsAny<Player>()))
                .Throws(new CommunicationException());

            _sessionProxy.Disconnect();

            _mockCommunicationObject.Verify(m => m.Close(), Times.Once);
        }

        [Test]
        public void ConnectionLost_ChannelFaulted_RaisesEvent()
        {
            bool eventRaised = false;
            _sessionProxy.ConnectionLost += (s, e) => eventRaised = true;
            _sessionProxy.Initialize(new PlayerDM { PlayerID = Guid.NewGuid() });

            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Faulted);
            _mockCommunicationObject.Raise(m => m.Faulted += null, EventArgs.Empty);

            Assert.That(eventRaised, "ConnectionLost event should be raised when the channel faults.");
            _mockCommunicationObject.Verify(m => m.Abort(), Times.Once);
        }
    }
}
