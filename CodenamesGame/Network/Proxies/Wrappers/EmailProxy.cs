using CodenamesGame.EmailService;
using CodenamesGame.Network.Proxies.Interfaces;
using CodenamesGame.Util;
using System;
using System.ServiceModel;

namespace CodenamesGame.Network.Proxies.Wrappers
{
    public class EmailProxy : IEmailProxy
    {
        private readonly Func<IEmailManager> _clientFactory;
        private const string _ENDPOINT_NAME = "NetTcpBinding_IEmailManager";

        public EmailProxy() : this(() => new EmailManagerClient(_ENDPOINT_NAME)) { }

        public EmailProxy(Func<IEmailManager> clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public CommunicationRequest SendVerificationEmail(string email, EmailType emailType)
        {
            CommunicationRequest request = new CommunicationRequest();
            var client = _clientFactory();
            try
            {
                return client.SendVerificationCode(email, emailType);
            }
            catch (TimeoutException)
            {
                request.IsSuccess = false;
                request.StatusCode = StatusCode.SERVER_TIMEOUT;
            }
            catch (EndpointNotFoundException)
            {
                request.IsSuccess = false;
                request.StatusCode = StatusCode.SERVER_UNREACHABLE;
            }
            catch (CommunicationException)
            {
                request.IsSuccess = false;
                request.StatusCode = StatusCode.SERVER_UNAVAIBLE;
            }
            catch (Exception ex)
            {
                CodenamesGameLogger.Log.Error("Unexpected exception while requesting verification code:", ex);
                request.IsSuccess = false;
                request.StatusCode = StatusCode.CLIENT_ERROR;
            }
            finally
            {
                CloseProxy(client);
            }
            return request;
        }

        public ConfirmEmailRequest SendVerificationCode(string email, string code, EmailType emailType)
        {
            ConfirmEmailRequest request = new ConfirmEmailRequest();
            var client = _clientFactory();
            try
            {
                return client.ValidateVerificationCode(email, code, emailType);
            }
            catch (TimeoutException)
            {
                request.IsSuccess = false;
                request.StatusCode = StatusCode.SERVER_TIMEOUT;
            }
            catch (EndpointNotFoundException)
            {
                request.IsSuccess = false;
                request.StatusCode = StatusCode.SERVER_UNREACHABLE;
            }
            catch (CommunicationException)
            {
                request.IsSuccess = false;
                request.StatusCode = StatusCode.SERVER_UNAVAIBLE;
            }
            catch (Exception ex)
            {
                CodenamesGameLogger.Log.Error("Unexpected exception while validating verification code: ", ex);
                request.IsSuccess = false;
                request.StatusCode = StatusCode.CLIENT_ERROR;
            }
            finally
            {
                CloseProxy(client);
            }
            return request;
        }

        private static void CloseProxy(IEmailManager client)
        {
            if (client is ICommunicationObject commObject)
            {
                NetworkUtil.SafeClose(commObject);
            }
        }
    }
}
