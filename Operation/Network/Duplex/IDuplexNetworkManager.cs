using CodenamesGame.Domain.POCO;
using CodenamesGame.Domain.POCO.Match;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CodenamesClient.Operation.Network.Duplex
{
    public interface IDuplexNetworkManager
    {
        // Session Service
        CodenamesGame.SessionService.CommunicationRequest ConnectToSessionService(PlayerDM player);
        void DisconnectFromSessionService();

        // Friend Service
        void ConnectToFriendService(Guid mePlayerId);
        void DisconnectFromFriendService();
        List<PlayerDM> SearchPlayers(string query);
        List<PlayerDM> GetFriends();
        List<PlayerDM> GetIncomingRequests();
        List<PlayerDM> GetSentRequests();
        CodenamesGame.FriendService.FriendshipRequest SendFriendRequest(Guid toPlayerId);
        CodenamesGame.FriendService.FriendshipRequest AcceptFriendRequest(Guid requesterPlayerId);
        CodenamesGame.FriendService.FriendshipRequest RejectFriendRequest(Guid requesterPlayerId);
        CodenamesGame.FriendService.FriendshipRequest RemoveFriend(Guid friendPlayerId);

        // Lobby Service
        CodenamesGame.LobbyService.CommunicationRequest ConnectLobbyService(Guid playerID);
        void DisconnectFromLobbyService();
        CodenamesGame.LobbyService.CreateLobbyRequest CreateLobby(PlayerDM player);
        CodenamesGame.LobbyService.CommunicationRequest InviteToParty(PlayerDM hostPlayer, Guid friendToInviteID, string lobbyCode);
        CodenamesGame.LobbyService.JoinPartyRequest JoinParty(PlayerDM joiningPlayer, string lobbyCode);

        // Matchmaking Service
        CodenamesGame.MatchmakingService.CommunicationRequest ConnectMatchmakingService(Guid playerID);
        void DisconnectFromMatchmakingService();
        Task<CodenamesGame.MatchmakingService.CommunicationRequest> RequestArrangedMatch(MatchConfigurationDM matchConfig);
        void ConfirmMatch(Guid matchID);
        void CancelMatch();

        // Match Service
        CodenamesGame.MatchService.CommunicationRequest ConnectMatchService(Guid playerID);
        void DisconnectFromMatchService();
        CodenamesGame.MatchService.CommunicationRequest JoinMatch(MatchDM match);
        void SendClue(string clue);
        void NotifyTurnTimeout(CodenamesGame.MatchService.MatchRoleType currentRole);
        void NotifyPickedAgent(BoardCoordinatesDM coordinates, int newTurnLength);
        void NotifyPickedBystander(BoardCoordinatesDM coordinates);
        void NotifyPickedAssassin(BoardCoordinatesDM coordinates);

        // Scoreboard Service
        void ConnectToScoreboardService(Guid playerID);
        void DisconnectFromScoreboardService();
        ScoreboardDM GetMyScore(Guid playerID);
    }
}
