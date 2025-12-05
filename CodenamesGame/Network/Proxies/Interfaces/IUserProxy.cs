using CodenamesGame.Domain.POCO;
using CodenamesGame.UserService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodenamesGame.Network.Proxies.Interfaces
{
    public interface IUserProxy
    {
        SignInRequest SignIn(UserDM user, PlayerDM player);
        PlayerDM GetPlayer(Guid userID);
        CommunicationRequest UpdateProfile(PlayerDM player);
    }
}
