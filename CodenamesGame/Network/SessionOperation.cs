using CodenamesGame.Domain.POCO;
using CodenamesGame.Network.EventArguments;
using CodenamesGame.SessionService;
using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;

namespace CodenamesGame.Network
{
    public class SessionOperation
    {
        private const string _ENDPOINT_NAME = "NetTcpBinding_ISessionManager";
        private static readonly Lazy<SessionOperation> _instance = new Lazy<SessionOperation>(() => new SessionOperation());
        private SessionManagerClient _client;
        private PlayerDM _player;

        public static SessionOperation Instance
        {
            get => _instance.Value;
        }

        private SessionOperation()
        {
            
        }

        public CommunicationRequest Initialize(PlayerDM player)
        {
            CommunicationRequest request = new CommunicationRequest();
            if (_client != null && _client.State == CommunicationState.Opened)
            {
                request.IsSuccess = false;
                request.StatusCode = StatusCode.UNAUTHORIZED;
            }

            SessionCallbackHandler callbackHandler = new SessionCallbackHandler();
            InstanceContext context = new InstanceContext(callbackHandler);
            _client = new SessionManagerClient(context, _ENDPOINT_NAME);
            
            if (player != null && player.PlayerID.HasValue)
            {
                _player = player;
                Player auxPlayer = PlayerDM.AssembleSessionSvPlayer(player);

                try
                {
                    _client.Open();
                    return _client.Connect(auxPlayer);
                }
                catch (CommunicationException)
                {
                    request.StatusCode = StatusCode.SERVER_UNAVAIBLE;
                    request.IsSuccess = false;
                    CloseProxy();
                }
                catch (TimeoutException)
                {
                    request.StatusCode = StatusCode.SERVER_TIMEOUT;
                    request.IsSuccess = false;
                    CloseProxy();
                }
            }
            else
            {
                request.IsSuccess = false;
                request.StatusCode = StatusCode.MISSING_DATA;
            }
            return request;
        }

        public void Disconnect()
        {
            if (_client != null && _client.State == CommunicationState.Opened)
            {
                Player svPlayer = PlayerDM.AssembleSessionSvPlayer(_player);
                try
                {
                    _client.DisconnectAsync(svPlayer);
                }
                catch (EndpointNotFoundException)
                {
                    CloseProxy();
                }
                catch (CommunicationObjectFaultedException)
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