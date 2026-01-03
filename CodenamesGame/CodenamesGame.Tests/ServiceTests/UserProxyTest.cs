using CodenamesGame.Domain.POCO;
using CodenamesGame.Network.Proxies.Wrappers;
using CodenamesGame.UserService;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace CodenamesGame.Tests.ServiceTests
{
    [TestFixture]
    public class UserProxyTest
    {
        private Mock<IUserManager> _mockUserManager;
        private UserProxy _userProxy;

        [SetUp]
        public void Setup()
        {
            _mockUserManager = new Mock<IUserManager>();
            _userProxy = new UserProxy(() => _mockUserManager.Object);
        }

        [Test]
        public void SignIn_ValidProfile_ReturnsSuccessRequest()
        {
            // Arrange
            var user = new UserDM { Email = "test@test.com", Password = "password" };
            var player = new PlayerDM { Username = "TestUser" };

            var expectedRequest = new SignInRequest
            {
                IsSuccess = true,
                StatusCode = StatusCode.OK
            };

            _mockUserManager
                .Setup(m => m.SignIn(It.IsAny<Player>()))
                .Returns(expectedRequest);

            // Act
            var result = _userProxy.SignIn(user, player);

            // Assert
            Assert.That(result.IsSuccess);
            Assert.That(StatusCode.OK.Equals(result.StatusCode));
        }

        [Test]
        public void SignIn_ServerTimeout_ReturnsTimeoutStatusCode()
        {
            // Arrange
            _mockUserManager
                .Setup(m => m.SignIn(It.IsAny<Player>()))
                .Throws(new TimeoutException());

            // Act
            var result = _userProxy.SignIn(new UserDM(), new PlayerDM());

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(StatusCode.SERVER_TIMEOUT.Equals(result.StatusCode));
        }

        [Test]
        public void SignIn_EndpointNotFound_ReturnsServerUnreachableStatusCode()
        {
            // Arrange
            _mockUserManager
                .Setup(m => m.SignIn(It.IsAny<Player>()))
                .Throws(new EndpointNotFoundException());

            // Act
            var result = _userProxy.SignIn(new UserDM(), new PlayerDM());

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(StatusCode.SERVER_UNREACHABLE.Equals(result.StatusCode));
        }

        [Test]
        public void SignIn_CommunicationException_ReturnsServerUnavailableStatusCode()
        {
            // Arrange
            _mockUserManager
                .Setup(m => m.SignIn(It.IsAny<Player>()))
                .Throws(new CommunicationException());

            // Act
            var result = _userProxy.SignIn(new UserDM(), new PlayerDM());

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(StatusCode.SERVER_UNAVAIBLE.Equals(result.StatusCode));
        }

        [Test]
        public void SignIn_GeneralException_ReturnsClientErrorStatusCode()
        {
            // Arrange
            _mockUserManager
                .Setup(m => m.SignIn(It.IsAny<Player>()))
                .Throws(new Exception("Unexpected"));

            // Act
            var result = _userProxy.SignIn(new UserDM(), new PlayerDM());

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(StatusCode.CLIENT_ERROR.Equals(result.StatusCode));
        }

        [Test]
        public void GetPlayer_ValidID_ReturnsPlayerDM()
        {
            // Arrange
            Guid userId = Guid.NewGuid();
            var expectedDto = new Player
            {
                PlayerID = Guid.NewGuid(),
                Username = "RetrievedUser",
                User = new User { UserID = userId }
            };

            _mockUserManager
                .Setup(m => m.GetPlayerByUserID(userId))
                .Returns(expectedDto);

            // Act
            var result = _userProxy.GetPlayer(userId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(expectedDto.Username.Equals(result.Username));
            Assert.That(Guid.Empty.Equals(result.PlayerID), Is.False);
        }

        [Test]
        public void GetPlayer_ServerTimeout_ReturnsEmptyPlayerDM()
        {
            // Arrange
            _mockUserManager
                .Setup(m => m.GetPlayerByUserID(It.IsAny<Guid>()))
                .Throws(new TimeoutException());

            // Act
            var result = _userProxy.GetPlayer(Guid.NewGuid());

            // Assert
            Assert.That(Guid.Empty.Equals(result.PlayerID));
        }

        [Test]
        public void GetPlayer_EndpointNotFound_ReturnsEmptyPlayerDM()
        {
            // Arrange
            _mockUserManager
                .Setup(m => m.GetPlayerByUserID(It.IsAny<Guid>()))
                .Throws(new EndpointNotFoundException());

            // Act
            var result = _userProxy.GetPlayer(Guid.NewGuid());

            // Assert
            Assert.That(Guid.Empty.Equals(result.PlayerID));
        }

        [Test]
        public void GetPlayer_CommunicationException_ReturnsEmptyPlayerDM()
        {
            // Arrange
            _mockUserManager
                .Setup(m => m.GetPlayerByUserID(It.IsAny<Guid>()))
                .Throws(new CommunicationException());

            // Act
            var result = _userProxy.GetPlayer(Guid.NewGuid());

            // Assert
            Assert.That(Guid.Empty.Equals(result.PlayerID));
        }

        [Test]
        public void GetPlayer_GeneralException_ReturnsEmptyPlayerDM()
        {
            // Arrange
            _mockUserManager
                .Setup(m => m.GetPlayerByUserID(It.IsAny<Guid>()))
                .Throws(new Exception());

            // Act
            var result = _userProxy.GetPlayer(Guid.NewGuid());

            // Assert
            Assert.That(Guid.Empty.Equals(result.PlayerID));
        }

        [Test]
        public void UpdateProfile_ValidData_ReturnsSuccessRequest()
        {
            // Arrange
            var player = new PlayerDM
            {
                Username = "UpdatedUser",
                User = new UserDM { Email = "test@test.com" }
            };

            var expectedRequest = new CommunicationRequest
            {
                IsSuccess = true,
                StatusCode = StatusCode.OK
            };

            _mockUserManager
                .Setup(m => m.UpdateProfile(It.IsAny<Player>()))
                .Returns(expectedRequest);

            // Act
            var result = _userProxy.UpdateProfile(player);

            // Assert
            Assert.That(result.IsSuccess);
            Assert.That(StatusCode.OK.Equals(result.StatusCode));
        }

        [Test]
        public void UpdateProfile_ServerTimeout_ReturnsTimeoutStatusCode()
        {
            // Arrange
            _mockUserManager
                .Setup(m => m.UpdateProfile(It.IsAny<Player>()))
                .Throws(new TimeoutException());

            // Act
            var result = _userProxy.UpdateProfile(new PlayerDM());

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(StatusCode.SERVER_TIMEOUT.Equals(result.StatusCode));
        }

        [Test]
        public void UpdateProfile_EndpointNotFound_ReturnsServerUnreachableStatusCode()
        {
            // Arrange
            _mockUserManager
                .Setup(m => m.UpdateProfile(It.IsAny<Player>()))
                .Throws(new EndpointNotFoundException());

            // Act
            var result = _userProxy.UpdateProfile(new PlayerDM());

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(StatusCode.SERVER_UNREACHABLE.Equals(result.StatusCode));
        }

        [Test]
        public void UpdateProfile_CommunicationException_ReturnsServerUnavaibleStatusCode()
        {
            // Arrange
            _mockUserManager
                .Setup(m => m.UpdateProfile(It.IsAny<Player>()))
                .Throws(new CommunicationException());

            // Act
            var result = _userProxy.UpdateProfile(new PlayerDM());

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(StatusCode.SERVER_UNAVAIBLE.Equals(result.StatusCode));
        }

        [Test]
        public void UpdateProfile_GeneralException_ReturnsClientErrorStatusCode()
        {
            // Arrange
            _mockUserManager
                .Setup(m => m.UpdateProfile(It.IsAny<Player>()))
                .Throws(new Exception());

            // Act
            var result = _userProxy.UpdateProfile(new PlayerDM());

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(StatusCode.CLIENT_ERROR.Equals(result.StatusCode));
        }
    }
}
