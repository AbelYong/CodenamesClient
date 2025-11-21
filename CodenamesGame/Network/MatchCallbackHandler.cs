using CodenamesGame.MatchService;
using CodenamesGame.Network.EventArguments;
using System;

namespace CodenamesGame.Network
{
    public class MatchCallbackHandler : IMatchManagerCallback
    {
        public static event Action OnCompanionDisconnect;
        public static event EventHandler<string> OnClueReceived;
        public static event Action OnTurnChange;
        public static event Action OnRolesChanged;
        public static event EventHandler<int> OnAgentPicked;
        public static event EventHandler<BystanderPickedEventArgs> OnBystanderPicked;
        public static event EventHandler<string> OnAssassinPicked;
        public static event EventHandler<string> OnMatchTimeout;
        public static event EventHandler<string> OnMatchWon;
        public static event Action OnScoreNotSaved;

        public MatchCallbackHandler()
        {

        }

        public void NotifyCompanionDisconnect()
        {
            OnCompanionDisconnect?.Invoke();
        }

        public void NotifyClueReceived(string clue)
        {
            OnClueReceived?.Invoke(null, clue);
        }

        public void NotifyTurnChange()
        {
            OnTurnChange?.Invoke();
        }

        public void NotifyRolesChanged()
        {
            OnRolesChanged?.Invoke();
        }

        public void NotifyAgentPicked(int newTurnLength)
        {
            OnAgentPicked?.Invoke(null, newTurnLength);
        }

        public void NotifyBystanderPicked(TokenType tokenToUpdate, int remainingTokens)
        {
            OnBystanderPicked?.Invoke(null,
                new BystanderPickedEventArgs { TokenToUpdate = tokenToUpdate, RemainingTokens = remainingTokens });
        }

        public void NotifyAssassinPicked(string finalMatchLength)
        {
            OnAssassinPicked?.Invoke(null, finalMatchLength);
        }

        public void NotifyMatchTimeout(string finalMatchLength)
        {
            OnMatchTimeout?.Invoke(null, finalMatchLength);
        }

        public void NotifyMatchWon(string finalMatchLength)
        {
            OnMatchWon?.Invoke(null, finalMatchLength);
        }

        public void NotifyStatsCouldNotBeSaved()
        {
            OnScoreNotSaved?.Invoke();
        }
    }
}
