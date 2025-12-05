using CodenamesGame.Domain.POCO;
using System;
using System.Collections.Generic;
using CodenamesGame.FriendService;

namespace CodenamesGame.Network.Proxies.Interfaces
{
    public interface ISocialProxy
    {
        void Initialize(Guid mePlayerId);
        void Disconnect();
        List<PlayerDM> SearchPlayers(string query);
        List<PlayerDM> GetFriends();
        List<PlayerDM> GetIncomingRequests();
        List<PlayerDM> GetSentRequests();
        FriendshipRequest SendFriendRequest(Guid toPlayerId);
        FriendshipRequest AcceptFriendRequest(Guid requesterPlayerId);
        FriendshipRequest RejectFriendRequest(Guid requesterPlayerId);
        FriendshipRequest RemoveFriend(Guid friendPlayerId);
    }
}
