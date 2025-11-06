using CodenamesGame.Domain.POCO;
using CodenamesGame.SessionService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
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

        public bool Connect(PlayerDM player)
        {
            if (_client == null)
            {
                InitializeCallbackChannel();
            }

            Player svPlayer = PlayerDM.AssembleSessionSvPlayer(player);
            try
            {
                _client.ConnectAsync(svPlayer);
                return true;
            }
            catch (EndpointNotFoundException)
            {
                Util.NetworkUtil.SafeClose(_client);
                return false;
            }
        }

        public void Disconnect(PlayerDM player)
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
