using CodenamesGame.AuthenticationService;
using CodenamesGame.Network.Proxies.Interfaces;
using CodenamesGame.Util;
using System;
using System.ServiceModel;

namespace CodenamesGame.Network.Proxies.Wrappers
{
    public class AuthenticationProxy : IAuthenticationProxy
    {
        private readonly Func<IAuthenticationManager> _clientFactory;
        private const string _ENDPOINT_NAME = "NetTcpBinding_IAuthenticationManager";

        public AuthenticationProxy() : this(() => new AuthenticationManagerClient(_ENDPOINT_NAME)) { }

        public AuthenticationProxy(Func<IAuthenticationManager> clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public AuthenticationRequest Authenticate(string username, string password)
        {
            AuthenticationRequest request = new AuthenticationRequest();
            var client = _clientFactory();
            try
            {
                request = client.Authenticate(username, password);
            }
            catch (TimeoutException)
            {
                request.StatusCode = StatusCode.SERVER_TIMEOUT;
            }
            catch (EndpointNotFoundException)
            {
                request.StatusCode = StatusCode.SERVER_UNREACHABLE;
            }
            catch (CommunicationException)
            {
                request.StatusCode = StatusCode.SERVER_UNAVAIBLE;
            }
            catch (Exception ex)
            {
                CodenamesGameLogger.Log.Error("Unexpected exception on authentication attempt: ", ex);
                request.StatusCode = StatusCode.CLIENT_ERROR;
            }
            finally
            {
                CloseProxy(client);
            }
            return request;
        }

        public PasswordResetRequest CompletePasswordReset(string email, string code, string newPassword)
        {
            PasswordResetRequest request = new PasswordResetRequest();
            var client = _clientFactory();
            try
            {
                return client.CompletePasswordReset(email, code, newPassword);
            }
            catch (TimeoutException)
            {
                request.StatusCode = StatusCode.SERVER_TIMEOUT;
            }
            catch (EndpointNotFoundException)
            {
                request.StatusCode = StatusCode.SERVER_UNREACHABLE;
            }
            catch (CommunicationException)
            {
                request.StatusCode = StatusCode.SERVER_UNAVAIBLE;
            }
            catch (Exception ex)
            {
                CodenamesGameLogger.Log.Error("Unexpected exception on password reset completion: ", ex);
                request.StatusCode = StatusCode.CLIENT_ERROR;
            }
            finally
            {
                CloseProxy(client);
            }
            return request;
        }

        public CommunicationRequest UpdatePassword(string username, string currentPassword, string newPassword)
        {
            CommunicationRequest request = new CommunicationRequest();
            var client = _clientFactory();
            try
            {
                return client.UpdatePassword(username, currentPassword, newPassword);
            }
            catch (TimeoutException)
            {
                request.StatusCode = StatusCode.SERVER_TIMEOUT;
            }
            catch (EndpointNotFoundException)
            {
                request.StatusCode = StatusCode.SERVER_UNREACHABLE;
            }
            catch (CommunicationException)
            {
                request.StatusCode = StatusCode.SERVER_UNAVAIBLE;
            }
            catch (Exception ex)
            {
                CodenamesGameLogger.Log.Error("Unexpected exception on password reset completion: ", ex);
                request.StatusCode = StatusCode.CLIENT_ERROR;
            }
            finally
            {
                CloseProxy(client);
            }
            return request;
        }

        private static void CloseProxy(IAuthenticationManager client)
        {
            if (client is ICommunicationObject commObject)
            {
                NetworkUtil.SafeClose(commObject);
            }
        }
    }
}
