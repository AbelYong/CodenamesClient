using CodenamesGame.Domain.POCO;
using CodenamesGame.FriendService;
using CodenamesGame.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;

namespace CodenamesGame.Network
{
    /// <summary>
    /// Manages communication with the IFriendManager duplex service.
    /// Implemented as a Singleton to maintain a single active connection
    /// that can receive callbacks.
    /// </summary>
    public sealed class SocialOperation
    {
        private const string _ENDPOINT_NAME = "NetTcpBinding_IFriendManager";

        private static readonly Lazy<SocialOperation> _instance =
            new Lazy<SocialOperation>(() => new SocialOperation());

        /// <summary>
        /// Gets the single instance of the social operations manager.
        /// </summary>
        public static SocialOperation Instance => _instance.Value;

        private FriendManagerClient _client;
        private Guid _currentPlayerId;

        private SocialOperation() { }

        /// <summary>
        /// Initializes the duplex client and connects to the server.
        /// Must be called after login.
        /// </summary>
        public void Initialize(Guid mePlayerId)
        {
            FriendCallbackHandler callbackHandler;
            if (_client != null && _client.State == CommunicationState.Opened)
            {
                return;
            }

            if (mePlayerId == Guid.Empty)
            {
                throw new ArgumentException("PlayerId cannot be empty for duplex connection.", nameof(mePlayerId));
            }

            _currentPlayerId = mePlayerId;
            callbackHandler = new FriendCallbackHandler();
            var context = new InstanceContext(callbackHandler);
            _client = new FriendManagerClient(context, _ENDPOINT_NAME);

            try
            {
                _client.Open();
                _client.Connect(_currentPlayerId);
            }
            catch (Exception ex)
            {
                NetworkUtil.SafeClose(_client);
                _client = null;
                throw new CommunicationException($"Failed to connect to FriendService: {ex.Message}", ex);
            }
        }

        public void Terminate()
        {
            if (_client != null && _client.State == CommunicationState.Opened)
            {
                try
                {
                    _client.Disconnect(_currentPlayerId);
                }
                catch (Exception) { }
                finally
                {
                    NetworkUtil.SafeClose(_client);
                    _client = null;
                    _currentPlayerId = Guid.Empty;
                }
            }
        }

        private FriendManagerClient GetClient()
        {
            if (_client == null ||
                _client.State == CommunicationState.Closed ||
                _client.State == CommunicationState.Faulted)
            {
                Initialize(_currentPlayerId);
            }

            return _client;
        }

        public List<PlayerDM> SearchPlayers(string query, int limit = 20)
        {
            try
            {
                var list = GetClient().SearchPlayers(query ?? "", _currentPlayerId, limit);
                return list?.Select(PlayerDM.AssemblePlayer).ToList() ?? new List<PlayerDM>();
            }
            catch (Exception ex)
            {
                try
                {
                    Initialize(_currentPlayerId);
                    var list = GetClient().SearchPlayers(query ?? "", _currentPlayerId, limit);
                    return list?.Select(PlayerDM.AssemblePlayer).ToList() ?? new List<PlayerDM>();
                }
                catch
                {
                    OnOperationFailure(ex);
                    return new List<PlayerDM>();
                }
            }
        }

        public List<PlayerDM> GetFriends()
        {
            try
            {
                var list = GetClient().GetFriends(_currentPlayerId);
                return list?.Select(PlayerDM.AssemblePlayer).ToList() ?? new List<PlayerDM>();
            }
            catch (Exception ex)
            {
                OnOperationFailure(ex);
                return new List<PlayerDM>();
            }
        }

        public List<PlayerDM> GetIncomingRequests()
        {
            try
            {
                var list = GetClient().GetIncomingRequests(_currentPlayerId);
                return list?.Select(PlayerDM.AssemblePlayer).ToList() ?? new List<PlayerDM>();
            }
            catch (Exception ex)
            {
                OnOperationFailure(ex);
                return new List<PlayerDM>();
            }
        }

        public List<PlayerDM> GetSentRequests()
        {
            try
            {
                var list = GetClient().GetSentRequests(_currentPlayerId);

                return list?.Select(PlayerDM.AssemblePlayer).ToList() ?? new List<PlayerDM>();
            }
            catch (Exception ex)
            {
                OnOperationFailure(ex);
                return new List<PlayerDM>();
            }
        }

        public FriendshipRequest SendFriendRequest(Guid toPlayerId)
        {
            try
            {
                return GetClient().SendFriendRequest(_currentPlayerId, toPlayerId);
            }
            catch (Exception ex)
            {
                OnOperationFailure(ex);
                return new FriendshipRequest
                {
                    IsSuccess = false,
                    StatusCode = StatusCode.SERVER_ERROR
                };
            }
        }

        public FriendshipRequest AcceptFriendRequest(Guid requesterPlayerId)
        {
            try
            {
                return GetClient().AcceptFriendRequest(_currentPlayerId, requesterPlayerId);
            }
            catch (Exception ex)
            {
                OnOperationFailure(ex);
                return new FriendshipRequest
                {
                    IsSuccess = false,
                    StatusCode = StatusCode.SERVER_ERROR
                };
            }
        }

        public FriendshipRequest RejectFriendRequest(Guid requesterPlayerId)
        {
            try
            {
                return GetClient().RejectFriendRequest(_currentPlayerId, requesterPlayerId);
            }
            catch (Exception ex)
            {
                OnOperationFailure(ex);
                return new FriendshipRequest
                {
                    IsSuccess = false,
                    StatusCode = StatusCode.SERVER_ERROR
                };
            }
        }

        public FriendshipRequest RemoveFriend(Guid friendPlayerId)
        {
            try
            {
                return GetClient().RemoveFriend(_currentPlayerId, friendPlayerId);
            }
            catch (Exception ex)
            {
                OnOperationFailure(ex);
                return new FriendshipRequest
                {
                    IsSuccess = false,
                    StatusCode = StatusCode.SERVER_ERROR
                };
            }
        }

        private static void OnOperationFailure(Exception ex)
        {
            FriendCallbackHandler.RaiseOperationFailure($"Error de conexión: {ex.Message}");
        }
    }
}