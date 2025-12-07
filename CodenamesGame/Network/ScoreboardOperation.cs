using CodenamesGame.Domain.POCO;
using CodenamesGame.Network.Proxies.CallbackHandlers;
using CodenamesGame.Network.Proxies.Wrappers;
using CodenamesGame.Util;
using System;
using System.ServiceModel;

namespace CodenamesGame.Network
{
    public class ScoreboardOperation
    {
        private ScoreboardProxy _proxy;
        private ScoreboardCallbackHandler _callbackHandler;
        private Guid _currentPlayerID;

        public void Initialize(Guid playerID)
        {
            _currentPlayerID = playerID;
            _callbackHandler = new ScoreboardCallbackHandler();
            _proxy = new ScoreboardProxy(_callbackHandler);

            try
            {
                _proxy.SubscribeToScoreboardUpdates(_currentPlayerID);
            }
            catch (CommunicationException ex)
            {
                CodenamesGameLogger.Log.Error("Error connecting to ScoreboardService", ex);
            }
        }

        public void Disconnect()
        {
            if (_proxy != null && _proxy.State == CommunicationState.Opened)
            {
                try
                {
                    _proxy.UnsubscribeFromScoreboardUpdates(_currentPlayerID);
                    _proxy.Close();
                }
                catch (CommunicationException)
                {
                    _proxy.Abort();
                }
            }
        }

        public ScoreboardDM GetMyScore(Guid playerID)
        {
            if (_proxy != null && _proxy.State == CommunicationState.Opened)
            {
                try
                {
                    var dto = _proxy.GetMyScore(playerID);
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
                catch (CommunicationException ex)
                {
                    CodenamesGameLogger.Log.Error("Error getting my score", ex);
                }
            }
            return null;
        }
    }
}