using System;

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
            PlayerDM player = new PlayerDM
            {
                PlayerID = svPlayer.PlayerID,
                Username = svPlayer.Username,
                AvatarID = svPlayer.AvatarID,
                Name = svPlayer.Name,
                LastName = svPlayer.LastName,
                FacebookUsername = svPlayer.FacebookUsername,
                InstagramUsername = svPlayer.InstagramUsername,
                DiscordUsername = svPlayer.DiscordUsername,
                User = UserDM.AssembleUser(svPlayer.User)
            };
            return player;
        }

        public static PlayerDM AssemblePlayer(FriendService.Player player)
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

        public static PlayerDM AssemblePlayer(SessionService.Player player)
        {
            if (player == null)
            {
                return null;
            }
            return new PlayerDM
            {
                PlayerID = player.PlayerID,
                Username = player.Username,
                AvatarID = player.AvatarID,
                Name = player.Name,
                LastName = player.LastName,
                FacebookUsername = player.FacebookUsername,
                InstagramUsername = player.InstagramUsername,
                DiscordUsername = player.DiscordUsername,
            };
        }

        public static PlayerDM AssemblePlayer(LobbyService.Player player)
        {
            if (player == null)
            {
                return null;
            }
            return new PlayerDM
            {
                PlayerID = player.PlayerID,
                Username = player.Username,
                AvatarID = player.AvatarID,
            };
        }

        public static PlayerDM AssemblePlayer(MatchmakingService.Player player)
        {
            if (player == null)
            {
                return null;
            }
            return new PlayerDM
            {
                PlayerID = player.PlayerID,
                Username = player.Username,
                AvatarID = player.AvatarID
            };
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

        public static LobbyService.Player AssembleLobbySvPlayer(PlayerDM player)
        {
            LobbyService.Player svPlayer = new LobbyService.Player();
            svPlayer.PlayerID = player.PlayerID;
            svPlayer.Username = player.Username;
            svPlayer.AvatarID = player.AvatarID;
            return svPlayer;
        }

        public static MatchmakingService.Player AssembleMatchmakingSvPlayer(PlayerDM player)
        {
            MatchmakingService.Player svPlayer = new MatchmakingService.Player();
            svPlayer.PlayerID = player.PlayerID;
            svPlayer.Username = player.Username;
            svPlayer.AvatarID = player.AvatarID;
            return svPlayer;
        }

        public static MatchService.Player AssembleMatchSvPlayer(PlayerDM player)
        {
            MatchService.Player svPlayer = new MatchService.Player();
            svPlayer.PlayerID = player.PlayerID;
            return svPlayer;
        }
    }
}
