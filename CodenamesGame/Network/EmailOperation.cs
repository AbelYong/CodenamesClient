using CodenamesGame.EmailService;
using CodenamesGame.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodenamesGame.Network
{
    public static class EmailOperation
    {
        private const string _ENDPOINT_NAME = "NetTcpBinding_IEmailManager";
        public static RequestResult SendVerificationEmail(string email)
        {
            var client = new EmailManagerClient(_ENDPOINT_NAME);
            try
            {
                return client.SendVerificationCode(email);
            }
            finally
            {
                NetworkUtil.SafeClose(client);
            }
        }

        public static RequestResult SendVerificationCode(string email, string code)
        {
            var client = new EmailManagerClient(_ENDPOINT_NAME);
            try
            {
                return client.ValidateVerificationCode(email, code);
            }
            finally
            {
                NetworkUtil.SafeClose(client);
            }
        }
    }
}
