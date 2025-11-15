using CodenamesGame.Domain.POCO.Match;
using CodenamesGame.MatchmakingService;
using CodenamesGame.Util;
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
        private static readonly Lazy<MatchmakingOperation> _instance = new Lazy<MatchmakingOperation>(() => new MatchmakingOperation());
        private MatchmakingManagerClient _client;
        private MatchmakingCallbackHandler _callbackHandler;
        private Guid _currentPlayerID;

        public static MatchmakingOperation Instance
        {
            get => _instance.Value;
        }

        private MatchmakingOperation() { }

        public CommunicationRequest Initialize(Guid playerID)
        {
            CommunicationRequest request = new CommunicationRequest();

            if (_client != null && _client.State == CommunicationState.Opened)
            {
                request.IsSuccess = false;
                //Todo add a status code for this case
                return request;
            }

            if (playerID == Guid.Empty)
            {
                //TODO translate me
                request.IsSuccess = false;
                request.StatusCode = StatusCode.MISSING_DATA;
                return request;
            }

            _currentPlayerID = playerID;
            _callbackHandler = new MatchmakingCallbackHandler();
            InstanceContext context = new InstanceContext(_callbackHandler);
            _client = new MatchmakingManagerClient(context, _ENDPOINT_NAME);

            try
            {
                //todo translate me
                _client.Open();
                _client.Connect(_currentPlayerID);
            }
            catch (CommunicationException)
            {
                request.IsSuccess = false;
                request.StatusCode = StatusCode.SERVER_UNAVAIBLE;
                NetworkUtil.SafeClose(_client);
                _client = null;
            }
            catch (TimeoutException)
            {
                //TODO translate me
                request.IsSuccess = false;
                request.StatusCode = StatusCode.SERVER_TIMEOUT;
                NetworkUtil.SafeClose(_client);
                _client = null;
            }
            return request;
        }


        public void Terminate()
        {
            if (_client != null && _client.State == CommunicationState.Opened)
            {
                try
                {
                    _client.Disconnect(_currentPlayerID);
                }
                catch (CommunicationException)
                {
                    NetworkUtil.SafeClose(_client);
                    _client = null;
                    _currentPlayerID = Guid.Empty;
                }
            }
        }

        private MatchmakingManagerClient GetClient()
        {
            if (_client == null || _client.State != CommunicationState.Opened)
            {
                throw new InvalidOperationException("Matchmaking service connection is not available or has been closed.");
            }
            return _client;
        }

        public CommunicationRequest RequestMatch(MatchConfigurationDM matchConfig)
        {
            CommunicationRequest request = new CommunicationRequest();
            MatchConfiguration configuration = MatchConfigurationDM.AssembleMatchmakingSvMatchConfig(matchConfig);
            try
            {
                request = _client.RequestArrangedMatch(configuration);
            }
            catch (CommunicationException)
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
