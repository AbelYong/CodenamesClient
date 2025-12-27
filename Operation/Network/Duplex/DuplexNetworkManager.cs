using CodenamesGame.Domain.POCO;
using CodenamesGame.Domain.POCO.Match;
using CodenamesGame.Network;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CodenamesClient.Operation.Network.Duplex
{
    public class DuplexNetworkManager : IDuplexNetworkManager
    {
        private static readonly Lazy<DuplexNetworkManager> _instance = new Lazy<DuplexNetworkManager>(() => new DuplexNetworkManager());
        private readonly SessionOperation _sessionOperation;
        private readonly SocialOperation _socialOperation;
        private readonly LobbyOperation _lobbyOperation;
        private readonly MatchmakingOperation _matchmakingOperation;
        private readonly MatchOperation _matchOperation;
        private readonly ScoreboardOperation _scoreboardOperation;

        public event EventHandler ServerConnectionLost;

        public static DuplexNetworkManager Instance
        {
            get => _instance.Value;
        }

        private DuplexNetworkManager()
        {
            _sessionOperation = new SessionOperation();
            _sessionOperation.ConnectionLost += (s, e) => ServerConnectionLost?.Invoke(this, EventArgs.Empty);
            _socialOperation = new SocialOperation();
            _lobbyOperation = new LobbyOperation();
            _matchmakingOperation = new MatchmakingOperation();
            _matchOperation = new MatchOperation();
            _scoreboardOperation = new ScoreboardOperation();
        }

        public CodenamesGame.SessionService.CommunicationRequest ConnectToSessionService(PlayerDM player)
        {
            return _sessionOperation.Initialize(player);
        }

        public void DisconnectFromSessionService()
        {
            _sessionOperation.Disconnect();
        }

        public void ConnectToFriendService(Guid mePlayerId)
        {
            _socialOperation.Initialize(mePlayerId);
        }

        public void DisconnectFromFriendService()
        {
            _socialOperation.Disconnect();
        }

        public List<PlayerDM> SearchPlayers(string query)
        {
            return _socialOperation.SearchPlayers(query);
        }

        public List<PlayerDM> GetFriends()
        {
            return _socialOperation.GetFriends();
        }

        public List<PlayerDM> GetIncomingRequests()
        {
            return _socialOperation.GetIncomingRequests();
        }

        public List<PlayerDM> GetSentRequests()
        {
            return _socialOperation.GetSentRequests();
        }

        public CodenamesGame.FriendService.FriendshipRequest SendFriendRequest(Guid toPlayerId)
        {
            return _socialOperation.SendFriendRequest(toPlayerId);
        }

        public CodenamesGame.FriendService.FriendshipRequest AcceptFriendRequest(Guid requesterPlayerId)
        {
            return _socialOperation.AcceptFriendRequest(requesterPlayerId);
        }

        public CodenamesGame.FriendService.FriendshipRequest RejectFriendRequest(Guid requesterPlayerId)
        {
            return _socialOperation.RejectFriendRequest(requesterPlayerId);
        }

        public CodenamesGame.FriendService.FriendshipRequest RemoveFriend(Guid friendPlayerId)
        {
            return _socialOperation.RemoveFriend(friendPlayerId);
        }

        public CodenamesGame.LobbyService.CommunicationRequest ConnectLobbyService(Guid playerID)
        {
            return _lobbyOperation.Initialize(playerID);
        }

        public void DisconnectFromLobbyService()
        {
            _lobbyOperation.Disconnect();
        }

        public CodenamesGame.LobbyService.CreateLobbyRequest CreateLobby(PlayerDM player)
        {
            return _lobbyOperation.CreateLobby(player);
        }

        public CodenamesGame.LobbyService.CommunicationRequest InviteToParty(PlayerDM hostPlayer, Guid friendToInviteID, string lobbyCode)
        {
            return _lobbyOperation.InviteToParty(hostPlayer, friendToInviteID, lobbyCode);
        }

        public CodenamesGame.LobbyService.JoinPartyRequest JoinParty(PlayerDM joiningPlayer, string lobbyCode)
        {
            return _lobbyOperation.JoinParty(joiningPlayer, lobbyCode);
        }

        public CodenamesGame.MatchmakingService.CommunicationRequest ConnectMatchmakingService(Guid playerID)
        {
            return _matchmakingOperation.Initialize(playerID);
        }

        public void DisconnectFromMatchmakingService()
        {
            _matchmakingOperation.Disconnect();
        }

        public Task<CodenamesGame.MatchmakingService.CommunicationRequest> RequestArrangedMatch(MatchConfigurationDM matchConfig)
        {
            return _matchmakingOperation.RequestArrangedMatch(matchConfig);
        }

        public void ConfirmMatch(Guid matchID)
        {
            _matchmakingOperation.ConfirmMatch(matchID);
        }

        public void CancelMatch()
        {
            _matchmakingOperation.CancelMatch();
        }

        public CodenamesGame.MatchService.CommunicationRequest ConnectMatchService(Guid playerID)
        {
            return _matchOperation.Initialize(playerID);
        }

        public void DisconnectFromMatchService()
        {
            _matchOperation.Disconnect();
        }

        public CodenamesGame.MatchService.CommunicationRequest JoinMatch(MatchDM match)
        {
            return _matchOperation.JoinMatch(match);
        }

        public async Task SendClue(string clue)
        {
            await _matchOperation.SendClue(clue);
        }

        public async Task NotifyTurnTimeout(CodenamesGame.MatchService.MatchRoleType currentRole)
        {
            await _matchOperation.NotifyTurnTimeout(currentRole);
        }

        public async Task NotifyPickedAgent(BoardCoordinatesDM coordinates, int newTurnLength)
        {
            await _matchOperation.NotifyPickedAgent(coordinates, newTurnLength);
        }

        public async Task NotifyPickedBystander(BoardCoordinatesDM coordinates)
        {
            await _matchOperation.NotifyPickedBystander(coordinates);
        }

        public async Task NotifyPickedAssassin(BoardCoordinatesDM coordinates)
        {
            await _matchOperation.NotifyPickedAssassin(coordinates);
        }

        public void ConnectToScoreboardService(Guid playerID)
        {
            _scoreboardOperation.Initialize(playerID);
        }

        public void DisconnectFromScoreboardService()
        {
            _scoreboardOperation.Disconnect();
        }

        public ScoreboardDM GetMyScore(Guid playerID)
        {
            return _scoreboardOperation.GetMyScore(playerID);
        }
    }
}
