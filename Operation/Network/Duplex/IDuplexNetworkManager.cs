using CodenamesGame.Domain.POCO;
using CodenamesGame.Domain.POCO.Match;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CodenamesClient.Operation.Network.Duplex
{
    public interface IDuplexNetworkManager
    {
        CodenamesGame.SessionService.CommunicationRequest ConnectToSessionService(PlayerDM player);
        void DisconnectFromSessionService();
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
        CodenamesGame.LobbyService.CommunicationRequest ConnectLobbyService(Guid playerID);
        void DisconnectFromLobbyService();
        CodenamesGame.LobbyService.CreateLobbyRequest CreateLobby(PlayerDM player);
        CodenamesGame.LobbyService.CommunicationRequest SendEmailInvitation(string toAddress);
        CodenamesGame.LobbyService.CommunicationRequest InviteToParty(PlayerDM hostPlayer, Guid friendToInviteID, string lobbyCode);
        CodenamesGame.LobbyService.JoinPartyRequest JoinParty(PlayerDM joiningPlayer, string lobbyCode);
        CodenamesGame.MatchmakingService.CommunicationRequest ConnectMatchmakingService(Guid playerID);
        void DisconnectFromMatchmakingService();
        Task<CodenamesGame.MatchmakingService.CommunicationRequest> RequestArrangedMatch(MatchConfigurationDM matchConfig);
        void ConfirmMatch(Guid matchID);
        void CancelMatch();
        CodenamesGame.MatchService.CommunicationRequest ConnectMatchService(Guid playerID);
        void DisconnectFromMatchService();
        CodenamesGame.MatchService.CommunicationRequest JoinMatch(MatchDM match);
        Task SendClue(string clue);
        Task NotifyTurnTimeout(CodenamesGame.MatchService.MatchRoleType currentRole);
        Task NotifyPickedAgent(BoardCoordinatesDM coordinates, int newTurnLength);
        Task NotifyPickedBystander(BoardCoordinatesDM coordinates);
        Task NotifyPickedAssassin(BoardCoordinatesDM coordinates);
        Task<bool> CheckCompanionStatus();
        void ConnectToScoreboardService(Guid playerID);
        void DisconnectFromScoreboardService();
        ScoreboardDM GetMyScore(Guid playerID);
    }
}
