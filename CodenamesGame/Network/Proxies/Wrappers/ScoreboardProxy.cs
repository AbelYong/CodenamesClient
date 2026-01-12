using CodenamesGame.Domain.POCO;
using CodenamesGame.Network.Proxies.CallbackHandlers;
using CodenamesGame.Network.Proxies.Interfaces;
using CodenamesGame.ScoreboardService;
using CodenamesGame.Util;
using System;
using System.ServiceModel;

namespace CodenamesGame.Network.Proxies.Wrappers
{
    public class ScoreboardProxy : IScoreboardProxy
    {
        public delegate IScoreboardManager ScoreboardClientFactory(InstanceContext context, string endpointName);
        private readonly ScoreboardClientFactory _clientFactory;
        private static readonly Lazy<ScoreboardProxy> _instance = new Lazy<ScoreboardProxy>(() => new ScoreboardProxy());
        private const string _ENDPOINT_NAME = "NetTcpBinding_IScoreboardManager";
        private IScoreboardManager _client;
        private Guid _currentPlayerID;

        public static ScoreboardProxy Instance
        {
            get => _instance.Value;
        }

        private ScoreboardProxy() : this((context, endpointName) =>
        {
            var factory = new DuplexChannelFactory<IScoreboardManager>(context, endpointName);
            return factory.CreateChannel();
        })
        {

        }

        public ScoreboardProxy(ScoreboardClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public void Initialize(Guid playerID)
        {
            _currentPlayerID = playerID;
            ScoreboardCallbackHandler callbackHandler = new ScoreboardCallbackHandler();
            InstanceContext context = new InstanceContext(callbackHandler);
            _client = _clientFactory(context, _ENDPOINT_NAME);
            try
            {
                ((ICommunicationObject)_client).Open();
                _client.SubscribeToScoreboardUpdates(playerID);
            }
            catch (Exception ex) when (ex is CommunicationException || ex is TimeoutException)
            {
                CloseProxy();
            }
            catch (Exception ex)
            {
                CodenamesGameLogger.Log.Error("Unexpected exception while connecting to ScoreboardService", ex);
                CloseProxy();
            }
        }

        public void Disconnect()
        {
            if (VerifyClientOpen())
            {
                try
                {
                    _client.UnsubscribeFromScoreboardUpdates(_currentPlayerID);
                    CloseProxy();
                }
                catch (Exception ex) when (ex is CommunicationException || ex is TimeoutException)
                {
                    CloseProxy();
                }
                catch (Exception ex)
                {
                    CloseProxy();
                    CodenamesGameLogger.Log.Error("Unexpected exception while disconnecting from ScoreboardService", ex);
                }
            }
        }

        public ScoreboardDM GetMyScore(Guid playerID)
        {
            TryReconnect();
            if (VerifyClientOpen())
            {
                try
                {
                    var dto = _client.GetMyScore(playerID);
                    if (dto != null)
                    {
                        return new ScoreboardDM
                        {
                            Username = dto.Username,
                            GamesWon = dto.GamesWon,
                            FastestMatch = dto.FastestMatch,
                            AssassinsRevealed = dto.AssassinsRevealed
                        };
                    }
                }
                catch (Exception ex) when (ex is CommunicationException || ex is TimeoutException) 
                {
                    CloseProxy();
                }
                catch (Exception ex)
                {
                    CloseProxy();
                    CodenamesGameLogger.Log.Error("Unexpected exception getting personal score: ", ex);
                }
            }
            return null;
        }

        private bool VerifyClientOpen()
        {
            return _client != null && ((ICommunicationObject)_client).State == CommunicationState.Opened;
        }

        private void TryReconnect()
        {
            if (_client == null || ((ICommunicationObject)_client).State != CommunicationState.Opened)
            {
                Initialize(_currentPlayerID);
            }
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