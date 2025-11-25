using CodenamesGame.Domain.POCO.Match;
using CodenamesGame.MatchService;
using System;
using System.ServiceModel;

namespace CodenamesGame.Network
{
    public sealed class MatchOperation
    {
        private const string _ENDPOINT_NAME = "NetTcpBinding_IMatchManager";
        private static readonly Lazy<MatchOperation> _instance = new Lazy<MatchOperation>(() => new MatchOperation());
        private MatchManagerClient _client;
        private Guid _currentPlayerID;

        public static MatchOperation Instance
        {
            get => _instance.Value;
        }

        private MatchOperation()
        {

        }

        public CommunicationRequest Initialize(Guid playerID)
        {
            CommunicationRequest request = new CommunicationRequest();
            if (_client != null && _client.State == CommunicationState.Opened)
            {
                request.IsSuccess = false;
                request.StatusCode = StatusCode.UNAUTHORIZED;
                return request;
            }

            MatchCallbackHandler callbackHandler = new MatchCallbackHandler();
            InstanceContext context = new InstanceContext(callbackHandler);
            _client = new MatchManagerClient(context, _ENDPOINT_NAME);
            _currentPlayerID = playerID;

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
            catch (CommunicationException)
            {
                request.IsSuccess = false;
                request.StatusCode = StatusCode.SERVER_UNAVAIBLE;
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
                catch (CommunicationException)
                {
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
                catch (CommunicationException)
                {
                    request.IsSuccess = false;
                    request.StatusCode = StatusCode.SERVER_ERROR;
                    CloseProxy();
                }
                catch (TimeoutException)
                {
                    request.IsSuccess = false;
                    request.StatusCode = StatusCode.SERVER_TIMEOUT;
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

        public void NotifyTurnTimeout(MatchRoleType currentRole)
        {
            if (_client != null && _client.State == CommunicationState.Opened)
            {
                try
                {
                    _client.NotifyTurnTimeoutAsync(_currentPlayerID, currentRole);
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
            Util.NetworkUtil.SafeClose(_client);
            _client = null;
        }
    }
}
