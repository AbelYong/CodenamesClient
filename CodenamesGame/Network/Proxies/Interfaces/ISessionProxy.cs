using CodenamesGame.Domain.POCO;
using CodenamesGame.SessionService;

namespace CodenamesGame.Network.Proxies.Interfaces
{
    public interface ISessionProxy
    {
        CommunicationRequest Initialize(PlayerDM player);
        void Disconnect();
    }
}
