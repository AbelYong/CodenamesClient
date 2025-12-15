using CodenamesGame.Domain.POCO;
using CodenamesGame.Network.Proxies.Interfaces;
using CodenamesGame.Network.Proxies.Wrappers;
using CodenamesGame.SessionService;
using System;

namespace CodenamesGame.Network
{
    public class SessionOperation
    {
        private readonly ISessionProxy _proxy;
        public event EventHandler ConnectionLost;

        public SessionOperation() : this (SessionProxy.Instance) { }

        public SessionOperation(ISessionProxy proxy)
        {
            _proxy = proxy;
            if (_proxy is SessionProxy concreteProxy)
            {
                concreteProxy.ConnectionLost += (s, e) => ConnectionLost?.Invoke(this, EventArgs.Empty);
            }
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