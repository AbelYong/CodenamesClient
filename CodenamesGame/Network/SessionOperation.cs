using CodenamesGame.Domain.POCO;
using CodenamesGame.SessionService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace CodenamesGame.Network
{
    public class SessionOperation : ISessionManagerCallback
    {
        private const string _ENDPOINT_NAME = "NetTcpBinding_ISessionManager";
        private readonly InstanceContext _context;
        private readonly SessionManagerClient _client;

        public SessionOperation()
        {
            _context = new InstanceContext(this);
            _client = new SessionManagerClient(_context, _ENDPOINT_NAME);
        }

        public bool Connect(PlayerDM player)
        {
            Player svPlayer = PlayerDM.AssembleSessionSvPlayer(player);
            try
            {
                _client.ConnectAsync(svPlayer);
                return true;
            }
            catch (EndpointNotFoundException)
            {
                return false;
            }
        }

        public void Disconnect(PlayerDM player)
        {
            Player svPlayer = PlayerDM.AssembleSessionSvPlayer(player);
            try
            {
                _client.Disconnect(svPlayer);
            }
            catch (EndpointNotFoundException)
            {
                //TODO log
            }
        }

        public void NotifyFriendOffline(Guid playerId)
        {
            //TODO
        }

        public void NotifyFriendOnline(Player player)
        {
            //TODO
        }

        public void ReceiveOnlineFriends(Player[] friends)
        {
            //TODO
        }
    }
}
