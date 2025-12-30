using CodenamesGame.Domain.POCO;
using CodenamesGame.FriendService;
using CodenamesGame.Network.Proxies.CallbackHandlers;
using CodenamesGame.Network.Proxies.Interfaces;
using CodenamesGame.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;

namespace CodenamesGame.Network.Proxies.Wrappers
{
    public class SocialProxy : ISocialProxy
    {
        private const string _ENDPOINT_NAME = "NetTcpBinding_IFriendManager";
        private static readonly Lazy<SocialProxy> _instance = new Lazy<SocialProxy>(() => new SocialProxy());
        private FriendManagerClient _client;
        private Guid _currentPlayerId;

        public static SocialProxy Instance
        {
            get => _instance.Value;
        }

        private SocialProxy()
        {
        }

        public void Initialize(Guid mePlayerId)
        {
            if (_client != null && _client.State == CommunicationState.Opened)
            {
                return;
            }

            if (mePlayerId == Guid.Empty)
            {
                throw new ArgumentException("PlayerId cannot be empty for duplex connection.", nameof(mePlayerId));
            }

            _currentPlayerId = mePlayerId;
            FriendCallbackHandler callbackHandler = new FriendCallbackHandler();
            InstanceContext context = new InstanceContext(callbackHandler);
            _client = new FriendManagerClient(context, _ENDPOINT_NAME);

            try
            {
                _client.Open();
                _client.Connect(_currentPlayerId);
            }
            catch (TimeoutException ex)
            {
                CodenamesGameLogger.Log.Error("Timeout connecting to FriendService", ex);
                CloseProxy();
            }
            catch (EndpointNotFoundException ex)
            {
                CodenamesGameLogger.Log.Error("FriendService endpoint not found", ex);
                CloseProxy();
            }
            catch (CommunicationException ex)
            {
                CodenamesGameLogger.Log.Error("Communication error connecting to FriendService", ex);
                CloseProxy();
            }
            catch (Exception ex)
            {
                CodenamesGameLogger.Log.Error("Unexpected error connecting to FriendService", ex);
                CloseProxy();
            }
        }

        public void Disconnect()
        {
            if (_client != null && _client.State == CommunicationState.Opened)
            {
                try
                {
                    _client.Disconnect(_currentPlayerId);
                }
                catch (TimeoutException ex)
                {
                    CodenamesGameLogger.Log.Warn("Timeout disconnecting from FriendService players", ex);
                }
                catch (CommunicationException ex)
                {
                    CodenamesGameLogger.Log.Warn("Communication error disconnecting from FriendService players", ex);
                }
                catch (Exception ex)
                {
                    CodenamesGameLogger.Log.Error("Error disconnecting from FriendService", ex);
                }
                finally
                {
                    CloseProxy();
                }
            }
        }

        public List<PlayerDM> SearchPlayers(string query)
        {
            int limit = 20;
            TryReconnect();

            if (_client != null && _client.State == CommunicationState.Opened)
            {
                try
                {
                    var list = _client.SearchPlayers(query ?? "", _currentPlayerId, limit);
                    return list?.Select(PlayerDM.AssemblePlayer).ToList() ?? new List<PlayerDM>();
                }
                catch (CommunicationException ex)
                {
                    CodenamesGameLogger.Log.Warn("Communication error searching players", ex);
                    CloseProxy();
                }
                catch (TimeoutException ex)
                {
                    CodenamesGameLogger.Log.Warn("Timeout searching players", ex);
                    CloseProxy();
                }
                catch (Exception ex)
                {
                    CodenamesGameLogger.Log.Error("Unexpected error searching players", ex);
                    CloseProxy();
                }
            }

            return new List<PlayerDM>();
        }

