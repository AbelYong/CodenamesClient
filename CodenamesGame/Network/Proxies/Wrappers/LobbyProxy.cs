using CodenamesGame.Domain.POCO;
using CodenamesGame.LobbyService;
using CodenamesGame.Network.Proxies.CallbackHandlers;
using CodenamesGame.Network.Proxies.Interfaces;
using CodenamesGame.Util;
using System;
using System.ServiceModel;

namespace CodenamesGame.Network.Proxies.Wrappers
{
    public class LobbyProxy : ILobbyProxy
    {
        private const string _ENDPOINT_NAME = "NetTcpBinding_ILobbyManager";
        private static readonly Lazy<LobbyProxy> _instance = new Lazy<LobbyProxy>(() => new LobbyProxy());
        private LobbyManagerClient _client;
        private Guid _currentPlayerID;

        public static LobbyProxy Instance
        {
            get => _instance.Value;
        }

        private LobbyProxy()
        {

        }

        public CommunicationRequest Initialize(Guid playerID)
        {
            CommunicationRequest request = new CommunicationRequest();
            if (_client != null && _client.State == CommunicationState.Opened)
            {
                request.IsSuccess = true; //Already connected
                request.StatusCode = StatusCode.UNAUTHORIZED;
                return request;
            }

            if (playerID == Guid.Empty)
            {
                request.IsSuccess = true;
                request.StatusCode = StatusCode.MISSING_DATA;
                return request;
            }

            LobbyCallbackHandler callbackHandler = new LobbyCallbackHandler();
            InstanceContext context = new InstanceContext(callbackHandler);
            _client = new LobbyManagerClient(context, _ENDPOINT_NAME);
            _currentPlayerID = playerID;

            return Connect();
        }

        private CommunicationRequest Connect()
        {
            CommunicationRequest request;
            try
            {
                _client.Open();
                return _client.Connect(_currentPlayerID);
            }
            catch (TimeoutException)
            {
                request = GenerateServerTimeoutRequest<CommunicationRequest>();
                CloseProxy();
            }
            catch (EndpointNotFoundException)
            {
                request = GenerateServerUnreachableRequest<CommunicationRequest>();
                CloseProxy();
            }
            catch (CommunicationException)
            {
                request = GenerateServerUnavaibleRequest<CommunicationRequest>();
                CloseProxy();
            }
            catch (Exception ex)
            {
                CodenamesGameLogger.Log.Error("Unexpected exception while connecting to Lobby Service: ", ex);
                request = GenerateClientErrorRequest<CommunicationRequest>();
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
                    CodenamesGameLogger.Log.Error("Unexpected exception while disconnecting from Lobby Service: ", ex);
                    CloseProxy();
                }
            }
        }

        public CreateLobbyRequest CreateLobby(PlayerDM player)
        {
            CreateLobbyRequest request = new CreateLobbyRequest();

            TryReconnect();
            if (_client != null && _client.State == CommunicationState.Opened)
            {
                if (player != null && player.PlayerID.HasValue)
                {
                    request = SendCreateLobbyRequest(player);
                }
                else
                {
                    request.IsSuccess = false;
                    request.StatusCode = StatusCode.MISSING_DATA;
                }
            }
            else
            {
                request = GenerateServerUnavaibleRequest<CreateLobbyRequest>();
            }
            return request;
        }

        private CreateLobbyRequest SendCreateLobbyRequest(PlayerDM player)
        {
            CreateLobbyRequest request;
            try
            {
                Player auxPlayer = PlayerDM.AssembleLobbySvPlayer(player);
                return _client.CreateParty(auxPlayer);
            }
            catch (TimeoutException)
            {
                request = GenerateServerTimeoutRequest<CreateLobbyRequest>();
                CloseProxy();
            }
            catch (EndpointNotFoundException)
            {
                request = GenerateServerUnreachableRequest<CreateLobbyRequest>();
                CloseProxy();
            }
            catch (CommunicationException)
            {
                request = GenerateServerUnavaibleRequest<CreateLobbyRequest>();
                CloseProxy();
            }
            catch (Exception ex)
            {
                CodenamesGameLogger.Log.Error("Unexpected exception while sending create lobby request: ", ex);
                request = GenerateClientErrorRequest<CreateLobbyRequest>();
                CloseProxy();
            }
            return request;
        }

