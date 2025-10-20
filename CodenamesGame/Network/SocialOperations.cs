using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using CodenamesGame.FriendService;
using CodenamesGame.Domain.POCO;

namespace CodenamesGame.Network
{
    public static class SocialOperations
    {
        public static List<PlayerPOCO> SearchPlayers(Guid mePlayerId, string query, int limit = 20)
        {
            var client = new FriendManagerClient("NetTcpBinding_IFriendManager");
            try
            {
                var list = client.SearchPlayers(query ?? "", mePlayerId, limit);
                return list?.Select(PlayerPOCO.AssemblePlayer).ToList() ?? new List<PlayerPOCO>();
            }
            finally { SafeClose(client); }
        }

        public static List<PlayerPOCO> GetFriends(Guid mePlayerId)
        {
            var client = new FriendManagerClient("NetTcpBinding_IFriendManager");
            try
            {
                var list = client.GetFriends(mePlayerId);
                return list?.Select(PlayerPOCO.AssemblePlayer).ToList() ?? new List<PlayerPOCO>();
            }
            finally { SafeClose(client); }
        }

        public static List<PlayerPOCO> GetIncomingRequests(Guid mePlayerId)
        {
            var client = new FriendManagerClient("NetTcpBinding_IFriendManager");
            try
            {
                var list = client.GetIncomingRequests(mePlayerId);
                return list?.Select(PlayerPOCO.AssemblePlayer).ToList() ?? new List<PlayerPOCO>();
            }
            finally { SafeClose(client); }
        }

        public static (bool ok, string msg) SendFriendRequest(Guid fromPlayerId, Guid toPlayerId)
        {
            var client = new FriendManagerClient("NetTcpBinding_IFriendManager");
            try
            {
                var r = client.SendFriendRequest(fromPlayerId, toPlayerId);
                return (r.Success, r.Message);
            }
            finally { SafeClose(client); }
        }

        public static (bool ok, string msg) AcceptFriendRequest(Guid mePlayerId, Guid requesterPlayerId)
        {
            var client = new FriendManagerClient("NetTcpBinding_IFriendManager");
            try
            {
                var r = client.AcceptFriendRequest(mePlayerId, requesterPlayerId);
                return (r.Success, r.Message);
            }
            finally { SafeClose(client); }
        }

        public static (bool ok, string msg) RejectFriendRequest(Guid mePlayerId, Guid requesterPlayerId)
        {
            var client = new FriendManagerClient("NetTcpBinding_IFriendManager");
            try
            {
                var r = client.RejectFriendRequest(mePlayerId, requesterPlayerId);
                return (r.Success, r.Message);
            }
            finally { SafeClose(client); }
        }

        public static (bool ok, string msg) RemoveFriend(Guid mePlayerId, Guid friendPlayerId)
        {
            var client = new FriendManagerClient("NetTcpBinding_IFriendManager");
            try
            {
                var r = client.RemoveFriend(mePlayerId, friendPlayerId);
                return (r.Success, r.Message);
            }
            finally { SafeClose(client); }
        }

        private static void SafeClose(ICommunicationObject client)
        {
            try
            {
                if (client.State == CommunicationState.Faulted) client.Abort();
                else client.Close();
            }
            catch { client.Abort(); }
        }
    }
}