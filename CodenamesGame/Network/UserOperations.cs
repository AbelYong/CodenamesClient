using CodenamesGame.Domain.POCO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodenamesGame.Network
{
    public class UserOperations
    {
        public UserOperations()
        {

        }

        public static Guid? Authenticate(string username, string password)
        {
            var client = new AuthenticationService.AuthenticationManagerClient("NetTcpBinding_IAuthenticationManager");
            return client.Login(username, password);
        }

        public static Guid? SignIn(UserPOCO user, PlayerPOCO player)
        {
            var client = new AuthenticationService.AuthenticationManagerClient("NetTcpBinding_IAuthenticationManager");
            AuthenticationService.User svUser = AssembleSvUser(user);
            AuthenticationService.Player svPlayer = AssembleSvPlayer(player);
            return client.SignIn(svUser, svPlayer);
        }

        private static AuthenticationService.User AssembleSvUser(UserPOCO user)
        {
            AuthenticationService.User svUser = new AuthenticationService.User();
            svUser.Email = user.Email;
            svUser.Password = user.Password;
            return svUser;
        }

        private static AuthenticationService.Player AssembleSvPlayer(PlayerPOCO player)
        {
            AuthenticationService.Player svPlayer = new AuthenticationService.Player();
            svPlayer.Username = player.Username;
            svPlayer.Name = player.Name;
            svPlayer.LastName = player.LastName;
            return svPlayer;
        }
    }
}
