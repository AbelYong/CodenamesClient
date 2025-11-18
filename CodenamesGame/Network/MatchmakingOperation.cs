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
                request.StatusCode = StatusCode.UNAUTHORIZED;
                return request;
            }

            if (playerID == Guid.Empty)
            {
                request.IsSuccess = false;
                request.StatusCode = StatusCode.MISSING_DATA;
                return request;
            }

            _currentPlayerID = playerID;
            MatchmakingCallbackHandler _callbackHandler = new MatchmakingCallbackHandler(_currentPlayerID);
            InstanceContext context = new InstanceContext(_callbackHandler);
            _client = new MatchmakingManagerClient(context, _ENDPOINT_NAME);

            try
            {
                //todo translate me
                _client.Open();
                request = _client.Connect(_currentPlayerID);
            }
            catch (CommunicationException)
            {
                CloseProxy();
                request.IsSuccess = false;
                request.StatusCode = StatusCode.SERVER_UNAVAIBLE;
            }
            catch (TimeoutException)
            {
                //TODO translate me
                CloseProxy();
                request.IsSuccess = false;
                request.StatusCode = StatusCode.SERVER_TIMEOUT;
            }
            return request;
        }


        public void Disconnect()
        {
            if (_client != null && _client.State == CommunicationState.Opened)
            {
                try
                {
                    _client.DisconnectAsync(_currentPlayerID);
                    CloseProxy();
                }
                catch (CommunicationException)
                {
                    CloseProxy();
                    _currentPlayerID = Guid.Empty;
                }
            }
        }

        public async Task<CommunicationRequest> RequestArrangedMatch(MatchConfigurationDM matchConfig)
        {
            CommunicationRequest request = new CommunicationRequest();
            MatchConfiguration configuration = MatchConfigurationDM.AssembleMatchmakingSvMatchConfig(matchConfig);
            try
            {
                request = await _client.RequestArrangedMatchAsync(configuration);
            }
            catch (CommunicationException)
            {
                CloseProxy();
                request.StatusCode = StatusCode.SERVER_UNAVAIBLE;
                request.IsSuccess = false;
            }
            catch (TimeoutException)
            {
                CloseProxy();
                request.StatusCode = StatusCode.SERVER_TIMEOUT;
                request.IsSuccess = false;
            }
            return request;
        }

        public void ConfirmMatch(Guid matchID)
        {
            if (_client != null && _client.State == CommunicationState.Opened)
            {
                try
                {
                    _client.ConfirmMatchReceivedAsync(_currentPlayerID, matchID);
                }
                catch (CommunicationException)
                {
                    CloseProxy();
                }
                catch (TimeoutException)
                {
                    CloseProxy();
                }
            }
        }

        public void CancelMatch()
        {
            if (_client != null && _client.State == CommunicationState.Opened)
            {
                try
                {
                    _client.RequestMatchCancelAsync(_currentPlayerID);
                }
                catch (CommunicationException)
                {
                    CloseProxy();
                }
                catch (TimeoutException)
                {
                    CloseProxy();
                }
            }
        }

        private void CloseProxy()
        {
            NetworkUtil.SafeClose(_client);
            _client = null;
        }
    }
}
