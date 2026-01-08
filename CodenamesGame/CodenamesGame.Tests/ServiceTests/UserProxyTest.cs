using CodenamesGame.Domain.POCO;
using CodenamesGame.Network.Proxies.Wrappers;
using CodenamesGame.UserService;
using Moq;
using NUnit.Framework;
using System;
using System.ServiceModel;

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
        public void SignIn_SignInSuccessful_ReturnsSuccessRequest()
        {
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

            var result = _userProxy.SignIn(user, player);

            Assert.That(result.IsSuccess);
        }

        [Test]
        public void SignIn_ServerTimeout_ReturnsTimeoutStatusCode()
        {
            _mockUserManager
                .Setup(m => m.SignIn(It.IsAny<Player>()))
                .Throws(new TimeoutException());

            var result = _userProxy.SignIn(new UserDM(), new PlayerDM());

            Assert.That(StatusCode.SERVER_TIMEOUT.Equals(result.StatusCode));
        }

        [Test]
        public void SignIn_EndpointNotFound_ReturnsServerUnreachableStatusCode()
        {
            _mockUserManager
                .Setup(m => m.SignIn(It.IsAny<Player>()))
                .Throws(new EndpointNotFoundException());

            var result = _userProxy.SignIn(new UserDM(), new PlayerDM());

            Assert.That(StatusCode.SERVER_UNREACHABLE.Equals(result.StatusCode));
        }

        [Test]
        public void SignIn_CommunicationException_ReturnsServerUnavailableStatusCode()
        {
            _mockUserManager
                .Setup(m => m.SignIn(It.IsAny<Player>()))
                .Throws(new CommunicationException());

            var result = _userProxy.SignIn(new UserDM(), new PlayerDM());

            Assert.That(StatusCode.SERVER_UNAVAIBLE.Equals(result.StatusCode));
        }

        [Test]
        public void SignIn_GeneralException_ReturnsClientErrorStatusCode()
        {
            _mockUserManager
                .Setup(m => m.SignIn(It.IsAny<Player>()))
                .Throws(new Exception("Unexpected"));

            var result = _userProxy.SignIn(new UserDM(), new PlayerDM());

            Assert.That(StatusCode.CLIENT_ERROR.Equals(result.StatusCode));
        }

        [Test]
        public void GetPlayer_ValidID_ReturnsPlayerDM()
        {
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

            var result = _userProxy.GetPlayer(userId);

            Assert.That(expectedDto.Username.Equals(result.Username) && !result.PlayerID.Equals(Guid.Empty));
        }

        [Test]
        public void GetPlayer_ServerTimeout_ReturnsEmptyPlayerDM()
        {
            _mockUserManager
                .Setup(m => m.GetPlayerByUserID(It.IsAny<Guid>()))
                .Throws(new TimeoutException());

            var result = _userProxy.GetPlayer(Guid.NewGuid());

            Assert.That(Guid.Empty.Equals(result.PlayerID));
        }

        [Test]
        public void GetPlayer_EndpointNotFound_ReturnsEmptyPlayerDM()
        {
            _mockUserManager
                .Setup(m => m.GetPlayerByUserID(It.IsAny<Guid>()))
                .Throws(new EndpointNotFoundException());

            var result = _userProxy.GetPlayer(Guid.NewGuid());

            Assert.That(Guid.Empty.Equals(result.PlayerID));
        }

        [Test]
        public void GetPlayer_CommunicationException_ReturnsEmptyPlayerDM()
        {
            _mockUserManager
                .Setup(m => m.GetPlayerByUserID(It.IsAny<Guid>()))
                .Throws(new CommunicationException());

            var result = _userProxy.GetPlayer(Guid.NewGuid());

            Assert.That(Guid.Empty.Equals(result.PlayerID));
        }

        [Test]
        public void GetPlayer_GeneralException_ReturnsEmptyPlayerDM()
        {
            _mockUserManager
                .Setup(m => m.GetPlayerByUserID(It.IsAny<Guid>()))
                .Throws(new Exception());

            var result = _userProxy.GetPlayer(Guid.NewGuid());

            Assert.That(Guid.Empty.Equals(result.PlayerID));
        }

        [Test]
        public void UpdateProfile_SuccessfulUpdate_ReturnsSuccessRequest()
        {
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

            var result = _userProxy.UpdateProfile(player);

            Assert.That(result.IsSuccess);
        }

        [Test]
        public void UpdateProfile_ServerTimeout_ReturnsTimeoutStatusCode()
        {
            _mockUserManager
                .Setup(m => m.UpdateProfile(It.IsAny<Player>()))
                .Throws(new TimeoutException());

            var result = _userProxy.UpdateProfile(new PlayerDM());

            Assert.That(StatusCode.SERVER_TIMEOUT.Equals(result.StatusCode));
        }

        [Test]
        public void UpdateProfile_EndpointNotFound_ReturnsServerUnreachableStatusCode()
        {
            _mockUserManager
                .Setup(m => m.UpdateProfile(It.IsAny<Player>()))
                .Throws(new EndpointNotFoundException());

            var result = _userProxy.UpdateProfile(new PlayerDM());

            Assert.That(StatusCode.SERVER_UNREACHABLE.Equals(result.StatusCode));
        }

        [Test]
        public void UpdateProfile_CommunicationException_ReturnsServerUnavaibleStatusCode()
        {
            _mockUserManager
                .Setup(m => m.UpdateProfile(It.IsAny<Player>()))
                .Throws(new CommunicationException());

            var result = _userProxy.UpdateProfile(new PlayerDM());

            Assert.That(StatusCode.SERVER_UNAVAIBLE.Equals(result.StatusCode));
        }

        [Test]
        public void UpdateProfile_GeneralException_ReturnsClientErrorStatusCode()
        {
            _mockUserManager
                .Setup(m => m.UpdateProfile(It.IsAny<Player>()))
                .Throws(new Exception());

            var result = _userProxy.UpdateProfile(new PlayerDM());

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(StatusCode.CLIENT_ERROR.Equals(result.StatusCode));
        }
    }
}
