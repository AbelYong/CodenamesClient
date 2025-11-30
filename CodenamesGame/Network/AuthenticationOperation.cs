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

        public LoginRequest Authenticate(string username, string password)
        {
            return _proxy.Authenticate(username, password);
        }

        public void BeginPasswordReset(string username, string email)
        {
            _proxy.BeginPasswordReset(username, email);
        }

        public ResetResult CompletePasswordReset(string username, string code, string newPassword)
        {
            return _proxy.CompletePasswordReset(username, code, newPassword);
        }
    }
}
