using CodenamesGame.EmailService;

namespace CodenamesGame.Network.Proxies.Interfaces
{
    public interface IEmailProxy
    {
        CommunicationRequest SendVerificationEmail(string email);
        ConfirmEmailRequest SendVerificationCode(string email, string code);
    }
}
