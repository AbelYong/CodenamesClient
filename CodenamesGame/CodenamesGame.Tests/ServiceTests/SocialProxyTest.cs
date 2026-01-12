using CodenamesGame.FriendService;
using CodenamesGame.Network.Proxies.Wrappers;
using Moq;
using NUnit.Framework;
using System;
using System.ServiceModel;

namespace CodenamesGame.Tests.ServiceTests
{
    [TestFixture]
    public class SocialProxyTest
    {
        private Mock<IFriendManager> _mockFriendManager;
        private Mock<ICommunicationObject> _mockCommunicationObject;
        private SocialProxy _socialProxy;

        [SetUp]
        public void Setup()
        {
            _mockFriendManager = new Mock<IFriendManager>();
            _mockCommunicationObject = _mockFriendManager.As<ICommunicationObject>();
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

            _socialProxy = new SocialProxy((context, endpoint) => _mockFriendManager.Object);
        }

        [Test]
        public void Initialize_ValidData_Connects()
        {
            Guid playerId = Guid.NewGuid();

            _socialProxy.Initialize(playerId);

            _mockFriendManager.Verify(m => m.Connect(playerId), Times.Once);
            Assert.That(_mockCommunicationObject.Object.State.Equals(CommunicationState.Opened));
        }

        [Test]
        public void Initialize_EmptyGuid_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => _socialProxy.Initialize(Guid.Empty));
        }

        [Test]
        public void Initialize_AlreadyConnected_ReturnsDoesNotReconnect()
        {
            Guid playerId = Guid.NewGuid();
            _socialProxy.Initialize(playerId);

            _socialProxy.Initialize(playerId);

            _mockFriendManager.Verify(m => m.Connect(playerId), Times.Once);
            Assert.That(_mockCommunicationObject.Object.State.Equals(CommunicationState.Opened));
        }

        [Test]
        public void Initialize_TimeoutException_ClosesProxy()
        {
            _mockFriendManager.Setup(m => m.Connect(It.IsAny<Guid>())).Throws(new TimeoutException());

            _socialProxy.Initialize(Guid.NewGuid());

            Assert.That(_mockCommunicationObject.Object.State.Equals(CommunicationState.Closed));
        }

        [Test]
        public void Initialize_EndpointNotFoundException_ClosesProxy()
        {
            _mockFriendManager.Setup(m => m.Connect(It.IsAny<Guid>())).Throws(new EndpointNotFoundException());

            _socialProxy.Initialize(Guid.NewGuid());

            Assert.That(_mockCommunicationObject.Object.State.Equals(CommunicationState.Closed));
        }

        [Test]
        public void Initialize_CommunicationException_ClosesProxy()
        {
            _mockFriendManager.Setup(m => m.Connect(It.IsAny<Guid>())).Throws(new CommunicationException());

            _socialProxy.Initialize(Guid.NewGuid());

            Assert.That(_mockCommunicationObject.Object.State.Equals(CommunicationState.Closed));
        }

        [Test]
        public void Initialize_GeneralException_ClosesProxy()
        {
            _mockFriendManager.Setup(m => m.Connect(It.IsAny<Guid>())).Throws(new Exception());

            _socialProxy.Initialize(Guid.NewGuid());

            Assert.That(_mockCommunicationObject.Object.State.Equals(CommunicationState.Closed));
        }

        [Test]
        public void Disconnect_ClientOpen_DisconnectsAndCloses()
        {
            Guid playerId = Guid.NewGuid();
            _socialProxy.Initialize(playerId);

            _socialProxy.Disconnect();

            _mockFriendManager.Verify(m => m.Disconnect(playerId), Times.Once);
            Assert.That(_mockCommunicationObject.Object.State.Equals(CommunicationState.Closed));
        }

        [Test]
        public void Disconnect_ClientNotOpen_DoesNothing()
        {
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Closed);

            _socialProxy.Disconnect();

            _mockFriendManager.Verify(m => m.Disconnect(It.IsAny<Guid>()), Times.Never);
        }

        [Test]
        public void Disconnect_TimeoutException_ClosesProxy()
        {
            _socialProxy.Initialize(Guid.NewGuid());
            _mockFriendManager.Setup(m => m.Disconnect(It.IsAny<Guid>()))
                .Callback(() => _mockCommunicationObject.Setup(s => s.State).Returns(CommunicationState.Faulted))
                .Throws(new TimeoutException());

            _socialProxy.Disconnect();

            Assert.That(_mockCommunicationObject.Object.State.Equals(CommunicationState.Closed));
        }

        [Test]
        public void Disconnect_CommunicationException_ClosesProxy()
        {
            _socialProxy.Initialize(Guid.NewGuid());
            _mockFriendManager.Setup(m => m.Disconnect(It.IsAny<Guid>()))
                .Callback(() => _mockCommunicationObject.Setup(s => s.State).Returns(CommunicationState.Faulted))
                .Throws(new CommunicationException());

            _socialProxy.Disconnect();

            Assert.That(_mockCommunicationObject.Object.State.Equals(CommunicationState.Closed));
        }

        [Test]
        public void Disconnect_GeneralException_ClosesProxy()
        {
            _socialProxy.Initialize(Guid.NewGuid());
            _mockFriendManager
                .Setup(m => m.Disconnect(It.IsAny<Guid>()))
                .Callback(() => _mockCommunicationObject.Setup(s => s.State).Returns(CommunicationState.Faulted))
                .Throws(new Exception());

            _socialProxy.Disconnect();

            Assert.That(_mockCommunicationObject.Object.State.Equals(CommunicationState.Closed));
        }

        [Test]
        public void SearchPlayers_SuccessfulSearch_ReturnsList()
        {
            _socialProxy.Initialize(Guid.NewGuid());
            var expectedList = new Player[] { new Player { Username = "TestUser" } };
            _mockFriendManager.Setup(m => m.SearchPlayers(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<int>()))
                .Returns(expectedList);

            var result = _socialProxy.SearchPlayers("query");

            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void SearchPlayers_CommunicationException_ReturnsEmptyListAndAborts()
        {
            _socialProxy.Initialize(Guid.NewGuid());
            _mockFriendManager.Setup(m => m.SearchPlayers(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<int>()))
                .Callback(() => _mockCommunicationObject.Setup(s => s.State).Returns(CommunicationState.Faulted))
                .Throws(new CommunicationException());

            var result = _socialProxy.SearchPlayers("query");

            Assert.That(result, Is.Empty);
            _mockCommunicationObject.Verify(m => m.Abort(), Times.Once);
        }

        [Test]
        public void SearchPlayers_TimeoutException_ReturnsEmptyListAndAborts()
        {
            _socialProxy.Initialize(Guid.NewGuid());
            _mockFriendManager.Setup(m => m.SearchPlayers(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<int>()))
                .Callback(() => _mockCommunicationObject.Setup(s => s.State).Returns(CommunicationState.Faulted))
                .Throws(new TimeoutException());

            var result = _socialProxy.SearchPlayers("query");

            Assert.That(result, Is.Empty);
            _mockCommunicationObject.Verify(m => m.Abort(), Times.Once);
        }

        [Test]
        public void SearchPlayers_GeneralException_ReturnsEmptyListAndAborts()
        {
            _socialProxy.Initialize(Guid.NewGuid());
            _mockFriendManager.Setup(m => m.SearchPlayers(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<int>()))
                .Callback(() => _mockCommunicationObject.Setup(s => s.State).Returns(CommunicationState.Faulted))
                .Throws(new Exception());

            var result = _socialProxy.SearchPlayers("query");

            Assert.That(result, Is.Empty);
            _mockCommunicationObject.Verify(m => m.Abort(), Times.Once);
        }

        [Test]
        public void GetFriends_ValidData_ReturnsList()
        {
            _socialProxy.Initialize(Guid.NewGuid());
            _mockFriendManager.Setup(m => m.GetFriends(It.IsAny<Guid>()))
                .Returns(new Player[] { new Player() });

            var result = _socialProxy.GetFriends();

            Assert.That(result.Count.Equals(1));
        }

        [Test]
        public void GetFriends_CommunicationException_ReturnsEmptyListAndAborts()
        {
            _socialProxy.Initialize(Guid.NewGuid());
            _mockFriendManager.Setup(m => m.GetFriends(It.IsAny<Guid>()))
                .Callback(() => _mockCommunicationObject.Setup(s => s.State).Returns(CommunicationState.Faulted))
                .Throws(new CommunicationException());

            var result = _socialProxy.GetFriends();

            Assert.That(result, Is.Empty);
            _mockCommunicationObject.Verify(m => m.Abort(), Times.Once);
        }

        [Test]
        public void GetIncomingRequests_ReceptionSuccess_ReturnsList()
        {
            _socialProxy.Initialize(Guid.NewGuid());
            _mockFriendManager.Setup(m => m.GetIncomingRequests(It.IsAny<Guid>()))
                .Returns(new Player[] { new Player() });

            var result = _socialProxy.GetIncomingRequests();

            Assert.That(result.Count.Equals(1));
        }

        [Test]
        public void GetIncomingRequests_Exception_ReturnsEmptyListAndAborts()
        {
            _socialProxy.Initialize(Guid.NewGuid());
            _mockFriendManager.Setup(m => m.GetIncomingRequests(It.IsAny<Guid>()))
                .Callback(() => _mockCommunicationObject.Setup(s => s.State).Returns(CommunicationState.Faulted))
                .Throws(new Exception());

            var result = _socialProxy.GetIncomingRequests();

            Assert.That(result, Is.Empty);
            _mockCommunicationObject.Verify(m => m.Abort(), Times.Once);
        }

        [Test]
        public void GetSentRequests_ReceptionSuccess_ReturnsList()
        {
            _socialProxy.Initialize(Guid.NewGuid());
            _mockFriendManager.Setup(m => m.GetSentRequests(It.IsAny<Guid>()))
                .Returns(new Player[] { new Player() });

            var result = _socialProxy.GetSentRequests();

            Assert.That(result.Count.Equals(1));
        }

        [Test]
        public void GetSentRequests_TimeoutException_ReturnsEmptyListAndAborts()
        {
            _socialProxy.Initialize(Guid.NewGuid());
            _mockFriendManager.Setup(m => m.GetSentRequests(It.IsAny<Guid>()))
                .Callback(() => _mockCommunicationObject.Setup(s => s.State).Returns(CommunicationState.Faulted))
                .Throws(new TimeoutException());

            var result = _socialProxy.GetSentRequests();

            Assert.That(result, Is.Empty);
            _mockCommunicationObject.Verify(m => m.Abort(), Times.Once);
        }

        [Test]
        public void SendFriendRequest_SentSuccess_ReturnsSuccess()
        {
            _socialProxy.Initialize(Guid.NewGuid());
            _mockFriendManager.Setup(m => m.SendFriendRequest(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(new FriendshipRequest { IsSuccess = true });

            var result = _socialProxy.SendFriendRequest(Guid.NewGuid());

            Assert.That(result.IsSuccess);
        }

        [Test]
        public void SendFriendRequest_NotConnected_ReturnsServerUnavailable()
        {
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Closed);

            var result = _socialProxy.SendFriendRequest(Guid.NewGuid());

            Assert.That(result.StatusCode.Equals(StatusCode.SERVER_UNAVAIBLE));
        }

        [Test]
        public void SendFriendRequest_TimeoutException_ReturnsServerTimeoutAndClosesProxy()
        {
            _socialProxy.Initialize(Guid.NewGuid());
            _mockFriendManager.Setup(m => m.SendFriendRequest(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Callback(() => _mockCommunicationObject.Setup(s => s.State).Returns(CommunicationState.Faulted))
                .Throws(new TimeoutException());

            var result = _socialProxy.SendFriendRequest(Guid.NewGuid());

            Assert.That(result.StatusCode.Equals(StatusCode.SERVER_TIMEOUT) &&
                _mockCommunicationObject.Object.State.Equals(CommunicationState.Closed));
        }

        [Test]
        public void SendFriendRequest_EndpointNotFound_ReturnsServerUnreachableAndClosesProxy()
        {
            _socialProxy.Initialize(Guid.NewGuid());
            _mockFriendManager.Setup(m => m.SendFriendRequest(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Callback(() => _mockCommunicationObject.Setup(s => s.State).Returns(CommunicationState.Faulted))
                .Throws(new EndpointNotFoundException());

            var result = _socialProxy.SendFriendRequest(Guid.NewGuid());

            Assert.That(result.StatusCode.Equals(StatusCode.SERVER_UNREACHABLE) &&
                _mockCommunicationObject.Object.State.Equals(CommunicationState.Closed));
        }

        [Test]
        public void SendFriendRequest_CommunicationException_ReturnsServerUnavailableAndClosesProxy()
        {
            _socialProxy.Initialize(Guid.NewGuid());
            _mockFriendManager.Setup(m => m.SendFriendRequest(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Callback(() => _mockCommunicationObject.Setup(s => s.State).Returns(CommunicationState.Faulted))
                .Throws(new CommunicationException());

            var result = _socialProxy.SendFriendRequest(Guid.NewGuid());

            Assert.That(result.StatusCode.Equals(StatusCode.SERVER_UNAVAIBLE) &&
                _mockCommunicationObject.Object.State.Equals(CommunicationState.Closed));
        }

        [Test]
        public void SendFriendRequest_GeneralException_ReturnsClientErrorAndClosesProxy()
        {
            _socialProxy.Initialize(Guid.NewGuid());

            _mockFriendManager.Setup(m => m.SendFriendRequest(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Callback(() => _mockCommunicationObject.Setup(s => s.State).Returns(CommunicationState.Faulted))
                .Throws(new Exception());

            var result = _socialProxy.SendFriendRequest(Guid.NewGuid());

            Assert.That(result.StatusCode.Equals(StatusCode.CLIENT_ERROR) &&
                _mockCommunicationObject.Object.State.Equals(CommunicationState.Closed));
        }

        [Test]
        public void AcceptFriendRequest_AcceptedSuccess_ReturnsSuccess()
        {
            _socialProxy.Initialize(Guid.NewGuid());
            _mockFriendManager.Setup(m => m.AcceptFriendRequest(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(new FriendshipRequest { IsSuccess = true });

            var result = _socialProxy.AcceptFriendRequest(Guid.NewGuid());

            Assert.That(result.IsSuccess);
        }

        [Test]
        public void AcceptFriendRequest_Exception_ReturnsClientErrorAndClosesProxy()
        {
            _socialProxy.Initialize(Guid.NewGuid());
            _mockFriendManager.Setup(m => m.AcceptFriendRequest(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Callback(() => _mockCommunicationObject.Setup(s => s.State).Returns(CommunicationState.Faulted))
                .Throws(new Exception());

            var result = _socialProxy.AcceptFriendRequest(Guid.NewGuid());

            Assert.That(result.StatusCode.Equals(StatusCode.CLIENT_ERROR) &&
                _mockCommunicationObject.Object.State.Equals(CommunicationState.Closed));
        }

        [Test]
        public void RejectFriendRequest_RejectionSuccess_ReturnsSuccess()
        {
            _socialProxy.Initialize(Guid.NewGuid());
            _mockFriendManager.Setup(m => m.RejectFriendRequest(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(new FriendshipRequest { IsSuccess = true });

            var result = _socialProxy.RejectFriendRequest(Guid.NewGuid());

            Assert.That(result.IsSuccess);
        }

        [Test]
        public void RejectFriendRequest_Exception_ReturnsClientErrorAndClosesProxy()
        {
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);
            _socialProxy.Initialize(Guid.NewGuid());
            _mockFriendManager.Setup(m => m.RejectFriendRequest(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Callback(() => _mockCommunicationObject.Setup(s => s.State).Returns(CommunicationState.Faulted))
                .Throws(new Exception());

            var result = _socialProxy.RejectFriendRequest(Guid.NewGuid());

            Assert.That(result.StatusCode.Equals(StatusCode.CLIENT_ERROR) &&
                _mockCommunicationObject.Object.State.Equals(CommunicationState.Closed));
        }

        [Test]
        public void RemoveFriend_RemoveSuccess_ReturnsSuccess()
        {
            _socialProxy.Initialize(Guid.NewGuid());
            _mockFriendManager.Setup(m => m.RemoveFriend(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(new FriendshipRequest { IsSuccess = true });

            var result = _socialProxy.RemoveFriend(Guid.NewGuid());

            Assert.That(result.IsSuccess);
        }

        [Test]
        public void RemoveFriend_TimeoutException_ReturnsServerTimeoutAndClosesProxy()
        {
            _socialProxy.Initialize(Guid.NewGuid());
            _mockFriendManager.Setup(m => m.RemoveFriend(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Callback(() => _mockCommunicationObject.Setup(s => s.State).Returns(CommunicationState.Faulted))
                .Throws(new TimeoutException());

            var result = _socialProxy.RemoveFriend(Guid.NewGuid());

            Assert.That(result.StatusCode.Equals(StatusCode.SERVER_TIMEOUT) &&
                _mockCommunicationObject.Object.State.Equals(CommunicationState.Closed));
        }
    }
}