using CodenamesGame.Domain.POCO;
using CodenamesGame.Network.EventArguments;
using CodenamesGame.SessionService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;

namespace CodenamesGame.Network
{
    public class SessionOperation : ISessionManagerCallback
    {
        private const string _ENDPOINT_NAME = "NetTcpBinding_ISessionManager";
        private SessionManagerClient _client;
        private List<PlayerDM> _onlineFriends { get; set; }

        public static event EventHandler<PlayerEventArgs> OnFriendOnline;
        public static event EventHandler<Guid> OnFriendOffline;
        public static event EventHandler<List<PlayerDM>> OnOnlineFriendsReceived;

        public SessionOperation()
        {
            InitializeCallbackChannel();
            _onlineFriends = new List<PlayerDM>();
        }

        private void InitializeCallbackChannel()
        {
            InstanceContext context = new InstanceContext(this);
            _client = new SessionManagerClient(context, _ENDPOINT_NAME);
        }

        /// <summary>
        /// Gets the current list of cached online friends.
        /// </summary>
        public List<PlayerDM> GetOnlineFriendsList()
        {
            return _onlineFriends;
        }

        public CommunicationRequest Connect(PlayerDM player)
        {
            if (_client == null)
            {
                InitializeCallbackChannel();
            }

            CommunicationRequest request = new CommunicationRequest();
            Player svPlayer = PlayerDM.AssembleSessionSvPlayer(player);
            try
            {
                request = _client.Connect(svPlayer);
            }
            catch (EndpointNotFoundException)
            {
                request.StatusCode = StatusCode.SERVER_UNAVAIBLE;
                request.IsSuccess = false;
                Util.NetworkUtil.SafeClose(_client);
                _client = null;
            }
            catch (TimeoutException)
            {
                request.StatusCode = StatusCode.SERVER_TIMEOUT;
                request.IsSuccess = false;
                Util.NetworkUtil.SafeClose(_client);
                _client = null;
            }
            return request;
        }

        public void Disconnect(PlayerDM player)
        {
            if (_client != null)
            {
                Player svPlayer = PlayerDM.AssembleSessionSvPlayer(player);
                try
                {
                    _client.DisconnectAsync(svPlayer);
                }
                catch (EndpointNotFoundException)
                {
                    Util.NetworkUtil.SafeClose(_client);
                }
                catch (CommunicationObjectFaultedException)
                {
                    Util.NetworkUtil.SafeClose(_client);
                }
            }
        }

        public void NotifyFriendOffline(Guid playerId)
        {
            Guid? auxFriendId = playerId;
            _onlineFriends.RemoveAll((friend) => friend.PlayerID == auxFriendId);

            OnFriendOffline?.Invoke(this, playerId);
        }

        public void NotifyFriendOnline(Player player)
        {
            PlayerDM auxFriend = PlayerDM.AssemblePlayer(player);
            if (auxFriend != null)
            {
                _onlineFriends.Add(auxFriend);

                OnFriendOnline?.Invoke(this, new PlayerEventArgs { Player = auxFriend });
            }
        }

        public void ReceiveOnlineFriends(Player[] friends)
        {
            List<PlayerDM> auxFriends = new List<PlayerDM>();
            foreach (Player friend in friends)
            {
                PlayerDM auxFriend = PlayerDM.AssemblePlayer(friend);
                if (auxFriend != null)
                {
                    auxFriends.Add(auxFriend);
                }
            }
            _onlineFriends = auxFriends;

            OnOnlineFriendsReceived?.Invoke(this, _onlineFriends);
        }
    }
}