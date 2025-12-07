using CodenamesGame.AuthenticationService;
using CodenamesGame.Network.Proxies.Interfaces;
using CodenamesGame.Util;
using System;
using System.ServiceModel;

namespace CodenamesGame.Network.Proxies.Wrappers
{
    public class AuthenticationProxy : IAuthenticationProxy
    {
        private const string _AUTHENTICATION_ENDPOINT_NAME = "NetTcpBinding_IAuthenticationManager";

        public AuthenticationRequest Authenticate(string username, string password)
        {
            AuthenticationRequest request = new AuthenticationRequest();
            var client = new AuthenticationManagerClient(_AUTHENTICATION_ENDPOINT_NAME);
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
                NetworkUtil.SafeClose(client);
            }
            return request;
        }

        public CommunicationRequest CompletePasswordReset(string email, string code, string newPassword)
        {
            CommunicationRequest request = new CommunicationRequest();
            var client = new AuthenticationManagerClient(_AUTHENTICATION_ENDPOINT_NAME);
            try
            {
                return client.CompletePasswordReset(email, code, newPassword);
            }
            catch (TimeoutException)
            {
                NetworkUtil.SafeClose(client);
            }
            catch (EndpointNotFoundException)
            {
                NetworkUtil.SafeClose(client);
            }
            catch (CommunicationException)
            {
                NetworkUtil.SafeClose(client);
            }
            catch (Exception ex)
            {
                CodenamesGameLogger.Log.Error("Unexpected exception on password reset completion: ", ex);
                NetworkUtil.SafeClose(client);
            }
            finally
            {
                NetworkUtil.SafeClose(client);
            }
            return request;
        }

        public CommunicationRequest UpdatePassword(string username, string currentPassword, string newPassword)
        {
            CommunicationRequest request = new CommunicationRequest();
            var client = new AuthenticationManagerClient(_AUTHENTICATION_ENDPOINT_NAME);
            try
            {
                return client.UpdatePassword(username, currentPassword, newPassword);
            }
            catch (TimeoutException)
            {
                NetworkUtil.SafeClose(client);
            }
            catch (EndpointNotFoundException)
            {
                NetworkUtil.SafeClose(client);
            }
            catch (CommunicationException)
            {
                NetworkUtil.SafeClose(client);
            }
            catch (Exception ex)
            {
                CodenamesGameLogger.Log.Error("Unexpected exception on password reset completion: ", ex);
                NetworkUtil.SafeClose(client);
            }
            finally
            {
                NetworkUtil.SafeClose(client);
            }
            return request;
        }
    }
}
