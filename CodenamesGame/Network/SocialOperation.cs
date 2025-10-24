using CodenamesGame.Domain.POCO;
using CodenamesGame.FriendService;
using CodenamesGame.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CodenamesGame.Network
{
    public static class SocialOperation
    {
        private const string _ENDPOINT_NAME = "NetTcpBinding_IFriendManager";
        public static List<PlayerDM> SearchPlayers(Guid mePlayerId, string query, int limit = 20)
        {
            var client = new FriendManagerClient(_ENDPOINT_NAME);
            try
            {
                var list = client.SearchPlayers(query ?? "", mePlayerId, limit);
                return list?.Select(PlayerDM.AssemblePlayer).ToList() ?? new List<PlayerDM>();
            }
            finally
            { 
                NetworkUtil.SafeClose(client); 
            }
        }

        public static List<PlayerDM> GetFriends(Guid mePlayerId)
        {
            var client = new FriendManagerClient(_ENDPOINT_NAME);
            try
            {
                var list = client.GetFriends(mePlayerId);
                return list?.Select(PlayerDM.AssemblePlayer).ToList() ?? new List<PlayerDM>();
            }
            finally
            { 
                NetworkUtil.SafeClose(client);
            }
        }

        public static List<PlayerDM> GetIncomingRequests(Guid mePlayerId)
        {
            var client = new FriendManagerClient(_ENDPOINT_NAME);
            try
            {
                var list = client.GetIncomingRequests(mePlayerId);
                return list?.Select(PlayerDM.AssemblePlayer).ToList() ?? new List<PlayerDM>();
            }
            finally 
            { 
                NetworkUtil.SafeClose(client);
            }
        }

        public static (bool ok, string msg) SendFriendRequest(Guid fromPlayerId, Guid toPlayerId)
        {
            var client = new FriendManagerClient(_ENDPOINT_NAME);
            try
            {
                var r = client.SendFriendRequest(fromPlayerId, toPlayerId);
                return (r.Success, r.Message);
            }
            finally 
            { 
                NetworkUtil.SafeClose(client);
            }
        }

        public static (bool ok, string msg) AcceptFriendRequest(Guid mePlayerId, Guid requesterPlayerId)
        {
            var client = new FriendManagerClient(_ENDPOINT_NAME);
            try
            {
                var r = client.AcceptFriendRequest(mePlayerId, requesterPlayerId);
                return (r.Success, r.Message);
            }
            finally
            { 
                NetworkUtil.SafeClose(client); 
            }
        }

        public static (bool ok, string msg) RejectFriendRequest(Guid mePlayerId, Guid requesterPlayerId)
        {
            var client = new FriendManagerClient(_ENDPOINT_NAME);
            try
            {
                var r = client.RejectFriendRequest(mePlayerId, requesterPlayerId);
                return (r.Success, r.Message);
            }
            finally
            {
                NetworkUtil.SafeClose(client); 
            }
        }

        public static (bool ok, string msg) RemoveFriend(Guid mePlayerId, Guid friendPlayerId)
        {
            var client = new FriendManagerClient(_ENDPOINT_NAME);
            try
            {
                var r = client.RemoveFriend(mePlayerId, friendPlayerId);
                return (r.Success, r.Message);
            }
            finally
            {
                NetworkUtil.SafeClose(client);
            }
        }
    }
}