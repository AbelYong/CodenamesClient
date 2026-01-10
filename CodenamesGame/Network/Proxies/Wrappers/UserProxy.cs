using CodenamesGame.Domain.POCO;
using CodenamesGame.Network.Proxies.Interfaces;
using CodenamesGame.UserService;
using CodenamesGame.Util;
using System;
using System.ServiceModel;

namespace CodenamesGame.Network.Proxies.Wrappers
{
    public class UserProxy : IUserProxy
    {
        private readonly Func<IUserManager> _clientFactory;
        private const string _ENDPOINT_NAME = "NetTcpBinding_IUserManager";

        public UserProxy() : this (() => new UserManagerClient(_ENDPOINT_NAME)) { }

        public UserProxy(Func<IUserManager> clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public SignInRequest SignIn(UserDM user, PlayerDM player)
        {
            SignInRequest request = new SignInRequest();
            var client = _clientFactory();
            try
            {
                player.User = user;
                Player svPlayer = PlayerDM.AssembleUserSvPlayer(player);
                return client.SignIn(svPlayer);
            }
            catch (TimeoutException)
            {
                request = GenerateServerTimeoutRequest<SignInRequest>();
            }
            catch (EndpointNotFoundException)
            {
                request = GenerateServerUnreachableRequest<SignInRequest>();
            }
            catch (CommunicationException)
            {
                request = GenerateServerUnavaibleRequest<SignInRequest>();
            }
            catch (Exception ex)
            {
                CodenamesGameLogger.Log.Error("Unexpected exception on sign in: ", ex);
                request = GenerateClientErrorRequest<SignInRequest>();
            }
            finally
            {
                CloseProxy(client);
            }
            return request;
        }

        public PlayerDM GetPlayer(Guid userID)
        {
            var client = _clientFactory();
            try
            {
                Player svPlayer = client.GetPlayerByUserID(userID);
                return PlayerDM.AssemblePlayer(svPlayer);
            }
            catch (TimeoutException)
            {
                return new PlayerDM { PlayerID = Guid.Empty };
            }
            catch (EndpointNotFoundException)
            {
                return new PlayerDM { PlayerID = Guid.Empty };
            }
            catch (CommunicationException)
            {
                return new PlayerDM { PlayerID = Guid.Empty };
            }
            catch (Exception ex)
            {
                CodenamesGameLogger.Log.Error("Unexpected exception while trying to get a Player by UserID", ex);
                return new PlayerDM { PlayerID = Guid.Empty };
            }
            finally
            {
                CloseProxy(client);
            }
        }

        public CommunicationRequest UpdateProfile(PlayerDM player)
        {
            CommunicationRequest request = new CommunicationRequest();
            var client = _clientFactory();
            try
            {
                Player svPlayer = PlayerDM.AssembleUserSvPlayer(player);
                return client.UpdateProfile(svPlayer);
            }
            catch (TimeoutException)
            {
                request = GenerateServerTimeoutRequest<CommunicationRequest>();
            }
            catch (EndpointNotFoundException)
            {
                request = GenerateServerUnreachableRequest<CommunicationRequest>();
            }
            catch (CommunicationException)
            {
                request = GenerateServerUnavaibleRequest<CommunicationRequest>();
            }
            catch (Exception ex)
            {
                CodenamesGameLogger.Log.Error("Unexpected exception on profile update: ", ex);
                request = GenerateClientErrorRequest<CommunicationRequest>();
            }
            finally
            {
                CloseProxy(client);
            }
            return request;
        }

        private static void CloseProxy(IUserManager client)
        {
            if (client is ICommunicationObject commObject)
            {
                NetworkUtil.SafeClose(commObject);
            }
        }

        private static T GenerateServerTimeoutRequest<T>() where T : Request, new()
        {
            var request = new T();
            request.IsSuccess = false;
            request.StatusCode = StatusCode.SERVER_TIMEOUT;
            return request;
        }

        private static T GenerateServerUnreachableRequest<T>() where T : Request, new()
        {
            var request = new T();
            request.IsSuccess = false;
            request.StatusCode = StatusCode.SERVER_UNREACHABLE;
            return request;
        }

        private static T GenerateServerUnavaibleRequest<T>() where T : Request, new()
        {
            var request = new T();
            request.IsSuccess = false;
            request.StatusCode = StatusCode.SERVER_UNAVAIBLE;
            return request;
        }

        private static T GenerateClientErrorRequest<T>() where T : Request, new()
        {
            var request = new T();
            request.IsSuccess = false;
            request.StatusCode = StatusCode.CLIENT_ERROR;
            return request;
        }
    }
}
