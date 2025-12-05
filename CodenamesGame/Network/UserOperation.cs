using CodenamesGame.Domain.POCO;
using System;
using CodenamesGame.UserService;
using CodenamesGame.Network.Proxies.Interfaces;
using CodenamesGame.Network.Proxies.Wrappers;

namespace CodenamesGame.Network
{
    public class UserOperation
    {
        private readonly IUserProxy _proxy;

        public UserOperation() : this (new UserProxy()) { }

        public UserOperation(IUserProxy proxy)
        {
            _proxy = proxy;
        }

        public SignInRequest SignIn(UserDM user, PlayerDM player)
        {
            return _proxy.SignIn(user, player);
        }

        public PlayerDM GetPlayer(Guid userID)
        {
            return _proxy.GetPlayer(userID);
        }

        public CommunicationRequest UpdateProfile(PlayerDM player)
        {
            return _proxy.UpdateProfile(player);
        }
    }
}