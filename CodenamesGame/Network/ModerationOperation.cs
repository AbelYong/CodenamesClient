using CodenamesGame.ModerationService;
using System;
using System.ServiceModel;

namespace CodenamesGame.Network
{
    public class ModerationOperation
    {
        private const string _ENDPOINT_NAME = "NetTcpBinding_IModerationManager";
        private ModerationManagerClient _client;

        public ModerationOperation()
        {
            _client = new ModerationManagerClient(_ENDPOINT_NAME);
        }

        public CommunicationRequest ReportPlayer(Guid reportedUserID, string reason)
        {
            CommunicationRequest response = new CommunicationRequest();

            try
            {
                response = _client.ReportPlayer(reportedUserID, reason);
            }
            catch (EndpointNotFoundException)
            {
                response.IsSuccess = false;
                response.StatusCode = StatusCode.SERVER_UNAVAIBLE;
                Util.NetworkUtil.SafeClose(_client);
            }
            catch (TimeoutException)
            {
                response.IsSuccess = false;
                response.StatusCode = StatusCode.SERVER_TIMEOUT;
                Util.NetworkUtil.SafeClose(_client);
            }
            catch (Exception)
            {
                response.IsSuccess = false;
                response.StatusCode = StatusCode.SERVER_ERROR;
                Util.NetworkUtil.SafeClose(_client);
            }

            return response;
        }
    }
}