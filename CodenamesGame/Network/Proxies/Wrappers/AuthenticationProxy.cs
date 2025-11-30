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
            catch (Exception ex) when (ex is EndpointNotFoundException || ex is CommunicationException)
            {
                request.StatusCode = StatusCode.SERVER_UNAVAIBLE;
            }
            catch (TimeoutException)
            {
                request.StatusCode = StatusCode.SERVER_TIMEOUT;
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
            catch (Exception ex) when (ex is EndpointNotFoundException || ex is CommunicationException)
            {
                NetworkUtil.SafeClose(client);
            }
            catch (TimeoutException)
            {
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
            catch (Exception ex) when (ex is EndpointNotFoundException || ex is CommunicationException)
            {
                NetworkUtil.SafeClose(client);
            }
            catch (TimeoutException)
            {
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