        public CommunicationRequest InviteToParty(PlayerDM hostPlayer, Guid friendToInviteID, string lobbyCode)
        {
            CommunicationRequest request = new CommunicationRequest();

            TryReconnect();
            if (_client != null && _client.State == CommunicationState.Opened)
            {
                if (hostPlayer != null && hostPlayer.PlayerID.HasValue)
                {
                    return SendPartyInvitation(hostPlayer, friendToInviteID, lobbyCode);
                }
                else
                {
                    request.IsSuccess = false;
                    request.StatusCode = StatusCode.MISSING_DATA;
                }
            }
            else
            {
                request = GenerateServerUnavaibleRequest<CommunicationRequest>();
            }
            return request;
        }

        private CommunicationRequest SendPartyInvitation(PlayerDM hostPlayer, Guid friendToInviteID, string lobbyCode)
        {
            CommunicationRequest request;
            try
            {
                Player auxPlayer = PlayerDM.AssembleLobbySvPlayer(hostPlayer);
                return _client.InviteToParty(auxPlayer, friendToInviteID, lobbyCode);
            }
            catch (TimeoutException)
            {
                request = GenerateServerTimeoutRequest<CommunicationRequest>();
                CloseProxy();
            }
            catch (EndpointNotFoundException)
            {
                request = GenerateServerUnreachableRequest<CommunicationRequest>();
                CloseProxy();
            }
            catch (CommunicationException)
            {
                request = GenerateServerUnavaibleRequest<CommunicationRequest>();
                CloseProxy();
            }
            catch (Exception ex)
            {
                CodenamesGameLogger.Log.Error("Unexpected exception while sending lobby/party invitation: ", ex);
                request = GenerateClientErrorRequest<CommunicationRequest>();
                CloseProxy();
            }
            return request;
        }

        public JoinPartyRequest JoinParty(PlayerDM joiningPlayer, string lobbyCode)
        {
            JoinPartyRequest request = new JoinPartyRequest();

            TryReconnect();
            if (_client != null && _client.State == CommunicationState.Opened)
            {
                if (joiningPlayer != null && joiningPlayer.PlayerID.HasValue)
                {
                    return SendJoinPartyRequest(joiningPlayer, lobbyCode);
                }
                else
                {
                    request.IsSuccess = false;
                    request.StatusCode = StatusCode.MISSING_DATA;
                }
            }
            else
            {
                request = GenerateServerUnavaibleRequest<JoinPartyRequest>();
            }
            return request;
        }

        private JoinPartyRequest SendJoinPartyRequest(PlayerDM joiningPlayer, string lobbyCode)
        {
            JoinPartyRequest request;
            try
            {
                Player auxPlayer = PlayerDM.AssembleLobbySvPlayer(joiningPlayer);
                return _client.JoinParty(auxPlayer, lobbyCode);
            }
            catch (TimeoutException)
            {
                request = GenerateServerTimeoutRequest<JoinPartyRequest>();
                CloseProxy();
            }
            catch (EndpointNotFoundException)
            {
                request = GenerateServerUnreachableRequest<JoinPartyRequest>();
                CloseProxy();
            }
            catch (CommunicationException)
            {
                request = GenerateServerUnavaibleRequest<JoinPartyRequest>();
                CloseProxy();
            }
            catch (Exception ex)
            {
                CodenamesGameLogger.Log.Error("Unexpected exception on join party/lobby attempt: ", ex);
                request = GenerateClientErrorRequest<JoinPartyRequest>();
                CloseProxy();
            }
            return request;
        }

        private void TryReconnect()
        {
            if (_client == null || _client.State != CommunicationState.Opened)
            {
                Initialize(_currentPlayerID);
            }
        }

        private static T GenerateServerUnavaibleRequest<T>() where T : Request, new()
        {
            var request = new T();
            request.IsSuccess = false;
            request.StatusCode = StatusCode.SERVER_UNAVAIBLE;
            return request;
        }

        private static T GenerateServerUnreachableRequest<T>() where T : Request, new()
        {
            var request = new T();
            request.IsSuccess = false;
            request.StatusCode = StatusCode.SERVER_UNREACHABLE;
            return request;
        }

        private static T GenerateServerTimeoutRequest<T>() where T : Request, new()
        {
            var request = new T();
            request.IsSuccess = false;
            request.StatusCode = StatusCode.SERVER_TIMEOUT;
            return request;
        }

        private static T GenerateClientErrorRequest<T>() where T : Request, new()
        {
            var request = new T();
            request.IsSuccess = false;
            request.StatusCode = StatusCode.CLIENT_ERROR;
            return request;
        }

        private void CloseProxy()
        {
            NetworkUtil.SafeClose(_client);
            _client = null;
            _currentPlayerID = Guid.Empty;
        }
    }
}
