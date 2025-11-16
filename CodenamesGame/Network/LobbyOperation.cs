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

        public static LobbyOperation Instance
        {
            get => _instance.Value;
        }

        private LobbyOperation()
        {

        }

        public void Initialize()
        {
            if (_client != null && _client.State == CommunicationState.Opened)
            {
                return;
            }

            LobbyCallbackHandler callbackHandler = new LobbyCallbackHandler();
            InstanceContext context = new InstanceContext(callbackHandler);
            _client = new LobbyManagerClient(context, _ENDPOINT_NAME);

            try
            {
                _client.Open();
            }
            catch (CommunicationException)
            {
                NetworkUtil.SafeClose(_client);
                _client = null;
            }
            catch (TimeoutException)
            {
                NetworkUtil.SafeClose(_client);
                _client = null;
            }
        }

        public CommunicationRequest Connect(Guid playerID)
        {
            CommunicationRequest request = new CommunicationRequest();
            if (_client != null && _client.State == CommunicationState.Opened)
            {
                try
                {
                    request = _client.Connect(playerID);
                }
                catch (CommunicationException)
                {
                    CloseProxy();
                    request = (CommunicationRequest)GenerateServerUnavaibleRequest();
                }
                catch (TimeoutException)
                {
                    CloseProxy();
                    request = (CommunicationRequest)GenerateServerTimeoutRequest();
                }
            }
            return request;
        }

        public void Disconnect(Guid playerID)
        {
            if (_client != null && _client.State == CommunicationState.Opened)
            {
                try
                {
                    _client.DisconnectAsync(playerID);
                }
                catch (CommunicationException)
                {
                    CloseProxy();
                }
            } 
        }

        public CreateLobbyRequest CreateLobby(Guid playerID)
        {
            CreateLobbyRequest request = new CreateLobbyRequest();
            if (_client != null && _client.State == CommunicationState.Opened)
            {
                try
                {
                    request = _client.CreateParty(playerID);
                }
                catch (CommunicationException)
                {
                    CloseProxy();
                    request = (CreateLobbyRequest)GenerateServerUnavaibleRequest();
                }
                catch (TimeoutException)
                {
                    CloseProxy();
                    request = (CreateLobbyRequest)GenerateServerTimeoutRequest();
                }
            }
            return request;
        }

        public CommunicationRequest InviteToParty(Guid hostPlayerID, Guid friendToInviteID, string lobbyCode)
        {
            CommunicationRequest request = new CommunicationRequest();
            if (_client != null && _client.State == CommunicationState.Opened)
            {
                try
                {
                    request = _client.InviteToParty(hostPlayerID, friendToInviteID, lobbyCode);
                }
                catch (CommunicationException)
                {
                    CloseProxy();
                    request = (CommunicationRequest)GenerateServerUnavaibleRequest();
                }
                catch (TimeoutException)
                {
                    CloseProxy();
                    request = (CommunicationRequest)GenerateServerTimeoutRequest();
                }
            }
            return request;
        }

        public JoinPartyRequest JoinParty(Guid joiningPlayerID, string lobbyCode)
        {
            JoinPartyRequest request = new JoinPartyRequest();
            if (_client != null && _client.State == CommunicationState.Opened)
            {
                try
                {
                    request = _client.JoinParty(joiningPlayerID, lobbyCode);
                }
                catch (CommunicationException)
                {
                    CloseProxy();
                    request = (JoinPartyRequest)GenerateServerUnavaibleRequest();
                }
                catch (TimeoutException)
                {
                    CloseProxy();
                    request = (JoinPartyRequest)GenerateServerTimeoutRequest();
                }
            }
            return request;
        }

        private Request GenerateServerUnavaibleRequest()
        {
            Request request = new Request();
            request.IsSuccess = false;
            request.StatusCode = StatusCode.SERVER_UNAVAIBLE;
            return request;
        }

        private Request GenerateServerTimeoutRequest()
        {
            Request request = new Request();
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
