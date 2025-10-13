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

        public UserPOCO User { get; set; }

        public PlayerPOCO()
        {

        }

        public static AuthenticationService.Player AssembleSvPlayer(PlayerPOCO player)
        {
            AuthenticationService.Player svPlayer = new AuthenticationService.Player();
            svPlayer.Username = player.Username;
            svPlayer.Name = player.Name;
            svPlayer.LastName = player.LastName;
            return svPlayer;
        }

        public static PlayerPOCO AssemblePlayer(UserService.Player svPlayer)
        {
            PlayerPOCO player = new PlayerPOCO();
            player.PlayerID = svPlayer.PlayerID;
            player.Username = svPlayer.Username;
            player.AvatarID = svPlayer.AvatarID;
            player.Name = svPlayer.Name;
            player.LastName = svPlayer.LastName;
            player.FacebookUsername = svPlayer.FacebookUsername;
            player.InstagramUsername = svPlayer.InstagramUsername;
            player.DiscordUsername = svPlayer.DiscordUsername;
            player.User = UserPOCO.AssembleUser(svPlayer.User);
            return player;
        }
    }
}
