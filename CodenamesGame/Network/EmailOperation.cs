using CodenamesGame.EmailService;
using CodenamesGame.Network.Proxies.Interfaces;
using CodenamesGame.Network.Proxies.Wrappers;

namespace CodenamesGame.Network
{
    public class EmailOperation
    {
        private readonly IEmailProxy _proxy;

        public EmailOperation() : this (new EmailProxy())
        {

        }

        public EmailOperation(IEmailProxy proxy)
        {
            _proxy = proxy;
        }

        public CommunicationRequest SendVerificationEmail(string email, EmailType emailType)
        {
            return _proxy.SendVerificationEmail(email, emailType);
        }

        public ConfirmEmailRequest SendVerificationCode(string email, string code, EmailType emailType)
        {
            return _proxy.SendVerificationCode(email, code, emailType);
        }
    }
}
