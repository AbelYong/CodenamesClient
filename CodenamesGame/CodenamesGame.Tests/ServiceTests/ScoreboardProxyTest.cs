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

            _scoreboardProxy = new ScoreboardProxy((context, endpoint) => _mockScoreboardManager.Object);
        }

        [Test]
        public void Initialize_ValidData_SubscribesToUpdates()
        {
            Guid playerId = Guid.NewGuid();

            _scoreboardProxy.Initialize(playerId);

            _mockScoreboardManager.Verify(m => m.SubscribeToScoreboardUpdates(playerId), Times.Once);
            Assert.That(_mockCommunicationObject.Object.State.Equals(CommunicationState.Opened));
        }

        [Test]
        public void Initialize_CommunicationException_ClosesProxy()
        {
            _mockScoreboardManager.Setup(m => m.SubscribeToScoreboardUpdates(It.IsAny<Guid>()))
                .Throws(new CommunicationException());

            _scoreboardProxy.Initialize(Guid.NewGuid());

            Assert.That(_mockCommunicationObject.Object.State.Equals(CommunicationState.Closed));
        }

        [Test]
        public void Initialize_TimeoutException_ClosesProxy()
        {
            _mockScoreboardManager.Setup(m => m.SubscribeToScoreboardUpdates(It.IsAny<Guid>()))
                .Throws(new TimeoutException());

            _scoreboardProxy.Initialize(Guid.NewGuid());

            Assert.That(_mockCommunicationObject.Object.State.Equals(CommunicationState.Closed));
        }

        [Test]
        public void Initialize_GeneralException_ClosesProxy()
        {
            _mockScoreboardManager.Setup(m => m.SubscribeToScoreboardUpdates(It.IsAny<Guid>()))
                .Throws(new Exception());

            _scoreboardProxy.Initialize(Guid.NewGuid());

            Assert.That(_mockCommunicationObject.Object.State.Equals(CommunicationState.Closed));
        }

        [Test]
        public void Disconnect_ClientOpen_UnsubscribesAndClosesProxy()
        {
            Guid playerId = Guid.NewGuid();
            _scoreboardProxy.Initialize(playerId);

            _scoreboardProxy.Disconnect();

            _mockScoreboardManager.Verify(m => m.UnsubscribeFromScoreboardUpdates(playerId), Times.Once);
            _mockCommunicationObject.Verify(m => m.Close(), Times.Once);
        }

        [Test]
        public void Disconnect_ClientNotOpen_DoesNothing()
        {
            _mockCommunicationObject.Setup(m => m.State).Returns(CommunicationState.Closed);

            _scoreboardProxy.Disconnect();

            _mockScoreboardManager.Verify(m => m.UnsubscribeFromScoreboardUpdates(It.IsAny<Guid>()), Times.Never);
        }

        [Test]
        public void Disconnect_CommunicationException_ClosesProxy()
        {
            Guid playerID = Guid.NewGuid();
            _scoreboardProxy.Initialize(playerID);
            _mockScoreboardManager.Setup(m => m.UnsubscribeFromScoreboardUpdates(It.IsAny<Guid>()))
                .Callback(() => _mockCommunicationObject.Setup(s => s.State).Returns(CommunicationState.Faulted))
                .Throws(new CommunicationException());

            _scoreboardProxy.Disconnect();

            _mockScoreboardManager.Verify(m => m.UnsubscribeFromScoreboardUpdates(playerID), Times.Once);
            Assert.That(_mockCommunicationObject.Object.State.Equals(CommunicationState.Closed));
        }

        [Test]
        public void Disconnect_TimeoutException_ClosesProxy()
        {
            Guid playerID = Guid.NewGuid();
            _scoreboardProxy.Initialize(playerID);
            _mockScoreboardManager.Setup(m => m.UnsubscribeFromScoreboardUpdates(It.IsAny<Guid>()))
                .Callback(() => _mockCommunicationObject.Setup(s => s.State).Returns(CommunicationState.Faulted))
                .Throws(new TimeoutException());

            _scoreboardProxy.Disconnect();

            _mockScoreboardManager.Verify(m => m.UnsubscribeFromScoreboardUpdates(playerID), Times.Once);
            Assert.That(_mockCommunicationObject.Object.State.Equals(CommunicationState.Closed));
        }

        [Test]
        public void Disconnect_GeneralException_ClosesProxy()
        {
            Guid playerID = Guid.NewGuid();
            _scoreboardProxy.Initialize(playerID);
            _mockScoreboardManager.Setup(m => m.UnsubscribeFromScoreboardUpdates(It.IsAny<Guid>()))
                .Callback(() => _mockCommunicationObject.Setup(s => s.State).Returns(CommunicationState.Faulted))
                .Throws(new Exception());

            _scoreboardProxy.Disconnect();

            _mockScoreboardManager.Verify(m => m.UnsubscribeFromScoreboardUpdates(playerID), Times.Once);
            Assert.That(_mockCommunicationObject.Object.State.Equals(CommunicationState.Closed));
        }

        [Test]
        public void GetMyScore_ValidData_ReturnsScoreboardDM()
        {
            Guid playerId = Guid.NewGuid();
            var expectedScore = new Scoreboard
            {
                GamesWon = 10
            };
            _scoreboardProxy.Initialize(playerId);
            _mockScoreboardManager.Setup(m => m.GetMyScore(playerId))
                .Returns(expectedScore);

            var result = _scoreboardProxy.GetMyScore(playerId);

            Assert.That(result.GamesWon.Equals(expectedScore.GamesWon));
        }

        [Test]
        public void GetMyScore_NullResponse_ReturnsNull()
        {
            _scoreboardProxy.Initialize(Guid.NewGuid());
            _mockScoreboardManager.Setup(m => m.GetMyScore(It.IsAny<Guid>()))
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
            _mockScoreboardManager.Setup(m => m.GetMyScore(playerId))
                .Returns(expectedScore);

            var result = _scoreboardProxy.GetMyScore(playerId);

            Assert.That(result.GamesWon.Equals(expectedScore.GamesWon) &&
                _mockCommunicationObject.Object.State.Equals(CommunicationState.Opened));
        }

        [Test]
        public void GetMyScore_CommunicationException_ReturnsNullAndClosesProxy()
        {
            _scoreboardProxy.Initialize(Guid.NewGuid());
            _mockScoreboardManager.Setup(m => m.GetMyScore(It.IsAny<Guid>()))
                .Throws(new CommunicationException());

            var result = _scoreboardProxy.GetMyScore(Guid.NewGuid());

            Assert.That(result == null &&
                _mockCommunicationObject.Object.State.Equals(CommunicationState.Closed));
        }

        [Test]
        public void GetMyScore_TimeoutException_ReturnsNullAndClosesProxy()
        {
            _scoreboardProxy.Initialize(Guid.NewGuid());
            _mockScoreboardManager.Setup(m => m.GetMyScore(It.IsAny<Guid>()))
                .Throws(new TimeoutException());

            var result = _scoreboardProxy.GetMyScore(Guid.NewGuid());

            Assert.That(result == null &&
                _mockCommunicationObject.Object.State.Equals(CommunicationState.Closed));
        }

        [Test]
        public void GetMyScore_GeneralException_ReturnsNullAndClosesProxy()
        {
            _scoreboardProxy.Initialize(Guid.NewGuid());
            _mockScoreboardManager.Setup(m => m.GetMyScore(It.IsAny<Guid>()))
                .Throws(new Exception());

            var result = _scoreboardProxy.GetMyScore(Guid.NewGuid());

            Assert.That(result == null &&
                _mockCommunicationObject.Object.State.Equals(CommunicationState.Closed));
        }
    }
}