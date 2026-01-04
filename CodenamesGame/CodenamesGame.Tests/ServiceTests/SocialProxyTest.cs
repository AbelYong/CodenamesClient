using CodenamesGame.Domain.POCO;
using CodenamesGame.FriendService;
using CodenamesGame.Network.Proxies.Wrappers;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
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

            _socialProxy = new SocialProxy((context, endpoint) => _mockFriendManager.Object);
        }

        #region Initialize Tests

        [Test]
        public void Initialize_ValidData_Connects()
        {
            // Arrange
            Guid playerId = Guid.NewGuid();

            // Act
            _socialProxy.Initialize(playerId);

            // Assert
            _mockFriendManager.Verify(m => m.Connect(playerId), Times.Once);
        }

        [Test]
        public void Initialize_EmptyGuid_ThrowsArgumentException()
        {
            // Assert & Act
            Assert.Throws<ArgumentException>(() => _socialProxy.Initialize(Guid.Empty));
        }

        [Test]
        public void Initialize_AlreadyConnected_ReturnsWithoutConnecting()
        {
            // Arrange
            Guid playerId = Guid.NewGuid();
            _socialProxy.Initialize(playerId);

            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);

            // Act
            _socialProxy.Initialize(playerId);

            // Assert
            _mockFriendManager.Verify(m => m.Connect(playerId), Times.Once);
        }

        [Test]
        public void Initialize_TimeoutException_ClosesProxy()
        {
            // Arrange
            _mockFriendManager.Setup(m => m.Connect(It.IsAny<Guid>())).Throws(new TimeoutException());
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Faulted);

            // Act
            _socialProxy.Initialize(Guid.NewGuid());

            // Assert
            _mockCommunicationObject.Verify(m => m.Abort(), Times.Once);
        }

        [Test]
        public void Initialize_EndpointNotFoundException_ClosesProxy()
        {
            // Arrange
            _mockFriendManager.Setup(m => m.Connect(It.IsAny<Guid>())).Throws(new EndpointNotFoundException());
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Faulted);

            // Act
            _socialProxy.Initialize(Guid.NewGuid());

            // Assert
            _mockCommunicationObject.Verify(m => m.Abort(), Times.Once);
        }

        [Test]
        public void Initialize_CommunicationException_ClosesProxy()
        {
            // Arrange
            _mockFriendManager.Setup(m => m.Connect(It.IsAny<Guid>())).Throws(new CommunicationException());
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Faulted);

            // Act
            _socialProxy.Initialize(Guid.NewGuid());

            // Assert
            _mockCommunicationObject.Verify(m => m.Abort(), Times.Once);
        }

        [Test]
        public void Initialize_GeneralException_ClosesProxy()
        {
            // Arrange
            _mockFriendManager.Setup(m => m.Connect(It.IsAny<Guid>())).Throws(new Exception());
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Faulted);

            // Act
            _socialProxy.Initialize(Guid.NewGuid());

            // Assert
            _mockCommunicationObject.Verify(m => m.Abort(), Times.Once);
        }

        #endregion

        #region Disconnect Tests

        [Test]
        public void Disconnect_ClientOpen_DisconnectsAndCloses()
        {
            // Arrange
            Guid playerId = Guid.NewGuid();
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);
            _socialProxy.Initialize(playerId);

            // Act
            _socialProxy.Disconnect();

            // Assert
            _mockFriendManager.Verify(m => m.Disconnect(playerId), Times.Once);
            _mockCommunicationObject.Verify(m => m.Close(), Times.Once);
        }

        [Test]
        public void Disconnect_ClientNotOpen_DoesNothing()
        {
            // Arrange
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Closed);
            _socialProxy.Initialize(Guid.NewGuid());

            // Act
            _socialProxy.Disconnect();

            // Assert
            _mockFriendManager.Verify(m => m.Disconnect(It.IsAny<Guid>()), Times.Never);
        }

        [Test]
        public void Disconnect_TimeoutException_ClosesProxy()
        {
            // Arrange
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);
            _socialProxy.Initialize(Guid.NewGuid());

            _mockFriendManager
                .Setup(m => m.Disconnect(It.IsAny<Guid>()))
                .Callback(() => _mockCommunicationObject.Setup(s => s.State).Returns(CommunicationState.Faulted))
                .Throws(new TimeoutException());

            // Act
            _socialProxy.Disconnect();

            // Assert
            _mockCommunicationObject.Verify(m => m.Abort(), Times.AtLeastOnce);
        }

        [Test]
        public void Disconnect_CommunicationException_ClosesProxy()
        {
            // Arrange
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);
            _socialProxy.Initialize(Guid.NewGuid());

            _mockFriendManager
                .Setup(m => m.Disconnect(It.IsAny<Guid>()))
                .Callback(() => _mockCommunicationObject.Setup(s => s.State).Returns(CommunicationState.Faulted))
                .Throws(new CommunicationException());

            // Act
            _socialProxy.Disconnect();

            // Assert
            _mockCommunicationObject.Verify(m => m.Abort(), Times.AtLeastOnce);
        }

        [Test]
        public void Disconnect_GeneralException_ClosesProxy()
        {
            // Arrange
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);
            _socialProxy.Initialize(Guid.NewGuid());

            _mockFriendManager
                .Setup(m => m.Disconnect(It.IsAny<Guid>()))
                .Callback(() => _mockCommunicationObject.Setup(s => s.State).Returns(CommunicationState.Faulted))
                .Throws(new Exception());

            // Act
            _socialProxy.Disconnect();

            // Assert
            _mockCommunicationObject.Verify(m => m.Abort(), Times.AtLeastOnce);
        }

        #endregion

        #region SearchPlayers Tests

        [Test]
        public void SearchPlayers_ValidData_ReturnsList()
        {
            // Arrange
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);
            _socialProxy.Initialize(Guid.NewGuid());

            var expectedList = new Player[] { new Player { Username = "TestUser" } };
            _mockFriendManager.Setup(m => m.SearchPlayers(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<int>()))
                .Returns(expectedList);

            // Act
            var result = _socialProxy.SearchPlayers("query");

            // Assert
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void SearchPlayers_CommunicationException_ReturnsEmptyListAndCloses()
        {
            // Arrange
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);
            _socialProxy.Initialize(Guid.NewGuid());

            _mockFriendManager.Setup(m => m.SearchPlayers(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<int>()))
                .Callback(() => _mockCommunicationObject.Setup(s => s.State).Returns(CommunicationState.Faulted))
                .Throws(new CommunicationException());

            // Act
            var result = _socialProxy.SearchPlayers("query");

            // Assert
            Assert.That(result, Is.Empty);
            _mockCommunicationObject.Verify(m => m.Abort(), Times.AtLeastOnce);
        }

        [Test]
        public void SearchPlayers_TimeoutException_ReturnsEmptyListAndCloses()
        {
            // Arrange
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);
            _socialProxy.Initialize(Guid.NewGuid());

            _mockFriendManager.Setup(m => m.SearchPlayers(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<int>()))
                .Callback(() => _mockCommunicationObject.Setup(s => s.State).Returns(CommunicationState.Faulted))
                .Throws(new TimeoutException());

            // Act
            var result = _socialProxy.SearchPlayers("query");

            // Assert
            Assert.That(result, Is.Empty);
            _mockCommunicationObject.Verify(m => m.Abort(), Times.AtLeastOnce);
        }

        [Test]
        public void SearchPlayers_GeneralException_ReturnsEmptyListAndCloses()
        {
            // Arrange
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);
            _socialProxy.Initialize(Guid.NewGuid());

            _mockFriendManager.Setup(m => m.SearchPlayers(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<int>()))
                .Callback(() => _mockCommunicationObject.Setup(s => s.State).Returns(CommunicationState.Faulted))
                .Throws(new Exception());

            // Act
            var result = _socialProxy.SearchPlayers("query");

            // Assert
            Assert.That(result, Is.Empty);
            _mockCommunicationObject.Verify(m => m.Abort(), Times.AtLeastOnce);
        }

        #endregion

        #region GetFriends Tests

        [Test]
        public void GetFriends_ValidData_ReturnsList()
        {
            // Arrange
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);
            _socialProxy.Initialize(Guid.NewGuid());

            _mockFriendManager.Setup(m => m.GetFriends(It.IsAny<Guid>()))
                .Returns(new Player[] { new Player() });

            // Act
            var result = _socialProxy.GetFriends();

            // Assert
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void GetFriends_Exception_ReturnsEmptyListAndCloses()
        {
            // Arrange
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);
            _socialProxy.Initialize(Guid.NewGuid());

            _mockFriendManager.Setup(m => m.GetFriends(It.IsAny<Guid>()))
                .Callback(() => _mockCommunicationObject.Setup(s => s.State).Returns(CommunicationState.Faulted))
                .Throws(new CommunicationException());

            // Act
            var result = _socialProxy.GetFriends();

            // Assert
            Assert.That(result, Is.Empty);
            _mockCommunicationObject.Verify(m => m.Abort(), Times.AtLeastOnce);
        }

        #endregion

        #region GetIncomingRequests Tests

        [Test]
        public void GetIncomingRequests_ValidData_ReturnsList()
        {
            // Arrange
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);
            _socialProxy.Initialize(Guid.NewGuid());

            _mockFriendManager.Setup(m => m.GetIncomingRequests(It.IsAny<Guid>()))
                .Returns(new Player[] { new Player() });

            // Act
            var result = _socialProxy.GetIncomingRequests();

            // Assert
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void GetIncomingRequests_Exception_ReturnsEmptyListAndCloses()
        {
            // Arrange
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);
            _socialProxy.Initialize(Guid.NewGuid());

            _mockFriendManager.Setup(m => m.GetIncomingRequests(It.IsAny<Guid>()))
                .Callback(() => _mockCommunicationObject.Setup(s => s.State).Returns(CommunicationState.Faulted))
                .Throws(new Exception());

            // Act
            var result = _socialProxy.GetIncomingRequests();

            // Assert
            Assert.That(result, Is.Empty);
            _mockCommunicationObject.Verify(m => m.Abort(), Times.AtLeastOnce);
        }

        #endregion

        #region GetSentRequests Tests

        [Test]
        public void GetSentRequests_ValidData_ReturnsList()
        {
            // Arrange
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);
            _socialProxy.Initialize(Guid.NewGuid());

            _mockFriendManager.Setup(m => m.GetSentRequests(It.IsAny<Guid>()))
                .Returns(new Player[] { new Player() });

            // Act
            var result = _socialProxy.GetSentRequests();

            // Assert
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void GetSentRequests_Exception_ReturnsEmptyListAndCloses()
        {
            // Arrange
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);
            _socialProxy.Initialize(Guid.NewGuid());

            _mockFriendManager.Setup(m => m.GetSentRequests(It.IsAny<Guid>()))
                .Callback(() => _mockCommunicationObject.Setup(s => s.State).Returns(CommunicationState.Faulted))
                .Throws(new TimeoutException());

            // Act
            var result = _socialProxy.GetSentRequests();

            // Assert
            Assert.That(result, Is.Empty);
            _mockCommunicationObject.Verify(m => m.Abort(), Times.AtLeastOnce);
        }

        #endregion

        #region SendFriendRequest Tests

        [Test]
        public void SendFriendRequest_ValidData_ReturnsSuccess()
        {
            // Arrange
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);
            _socialProxy.Initialize(Guid.NewGuid());

            _mockFriendManager.Setup(m => m.SendFriendRequest(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(new FriendshipRequest { IsSuccess = true });

            // Act
            var result = _socialProxy.SendFriendRequest(Guid.NewGuid());

            // Assert
            Assert.That(result.IsSuccess, Is.True);
        }

        [Test]
        public void SendFriendRequest_NotConnected_ReturnsServerUnavailable()
        {
            // Arrange
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Closed);

            // Act
            var result = _socialProxy.SendFriendRequest(Guid.NewGuid());

            // Assert
            Assert.That(result.StatusCode, Is.EqualTo(StatusCode.SERVER_UNAVAIBLE));
        }

        [Test]
        public void SendFriendRequest_TimeoutException_ReturnsServerTimeout()
        {
            // Arrange
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);
            _socialProxy.Initialize(Guid.NewGuid());

            _mockFriendManager.Setup(m => m.SendFriendRequest(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Callback(() => _mockCommunicationObject.Setup(s => s.State).Returns(CommunicationState.Faulted))
                .Throws(new TimeoutException());

            // Act
            var result = _socialProxy.SendFriendRequest(Guid.NewGuid());

            // Assert
            Assert.That(result.StatusCode, Is.EqualTo(StatusCode.SERVER_TIMEOUT));
            _mockCommunicationObject.Verify(m => m.Abort(), Times.AtLeastOnce);
        }

        [Test]
        public void SendFriendRequest_EndpointNotFound_ReturnsServerUnreachable()
        {
            // Arrange
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);
            _socialProxy.Initialize(Guid.NewGuid());

            _mockFriendManager.Setup(m => m.SendFriendRequest(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Callback(() => _mockCommunicationObject.Setup(s => s.State).Returns(CommunicationState.Faulted))
                .Throws(new EndpointNotFoundException());

            // Act
            var result = _socialProxy.SendFriendRequest(Guid.NewGuid());

            // Assert
            Assert.That(result.StatusCode, Is.EqualTo(StatusCode.SERVER_UNREACHABLE));
            _mockCommunicationObject.Verify(m => m.Abort(), Times.AtLeastOnce);
        }

        [Test]
        public void SendFriendRequest_CommunicationException_ReturnsServerUnavailable()
        {
            // Arrange
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);
            _socialProxy.Initialize(Guid.NewGuid());

            _mockFriendManager.Setup(m => m.SendFriendRequest(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Callback(() => _mockCommunicationObject.Setup(s => s.State).Returns(CommunicationState.Faulted))
                .Throws(new CommunicationException());

            // Act
            var result = _socialProxy.SendFriendRequest(Guid.NewGuid());

            // Assert
            Assert.That(result.StatusCode, Is.EqualTo(StatusCode.SERVER_UNAVAIBLE));
            _mockCommunicationObject.Verify(m => m.Abort(), Times.AtLeastOnce);
        }

        [Test]
        public void SendFriendRequest_GeneralException_ReturnsClientError()
        {
            // Arrange
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);
            _socialProxy.Initialize(Guid.NewGuid());

            _mockFriendManager.Setup(m => m.SendFriendRequest(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Callback(() => _mockCommunicationObject.Setup(s => s.State).Returns(CommunicationState.Faulted))
                .Throws(new Exception());

            // Act
            var result = _socialProxy.SendFriendRequest(Guid.NewGuid());

            // Assert
            Assert.That(result.StatusCode, Is.EqualTo(StatusCode.CLIENT_ERROR));
            _mockCommunicationObject.Verify(m => m.Abort(), Times.AtLeastOnce);
        }

        #endregion

        #region AcceptFriendRequest Tests

        [Test]
        public void AcceptFriendRequest_ValidData_ReturnsSuccess()
        {
            // Arrange
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);
            _socialProxy.Initialize(Guid.NewGuid());

            _mockFriendManager.Setup(m => m.AcceptFriendRequest(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(new FriendshipRequest { IsSuccess = true });

            // Act
            var result = _socialProxy.AcceptFriendRequest(Guid.NewGuid());

            // Assert
            Assert.That(result.IsSuccess, Is.True);
        }

        [Test]
        public void AcceptFriendRequest_Exception_ReturnsClientError()
        {
            // Arrange
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);
            _socialProxy.Initialize(Guid.NewGuid());

            _mockFriendManager.Setup(m => m.AcceptFriendRequest(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Callback(() => _mockCommunicationObject.Setup(s => s.State).Returns(CommunicationState.Faulted))
                .Throws(new Exception());

            // Act
            var result = _socialProxy.AcceptFriendRequest(Guid.NewGuid());

            // Assert
            Assert.That(result.StatusCode, Is.EqualTo(StatusCode.CLIENT_ERROR));
            _mockCommunicationObject.Verify(m => m.Abort(), Times.AtLeastOnce);
        }

        #endregion

        #region RejectFriendRequest Tests

        [Test]
        public void RejectFriendRequest_ValidData_ReturnsSuccess()
        {
            // Arrange
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);
            _socialProxy.Initialize(Guid.NewGuid());

            _mockFriendManager.Setup(m => m.RejectFriendRequest(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(new FriendshipRequest { IsSuccess = true });

            // Act
            var result = _socialProxy.RejectFriendRequest(Guid.NewGuid());

            // Assert
            Assert.That(result.IsSuccess, Is.True);
        }

        [Test]
        public void RejectFriendRequest_Exception_ReturnsClientError()
        {
            // Arrange
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);
            _socialProxy.Initialize(Guid.NewGuid());

            _mockFriendManager.Setup(m => m.RejectFriendRequest(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Callback(() => _mockCommunicationObject.Setup(s => s.State).Returns(CommunicationState.Faulted))
                .Throws(new Exception());

            // Act
            var result = _socialProxy.RejectFriendRequest(Guid.NewGuid());

            // Assert
            Assert.That(result.StatusCode, Is.EqualTo(StatusCode.CLIENT_ERROR));
            _mockCommunicationObject.Verify(m => m.Abort(), Times.AtLeastOnce);
        }

        #endregion

        #region RemoveFriend Tests

        [Test]
        public void RemoveFriend_ValidData_ReturnsSuccess()
        {
            // Arrange
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);
            _socialProxy.Initialize(Guid.NewGuid());

            _mockFriendManager.Setup(m => m.RemoveFriend(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(new FriendshipRequest { IsSuccess = true });

            // Act
            var result = _socialProxy.RemoveFriend(Guid.NewGuid());

            // Assert
            Assert.That(result.IsSuccess, Is.True);
        }

        [Test]
        public void RemoveFriend_Exception_ReturnsClientError()
        {
            // Arrange
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);
            _socialProxy.Initialize(Guid.NewGuid());

            _mockFriendManager.Setup(m => m.RemoveFriend(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Callback(() => _mockCommunicationObject.Setup(s => s.State).Returns(CommunicationState.Faulted))
                .Throws(new Exception());

            // Act
            var result = _socialProxy.RemoveFriend(Guid.NewGuid());

            // Assert
            Assert.That(result.StatusCode, Is.EqualTo(StatusCode.CLIENT_ERROR));
            _mockCommunicationObject.Verify(m => m.Abort(), Times.AtLeastOnce);
        }

        #endregion
    }
}