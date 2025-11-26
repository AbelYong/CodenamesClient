using CodenamesGame.EmailService;
using CodenamesGame.Util;
using System;
using System.ServiceModel;

namespace CodenamesGame.Network
{
    public static class EmailOperation
    {
        private const string _ENDPOINT_NAME = "NetTcpBinding_IEmailManager";
        public static CommunicationRequest SendVerificationEmail(string email)
        {
            CommunicationRequest request = new CommunicationRequest();
            var client = new EmailManagerClient(_ENDPOINT_NAME);
            try
            {
                return client.SendVerificationCode(email);
            }
            catch (TimeoutException)
            {
                request.IsSuccess = false;
                request.StatusCode = StatusCode.SERVER_TIMEOUT;
            }
            catch (CommunicationException)
            {
                request.IsSuccess = false;
                request.StatusCode = StatusCode.SERVER_UNAVAIBLE;
            }
            catch (Exception)
            {
                request.IsSuccess = false;
            }
            finally
            {
                NetworkUtil.SafeClose(client);
            }
            return request;
        }

        public static ConfirmEmailRequest SendVerificationCode(string email, string code)
        {
            ConfirmEmailRequest request = new ConfirmEmailRequest();
            var client = new EmailManagerClient(_ENDPOINT_NAME);
            try
            {
                return client.ValidateVerificationCode(email, code);
            }
            catch (TimeoutException)
            {
                request.IsSuccess = false;
                request.StatusCode = StatusCode.SERVER_TIMEOUT;
            }
            catch (CommunicationException)
            {
                request.IsSuccess = false;
                request.StatusCode = StatusCode.SERVER_UNAVAIBLE;
            }
            catch (Exception)
            {
                request.IsSuccess = false;
            }
            finally
            {
                NetworkUtil.SafeClose(client);
            }
            return request;
        }
    }
}
