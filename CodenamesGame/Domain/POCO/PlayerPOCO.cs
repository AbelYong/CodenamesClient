using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodenamesGame.Domain.POCO
{
    public class PlayerPOCO
    {
        public const int USERNAME_MAX_LENGTH = 20;
        public const int NAME_MAX_LENGTH = 20;
        public const int LASTNAME_MAX_LENGTH = 30;
        public const int SOCIALMEDIA_MAX_LENGTH = 30;
        public Guid? PlayerID { get; set; }
        public string Username { get; set; }
        public int AvatarID { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string FacebookUsername { get; set; }
        public string InstagramUsername { get; set; }
        public string DiscordUsername { get; set; }

        public PlayerPOCO()
        {

        }
    }
}
