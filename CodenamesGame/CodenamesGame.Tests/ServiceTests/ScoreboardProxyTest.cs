using CodenamesGame.Domain.POCO;
using CodenamesGame.Network.Proxies.Wrappers;
using CodenamesGame.ScoreboardService;
using Moq;
using NUnit.Framework;
using System;
using System.ServiceModel;

namespace CodenamesGame.Tests.ServiceTests
{
    [TestFixture]
    public class ScoreboardProxyTest
    {
        private Mock<IScoreboardManager> _mockScoreboardManager;
        private Mock<ICommunicationObject> _mockCommunicationObject;
        private ScoreboardProxy _scoreboardProxy;

        [SetUp]
        public void Setup()
        {
            _mockScoreboardManager = new Mock<IScoreboardManager>();
            _mockCommunicationObject = _mockScoreboardManager.As<ICommunicationObject>();

            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Created);

            _scoreboardProxy = new ScoreboardProxy((context, endpoint) => _mockScoreboardManager.Object);
        }

        [Test]
        public void Initialize_ValidData_SubscribesToUpdates()
        {
            // Arrange
            Guid playerId = Guid.NewGuid();

            // Act
            _scoreboardProxy.Initialize(playerId);

            // Assert
            _mockScoreboardManager.Verify(m => m.SubscribeToScoreboardUpdates(playerId), Times.Once);
        }

        [Test]
        public void Initialize_CommunicationException_ClosesProxy()
        {
            // Arrange
            _mockScoreboardManager
                .Setup(m => m.SubscribeToScoreboardUpdates(It.IsAny<Guid>()))
                .Throws(new CommunicationException());

            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Faulted);

            // Act
            _scoreboardProxy.Initialize(Guid.NewGuid());

