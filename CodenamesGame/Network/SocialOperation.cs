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
        /// <param name="mePlayerId">The player ID for the current session.</param>
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

        /// <summary>
        /// Closes the duplex connection. Must be called when logging out.
        /// </summary>
        public void Terminate()
        {
            if (_client != null && _client.State == CommunicationState.Opened)
            {
                try
                {
                    _client.Disconnect(_currentPlayerId);
                }
                catch (Exception)
                {

                }
                finally
                {
                    NetworkUtil.SafeClose(_client);
                    _client = null;
                    _currentPlayerId = Guid.Empty;
                }
            }
        }

        /// <summary>
        /// Gets the WCF client, ensuring that it is open and ready.
        /// </summary>
        /// <returns>The FriendManagerClient instance.
        private FriendManagerClient GetClient()
        {
            if (_client == null || _client.State != CommunicationState.Opened)
            {
                throw new InvalidOperationException("Friend service connection is not available or has been closed.");
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
                OnOperationFailure(ex);
                return new List<PlayerDM>();
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

        /// <summary>
        /// Sends a friend request. The result will be received
        /// through the OnOperationSuccess/OnOperationFailure events.
        /// </summary>
        /// <param name="toPlayerId">The ID of the player to whom it is sent.</param>
        public void SendFriendRequest(Guid toPlayerId)
        {
            try
            {
                GetClient().SendFriendRequest(_currentPlayerId, toPlayerId);
            }
            catch (Exception ex) { OnOperationFailure(ex); }
        }

        public void AcceptFriendRequest(Guid requesterPlayerId)
        {
            try
            {
                GetClient().AcceptFriendRequest(_currentPlayerId, requesterPlayerId);
            }
            catch (Exception ex) { OnOperationFailure(ex); }
        }

        public void RejectFriendRequest(Guid requesterPlayerId)
        {
            try
            {
                GetClient().RejectFriendRequest(_currentPlayerId, requesterPlayerId);
            }
            catch (Exception ex) { OnOperationFailure(ex); }
        }

        public void RemoveFriend(Guid friendPlayerId)
        {
            try
            {
                GetClient().RemoveFriend(_currentPlayerId, friendPlayerId);
            }
            catch (Exception ex) { OnOperationFailure(ex); }
        }

        /// <summary>
        /// Helper method to trigger the failure event in case of
        /// a transport exception (e.g., server down).
        /// </summary>
        private static void OnOperationFailure(Exception ex)
        {
            FriendCallbackHandler.RaiseOperationFailure($"Error de conexión: {ex.Message}");
        }
    }
}