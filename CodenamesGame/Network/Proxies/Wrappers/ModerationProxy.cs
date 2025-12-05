using CodenamesGame.ModerationService;
using CodenamesGame.Network.Proxies.Interfaces;
using CodenamesGame.Util;
using System;
using System.ServiceModel;

namespace CodenamesGame.Network.Proxies.Wrappers
{
    public class ModerationProxy : IModerationProxy
    {
        private const string _ENDPOINT_NAME = "NetTcpBinding_IModerationManager";

        public CommunicationRequest ReportPlayer(Guid reporterUserID, Guid reportedUserID, string reason)
        {
            CommunicationRequest request = new CommunicationRequest();
            ModerationManagerClient client = new ModerationManagerClient(_ENDPOINT_NAME);
            try
            {
                request = client.ReportPlayer(reporterUserID, reportedUserID, reason);
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
                CodenamesGameLogger.Log.Error("Unexpected exception on player report: ", ex);
                request.IsSuccess = false;
                request.StatusCode = StatusCode.SERVER_ERROR;
            }
            finally
            {
                Util.NetworkUtil.SafeClose(client);
            }

            return request;
        }
    }
}