        public List<PlayerDM> GetFriends()
        {
            TryReconnect();

            if (_client != null && _client.State == CommunicationState.Opened)
            {
                try
                {
                    var list = _client.GetFriends(_currentPlayerId);
                    return list?.Select(PlayerDM.AssemblePlayer).ToList() ?? new List<PlayerDM>();
                }
                catch (CommunicationException ex)
                {
                    CodenamesGameLogger.Log.Warn("Communication error getting friends", ex);
                    CloseProxy();
                }
                catch (TimeoutException ex)
                {
                    CodenamesGameLogger.Log.Warn("Timeout getting friends", ex);
                    CloseProxy();
                }
                catch (Exception ex)
                {
                    CodenamesGameLogger.Log.Error("Unexpected error getting friends", ex);
                    CloseProxy();
                }
            }

            return new List<PlayerDM>();
        }

        public List<PlayerDM> GetIncomingRequests()
        {
            TryReconnect();

            if (_client != null && _client.State == CommunicationState.Opened)
            {
                try
                {
                    var list = _client.GetIncomingRequests(_currentPlayerId);
                    return list?.Select(PlayerDM.AssemblePlayer).ToList() ?? new List<PlayerDM>();
                }
                catch (CommunicationException ex)
                {
                    CodenamesGameLogger.Log.Warn("Communication error getting incoming requests", ex);
                    CloseProxy();
                }
                catch (TimeoutException ex)
                {
                    CodenamesGameLogger.Log.Warn("Timeout getting incoming requests", ex);
                    CloseProxy();
                }
                catch (Exception ex)
                {
                    CodenamesGameLogger.Log.Error("Unexpected error getting incoming requests", ex);
                    CloseProxy();
                }
            }

            return new List<PlayerDM>();
        }

        public List<PlayerDM> GetSentRequests()
        {
            TryReconnect();

            if (_client != null && _client.State == CommunicationState.Opened)
            {
                try
                {
                    var list = _client.GetSentRequests(_currentPlayerId);
                    return list?.Select(PlayerDM.AssemblePlayer).ToList() ?? new List<PlayerDM>();
                }
                catch (CommunicationException ex)
                {
                    CodenamesGameLogger.Log.Warn("Communication error getting sent requests", ex);
                    CloseProxy();
                }
                catch (TimeoutException ex)
                {
                    CodenamesGameLogger.Log.Warn("Timeout getting sent requests", ex);
                    CloseProxy();
                }
                catch (Exception ex)
                {
                    CodenamesGameLogger.Log.Error("Unexpected error getting sent requests", ex);
                    CloseProxy();
                }
            }

            return new List<PlayerDM>();
        }

        public FriendshipRequest SendFriendRequest(Guid toPlayerId)
        {
            FriendshipRequest request = new FriendshipRequest();
            TryReconnect();

            if (_client != null && _client.State == CommunicationState.Opened)
            {
                try
                {
                    return _client.SendFriendRequest(_currentPlayerId, toPlayerId);
                }
                catch (TimeoutException)
                {
                    request.IsSuccess = false;
                    request.StatusCode = StatusCode.SERVER_TIMEOUT;
                    CloseProxy();
                }
                catch (EndpointNotFoundException)
                {
                    request.IsSuccess = false;
                    request.StatusCode = StatusCode.SERVER_UNREACHABLE;
                    CloseProxy();
                }
                catch (CommunicationException)
                {
                    request.IsSuccess = false;
                    request.StatusCode = StatusCode.SERVER_UNAVAIBLE;
                    CloseProxy();
                }
                catch (Exception ex)
                {
                    CodenamesGameLogger.Log.Error("Unexpected error sending friend request", ex);
                    request.IsSuccess = false;
                    request.StatusCode = StatusCode.CLIENT_ERROR;
                    CloseProxy();
                }
            }
            else
            {
                request.IsSuccess = false;
                request.StatusCode = StatusCode.SERVER_UNAVAIBLE;
            }

            return request;
        }

