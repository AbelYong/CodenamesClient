using System;
using System.ServiceModel;
using CodenamesGame.AuthenticationService;

namespace CodenamesGame.Network
{
    public static class UserOperations
    {
        public static Guid? Authenticate(string username, string password)
        {
            var client = new AuthenticationManagerClient("NetTcpBinding_IAuthenticationManager");
            try
            {
                return client.Login(username, password);
            }
            finally
            {
                SafeClose(client);
            }
        }

        public static void BeginPasswordReset(string username, string email)
        {
            var client = new AuthenticationManagerClient("NetTcpBinding_IAuthenticationManager");
            try
            {
                client.BeginPasswordReset(username, email);
            }
            finally
            {
                SafeClose(client);
            }
        }

        public static ResetResult CompletePasswordReset(string username, string code, string newPassword)
        {
            var client = new AuthenticationManagerClient("NetTcpBinding_IAuthenticationManager");
            try
            {
                return client.CompletePasswordReset(username, code, newPassword);
            }
            finally
            {
                SafeClose(client);
            }
        }

        private static void SafeClose(ICommunicationObject client)
        {
            try
            {
                if (client.State == CommunicationState.Faulted) client.Abort();
                else client.Close();
            }
            catch
            {
                client.Abort();
            }
        }
    }
}