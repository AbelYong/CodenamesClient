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

        public LoginRequest Authenticate(string username, string password)
        {
            LoginRequest request = new LoginRequest();
            var client = new AuthenticationManagerClient(_AUTHENTICATION_ENDPOINT_NAME);
            try
            {
                request = client.Login(username, password);
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

        public void BeginPasswordReset(string username, string email)
        {
            var client = new AuthenticationManagerClient(_AUTHENTICATION_ENDPOINT_NAME);
            try
            {
                client.BeginPasswordReset(username, email);
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
                CodenamesGameLogger.Log.Error("Unexpected exception on password reset: ", ex);
                NetworkUtil.SafeClose(client);
            }
            finally
            {
                NetworkUtil.SafeClose(client);
            }
        }

        public ResetResult CompletePasswordReset(string username, string code, string newPassword)
        {
            ResetResult result = new ResetResult();
            result.Success = false;
            var client = new AuthenticationManagerClient(_AUTHENTICATION_ENDPOINT_NAME);
            try
            {
                return client.CompletePasswordReset(username, code, newPassword);
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
            return result;
        }

    }
}