        public FriendshipRequest AcceptFriendRequest(Guid requesterPlayerId)
        {
            FriendshipRequest request = new FriendshipRequest();
            TryReconnect();

            if (_client != null && _client.State == CommunicationState.Opened)
            {
                try
                {
                    return _client.AcceptFriendRequest(_currentPlayerId, requesterPlayerId);
                }
                catch (TimeoutException)
                {
                    request.IsSuccess = false;
                    request.StatusCode = StatusCode.SERVER_TIMEOUT;
                    CloseProxy();
                }
                catch (EndpointNotFoundException)
                {
                    request.IsSuccess = false;
                    request.StatusCode = StatusCode.SERVER_UNREACHABLE;
                    CloseProxy();
                }
                catch (CommunicationException)
                {
                    request.IsSuccess = false;
                    request.StatusCode = StatusCode.SERVER_UNAVAIBLE;
                    CloseProxy();
                }
                catch (Exception ex)
                {
                    CodenamesGameLogger.Log.Error("Unexpected error accepting friend request", ex);
                    request.IsSuccess = false;
                    request.StatusCode = StatusCode.CLIENT_ERROR;
                    CloseProxy();
                }
            }
            else
            {
                request.IsSuccess = false;
                request.StatusCode = StatusCode.SERVER_UNAVAIBLE;
            }

            return request;
        }

        public FriendshipRequest RejectFriendRequest(Guid requesterPlayerId)
        {
            FriendshipRequest request = new FriendshipRequest();
            TryReconnect();

            if (_client != null && _client.State == CommunicationState.Opened)
            {
                try
                {
                    return _client.RejectFriendRequest(_currentPlayerId, requesterPlayerId);
                }
                catch (TimeoutException)
                {
                    request.IsSuccess = false;
                    request.StatusCode = StatusCode.SERVER_TIMEOUT;
                    CloseProxy();
                }
                catch (EndpointNotFoundException)
                {
                    request.IsSuccess = false;
                    request.StatusCode = StatusCode.SERVER_UNREACHABLE;
                    CloseProxy();
                }
                catch (CommunicationException)
                {
                    request.IsSuccess = false;
                    request.StatusCode = StatusCode.SERVER_UNAVAIBLE;
                    CloseProxy();
                }
                catch (Exception ex)
                {
                    CodenamesGameLogger.Log.Error("Unexpected error rejecting friend request", ex);
                    request.IsSuccess = false;
                    request.StatusCode = StatusCode.CLIENT_ERROR;
                    CloseProxy();
                }
            }
            else
            {
                request.IsSuccess = false;
                request.StatusCode = StatusCode.SERVER_UNAVAIBLE;
            }

            return request;
        }

        public FriendshipRequest RemoveFriend(Guid friendPlayerId)
        {
            FriendshipRequest request = new FriendshipRequest();
            TryReconnect();

            if (_client != null && _client.State == CommunicationState.Opened)
            {
                try
                {
                    return _client.RemoveFriend(_currentPlayerId, friendPlayerId);
                }
                catch (TimeoutException)
                {
                    request.IsSuccess = false;
                    request.StatusCode = StatusCode.SERVER_TIMEOUT;
                    CloseProxy();
                }
                catch (EndpointNotFoundException)
                {
                    request.IsSuccess = false;
                    request.StatusCode = StatusCode.SERVER_UNREACHABLE;
                    CloseProxy();
                }
                catch (CommunicationException)
                {
                    request.IsSuccess = false;
                    request.StatusCode = StatusCode.SERVER_UNAVAIBLE;
                    CloseProxy();
                }
                catch (Exception ex)
                {
                    CodenamesGameLogger.Log.Error("Unexpected error removing friend", ex);
                    request.IsSuccess = false;
                    request.StatusCode = StatusCode.CLIENT_ERROR;
                    CloseProxy();
                }
            }
            else
            {
                request.IsSuccess = false;
                request.StatusCode = StatusCode.SERVER_UNAVAIBLE;
            }

            return request;
        }

        private void TryReconnect()
        {
            if (_client == null || _client.State != CommunicationState.Opened)
            {
                if (_currentPlayerId != Guid.Empty)
                {
                    Initialize(_currentPlayerId);
                }
            }
        }

        private void CloseProxy()
        {
            NetworkUtil.SafeClose(_client);
            _client = null;
        }
    }
}