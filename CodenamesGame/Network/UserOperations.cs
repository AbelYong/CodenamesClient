using CodenamesGame.Domain.POCO;
using System;
using System.ServiceModel;
using CodenamesGame.AuthenticationService;
using CodenamesGame.UserService;

namespace CodenamesGame.Network
{
    public static class UserOperations
    {
        public static Guid? Authenticate(string username, string password)
        {
            var client = new AuthenticationManagerClient("NetTcpBinding_IAuthenticationManager");
            try
            {
                return client.Login(username, password);
            }
            finally
            {
                SafeClose(client);
            }
        }

        public static Guid? SignIn(UserPOCO user, PlayerPOCO player)
        {
            var client = new AuthenticationService.AuthenticationManagerClient("NetTcpBinding_IAuthenticationManager");
            try
            {
                AuthenticationService.User svUser = UserPOCO.AssembleAuthSvUser(user);
                AuthenticationService.Player svPlayer = PlayerPOCO.AssembleAuthSvPlayer(player);
                return client.SignIn(svUser, svPlayer);
            }
            finally
            {
                SafeClose(client);
            }
        }

        public static PlayerPOCO GetPlayer(Guid userID)
        {
            var client = new UserService.UserManagerClient("NetTcpBinding_IUserManager");
            try
            {
                UserService.Player svPlayer = client.GetPlayerByUserID(userID);
                return PlayerPOCO.AssemblePlayer(svPlayer);
            }
            finally
            {
                SafeClose(client);
            }
        }

        public static void BeginPasswordReset(string username, string email)
        {
            var client = new AuthenticationManagerClient("NetTcpBinding_IAuthenticationManager");
            try
            {
                client.BeginPasswordReset(username, email);
            }
            finally
            {
                SafeClose(client);
            }
        }

        public static ResetResult CompletePasswordReset(string username, string code, string newPassword)
        {
            var client = new AuthenticationManagerClient("NetTcpBinding_IAuthenticationManager");
            try
            {
                return client.CompletePasswordReset(username, code, newPassword);
            }
            finally
            {
                SafeClose(client);
            }
        }

        public static UpdateResult UpdateProfile(PlayerPOCO player)
        {
            var client = new UserManagerClient("NetTcpBinding_IUserManager");
            try
            {
                UserService.Player svPlayer = PlayerPOCO.AssembleUserSvPlayer(player);
                return client.UpdateProfile(svPlayer);
            }
            finally
            {
                SafeClose(client);
            }
        }

        private static void SafeClose(ICommunicationObject client)
        {
            try
            {
                if (client.State == CommunicationState.Faulted)
                {
                    client.Abort();
                }
                else
                {
                    client.Close();
                }
            }
            catch
            {
                client.Abort();
            }
        }
    }
}