using CodenamesGame.AuthenticationService;
using CodenamesGame.Network.Proxies.Interfaces;
using CodenamesGame.Network.Proxies.Wrappers;

namespace CodenamesGame.Network
{
    public class AuthenticationOperation
    {
        private readonly IAuthenticationProxy _proxy;

        public AuthenticationOperation() : this (new AuthenticationProxy()) { }

        public AuthenticationOperation(IAuthenticationProxy proxy)
        {
            _proxy = proxy;
        }

        public AuthenticationRequest Authenticate(string username, string password)
        {
            return _proxy.Authenticate(username, password);
        }

        public CommunicationRequest CompletePasswordReset(string email, string code, string newPassword)
        {
            return _proxy.CompletePasswordReset(email, code, newPassword);
        }

        public CommunicationRequest UpdatePassword(string username, string currentPassword, string newPassword)
        {
            return _proxy.UpdatePassword(username, currentPassword, newPassword);
        }
    }
}
