using CodenamesGame.Domain.POCO;
using System;
using System.ServiceModel;
using CodenamesGame.AuthenticationService;
using CodenamesGame.UserService;
using CodenamesGame.Util;

namespace CodenamesGame.Network
{
    public static class UserOperation
    {
        private const string _AUTHENTICATION_ENDPOINT_NAME = "NetTcpBinding_IAuthenticationManager";
        private const string _USER_ENDPOINT_NAME = "NetTcpBinding_IUserManager";

        public static LoginRequest Authenticate(string username, string password)
        {
            LoginRequest request = new LoginRequest();
            var client = new AuthenticationManagerClient(_AUTHENTICATION_ENDPOINT_NAME);
            try
            {
                request = client.Login(username, password);
            }
            catch (EndpointNotFoundException)
            {
                request.StatusCode = StatusCode.SERVER_UNAVAIBLE;
            }
            catch (TimeoutException)
            {
                request.StatusCode = StatusCode.SERVER_UNAVAIBLE;
            }
            finally
            {
                NetworkUtil.SafeClose(client);
            }
            return request;
        }

        public static SignInRequest SignIn(UserDM user, PlayerDM player)
        {
            SignInRequest request = new SignInRequest();
            var client = new AuthenticationManagerClient(_AUTHENTICATION_ENDPOINT_NAME);
            try
            {
                AuthenticationService.Player svPlayer = PlayerDM.AssembleAuthSvPlayer(player);
                AuthenticationService.User svUser = UserDM.AssembleAuthSvUser(user);
                svPlayer.User = svUser;
                return client.SignIn(svPlayer);
            }
            catch (CommunicationException)
            {
                request.IsSuccess = false;
                request.StatusCode = StatusCode.SERVER_UNAVAIBLE;
            }
            finally
            {
                NetworkUtil.SafeClose(client);
            }
            return request;
        }

        public static PlayerDM GetPlayer(Guid userID)
        {
            var client = new UserManagerClient(_USER_ENDPOINT_NAME);
            try
            {
                UserService.Player svPlayer = client.GetPlayerByUserID(userID);
                return PlayerDM.AssemblePlayer(svPlayer);
            }
            finally
            {
                NetworkUtil.SafeClose(client);
            }
        }

        public static void BeginPasswordReset(string username, string email)
        {
            var client = new AuthenticationManagerClient(_AUTHENTICATION_ENDPOINT_NAME);
            try
            {
                client.BeginPasswordReset(username, email);
            }
            finally
            {
                NetworkUtil.SafeClose(client);
            }
        }

        public static ResetResult CompletePasswordReset(string username, string code, string newPassword)
        {
            var client = new AuthenticationManagerClient(_AUTHENTICATION_ENDPOINT_NAME);
            try
            {
                return client.CompletePasswordReset(username, code, newPassword);
            }
            finally
            {
                NetworkUtil.SafeClose(client);
            }
        }

        public static UpdateResult UpdateProfile(PlayerDM player)
        {
            var client = new UserManagerClient(_USER_ENDPOINT_NAME);
            try
            {
                UserService.Player svPlayer = PlayerDM.AssembleUserSvPlayer(player);
                return client.UpdateProfile(svPlayer);
            }
            finally
            {
                NetworkUtil.SafeClose(client);
            }
        }
    }
}