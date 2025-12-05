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

        public CommunicationRequest SendVerificationEmail(string email)
        {
            return _proxy.SendVerificationEmail(email);
        }

        public ConfirmEmailRequest SendVerificationCode(string email, string code)
        {
            return _proxy.SendVerificationCode(email, code);
        }
    }
}
