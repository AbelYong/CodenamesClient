using CodenamesGame.Domain.POCO;
using CodenamesGame.FriendService;
using CodenamesGame.Network.EventArguments;
using System;
using System.ServiceModel;

namespace CodenamesGame.Network
{

    /// <summary>
    /// Implements the WCF callback interface (IFriendManagerCallback)
    /// and exposes callbacks as static C# events to
    /// decouple the network layer from the ViewModels.
    /// </summary>
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
            OnNewFriendRequest?.Invoke(this,
                new PlayerEventArgs { Player = PlayerDM.AssemblePlayer(fromPlayer) });
        }

        public void NotifyFriendRequestAccepted(Player byPlayer)
        {
            OnFriendRequestAccepted?.Invoke(this,
                new PlayerEventArgs { Player = PlayerDM.AssemblePlayer(byPlayer) });
        }

        public void NotifyFriendRequestRejected(Player byPlayer)
        {
            OnFriendRequestRejected?.Invoke(this,
                new PlayerEventArgs { Player = PlayerDM.AssemblePlayer(byPlayer) });
        }

        public void NotifyFriendRemoved(Player byPlayer)
        {
            OnFriendRemoved?.Invoke(this,
                new PlayerEventArgs { Player = PlayerDM.AssemblePlayer(byPlayer) });
        }

        public void NotifyOperationSuccess(string message)
        {
            OnOperationSuccess?.Invoke(this,
                new OperationMessageEventArgs { Message = message });
        }

        public void NotifyOperationFailure(string message)
        {
            OnOperationFailure?.Invoke(this,
                new OperationMessageEventArgs { Message = message });
        }

        /// <summary>
        /// Safely invokes the failure event from outside the class.
        /// Used to report transport errors in the network layer.
        /// </summary>
        public static void RaiseOperationFailure(string message)
        {
            OnOperationFailure?.Invoke(null, new OperationMessageEventArgs { Message = message });
        }
    }
}