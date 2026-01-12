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
        FriendListRequest SearchPlayers(string query);
        FriendListRequest GetFriends();
        FriendListRequest GetIncomingRequests();
        FriendListRequest GetSentRequests();
        FriendshipRequest SendFriendRequest(Guid toPlayerId);
        FriendshipRequest AcceptFriendRequest(Guid requesterPlayerId);
        FriendshipRequest RejectFriendRequest(Guid requesterPlayerId);
        FriendshipRequest RemoveFriend(Guid friendPlayerId);
    }
}
