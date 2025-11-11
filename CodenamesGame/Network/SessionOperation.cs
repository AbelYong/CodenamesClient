using CodenamesGame.Domain.POCO;
using CodenamesGame.SessionService;
using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace CodenamesGame.Network
{
    public class SessionOperation : ISessionManagerCallback
    {
        private const string _ENDPOINT_NAME = "NetTcpBinding_ISessionManager";
        private SessionManagerClient _client;
        private List<PlayerDM> _onlineFriends { get; set; }

        public SessionOperation()
        {
            InitializeCallbackChannel();
        }

        private void InitializeCallbackChannel()
        {
            InstanceContext context = new InstanceContext(this);
            _client = new SessionManagerClient(context, _ENDPOINT_NAME);
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
        }

        public void NotifyFriendOnline(Player player)
        {
            PlayerDM auxFriend = PlayerDM.AssemblePlayer(player);
            if (auxFriend != null)
            {
                _onlineFriends.Add(auxFriend);
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
        }
    }
}
