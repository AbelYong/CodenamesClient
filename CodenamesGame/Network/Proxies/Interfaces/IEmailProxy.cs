using CodenamesGame.EmailService;

namespace CodenamesGame.Network.Proxies.Interfaces
{
    public interface IEmailProxy
    {
        CommunicationRequest SendVerificationEmail(string email, EmailType emailType);
        ConfirmEmailRequest SendVerificationCode(string email, string code, EmailType emailType);
    }
}
