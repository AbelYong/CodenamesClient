using CodenamesGame.AuthenticationService;
using CodenamesGame.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace CodenamesGame.Network
{
    public static class AuthenticationOperation
    {
        private const string _AUTHENTICATION_ENDPOINT_NAME = "NetTcpBinding_IAuthenticationManager";

        public static LoginRequest Authenticate(string username, string password)
        {
            LoginRequest request = new LoginRequest();
            var client = new AuthenticationManagerClient(_AUTHENTICATION_ENDPOINT_NAME);
            try
            {
                request = client.Login(username, password);
            }
            catch (EndpointNotFoundException)
            {
                request.StatusCode = StatusCode.SERVER_UNAVAIBLE;
            }
            catch (TimeoutException)
            {
                request.StatusCode = StatusCode.SERVER_UNAVAIBLE;
            }
            finally
            {
                NetworkUtil.SafeClose(client);
            }
            return request;
        }

        public static void BeginPasswordReset(string username, string email)
        {
            var client = new AuthenticationManagerClient(_AUTHENTICATION_ENDPOINT_NAME);
            try
            {
                client.BeginPasswordReset(username, email);
            }
            finally
            {
                NetworkUtil.SafeClose(client);
            }
        }

        public static ResetResult CompletePasswordReset(string username, string code, string newPassword)
        {
            var client = new AuthenticationManagerClient(_AUTHENTICATION_ENDPOINT_NAME);
            try
            {
                return client.CompletePasswordReset(username, code, newPassword);
            }
            finally
            {
                NetworkUtil.SafeClose(client);
            }
        }
    }
}
