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
    }
}
