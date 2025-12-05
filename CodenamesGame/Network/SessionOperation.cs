using CodenamesGame.Domain.POCO;
using CodenamesGame.Network.Proxies.Interfaces;
using CodenamesGame.Network.Proxies.Wrappers;
using CodenamesGame.SessionService;

namespace CodenamesGame.Network
{
    public class SessionOperation
    {
        private readonly ISessionProxy _proxy;

        public SessionOperation() : this (SessionProxy.Instance) { }

        public SessionOperation(ISessionProxy proxy)
        {
            _proxy = proxy;
        }

        public CommunicationRequest Initialize(PlayerDM player)
        {
            return _proxy.Initialize(player);
        }

        public void Disconnect()
        {
            _proxy.Disconnect();
        }
    }
}