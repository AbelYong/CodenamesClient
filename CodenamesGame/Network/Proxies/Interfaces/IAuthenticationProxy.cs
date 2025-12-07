using CodenamesGame.AuthenticationService;

namespace CodenamesGame.Network.Proxies.Interfaces
{
    public interface IAuthenticationProxy
    {
        AuthenticationRequest Authenticate(string username, string password);
        CommunicationRequest CompletePasswordReset(string email, string code, string newPassword);
        CommunicationRequest UpdatePassword(string username, string currentPassword, string newPassword);
    }
}
