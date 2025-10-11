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
        private Guid? _playerID { get; set; }
        private string _username { get; set; }
        private int _avatarID { get; set; }
        private string _name { get; set; }
        private string _lastName { get; set; }
        private string _facebookUsername { get; set; }
        private string _instagramUsername { get; set; }
        private string _discordUsername { get; set; }

        public PlayerPOCO()
        {

        }
    }
}
