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
            Guid playerId = Guid.NewGuid();

            _scoreboardProxy.Initialize(playerId);

            _mockScoreboardManager.Verify(m => m.SubscribeToScoreboardUpdates(playerId), Times.Once);
        }

        [Test]
        public void Initialize_CommunicationException_AbortsProxy()
        {
            _mockScoreboardManager
                .Setup(m => m.SubscribeToScoreboardUpdates(It.IsAny<Guid>()))
                .Throws(new CommunicationException());
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Faulted);

            _scoreboardProxy.Initialize(Guid.NewGuid());

            _mockCommunicationObject.Verify(m => m.Abort(), Times.Once);
        }

        [Test]
        public void Initialize_TimeoutException_AbortsProxy()
        {
            _mockScoreboardManager
                .Setup(m => m.SubscribeToScoreboardUpdates(It.IsAny<Guid>()))
                .Throws(new TimeoutException());
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Faulted);

            _scoreboardProxy.Initialize(Guid.NewGuid());

            _mockCommunicationObject.Verify(m => m.Abort(), Times.Once);
        }

        [Test]
        public void Initialize_GeneralException_AbortsProxy()
        {
            _mockScoreboardManager
                .Setup(m => m.SubscribeToScoreboardUpdates(It.IsAny<Guid>()))
                .Throws(new Exception());
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Faulted);

            _scoreboardProxy.Initialize(Guid.NewGuid());

            _mockCommunicationObject.Verify(m => m.Abort(), Times.Once);
        }

        [Test]
        public void Disconnect_ClientOpen_UnsubscribesAndCloses()
        {
            Guid playerId = Guid.NewGuid();
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);
            _scoreboardProxy.Initialize(playerId);

            _scoreboardProxy.Disconnect();

            _mockScoreboardManager.Verify(m => m.UnsubscribeFromScoreboardUpdates(playerId), Times.Once);
            _mockCommunicationObject.Verify(m => m.Close(), Times.Once);
        }

        [Test]
        public void Disconnect_ClientNotOpen_DoesNothing()
        {
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Closed);
            _scoreboardProxy.Initialize(Guid.NewGuid());

            _scoreboardProxy.Disconnect();

            _mockScoreboardManager.Verify(m => m.UnsubscribeFromScoreboardUpdates(It.IsAny<Guid>()), Times.Never);
        }

        [Test]
        public void Disconnect_CommunicationException_AbortsProxy()
        {
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);
            _scoreboardProxy.Initialize(Guid.NewGuid());
            _mockScoreboardManager
                .Setup(m => m.UnsubscribeFromScoreboardUpdates(It.IsAny<Guid>()))
                .Callback(() => _mockCommunicationObject.Setup(s => s.State).Returns(CommunicationState.Faulted))
                .Throws(new CommunicationException());

            _scoreboardProxy.Disconnect();

            _mockCommunicationObject.Verify(m => m.Abort(), Times.Once);
        }

        [Test]
        public void Disconnect_TimeoutException_AbortsProxy()
        {
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);
            _scoreboardProxy.Initialize(Guid.NewGuid());
            _mockScoreboardManager
                .Setup(m => m.UnsubscribeFromScoreboardUpdates(It.IsAny<Guid>()))
                .Callback(() => _mockCommunicationObject.Setup(s => s.State).Returns(CommunicationState.Faulted))
                .Throws(new TimeoutException());

            _scoreboardProxy.Disconnect();

            _mockCommunicationObject.Verify(m => m.Abort(), Times.Once);
        }

        [Test]
        public void Disconnect_GeneralException_AbortsProxy()
        {
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);
            _scoreboardProxy.Initialize(Guid.NewGuid());
            _mockScoreboardManager
                .Setup(m => m.UnsubscribeFromScoreboardUpdates(It.IsAny<Guid>()))
                .Callback(() => _mockCommunicationObject.Setup(s => s.State).Returns(CommunicationState.Faulted))
                .Throws(new Exception());

            _scoreboardProxy.Disconnect();

            _mockCommunicationObject.Verify(m => m.Abort(), Times.Once);
        }

        [Test]
        public void GetMyScore_ValidData_ReturnsScoreboardDM()
        {
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

            var result = _scoreboardProxy.GetMyScore(playerId);

            Assert.That(result.GamesWon.Equals(expectedScore.GamesWon));
        }

        [Test]
        public void GetMyScore_NullResponse_ReturnsNull()
        {
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Opened);
            _scoreboardProxy.Initialize(Guid.NewGuid());
            _mockScoreboardManager
                .Setup(m => m.GetMyScore(It.IsAny<Guid>()))
                .Returns((Scoreboard)null);

            var result = _scoreboardProxy.GetMyScore(Guid.NewGuid());

            Assert.That(result, Is.Null);
        }

        [Test]
        public void GetMyScore_ClientClosed_ReconnectsAndReturnsScore()
        {
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

            var result = _scoreboardProxy.GetMyScore(playerId);

            Assert.That(result.GamesWon.Equals(expectedScore.GamesWon));
            _mockScoreboardManager.Verify(m => m.SubscribeToScoreboardUpdates(playerId), Times.Exactly(2));
        }

        [Test]
        public void GetMyScore_CommunicationException_ReturnsNullAndAborts()
        {
            _mockCommunicationObject.SetupSequence(m => m.State)
                .Returns(CommunicationState.Opened)
                .Returns(CommunicationState.Opened)
                .Returns(CommunicationState.Faulted);
            _scoreboardProxy.Initialize(Guid.NewGuid());
            _mockScoreboardManager
                .Setup(m => m.GetMyScore(It.IsAny<Guid>()))
                .Throws(new CommunicationException());

            var result = _scoreboardProxy.GetMyScore(Guid.NewGuid());

            Assert.That(result, Is.Null);
            _mockCommunicationObject.Verify(m => m.Abort(), Times.Once);
        }

        [Test]
        public void GetMyScore_TimeoutException_ReturnsNullAndAborts()
        {
            _mockCommunicationObject.SetupSequence(m => m.State)
                .Returns(CommunicationState.Opened)
                .Returns(CommunicationState.Opened)
                .Returns(CommunicationState.Faulted);
            _scoreboardProxy.Initialize(Guid.NewGuid());
            _mockScoreboardManager
                .Setup(m => m.GetMyScore(It.IsAny<Guid>()))
                .Throws(new TimeoutException());

            var result = _scoreboardProxy.GetMyScore(Guid.NewGuid());

            Assert.That(result, Is.Null);
            _mockCommunicationObject.Verify(m => m.Abort(), Times.Once);
        }

        [Test]
        public void GetMyScore_GeneralException_ReturnsNullAndCloses()
        {
            _mockCommunicationObject.SetupSequence(m => m.State)
                .Returns(CommunicationState.Opened)
                .Returns(CommunicationState.Opened)
                .Returns(CommunicationState.Faulted);
            _scoreboardProxy.Initialize(Guid.NewGuid());
            _mockScoreboardManager
                .Setup(m => m.GetMyScore(It.IsAny<Guid>()))
                .Throws(new Exception());

            var result = _scoreboardProxy.GetMyScore(Guid.NewGuid());

            Assert.That(result, Is.Null);
            _mockCommunicationObject.Verify(m => m.Abort(), Times.Once);
        }
    }
}