            // Assert
            _mockCommunicationObject.Verify(m => m.Abort(), Times.Once);
        }

        [Test]
        public void Initialize_TimeoutException_ClosesProxy()
        {
            // Arrange
            _mockScoreboardManager
                .Setup(m => m.SubscribeToScoreboardUpdates(It.IsAny<Guid>()))
                .Throws(new TimeoutException());

            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Faulted);

            // Act
            _scoreboardProxy.Initialize(Guid.NewGuid());

            // Assert
            _mockCommunicationObject.Verify(m => m.Abort(), Times.Once);
        }

        [Test]
        public void Initialize_GeneralException_ClosesProxy()
        {
            // Arrange
            _mockScoreboardManager
                .Setup(m => m.SubscribeToScoreboardUpdates(It.IsAny<Guid>()))
                .Throws(new Exception());

            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Faulted);

            // Act
            _scoreboardProxy.Initialize(Guid.NewGuid());

            // Assert
            _mockCommunicationObject.Verify(m => m.Abort(), Times.Once);
        }

        [Test]
        public void Disconnect_ClientOpen_UnsubscribesAndCloses()
        {
            // Arrange
            Guid playerId = Guid.NewGuid();
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);

            _scoreboardProxy.Initialize(playerId);

            // Act
            _scoreboardProxy.Disconnect();

            // Assert
            _mockScoreboardManager.Verify(m => m.UnsubscribeFromScoreboardUpdates(playerId), Times.Once);
        }

        [Test]
        public void Disconnect_ClientNotOpen_DoesNothing()
        {
            // Arrange
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Closed);

            _scoreboardProxy.Initialize(Guid.NewGuid());

            // Act
            _scoreboardProxy.Disconnect();

            // Assert
            _mockScoreboardManager.Verify(m => m.UnsubscribeFromScoreboardUpdates(It.IsAny<Guid>()), Times.Never);
        }

        [Test]
        public void Disconnect_CommunicationException_ClosesProxy()
        {
            // Arrange
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);
            _scoreboardProxy.Initialize(Guid.NewGuid());

            _mockScoreboardManager
                .Setup(m => m.UnsubscribeFromScoreboardUpdates(It.IsAny<Guid>()))
                .Callback(() => _mockCommunicationObject.Setup(s => s.State).Returns(CommunicationState.Faulted))
                .Throws(new CommunicationException());

            // Act
            _scoreboardProxy.Disconnect();

            // Assert
            _mockCommunicationObject.Verify(m => m.Abort(), Times.AtLeastOnce);
        }

        [Test]
        public void Disconnect_TimeoutException_ClosesProxy()
        {
            // Arrange
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);
            _scoreboardProxy.Initialize(Guid.NewGuid());

            _mockScoreboardManager
                .Setup(m => m.UnsubscribeFromScoreboardUpdates(It.IsAny<Guid>()))
                .Callback(() => _mockCommunicationObject.Setup(s => s.State).Returns(CommunicationState.Faulted))
                .Throws(new TimeoutException());

            // Act
            _scoreboardProxy.Disconnect();

            // Assert
            _mockCommunicationObject.Verify(m => m.Abort(), Times.AtLeastOnce);
        }

        [Test]
        public void Disconnect_GeneralException_ClosesProxy()
        {
            // Arrange
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);
            _scoreboardProxy.Initialize(Guid.NewGuid());

            _mockScoreboardManager
                .Setup(m => m.UnsubscribeFromScoreboardUpdates(It.IsAny<Guid>()))
                .Callback(() => _mockCommunicationObject.Setup(s => s.State).Returns(CommunicationState.Faulted))
                .Throws(new Exception());

            // Act
            _scoreboardProxy.Disconnect();

            // Assert
            _mockCommunicationObject.Verify(m => m.Abort(), Times.AtLeastOnce);
        }

        [Test]
        public void GetMyScore_ValidData_ReturnsScoreboardDM()
        {
            // Arrange
            Guid playerId = Guid.NewGuid();
            var expectedScore = new Scoreboard
            {
                Username = "Player1",
                GamesWon = 10,
                FastestMatch = "05:00",
                AssassinsRevealed = 2
            };

            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);
            _scoreboardProxy.Initialize(playerId);

            _mockScoreboardManager
                .Setup(m => m.GetMyScore(playerId))
                .Returns(expectedScore);

            // Act
            var result = _scoreboardProxy.GetMyScore(playerId);

            // Assert
            Assert.That(result.GamesWon, Is.EqualTo(expectedScore.GamesWon));
        }

        [Test]
        public void GetMyScore_NullResponse_ReturnsNull()
        {
            // Arrange
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);
            _scoreboardProxy.Initialize(Guid.NewGuid());

            _mockScoreboardManager
                .Setup(m => m.GetMyScore(It.IsAny<Guid>()))
                .Returns((Scoreboard)null);

            // Act
            var result = _scoreboardProxy.GetMyScore(Guid.NewGuid());

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void GetMyScore_ClientClosed_ReconnectsAndReturnsScore()
        {
            // Arrange
            Guid playerId = Guid.NewGuid();
            var expectedScore = new Scoreboard { GamesWon = 5 };

            _scoreboardProxy.Initialize(playerId);

            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Closed);

            _mockScoreboardManager
                .Setup(m => m.GetMyScore(playerId))
                .Returns(expectedScore);

            _mockScoreboardManager
                .Setup(m => m.SubscribeToScoreboardUpdates(playerId))
                .Callback(() => _mockCommunicationObject.Setup(s => s.State).Returns(CommunicationState.Opened));

            // Act
            var result = _scoreboardProxy.GetMyScore(playerId);

            // Assert
            _mockScoreboardManager.Verify(m => m.SubscribeToScoreboardUpdates(playerId), Times.Exactly(2));
        }

        [Test]
        public void GetMyScore_CommunicationException_ReturnsNullAndCloses()
        {
            // Arrange
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);
            _scoreboardProxy.Initialize(Guid.NewGuid());

            _mockScoreboardManager
                .Setup(m => m.GetMyScore(It.IsAny<Guid>()))
                .Throws(new CommunicationException());

            // Act
            var result = _scoreboardProxy.GetMyScore(Guid.NewGuid());

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void GetMyScore_TimeoutException_ReturnsNullAndCloses()
        {
            // Arrange
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);
            _scoreboardProxy.Initialize(Guid.NewGuid());

            _mockScoreboardManager
                .Setup(m => m.GetMyScore(It.IsAny<Guid>()))
                .Throws(new TimeoutException());

            // Act
            var result = _scoreboardProxy.GetMyScore(Guid.NewGuid());

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void GetMyScore_GeneralException_ReturnsNullAndCloses()
        {
            // Arrange
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);
            _scoreboardProxy.Initialize(Guid.NewGuid());

            _mockScoreboardManager
                .Setup(m => m.GetMyScore(It.IsAny<Guid>()))
                .Throws(new Exception());

            // Act
            var result = _scoreboardProxy.GetMyScore(Guid.NewGuid());

            // Assert
            Assert.That(result, Is.Null);
        }
    }
}