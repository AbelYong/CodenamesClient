using CodenamesGame.Domain.POCO;
using System;
using System.ServiceModel;
using CodenamesGame.UserService;
using CodenamesGame.Util;

namespace CodenamesGame.Network
{
    public static class UserOperation
    {
        private const string _USER_ENDPOINT_NAME = "NetTcpBinding_IUserManager";

        public static SignInRequest SignIn(UserDM user, PlayerDM player)
        {
            SignInRequest request = new SignInRequest();
            var client = new UserManagerClient(_USER_ENDPOINT_NAME);
            try
            {
                player.User = user;
                Player svPlayer = PlayerDM.AssembleUserSvPlayer(player);
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
                Player svPlayer = client.GetPlayerByUserID(userID);
                return PlayerDM.AssemblePlayer(svPlayer);
            }
            finally
            {
                NetworkUtil.SafeClose(client);
            }
        }

        public static CommunicationRequest UpdateProfile(PlayerDM player)
        {
            var client = new UserManagerClient(_USER_ENDPOINT_NAME);
            try
            {
                Player svPlayer = PlayerDM.AssembleUserSvPlayer(player);
                return client.UpdateProfile(svPlayer);
            }
            finally
            {
                NetworkUtil.SafeClose(client);
            }
        }
    }
}