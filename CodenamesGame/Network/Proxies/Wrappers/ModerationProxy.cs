using CodenamesGame.ModerationService;
using CodenamesGame.Network.Proxies.Interfaces;
using CodenamesGame.Util;
using System;
using System.ServiceModel;

namespace CodenamesGame.Network.Proxies.Wrappers
{
    public class ModerationProxy : IModerationProxy
    {
        private readonly Func<IModerationManager> _clientFactory;
        private const string _ENDPOINT_NAME = "NetTcpBinding_IModerationManager";

        public ModerationProxy() : this(() => new ModerationManagerClient(_ENDPOINT_NAME)) { }

        public ModerationProxy(Func<IModerationManager> clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public CommunicationRequest ReportPlayer(Guid reporterUserID, Guid reportedUserID, string reason)
        {
            CommunicationRequest request = new CommunicationRequest();
            var client = _clientFactory();
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
                if (client is ICommunicationObject commObject)
                {
                    NetworkUtil.SafeClose(commObject);
                }
            }
            return request;
        }
    }
}
