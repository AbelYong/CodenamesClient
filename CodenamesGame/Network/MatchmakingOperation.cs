using CodenamesGame.Domain.POCO.Match;
using CodenamesGame.MatchmakingService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace CodenamesGame.Network
{
    public class MatchmakingOperation
    {
        private const string _ENDPOINT_NAME = "NetTcpBinding_IMatchmakingManager";
        private MatchmakingManagerClient _client;

        private void InitializeCallbackChannel()
        {
            InstanceContext context = new InstanceContext(new MatchmakingCallback());
            _client = new MatchmakingManagerClient(context, _ENDPOINT_NAME);
        }

        public MatchRequest RequestMatch(MatchConfigurationDM matchConfig)
        {
            if (_client == null)
            {
                InitializeCallbackChannel();
            }

            MatchRequest request = new MatchRequest();
            MatchConfiguration configuration = MatchConfigurationDM.AssembleMatchmakingSvMatchConfig(matchConfig);
            try
            {
                request = _client.GetMatchWithAFriend(configuration);
            }
            catch (EndpointNotFoundException)
            {
                request.StatusCode = StatusCode.SERVER_UNAVAIBLE;
                request.IsSuccess = false;
                Util.NetworkUtil.SafeClose(_client);
                _client = null;
            }
            catch (TimeoutException)
            {
                request.StatusCode = StatusCode.SERVER_TIMEOUT;
                request.IsSuccess = false;
                Util.NetworkUtil.SafeClose(_client);
                _client = null;
            }
            return request;
        }
    }
}
