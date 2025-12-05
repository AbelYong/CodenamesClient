using CodenamesGame.Domain.POCO.Match;
using CodenamesGame.MatchmakingService;
using CodenamesGame.Network.Proxies.CallbackHandlers;
using CodenamesGame.Network.Proxies.Interfaces;
using CodenamesGame.Util;
using System;
using System.ServiceModel;
using System.Threading.Tasks;

namespace CodenamesGame.Network.Proxies.Wrappers
{
    public class MatchmakingProxy : IMatchmakingProxy
    {
        private const string _ENDPOINT_NAME = "NetTcpBinding_IMatchmakingManager";
        private static readonly Lazy<MatchmakingProxy> _instance = new Lazy<MatchmakingProxy>(() => new MatchmakingProxy());
        private MatchmakingManagerClient _client;
        private Guid _currentPlayerID;

        public static MatchmakingProxy Instance
        {
            get => _instance.Value;
        }

        private MatchmakingProxy()
        {

        }

        public CommunicationRequest Initialize(Guid playerID)
        {
            CommunicationRequest request = new CommunicationRequest();

            if (_client != null && _client.State == CommunicationState.Opened)
            {
                request.IsSuccess = true; //Already connceted
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

            return Connect();
        }

        private CommunicationRequest Connect()
        {
            CommunicationRequest request = new CommunicationRequest();
            try
            {
                _client.Open();
                request = _client.Connect(_currentPlayerID);
            }
            catch (TimeoutException)
            {
                request.IsSuccess = false;
                request.StatusCode = StatusCode.SERVER_TIMEOUT;
                CloseProxy();
            }
            catch (EndpointNotFoundException)
            {
                request.IsSuccess = false;
                request.StatusCode = StatusCode.SERVER_UNREACHABLE;
                CloseProxy();
            }
            catch (CommunicationException)
            {
                request.IsSuccess = false;
                request.StatusCode = StatusCode.SERVER_UNAVAIBLE;
                CloseProxy();
            }
            catch (Exception ex)
            {
                CodenamesGameLogger.Log.Error("Unexpected exception while connecting to Matchmaking service: ", ex);
                request.IsSuccess = false;
                request.StatusCode = StatusCode.CLIENT_ERROR;
                CloseProxy();
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
                catch (TimeoutException)
                {
                    CloseProxy();
                }
                catch (EndpointNotFoundException)
                {
                    CloseProxy();
                }
                catch (CommunicationException)
                {
                    CloseProxy();
                }
                catch (Exception ex)
                {
                    CodenamesGameLogger.Log.Error("Unexpected exception while disconnecting from Matchmaking service: ", ex);
                    CloseProxy();
                }
            }
        }

        public async Task<CommunicationRequest> RequestArrangedMatch(MatchConfigurationDM matchConfig)
        {
            CommunicationRequest request = new CommunicationRequest();
            MatchConfiguration configuration = MatchConfigurationDM.AssembleMatchmakingSvMatchConfig(matchConfig);
            TryReconnect();
            if (_client != null && _client.State == CommunicationState.Opened)
            {
                try
                {
                    request = await _client.RequestArrangedMatchAsync(configuration);
                }
                catch (TimeoutException)
                {
                    request.StatusCode = StatusCode.SERVER_TIMEOUT;
                    request.IsSuccess = false;
                    CloseProxy();
                }
                catch (EndpointNotFoundException)
                {
                    request.IsSuccess = false;
                    request.StatusCode = StatusCode.SERVER_UNREACHABLE;
                    CloseProxy();
                }
                catch (CommunicationException)
                {
                    request.StatusCode = StatusCode.SERVER_UNAVAIBLE;
                    request.IsSuccess = false;
                    CloseProxy();
                }
                catch (Exception ex)
                {
                    CodenamesGameLogger.Log.Error("Unexpected exception on arranged match request attempt: ", ex);
                    request.IsSuccess = false;
                    request.StatusCode = StatusCode.CLIENT_ERROR;
                    CloseProxy();
                }
            }
            else
            {
                request.IsSuccess = false;
                request.StatusCode = StatusCode.SERVER_UNAVAIBLE;
            }
            return request;
        }

        public void ConfirmMatch(Guid matchID)
        {
            TryReconnect();
            if (_client != null && _client.State == CommunicationState.Opened)
            {
                try
                {
                    _client.ConfirmMatchReceivedAsync(_currentPlayerID, matchID);
                }
                catch (TimeoutException)
                {
                    CloseProxy();
                }
                catch (EndpointNotFoundException)
                {
                    CloseProxy();
                }
                catch (CommunicationException)
                {
                    CloseProxy();
                }
                catch (Exception ex)
                {
                    CodenamesGameLogger.Log.Error("Unexpected exception on match confirmation: ", ex);
                    CloseProxy();
                }
            }
        }

        public void CancelMatch()
        {
            TryReconnect();
            if (_client != null && _client.State == CommunicationState.Opened)
            {
                try
                {
                    _client.RequestMatchCancelAsync(_currentPlayerID);
                }
                catch (TimeoutException)
                {
                    CloseProxy();
                }
                catch (EndpointNotFoundException)
                {
                    CloseProxy();
                }
                catch (CommunicationException)
                {
                    CloseProxy();
                }
                catch (Exception ex)
                {
                    CodenamesGameLogger.Log.Error("Unexpected exception on match cancellation: ", ex);
                    CloseProxy();
                }
            }
        }

        private void TryReconnect()
        {
            if (_client == null || _client.State != CommunicationState.Opened)
            {
                Initialize(_currentPlayerID);
            }
        }

        private void CloseProxy()
        {
            NetworkUtil.SafeClose(_client);
            _client = null;
            _currentPlayerID = Guid.Empty;
        }
    }
}
