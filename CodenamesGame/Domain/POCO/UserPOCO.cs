using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodenamesGame.Domain.POCO
{
    public class UserPOCO
    {
        public const int EMAIL_MAX_LENGTH = 30;
        public const int PASSWORD_MAX_LENGTH = 16;
        public Guid? UserID {  get; set; }
        public string Email { get; set; }
        public string Password {  get; set; }

        public UserPOCO()
        {

        }
    }
}
