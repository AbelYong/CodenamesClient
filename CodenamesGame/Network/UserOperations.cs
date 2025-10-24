using CodenamesGame.Domain.POCO;
using System;
using System.ServiceModel;
using CodenamesGame.AuthenticationService;
using CodenamesGame.UserService;
using CodenamesGame.Util;

namespace CodenamesGame.Network
{
    public static class UserOperations
    {
        private const string _authenticationEndpointName = "NetTcpBinding_IAuthenticationManager";
        private const string _userEndpointName = "NetTcpBinding_IUserManager";

        public static Guid? Authenticate(string username, string password)
        {
            var client = new AuthenticationManagerClient(_authenticationEndpointName);
            try
            {
                return client.Login(username, password);
            }
            finally
            {
                NetworkUtil.SafeClose(client);
            }
        }

        public static Guid? SignIn(UserPOCO user, PlayerPOCO player)
        {
            var client = new AuthenticationManagerClient(_authenticationEndpointName);
            try
            {
                AuthenticationService.User svUser = UserPOCO.AssembleAuthSvUser(user);
                AuthenticationService.Player svPlayer = PlayerPOCO.AssembleAuthSvPlayer(player);
                return client.SignIn(svUser, svPlayer);
            }
            finally
            {
                NetworkUtil.SafeClose(client);
            }
        }

        public static PlayerPOCO GetPlayer(Guid userID)
        {
            var client = new UserManagerClient(_userEndpointName);
            try
            {
                UserService.Player svPlayer = client.GetPlayerByUserID(userID);
                return PlayerPOCO.AssemblePlayer(svPlayer);
            }
            finally
            {
                NetworkUtil.SafeClose(client);
            }
        }

        public static void BeginPasswordReset(string username, string email)
        {
            var client = new AuthenticationManagerClient(_authenticationEndpointName);
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
            var client = new AuthenticationManagerClient(_authenticationEndpointName);
            try
            {
                return client.CompletePasswordReset(username, code, newPassword);
            }
            finally
            {
                NetworkUtil.SafeClose(client);
            }
        }

        public static UpdateResult UpdateProfile(PlayerPOCO player)
        {
            var client = new UserManagerClient(_userEndpointName);
            try
            {
                UserService.Player svPlayer = PlayerPOCO.AssembleUserSvPlayer(player);
                return client.UpdateProfile(svPlayer);
            }
            finally
            {
                NetworkUtil.SafeClose(client);
            }
        }
    }
}