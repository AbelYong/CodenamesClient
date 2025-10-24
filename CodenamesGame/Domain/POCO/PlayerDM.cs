using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using FriendPlayer = CodenamesGame.FriendService.Player;

namespace CodenamesGame.Domain.POCO
{
    public class PlayerDM
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
        public UserDM User { get; set; } = new UserDM();

        public PlayerDM()
        {

        }
        public static PlayerDM AssemblePlayer(UserService.Player svPlayer)
        {
            PlayerDM player = new PlayerDM();
            player.PlayerID = svPlayer.PlayerID;
            player.Username = svPlayer.Username;
            player.AvatarID = svPlayer.AvatarID;
            player.Name = svPlayer.Name;
            player.LastName = svPlayer.LastName;
            player.FacebookUsername = svPlayer.FacebookUsername;
            player.InstagramUsername = svPlayer.InstagramUsername;
            player.DiscordUsername = svPlayer.DiscordUsername;
            player.User = UserDM.AssembleUser(svPlayer.User);
            return player;
        }

        public static AuthenticationService.Player AssembleAuthSvPlayer(PlayerDM player)
        {
            AuthenticationService.Player svPlayer = new AuthenticationService.Player();
            svPlayer.Username = player.Username;
            svPlayer.Name = player.Name;
            svPlayer.LastName = player.LastName;
            return svPlayer;
        }

        public static UserService.Player AssembleUserSvPlayer(PlayerDM player)
        {
            UserService.Player svPlayer = new UserService.Player();
            svPlayer.PlayerID = player.PlayerID;
            svPlayer.Username = player.Username;
            svPlayer.AvatarID  = player.AvatarID;
            svPlayer.Name = player.Name;
            svPlayer.LastName = player.LastName;
            svPlayer.FacebookUsername = player.FacebookUsername;
            svPlayer.InstagramUsername  = player.InstagramUsername;
            svPlayer.DiscordUsername = player.DiscordUsername;
            svPlayer.User = UserDM.AssembleUserSvUser(player.User);
            return svPlayer;
        }
        public static PlayerDM AssemblePlayer(FriendPlayer p)
        {
            if (p == null) return null;
            return new PlayerDM
            {
                PlayerID = p.PlayerID,
                Username = p.Username,
                Name = p.Name,
                LastName = p.LastName
            };
        }
    }
}
