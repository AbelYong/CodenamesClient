using CodenamesGame.Domain.POCO;
using CodenamesGame.FriendService;
using CodenamesGame.Network.EventArguments;
using System;
using System.ServiceModel;

namespace CodenamesGame.Network.Proxies.CallbackHandlers
{
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class FriendCallbackHandler : IFriendManagerCallback
    {
        public static event EventHandler<PlayerEventArgs> OnNewFriendRequest;
        public static event EventHandler<PlayerEventArgs> OnFriendRequestAccepted;
        public static event EventHandler<PlayerEventArgs> OnFriendRequestRejected;
        public static event EventHandler<PlayerEventArgs> OnFriendRemoved;

        public static event EventHandler<OperationMessageEventArgs> OnOperationSuccess;
        public static event EventHandler<OperationMessageEventArgs> OnOperationFailure;

        public void NotifyNewFriendRequest(Player fromPlayer)
        {
            OnNewFriendRequest?.Invoke(null,
                new PlayerEventArgs { Player = PlayerDM.AssemblePlayer(fromPlayer) });
        }

        public void NotifyFriendRequestAccepted(Player byPlayer)
        {
            OnFriendRequestAccepted?.Invoke(null,
                new PlayerEventArgs { Player = PlayerDM.AssemblePlayer(byPlayer) });
        }

        public void NotifyFriendRequestRejected(Player byPlayer)
        {
            OnFriendRequestRejected?.Invoke(null,
                new PlayerEventArgs { Player = PlayerDM.AssemblePlayer(byPlayer) });
        }

        public void NotifyFriendRemoved(Player byPlayer)
        {
            OnFriendRemoved?.Invoke(null,
                new PlayerEventArgs { Player = PlayerDM.AssemblePlayer(byPlayer) });
        }

        public void NotifyOperationSuccess(string message)
        {
            OnOperationSuccess?.Invoke(null,
                new OperationMessageEventArgs { Message = message });
        }

        public void NotifyOperationFailure(string message)
        {
            OnOperationFailure?.Invoke(null,
                new OperationMessageEventArgs { Message = message });
        }
    }
}