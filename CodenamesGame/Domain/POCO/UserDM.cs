using System;

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
            if (svUser == null)
            {
                return null;
            }

            UserDM user = new UserDM();
            user.UserID = svUser.UserID;
            user.Email = svUser.Email;
            user.Password = "";
            return user;
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
