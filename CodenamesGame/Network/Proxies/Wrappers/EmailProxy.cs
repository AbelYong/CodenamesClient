using CodenamesGame.EmailService;
using CodenamesGame.Network.Proxies.Interfaces;
using CodenamesGame.Util;
using System;
using System.ServiceModel;

namespace CodenamesGame.Network.Proxies.Wrappers
{
    public class EmailProxy : IEmailProxy
    {
        private const string _ENDPOINT_NAME = "NetTcpBinding_IEmailManager";
        public CommunicationRequest SendVerificationEmail(string email)
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
                NetworkUtil.SafeClose(client);
            }
            return request;
        }

        public ConfirmEmailRequest SendVerificationCode(string email, string code)
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
                NetworkUtil.SafeClose(client);
            }
            return request;
        }
    }
}
