using CodenamesGame.Domain.POCO.Match;
using CodenamesGame.MatchService;
using CodenamesGame.Network.EventArguments;
using System;
using System.ServiceModel;

namespace CodenamesGame.Network.Proxies.CallbackHandlers
{
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    public class MatchCallbackHandler : IMatchManagerCallback
    {
        public static event Action OnCompanionDisconnect;
        public static event EventHandler<string> OnClueReceived;
        public static event Action OnTurnChange;
        public static event Action OnRolesChanged;
        public static event EventHandler<int> OnGuesserTurnTimeout;
        public static event EventHandler<AgentPickedEventArgs> OnAgentPicked;
        public static event EventHandler<BystanderPickedEventArgs> OnBystanderPicked;
        public static event EventHandler<AssassinPickedEventArgs> OnAssassinPicked;
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

        public void NotifyGuesserTurnTimeout(int timerTokens)
        {
            OnGuesserTurnTimeout?.Invoke(null, timerTokens);
        }

        public void NotifyRolesChanged()
        {
            OnRolesChanged?.Invoke();
        }

        public void NotifyAgentPicked(AgentPickedNotification notification)
        {
            OnAgentPicked?.Invoke(null,
                new AgentPickedEventArgs
                {
                    Coordinates = BoardCoordinatesDM.AssembleBoardCoordinates(notification.Coordinates),
                    NewTurnLength = notification.NewTurnLength
                });
        }

        public void NotifyBystanderPicked(BystanderPickedNotification notification)
        {
            OnBystanderPicked?.Invoke(null,
                new BystanderPickedEventArgs
                {
                    Coordinates = BoardCoordinatesDM.AssembleBoardCoordinates(notification.Coordinates),
                    TokenToUpdate = notification.TokenToUpdate,
                    RemainingTokens = notification.RemainingTokens
                });
        }

        public void NotifyAssassinPicked(AssassinPickedNotification notification)
        {
            OnAssassinPicked?.Invoke(null,
                new AssassinPickedEventArgs
                {
                    Coordinates = BoardCoordinatesDM.AssembleBoardCoordinates(notification.Coordinates),
                    FinalMatchLength = notification.FinalMatchLength
                });
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
