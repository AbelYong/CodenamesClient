using CodenamesGame.Domain.POCO;
using CodenamesGame.UserService;
using System;

namespace CodenamesGame.Network.Proxies.Interfaces
{
    public interface IUserProxy
    {
        SignInRequest SignIn(UserDM user, PlayerDM player);
        PlayerDM GetPlayer(Guid userID);
        CommunicationRequest UpdateProfile(PlayerDM player);
    }
}
