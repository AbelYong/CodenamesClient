using CodenamesGame.AuthenticationService;

namespace CodenamesGame.Network.Proxies.Interfaces
{
    public interface IAuthenticationProxy
    {
        LoginRequest Authenticate(string username, string password);
        void BeginPasswordReset(string username, string email);
        ResetResult CompletePasswordReset(string username, string code, string newPassword);
    }
}
