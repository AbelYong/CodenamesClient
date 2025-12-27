using System.ServiceModel;

namespace CodenamesGame.Util
{
    public static class NetworkUtil
    {
        public static void SafeClose(ICommunicationObject client)
        {
            if (client == null)
            {
                return;
            }

            try
            {
                if (client.State == CommunicationState.Faulted)
                {
                    client.Abort();
                }
                else
                {
                    client.Close();
                }
            }
            catch
            {
                client.Abort();
            }
        }
    }
}
