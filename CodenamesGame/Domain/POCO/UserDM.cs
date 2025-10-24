using CodenamesGame.AuthenticationService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodenamesGame.Domain.POCO
{
    public class UserDM
    {
        public const int EMAIL_MAX_LENGTH = 30;
        public const int PASSWORD_MAX_LENGTH = 16;
        public Guid? UserID {  get; set; }
        public string Email { get; set; }
        public string Password {  get; set; }

        public UserDM()
        {

        }

        public static UserDM AssembleUser(UserService.User svUser)
        {
            UserDM user = new UserDM();
            user.UserID = svUser.UserID;
            user.Email = svUser.Email;
            user.Password = "";
            return user;
        }

        public static AuthenticationService.User AssembleAuthSvUser(UserDM user)
        {
            AuthenticationService.User svUser = new AuthenticationService.User();
            svUser.Email = user.Email;
            svUser.Password = user.Password;
            return svUser;
        }

        public static UserService.User AssembleUserSvUser(UserDM user)
        {
            UserService.User svUser = new UserService.User();
            svUser.UserID = (Guid) user.UserID;
            svUser.Email = user.Email;
            return svUser;
        } 
    }
}
