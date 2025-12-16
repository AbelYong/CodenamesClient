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
        private static readonly Lazy<SessionProxy> _instance = new Lazy<SessionProxy>(() => new SessionProxy());
        private const string _ENDPOINT_NAME = "NetTcpBinding_ISessionManager";
        private SessionManagerClient _client;
        private PlayerDM _player;
        public event EventHandler ConnectionLost;

        public static SessionProxy Instance
        {
            get => _instance.Value;
        }

        private SessionProxy()
        {

        }

        public CommunicationRequest Initialize(PlayerDM player)
        {
            CommunicationRequest request = new CommunicationRequest();
            if (_client != null && _client.State == CommunicationState.Opened)
            {
                request.IsSuccess = true;
                request.StatusCode = StatusCode.UNAUTHORIZED; //Player is already connected to the service
            }

            SessionCallbackHandler callbackHandler = new SessionCallbackHandler();
            InstanceContext context = new InstanceContext(callbackHandler);
            _client = new SessionManagerClient(context, _ENDPOINT_NAME);
            _client.InnerChannel.Faulted += OnChannelFaulted;

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
                _client.Open();
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
            if (_client != null && _client.State == CommunicationState.Opened)
            {
                Player svPlayer = PlayerDM.AssembleSessionSvPlayer(_player);
                try
                {
                    _client.DisconnectAsync(svPlayer);
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
            Util.NetworkUtil.SafeClose(_client);
            _client = null;
        }
    }
}
