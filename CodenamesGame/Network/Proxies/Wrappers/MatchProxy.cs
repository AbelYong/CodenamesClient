using CodenamesGame.Domain.POCO.Match;
using CodenamesGame.MatchService;
using CodenamesGame.Network.Proxies.CallbackHandlers;
using CodenamesGame.Network.Proxies.Interfaces;
using CodenamesGame.Util;
using System;
using System.ServiceModel;
using System.Threading.Tasks;

namespace CodenamesGame.Network.Proxies.Wrappers
{
    public class MatchProxy : IMatchProxy
    {
        public delegate IMatchManager MatchClientFactory(InstanceContext context, string endpointName);
        private readonly MatchClientFactory _clientFactory;
        private const string _ENDPOINT_NAME = "NetTcpBinding_IMatchManager";
        private static readonly Lazy<MatchProxy> _instance = new Lazy<MatchProxy>(() => new MatchProxy());
        private IMatchManager _client;
        private Guid _currentPlayerID;

        public static MatchProxy Instance
        {
            get => _instance.Value;
        }

        private MatchProxy() : this ((context, endpoint) =>
        {
            var factory = new DuplexChannelFactory<IMatchManager>(context, endpoint);
            return factory.CreateChannel();
        })
        {

        }

        public MatchProxy(MatchClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public CommunicationRequest Initialize(Guid playerID)
        {
            CommunicationRequest request = new CommunicationRequest();
            if (_client != null && ((ICommunicationObject)_client).State == CommunicationState.Opened)
            {
                request.IsSuccess = true;
                request.StatusCode = StatusCode.UNAUTHORIZED;
                return request;
            }

            MatchCallbackHandler callbackHandler = new MatchCallbackHandler();
            InstanceContext context = new InstanceContext(callbackHandler);
            _client = _clientFactory(context, _ENDPOINT_NAME);
            _currentPlayerID = playerID;

            return Connect(playerID);
        }

        private CommunicationRequest Connect(Guid playerID)
        {
            CommunicationRequest request = new CommunicationRequest();
            try
            {
                ((ICommunicationObject)_client).Open();
                return _client.Connect(playerID);
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
                CodenamesGameLogger.Log.Error("Unexpected exception while connecting to Match Service: ", ex);
                request.IsSuccess = false;
                request.StatusCode = StatusCode.CLIENT_ERROR;
                CloseProxy();
            }
            return request;
        }

        public void Disconnect()
        {
            if (_client != null && ((ICommunicationObject)_client).State == CommunicationState.Opened)
            {
                try
                {
                    _client.DisconnectAsync(_currentPlayerID);
                    _currentPlayerID = Guid.Empty;
                    CloseProxy();
                }
                catch (Exception ex) when (ex is TimeoutException || ex is EndpointNotFoundException || ex is CommunicationException)
                {
                    CloseProxy();
                }
                catch (Exception ex)
                {
                    CodenamesGameLogger.Log.Error("Unexpected exception while disconnecting from Match service: ", ex);
                    CloseProxy();
                }
            }
        }

        public CommunicationRequest JoinMatch(MatchDM match)
        {
            CommunicationRequest request = new CommunicationRequest();
            if (VerifyClientClosedOrFaulted())
            {
                request.IsSuccess = false;
                request.StatusCode = StatusCode.SERVER_UNAVAIBLE;
                CloseProxy();
                return request;
            }

            try
            {
                Match auxMatch = MatchDM.AssembleMatchSvMatch(match);
                return _client.JoinMatch(auxMatch, _currentPlayerID);
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
                CodenamesGameLogger.Log.Error("Unexpected exception on join match attempt: ", ex);
                request.IsSuccess = false;
                request.StatusCode = StatusCode.CLIENT_ERROR;
                CloseProxy();
            }
            return request;
        }

        public async Task SendClue(string clue)
        {
            if (VerifyClientClosedOrFaulted())
            {
                CloseProxy();
                return;
            }

            try
            {
                await _client.SendClueAsync(_currentPlayerID, clue);
            }
            catch (Exception ex) when (ex is TimeoutException || ex is EndpointNotFoundException || ex is CommunicationException)
            {
                CloseProxy();
            }
            catch (Exception ex)
            {
                CodenamesGameLogger.Log.Error("Unexpected exception while sending clue: ", ex);
                CloseProxy();
            }
        }

        public async Task NotifyTurnTimeout(MatchRoleType currentRole)
        {
            if (VerifyClientClosedOrFaulted())
            {
                CloseProxy();
                return;
            }

            try
            {
                await _client.NotifyTurnTimeoutAsync(_currentPlayerID, currentRole);
            }
            catch (Exception ex) when (ex is TimeoutException || ex is EndpointNotFoundException || ex is CommunicationException)
            {
                CloseProxy();
            }
            catch (Exception ex)
            {
                CodenamesGameLogger.Log.Error("Unexpected exception while notifying turn timeout: ", ex);
                CloseProxy();
            }
        }

        public async Task NotifyPickedAgent(BoardCoordinatesDM coordinates, int newTurnLength)
        {
            if (VerifyClientClosedOrFaulted())
            {
                CloseProxy();
                return;
            }

            try
            {
                AgentPickedNotification notification = new AgentPickedNotification
                {
                    SenderID = _currentPlayerID,
                    Coordinates = BoardCoordinatesDM.AssembleMatchSvBoardCoordinates(coordinates),
                    NewTurnLength = newTurnLength
                };
                await _client.NotifyPickedAgentAsync(notification);
            }
            catch (Exception ex) when (ex is TimeoutException || ex is EndpointNotFoundException || ex is CommunicationException)
            {
                CloseProxy();
            }
            catch (Exception ex)
            {
                CodenamesGameLogger.Log.Error("Unexpected exception while sending picked agent notification: ", ex);
                CloseProxy();
            }
        }

        public async Task NotifyPickedBystander(BoardCoordinatesDM coordinates)
        {
            if (VerifyClientClosedOrFaulted())
            {
                CloseProxy();
                return;
            }

            try
            {
                BystanderPickedNotification notification = new BystanderPickedNotification
                {
                    SenderID = _currentPlayerID,
                    Coordinates = BoardCoordinatesDM.AssembleMatchSvBoardCoordinates(coordinates)
                };

                await _client.NotifyPickedBystanderAsync(notification);
            }
            catch (Exception ex) when (ex is TimeoutException || ex is EndpointNotFoundException || ex is CommunicationException)
            {
                CloseProxy();
            }
            catch (Exception ex)
            {
                CodenamesGameLogger.Log.Error("Unexpected exception while sending picked bystander notification: ", ex);
                CloseProxy();
            }
        }

        public async Task NotifyPickedAssassin(BoardCoordinatesDM coordinates)
        {
            if (VerifyClientClosedOrFaulted())
            {
                CloseProxy();
                return;
            }

            try
            {
                AssassinPickedNotification notification = new AssassinPickedNotification
                {
                    SenderID = _currentPlayerID,
                    Coordinates = BoardCoordinatesDM.AssembleMatchSvBoardCoordinates(coordinates)
                };
                await _client.NotifyPickedAssassinAsync(notification);
            }
            catch (Exception ex) when (ex is TimeoutException || ex is EndpointNotFoundException || ex is CommunicationException)
            {
                CloseProxy();
            }
            catch (Exception ex)
            {
                CodenamesGameLogger.Log.Error("Unexpected exception while sending picked assassin notification: ", ex);
                CloseProxy();
            }
        }

        public async Task<bool> CheckCompanionStatus()
        {
            if (VerifyClientClosedOrFaulted())
            {
                CloseProxy();
                return false;
            }

            try
            {
                return await _client.CheckCompanionStatusAsync(_currentPlayerID);
            }
            catch (Exception ex) when (ex is TimeoutException || ex is EndpointNotFoundException || ex is CommunicationException)
            {
                CloseProxy();
                return false;
            }
            catch (Exception ex)
            {
                CodenamesGameLogger.Log.Error("Unexpected exception while checking on match companion's status: ", ex);
                CloseProxy();
                return false;
            }
        }

        private bool VerifyClientClosedOrFaulted()
        {
            return _client == null || ((ICommunicationObject)_client).State != CommunicationState.Opened;
        }

        private void CloseProxy()
        {
            MatchCallbackHandler.NotifyServerConnectionLost();
            if (_client is ICommunicationObject commObject)
            {
                NetworkUtil.SafeClose(commObject);
            }
            _client = null;
            _currentPlayerID = Guid.Empty;
        }
    }
}
