using System;
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

        public static PlayerDM AssemblePlayer(FriendPlayer player)
        {
            if (player == null)
            {
                return null;
            }
            return new PlayerDM
            {
                PlayerID = player.PlayerID,
                Username = player.Username,
                Name = player.Name,
                LastName = player.LastName
            };
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

        public static SessionService.Player AssembleSessionSvPlayer(PlayerDM player)
        {
            SessionService.Player svPlayer = new SessionService.Player();
            svPlayer.PlayerID = player.PlayerID;
            svPlayer.Username = player.Username;
            svPlayer.AvatarID = player.AvatarID;
            svPlayer.Name = player.Name;
            svPlayer.LastName = player.LastName;
            svPlayer.FacebookUsername = player.FacebookUsername;
            svPlayer.InstagramUsername = player.InstagramUsername;
            svPlayer.DiscordUsername = player.DiscordUsername;
            return svPlayer;
        }
    }
}
