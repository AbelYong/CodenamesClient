using CodenamesGame.Domain.POCO.Match;
using CodenamesGame.MatchService;
using CodenamesGame.Network.Proxies.CallbackHandlers;
using CodenamesGame.Network.Proxies.Interfaces;
using CodenamesGame.Util;
using System;
using System.ServiceModel;

namespace CodenamesGame.Network.Proxies.Wrappers
{
    public class MatchProxy : IMatchProxy
    {
        private const string _ENDPOINT_NAME = "NetTcpBinding_IMatchManager";
        private static readonly Lazy<MatchProxy> _instance = new Lazy<MatchProxy>(() => new MatchProxy());
        private MatchManagerClient _client;
        private Guid _currentPlayerID;

        public static MatchProxy Instance
        {
            get => _instance.Value;
        }

        private MatchProxy()
        {

        }

        public CommunicationRequest Initialize(Guid playerID)
        {
            CommunicationRequest request = new CommunicationRequest();
            if (_client != null && _client.State == CommunicationState.Opened)
            {
                request.IsSuccess = true;  //Player already connected
                request.StatusCode = StatusCode.UNAUTHORIZED;
                return request;
            }

            MatchCallbackHandler callbackHandler = new MatchCallbackHandler();
            InstanceContext context = new InstanceContext(callbackHandler);
            _client = new MatchManagerClient(context, _ENDPOINT_NAME);
            _currentPlayerID = playerID;


            return Connect(playerID);
        }

        private CommunicationRequest Connect(Guid playerID)
        {
            CommunicationRequest request = new CommunicationRequest();
            try
            {
                _client.Open();
                return _client.Connect(playerID);
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
                CodenamesGameLogger.Log.Error("Unexpected exception while connecting to Match Service: ", ex);
                request.IsSuccess = false;
                request.StatusCode = StatusCode.CLIENT_ERROR;
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
                    _currentPlayerID = Guid.Empty;
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
                    CodenamesGameLogger.Log.Error("Unexpected exception while disconnecting from Match service: ", ex);
                    CloseProxy();
                }
            }
        }

        public CommunicationRequest JoinMatch(MatchDM match)
        {
            CommunicationRequest request = new CommunicationRequest();
            if (_client != null && _client.State == CommunicationState.Opened)
            {
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
            }
            return request;
        }

        public void SendClue(string clue)
        {
            if (_client != null && _client.State == CommunicationState.Opened)
            {
                try
                {
                    _client.SendClueAsync(_currentPlayerID, clue);
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
                    CodenamesGameLogger.Log.Error("Unexpected exception while sending clue: ", ex);
                    CloseProxy();
                }
            }
        }

        public void NotifyTurnTimeout(MatchRoleType currentRole)
        {
            if (_client != null && _client.State == CommunicationState.Opened)
            {
                try
                {
                    _client.NotifyTurnTimeoutAsync(_currentPlayerID, currentRole);
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
                    CodenamesGameLogger.Log.Error("Unexpected exception while notifying turn timeout: ", ex);
                    CloseProxy();
                }
            }
        }

        public void NotifyPickedAgent(BoardCoordinatesDM coordinates, int newTurnLength)
        {
            if (_client != null && _client.State == CommunicationState.Opened)
            {
                try
                {
                    AgentPickedNotification notification = new AgentPickedNotification
                    {
                        SenderID = _currentPlayerID,
                        Coordinates = BoardCoordinatesDM.AssembleMatchSvBoardCoordinates(coordinates),
                        NewTurnLength = newTurnLength
                    };
                    _client.NotifyPickedAgentAsync(notification);
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
                    CodenamesGameLogger.Log.Error("Unexpected exception while sending picked agent notification: ", ex);
                    CloseProxy();
                }
            }
        }

        public void NotifyPickedBystander(BoardCoordinatesDM coordinates)
        {
            if (_client != null && _client.State == CommunicationState.Opened)
            {
                try
                {
                    BystanderPickedNotification notification = new BystanderPickedNotification
                    {
                        SenderID = _currentPlayerID,
                        Coordinates = BoardCoordinatesDM.AssembleMatchSvBoardCoordinates(coordinates)
                    };

                    _client.NotifyPickedBystanderAsync(notification);
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
                    CodenamesGameLogger.Log.Error("Unexpected exception while sending picked bystander notification: ", ex);
                    CloseProxy();
                }
            }
        }

        public void NotifyPickedAssassin(BoardCoordinatesDM coordinates)
        {
            if (_client != null && _client.State == CommunicationState.Opened)
            {
                try
                {
                    AssassinPickedNotification notification = new AssassinPickedNotification
                    {
                        SenderID = _currentPlayerID,
                        Coordinates = BoardCoordinatesDM.AssembleMatchSvBoardCoordinates(coordinates)
                    };
                    _client.NotifyPickedAssassinAsync(notification);
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
                    CodenamesGameLogger.Log.Error("Unexpected exception while sending picked assassin notification: ", ex);
                    CloseProxy();
                }
            }
        }

        private void CloseProxy()
        {
            Util.NetworkUtil.SafeClose(_client);
            _client = null;
            _currentPlayerID = Guid.Empty;
        }
    }
}
