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
        private static readonly Lazy<ScoreboardProxy> _instance = new Lazy<ScoreboardProxy>(() => new ScoreboardProxy());
        private const string _ENDPOINT_NAME = "NetTcpBinding_IScoreboardManager";
        private ScoreboardManagerClient _client;
        private Guid _currentPlayerID;

        public static ScoreboardProxy Instance
        {
            get => _instance.Value;
        }

        private ScoreboardProxy()
        {

        }

        public void Initialize(Guid playerID)
        {
            _currentPlayerID = playerID;
            ScoreboardCallbackHandler callbackHandler = new ScoreboardCallbackHandler();
            InstanceContext context = new InstanceContext(callbackHandler);
            _client = new ScoreboardManagerClient(context, _ENDPOINT_NAME);
            try
            {
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
            if (_client != null && _client.State == CommunicationState.Opened)
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
            if (_client != null && _client.State == CommunicationState.Opened)
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
                    CodenamesGameLogger.Log.Error("Unexpected exception getting personal score: ", ex);
                    CloseProxy();
                }
            }
            return null;
        }

        private void TryReconnect()
        {
            if (_client == null || _client.State != CommunicationState.Opened)
            {
                Initialize(_currentPlayerID);
            }
        }

        private void CloseProxy()
        {
            Util.NetworkUtil.SafeClose(_client);
            _client = null;
        }
    }
}