using CodenamesGame.Domain.POCO;
using CodenamesGame.Network.Proxies.CallbackHandlers;
using CodenamesGame.Network.Proxies.Interfaces;
using CodenamesGame.SessionService;
using CodenamesGame.Util;
using System;
using System.ServiceModel;

namespace CodenamesGame.Network.Proxies.Wrappers
{
    public class SessionProxy : ISessionProxy
    {
        public delegate ISessionManager SessionClientFactory(InstanceContext context, string endpointName);
        private readonly SessionClientFactory _clientFactory;
        private static readonly Lazy<SessionProxy> _instance = new Lazy<SessionProxy>(() => new SessionProxy());
        private const string _ENDPOINT_NAME = "NetTcpBinding_ISessionManager";
        private ISessionManager _client;
        private PlayerDM _player;
        public event EventHandler ConnectionLost;

        public static SessionProxy Instance
        {
            get => _instance.Value;
        }

        private SessionProxy() : this((context, endpoint) =>
        {
            var factory = new DuplexChannelFactory<ISessionManager>(context, endpoint);
            return factory.CreateChannel();
        })
        {

        }

        public SessionProxy(SessionClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public CommunicationRequest Initialize(PlayerDM player)
        {
            CommunicationRequest request = new CommunicationRequest();
            if (_client != null && ((ICommunicationObject)_client).State == CommunicationState.Opened)
            {
                request.IsSuccess = true;
                request.StatusCode = StatusCode.UNAUTHORIZED; //Player is already connected to the service
                return request;
            }

            SessionCallbackHandler callbackHandler = new SessionCallbackHandler();
            InstanceContext context = new InstanceContext(callbackHandler);
            _client = _clientFactory(context, _ENDPOINT_NAME);
            ((ICommunicationObject)_client).Faulted += OnChannelFaulted;

            if (player != null && player.PlayerID.HasValue)
            {
                _player = player;
                Player auxPlayer = PlayerDM.AssembleSessionSvPlayer(player);

                return Connect(auxPlayer);
            }
            else
            {
                request.IsSuccess = false;
                request.StatusCode = StatusCode.MISSING_DATA;
            }
            return request;
        }

        private CommunicationRequest Connect(Player player)
        {
            CommunicationRequest request = new CommunicationRequest();
            try
            {
                ((ICommunicationObject)_client).Open();
                return _client.Connect(player);
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
                request.StatusCode = StatusCode.SERVER_UNREACHABLE;
                CloseProxy();
            }
            catch (Exception ex)
            {
                CodenamesGameLogger.Log.Error("Unexpected exception while connecting to Session Service:", ex);
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
                Player svPlayer = PlayerDM.AssembleSessionSvPlayer(_player);
                try
                {
                    _client.DisconnectAsync(svPlayer);
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
                    CodenamesGameLogger.Log.Error("Unexpected exception while disconnecting from Session Service: ", ex);
                    CloseProxy();
                }
            }
        }
        private void OnChannelFaulted(object sender, EventArgs e)
        {
            CloseProxy();
            ConnectionLost?.Invoke(this, EventArgs.Empty);
        }

        private void CloseProxy()
        {
            if (_client is ICommunicationObject commObject)
            {
                NetworkUtil.SafeClose(commObject);
            }
            _client = null;
        }
    }
}
