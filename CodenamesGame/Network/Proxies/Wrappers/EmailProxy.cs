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
                request = GenerateServerTimeoutRequest<CommunicationRequest>();
            }
            catch (EndpointNotFoundException)
            {
                request = GenerateServerUnreachableRequest<CommunicationRequest>();
            }
            catch (CommunicationException)
            {
                request = GenerateServerUnavaibleRequest<CommunicationRequest>();
            }
            catch (Exception ex)
            {
                CodenamesGameLogger.Log.Error("Unexpected exception while requesting verification code:", ex);
                request = GenerateClientErrorRequest<CommunicationRequest>();
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
                request = GenerateServerTimeoutRequest<ConfirmEmailRequest>();
            }
            catch (EndpointNotFoundException)
            {
                request = GenerateServerUnreachableRequest<ConfirmEmailRequest>();
            }
            catch (CommunicationException)
            {
                request = GenerateServerUnavaibleRequest<ConfirmEmailRequest>();
            }
            catch (Exception ex)
            {
                CodenamesGameLogger.Log.Error("Unexpected exception while validating verification code: ", ex);
                request = GenerateClientErrorRequest<ConfirmEmailRequest>();
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

        private static T GenerateServerTimeoutRequest<T>() where T : Request, new()
        {
            var request = new T();
            request.IsSuccess = false;
            request.StatusCode = StatusCode.SERVER_TIMEOUT;
            return request;
        }

        private static T GenerateServerUnreachableRequest<T>() where T : Request, new()
        {
            var request = new T();
            request.IsSuccess = false;
            request.StatusCode = StatusCode.SERVER_UNREACHABLE;
            return request;
        }

        private static T GenerateServerUnavaibleRequest<T>() where T : Request, new()
        {
            var request = new T();
            request.IsSuccess = false;
            request.StatusCode = StatusCode.SERVER_UNAVAIBLE;
            return request;
        }

        private static T GenerateClientErrorRequest<T>() where T : Request, new()
        {
            var request = new T();
            request.IsSuccess = false;
            request.StatusCode = StatusCode.CLIENT_ERROR;
            return request;
        }
    }
}
