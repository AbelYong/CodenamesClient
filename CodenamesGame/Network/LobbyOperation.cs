using CodenamesGame.Domain.POCO;
using CodenamesGame.LobbyService;
using CodenamesGame.Util;
using System;
using System.ServiceModel;

namespace CodenamesGame.Network
{
    public sealed class LobbyOperation
    {
        private const string _ENDPOINT_NAME = "NetTcpBinding_ILobbyManager";
        private static readonly Lazy<LobbyOperation> _instance = new Lazy<LobbyOperation>(() => new LobbyOperation());
        private LobbyManagerClient _client;
        private Guid _currentPlayerID;

        public static LobbyOperation Instance
        {
            get => _instance.Value;
        }

        private LobbyOperation()
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

            LobbyCallbackHandler callbackHandler = new LobbyCallbackHandler();
            InstanceContext context = new InstanceContext(callbackHandler);
            _client = new LobbyManagerClient(context, _ENDPOINT_NAME);
            _currentPlayerID = playerID;

            try
            {
                _client.Open();
                return _client.Connect(_currentPlayerID);
            }
            catch (CommunicationException)
            {
                request = GenerateServerUnavaibleRequest<CommunicationRequest>();
                CloseProxy();
            }
            catch (TimeoutException)
            {
                request = GenerateServerTimeoutRequest<CommunicationRequest>();
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
                    _currentPlayerID = Guid.Empty;
                    CloseProxy();
                }
                catch (CommunicationException)
                {
                    CloseProxy();
                }
            } 
        }

        public CreateLobbyRequest CreateLobby(PlayerDM player)
        {
            CreateLobbyRequest request = new CreateLobbyRequest();
            if (_client != null && _client.State == CommunicationState.Opened)
            {
                if (player != null && player.PlayerID.HasValue)
                {
                    try
                    {
                        Player auxPlayer = PlayerDM.AssembleLobbySvPlayer(player);
                        request = _client.CreateParty(auxPlayer);
                    }
                    catch (CommunicationException)
                    {
                        request = GenerateServerUnavaibleRequest<CreateLobbyRequest>();
                        CloseProxy();
                    }
                    catch (TimeoutException)
                    {
                        request = GenerateServerTimeoutRequest<CreateLobbyRequest>();
                        CloseProxy();
                    }
                }
                else
                {
                    request.IsSuccess = false;
                    request.StatusCode = StatusCode.MISSING_DATA;
                }
            }
            return request;
        }

        public CommunicationRequest InviteToParty(PlayerDM hostPlayer, Guid friendToInviteID, string lobbyCode)
        {
            CommunicationRequest request = new CommunicationRequest();
            if (_client != null && _client.State == CommunicationState.Opened)
            {
                if (hostPlayer != null && hostPlayer.PlayerID.HasValue)
                {
                    try
                    {
                        Player auxPlayer = PlayerDM.AssembleLobbySvPlayer(hostPlayer);
                        request = _client.InviteToParty(auxPlayer, friendToInviteID, lobbyCode);
                    }
                    catch (CommunicationException)
                    {
                        request = GenerateServerUnavaibleRequest<CommunicationRequest>();
                        CloseProxy();
                    }
                    catch (TimeoutException)
                    {
                        request = GenerateServerTimeoutRequest<CommunicationRequest>();
                        CloseProxy();
                    }
                }
                else
                {
                    request.IsSuccess = false;
                    request.StatusCode = StatusCode.MISSING_DATA;
                }
            }
            return request;
        }

        public JoinPartyRequest JoinParty(PlayerDM joiningPlayer, string lobbyCode)
        {
            JoinPartyRequest request = new JoinPartyRequest();
            if (_client != null && _client.State == CommunicationState.Opened)
            {
                if (joiningPlayer != null && joiningPlayer.PlayerID.HasValue)
                {
                    try
                    {
                        Player auxPlayer = PlayerDM.AssembleLobbySvPlayer(joiningPlayer);
                        request = _client.JoinParty(auxPlayer, lobbyCode);
                    }
                    catch (CommunicationException)
                    {
                        request = GenerateServerUnavaibleRequest<JoinPartyRequest>();
                        CloseProxy();
                    }
                    catch (TimeoutException)
                    {
                        request = GenerateServerTimeoutRequest<JoinPartyRequest>();
                        CloseProxy();
                    }
                }
                else
                {
                    request.IsSuccess = false;
                    request.StatusCode = StatusCode.MISSING_DATA;
                }
            }
            return request;
        }

        private static T GenerateServerUnavaibleRequest<T>() where T : Request, new()
        {
            var request = new T();
            request.IsSuccess = false;
            request.StatusCode = StatusCode.SERVER_UNAVAIBLE;
            return request;
        }

        private static T GenerateServerTimeoutRequest<T>() where T : Request, new()
        {
            var request = new T();
            request.IsSuccess = false;
            request.StatusCode = StatusCode.SERVER_TIMEOUT;
            return request;
        }

        private void CloseProxy()
        {
            NetworkUtil.SafeClose(_client);
            _client = null;
        }
    }
}
