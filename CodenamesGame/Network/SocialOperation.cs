using CodenamesGame.Domain.POCO;
using CodenamesGame.FriendService;
using CodenamesGame.Network.Proxies.Interfaces;
using CodenamesGame.Network.Proxies.Wrappers;
using System;
using System.Collections.Generic;

namespace CodenamesGame.Network
{
    public class SocialOperation
    {
        private readonly ISocialProxy _proxy;

        public SocialOperation() : this (SocialProxy.Instance) { }

        public SocialOperation(ISocialProxy proxy)
        {
            _proxy = proxy;
        }

        public void Initialize(Guid mePlayerId)
        {
            _proxy.Initialize(mePlayerId);
        }

        public void Disconnect()
        {
            _proxy.Disconnect();
        }

        public FriendListRequest SearchPlayers(string query)
        {
            return _proxy.SearchPlayers(query);
        }

        public FriendListRequest GetFriends()
        {
            return _proxy.GetFriends();
        }

        public FriendListRequest GetIncomingRequests()
        {
            return _proxy.GetIncomingRequests();
        }

        public FriendListRequest GetSentRequests()
        {
            return _proxy.GetSentRequests();
        }

        public FriendshipRequest SendFriendRequest(Guid toPlayerId)
        {
            return _proxy.SendFriendRequest(toPlayerId);
        }

        public FriendshipRequest AcceptFriendRequest(Guid requesterPlayerId)
        {
            return _proxy.AcceptFriendRequest(requesterPlayerId);
        }

        public FriendshipRequest RejectFriendRequest(Guid requesterPlayerId)
        {
            return _proxy.RejectFriendRequest(requesterPlayerId);
        }

        public FriendshipRequest RemoveFriend(Guid friendPlayerId)
        {
            return _proxy.RemoveFriend(friendPlayerId);
        }
    }
}