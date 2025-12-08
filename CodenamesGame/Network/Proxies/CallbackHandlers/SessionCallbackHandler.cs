using CodenamesGame.Domain.POCO;
using CodenamesGame.Network.EventArguments;
using CodenamesGame.SessionService;
using System;
using System.Collections.Generic;

namespace CodenamesGame.Network.Proxies.CallbackHandlers
{
    public class SessionCallbackHandler : ISessionManagerCallback
    {
        public static List<PlayerDM> _onlineFriends { get; set; }
        public static event EventHandler<PlayerEventArgs> OnFriendOnline;
        public static event EventHandler<Guid> OnFriendOffline;
        public static event EventHandler<List<PlayerDM>> OnOnlineFriendsReceived;
        public static event EventHandler<KickReason> OnKicked;
        
        public SessionCallbackHandler()
        {
            _onlineFriends = new List<PlayerDM>();
        }

        public static List<PlayerDM> GetOnlineFriendsList()
        {
            return _onlineFriends;
        }

        public void NotifyFriendOffline(Guid playerId)
        {
            Guid? auxFriendId = playerId;
            _onlineFriends.RemoveAll((friend) => friend.PlayerID == auxFriendId);

            OnFriendOffline?.Invoke(null, playerId);
        }

        public void NotifyFriendOnline(Player player)
        {
            PlayerDM auxFriend = PlayerDM.AssemblePlayer(player);
            if (auxFriend != null)
            {
                _onlineFriends.Add(auxFriend);

                OnFriendOnline?.Invoke(null, new PlayerEventArgs { Player = auxFriend });
            }
        }

        public void ReceiveOnlineFriends(Player[] friends)
        {
            List<PlayerDM> auxFriends = new List<PlayerDM>();
            foreach (Player friend in friends)
            {
                PlayerDM auxFriend = PlayerDM.AssemblePlayer(friend);
                if (auxFriend != null)
                {
                    auxFriends.Add(auxFriend);
                }
            }
            _onlineFriends = auxFriends;

            OnOnlineFriendsReceived?.Invoke(null, _onlineFriends);
        }

        public void NotifyKicked(KickReason reason)
        {
            OnKicked?.Invoke(null, reason);
        }
    }
}
