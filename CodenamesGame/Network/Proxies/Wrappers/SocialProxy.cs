using CodenamesGame.Domain.POCO;
using CodenamesGame.FriendService;
using CodenamesGame.Network.Proxies.CallbackHandlers;
using CodenamesGame.Network.Proxies.Interfaces;
using CodenamesGame.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;

namespace CodenamesGame.Network.Proxies.Wrappers
{
    public class SocialProxy : ISocialProxy
    {
        private const string _ENDPOINT_NAME = "NetTcpBinding_IFriendManager";
        private static readonly Lazy<SocialProxy> _instance = new Lazy<SocialProxy>(() => new SocialProxy());
        private FriendManagerClient _client;
        private Guid _currentPlayerId;

        public static SocialProxy Instance
        {
            get => _instance.Value;
        }

        private SocialProxy()
        {

        }

        public void Initialize(Guid mePlayerId)
        {
            FriendCallbackHandler callbackHandler;
            if (_client != null && _client.State == CommunicationState.Opened)
            {
                return;
            }

            if (mePlayerId == Guid.Empty)
            {
                throw new ArgumentException("PlayerId cannot be empty for duplex connection.", nameof(mePlayerId));
            }

            _currentPlayerId = mePlayerId;
            callbackHandler = new FriendCallbackHandler();
            var context = new InstanceContext(callbackHandler);
            _client = new FriendManagerClient(context, _ENDPOINT_NAME);

            try
            {
                _client.Open();
                _client.Connect(_currentPlayerId);
            }
            catch (Exception ex) //fixme: Catch Exception should be last resort!!!! See the rest of proxies
            {
                NetworkUtil.SafeClose(_client);
                _client = null;
                throw new CommunicationException($"Failed to connect to FriendService: {ex.Message}", ex);
            }
        }

        public void Disconnect()
        {
            if (_client != null && _client.State == CommunicationState.Opened)
            {
                try
                {
                    _client.Disconnect(_currentPlayerId);
                }
                catch (Exception) //fixme: Catch Exception should be last resort!!!! See the rest of proxies
                {
                    //fixme: do something???
                }
                finally
                {
                    NetworkUtil.SafeClose(_client);
                    _client = null;
                    _currentPlayerId = Guid.Empty;
                }
            }
        }

        public List<PlayerDM> SearchPlayers(string query)
        {
            int limit = 20;
            try
            {
                var list = GetClient().SearchPlayers(query ?? "", _currentPlayerId, limit);
                return list?.Select(PlayerDM.AssemblePlayer).ToList() ?? new List<PlayerDM>();
            }
            catch (Exception ex) //fixme: Catch Exception should be last resort!!!! See the rest of proxies
            {
                try
                {
                    Initialize(_currentPlayerId);
                    var list = GetClient().SearchPlayers(query ?? "", _currentPlayerId, limit);
                    return list?.Select(PlayerDM.AssemblePlayer).ToList() ?? new List<PlayerDM>();
                }
                catch
                {
                    OnOperationFailure(ex);
                    return new List<PlayerDM>();
                }
            }
        }

        public List<PlayerDM> GetFriends()
        {
            try
            {
                var list = GetClient().GetFriends(_currentPlayerId);
                return list?.Select(PlayerDM.AssemblePlayer).ToList() ?? new List<PlayerDM>();
            }
            catch (Exception ex)
            {
                OnOperationFailure(ex);
                return new List<PlayerDM>();
            }
        }

        public List<PlayerDM> GetIncomingRequests()
        {
            try
            {
                var list = GetClient().GetIncomingRequests(_currentPlayerId);
                return list?.Select(PlayerDM.AssemblePlayer).ToList() ?? new List<PlayerDM>();
            }
            catch (Exception ex) //fixme: Catch Exception should be last resort!!!! See the rest of proxies
            {
                OnOperationFailure(ex);
                return new List<PlayerDM>();
            }
        }

        public List<PlayerDM> GetSentRequests()
        {
            try
            {
                var list = GetClient().GetSentRequests(_currentPlayerId);

                return list?.Select(PlayerDM.AssemblePlayer).ToList() ?? new List<PlayerDM>();
            }
            catch (Exception ex) //fixme: Catch Exception should be last resort!!!! See the rest of proxies
            {
                OnOperationFailure(ex);
                return new List<PlayerDM>();
            }
        }

        public FriendshipRequest SendFriendRequest(Guid toPlayerId)
        {
            try
            {
                return GetClient().SendFriendRequest(_currentPlayerId, toPlayerId);
            }
            catch (Exception ex) //fixme: Catch Exception should be last resort!!!! See the rest of proxies
            {
                OnOperationFailure(ex);
                return new FriendshipRequest
                {
                    IsSuccess = false,
                    StatusCode = StatusCode.SERVER_ERROR
                };
            }
        }

        public FriendshipRequest AcceptFriendRequest(Guid requesterPlayerId)
        {
            try
            {
                return GetClient().AcceptFriendRequest(_currentPlayerId, requesterPlayerId);
            }
            catch (Exception ex) //fixme: Catch Exception should be last resort!!!! See the rest of proxies
            {
                OnOperationFailure(ex);
                return new FriendshipRequest
                {
                    IsSuccess = false,
                    StatusCode = StatusCode.SERVER_ERROR
                };
            }
        }

        public FriendshipRequest RejectFriendRequest(Guid requesterPlayerId)
        {
            try
            {
                return GetClient().RejectFriendRequest(_currentPlayerId, requesterPlayerId);
            }
            catch (Exception ex) //fixme: Catch Exception should be last resort!!!! See the rest of proxies
            {
                OnOperationFailure(ex);
                return new FriendshipRequest
                {
                    IsSuccess = false,
                    StatusCode = StatusCode.SERVER_ERROR
                };
            }
        }

        public FriendshipRequest RemoveFriend(Guid friendPlayerId)
        {
            try
            {
                return GetClient().RemoveFriend(_currentPlayerId, friendPlayerId);
            }
            catch (Exception ex) //fixme: Catch Exception should be last resort!!!! See the rest of proxies
            {
                OnOperationFailure(ex);
                return new FriendshipRequest
                {
                    IsSuccess = false,
                    StatusCode = StatusCode.SERVER_ERROR
                };
            }
        }

        private FriendManagerClient GetClient()
        {
            if (_client == null ||
                _client.State == CommunicationState.Closed ||
                _client.State == CommunicationState.Faulted)
            {
                Initialize(_currentPlayerId);
            }

            return _client;
        }

        private static void OnOperationFailure(Exception ex)
        {
            //fixme: USE REQUEST!!!! Translate! Never send the exception directly to the user!!
            FriendCallbackHandler.RaiseOperationFailure($"Error de conexión: {ex.Message}");
        }
    }
}
