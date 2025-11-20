using CodenamesGame.Domain.POCO;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CodenamesClient.Properties.Langs;
using CodenamesGame.Network;
using System.Collections.ObjectModel;
using System.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using CodenamesGame.Network.EventArguments;
using System.ServiceModel;
using CodenamesGame.Domain.POCO.Match;
using System.Threading.Tasks;

namespace CodenamesClient.GameUI.ViewModels
{
    public class LobbyViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event Action<MatchDM> BeginMatch;
        private MatchDM _match;
        private readonly PlayerDM _me;
        private PlayerDM _partyHost;
        private PlayerDM _partyGuest;
        private GamemodeDM _gamemode;
        private string _gamemodeName;
        private string _readyOrCancelTgBtnContent;
        private string _lobbyCode = string.Empty;
        private string _visibleLobbyCodeTag = string.Empty;
        private int _timerTokens;
        private int _bystanderTokens;
        private int _turnTimer;
        private bool _playBtnEnabled;
        private bool _isReadyOrCancelTgBtnActive;
        private bool _isCustomGame;
        private bool _isPartyFull;
        private bool _canMatchBeCanceled;
        private Visibility _playBtnVisibility;
        private Visibility _readyOrCancelTgBtnVisibility;
        private Visibility _createLobbyBtnVisibility;
        private Visibility _guestBtnVisibility;
        private Visibility _inviteBtnVisibility;
        private Visibility _jointBtnVisibility;

        private readonly SessionOperation _session;

        public ObservableCollection<FriendItem> Friends { get; }

        public MatchDM match
        {
            get => _match;
            set
            {
                _match = value;
                OnPropertyChanged();
            }
        }

        public PlayerDM PartyHost
        {
            get => _partyHost;
            set
            {
                _partyHost = value;
                OnPropertyChanged();
            }
        }

        public PlayerDM PartyGuest
        {
            get => _partyGuest;
            set
            {
                _partyGuest = value;
                OnPropertyChanged();
            }
        }

        public GamemodeDM Gamemode
        {
            get => _gamemode;
            set
            {
                _gamemode = value;
                SetGamemodeName(value);
                OnPropertyChanged();
                switch(value)
                {
                    case GamemodeDM.NORMAL:
                        IsCustomGame = false;
                        break;
                    case GamemodeDM.CUSTOM:
                        IsCustomGame = true;
                        break;
                    case GamemodeDM.COUNTERINTELLIGENCE:
                        IsCustomGame = false;
                        break;
                }
            }
        }

        public string GamemodeName
        {
            get => _gamemodeName;
            set
            {
                _gamemodeName = value;
                OnPropertyChanged();
            }
        }

        public bool IsPartyFull
        {
            get => _isPartyFull;
            set
            {
                _isPartyFull = value;
                OnPropertyChanged();
            }
        }

        public int TimerTokens
        {
            get => _timerTokens;
            set
            {
                _timerTokens = value;
                OnPropertyChanged();
            }
        }

        public int BystanderTokens
        {
            get => _bystanderTokens;
            set
            {
                _bystanderTokens = value;
                OnPropertyChanged();
            }
        }

        public int TurnTimer
        {
            get => _turnTimer;
            set
            {
                _turnTimer = value;
                OnPropertyChanged();
            }
        }

        public bool IsCustomGame
        {
            get => !_isCustomGame;
            set
            {
                _isCustomGame = value;
                OnPropertyChanged();
            }
        }

        public Visibility PlayBtnVisibility
        {
            get => _playBtnVisibility;
            set
            {
                _playBtnVisibility = value;
                OnPropertyChanged();
            }
        }

        public bool PlayBtnEnabled
        {
            get => _playBtnEnabled;
            set
            {
                _playBtnEnabled = value;
                OnPropertyChanged();
            }
        }

        public bool IsReadyOrCancelTgBtnActive
        {
            get => _isReadyOrCancelTgBtnActive;
            set
            {
                if (_isReadyOrCancelTgBtnActive == value)
                {
                    return;
                }

                _isReadyOrCancelTgBtnActive = value;
                OnPropertyChanged();

                if (value)
                {
                    ConfirmReady();
                    ReadyOrCancelTgBtnContent = "Cancel";
                }
                else
                {
                    if (_canMatchBeCanceled)
                    {
                        RequestCancel();
                    }

                    ReadyOrCancelTgBtnContent = "Ready";
                }
            }
        }

        public Visibility ReadyOrCancelTgBtnVisibility
        {
            get => _readyOrCancelTgBtnVisibility;
            set
            {
                _readyOrCancelTgBtnVisibility = value;
                OnPropertyChanged();
            }
        }

        public Visibility CreateLobbyBtnVisbility
        {
            get => _createLobbyBtnVisibility;
            set
            {
                _createLobbyBtnVisibility = value;
                OnPropertyChanged();
            }
        }

        public Visibility GuestBtnVisibility
        {
            get => _guestBtnVisibility;
            set
            {
                _guestBtnVisibility = value;
                OnPropertyChanged();
            }
        }

        public Visibility InviteBtnVisibility
        {
            get => _inviteBtnVisibility;
            set
            {
                _inviteBtnVisibility= value;
                OnPropertyChanged();
            }
        }

        public Visibility JoinBtnVisibility
        {
            get => _jointBtnVisibility;
            set
            {
                _jointBtnVisibility= value;
                OnPropertyChanged();
            }
        }

        public string ReadyOrCancelTgBtnContent
        {
            get => _readyOrCancelTgBtnContent;
            set
            {
                _readyOrCancelTgBtnContent = value;
                OnPropertyChanged();
            }
        }

        public string VisibleLobbyCodeTag
        {
            get => _visibleLobbyCodeTag;
            set
            {
                _visibleLobbyCodeTag = value;
                OnPropertyChanged();
            }
        }

        public LobbyViewModel(PlayerDM player, GamemodeDM gamemode, SessionOperation session)
        {
            _me = player;
            Gamemode = gamemode;
            _session = session;
            PartyHost = player;

            PlayBtnVisibility = Visibility.Visible;
            ReadyOrCancelTgBtnVisibility = Visibility.Collapsed;
            ReadyOrCancelTgBtnContent = "Ready";
            PlayBtnEnabled = true;
            CreateLobbyBtnVisbility = Visibility.Visible;
            ReadyOrCancelTgBtnVisibility = Visibility.Collapsed;
            InviteBtnVisibility = Visibility.Collapsed;
            GuestBtnVisibility = Visibility.Collapsed;
            

            Friends = new ObservableCollection<FriendItem>();
            LoadFriends();
            ConnectToLobbyService(player);
            ConnectToMatchmakingService(player);

            switch (gamemode)
            {
                case (GamemodeDM.NORMAL):
                    LoadDefaultRules();
                    break;
                case (GamemodeDM.CUSTOM):
                    LoadDefaultRules();
                    break;
                case (GamemodeDM.COUNTERINTELLIGENCE):
                    LoadCounterintelligenceRules();
                    break;
                default:
                    LoadDefaultRules();
                    break;
            }
        }

        private void LoadDefaultRules()
        {
            const int NORMAL_TIMER_TOKENS = 9;
            const int NORMAL_BYSTANDER_TOKENS = 0;
            const int NORMAL_TIMER = 30;
            TimerTokens = NORMAL_TIMER_TOKENS;
            BystanderTokens = NORMAL_BYSTANDER_TOKENS;
            TurnTimer = NORMAL_TIMER;
        }

        private void LoadCounterintelligenceRules()
        {
            const int COUNTERINTELLIGENCE_TIMER_TOKENS = 12;
            const int COUNTERINTELLIGENCE_BYSTANDER_TOKENS = 0;
            const int COUNTERINTELLIGENCE_TIMER = 45;
            TimerTokens = COUNTERINTELLIGENCE_TIMER_TOKENS;
            BystanderTokens = COUNTERINTELLIGENCE_BYSTANDER_TOKENS;
            TurnTimer = COUNTERINTELLIGENCE_TIMER;
        }

        private void SetGamemodeName(GamemodeDM gamemode)
        {
            switch (gamemode)
            {
                case (GamemodeDM.NORMAL):
                    GamemodeName = Lang.gamemodeNormalGame;
                    break;
                case (GamemodeDM.CUSTOM):
                    GamemodeName = Lang.gamemodeCustomGame;
                    break;
                case (GamemodeDM.COUNTERINTELLIGENCE):
                    GamemodeName = Lang.gamemodeCounterintelligenceMode;
                    break;
                default:
                    GamemodeName = "ERROR";
                    break;
            }
        }

        private void ConnectToLobbyService(PlayerDM player)
        {
            Guid playerID = (Guid)player.PlayerID;
            CodenamesGame.LobbyService.CommunicationRequest request = LobbyOperation.Instance.Initialize(playerID);
            if (request.IsSuccess)
            {
                SuscribeToLobbyEvents();
            }
            else
            {
                MessageBox.Show(Util.StatusToMessageMapper.GetLobbyServiceMessage(Util.LobbyOperationType.CONNECT, request.StatusCode));
            }
        }

        public void DisconnectFromLobbyService()
        {
            UnsuscribeFromLobbyEvents();
            LobbyOperation.Instance.Disconnect();
        }

        private void SuscribeToLobbyEvents()
        {
            LobbyCallbackHandler.OnInvitationReceived += HandleInvitationReceived;
            LobbyCallbackHandler.OnPlayerJoined += HandlePlayerJoined;
            LobbyCallbackHandler.OnPlayerLeft += HandlePlayerLeft;
        }

        public void UnsuscribeFromLobbyEvents()
        {
            LobbyCallbackHandler.OnInvitationReceived -= HandleInvitationReceived;
            LobbyCallbackHandler.OnPlayerJoined -= HandlePlayerJoined;
            LobbyCallbackHandler.OnPlayerLeft -= HandlePlayerLeft;
        }

        private void HandleInvitationReceived(object sender, InvitationReceivedEventArgs e)
        {
            MessageBoxButton buttons = MessageBoxButton.YesNo;
            string message = string.Format(Lang.globalInvitationReceivedMessage, e.Player.Username);

            MessageBoxResult result = MessageBox.Show(message, Lang.globalInvitationReceivedTitle, buttons);

            if (result == MessageBoxResult.Yes)
            {
                JoinParty(e.LobbyCode);
            }
        }

        public void JoinParty(string lobbyCode)
        {
            Guid myID = (Guid)_me.PlayerID;
            CodenamesGame.LobbyService.JoinPartyRequest request = LobbyOperation.Instance.JoinParty(myID, lobbyCode);
            if (request.IsSuccess)
            {
                IsPartyFull = true;
                PartyHost = PlayerDM.AssemblePlayer(request.Party.PartyHost);
                PartyGuest = _me;
                GuestBtnVisibility = Visibility.Visible;
                CreateLobbyBtnVisbility = Visibility.Collapsed;
                InviteBtnVisibility = Visibility.Collapsed;
                JoinBtnVisibility = Visibility.Collapsed;
            }
            else
            {
                MessageBox.Show(Util.StatusToMessageMapper.GetLobbyServiceMessage(Util.LobbyOperationType.JOIN_PARTY, request.StatusCode));
            }
        }

        public void CreateLobby()
        {
            Guid playerID = (Guid)_me.PlayerID;
            CodenamesGame.LobbyService.CreateLobbyRequest request = LobbyOperation.Instance.CreateLobby(playerID);
            if (request.IsSuccess)
            {
                _lobbyCode = request.LobbyCode;
                PartyHost = _me;
                VisibleLobbyCodeTag = string.Format(Lang.lobbyLobyCode, _lobbyCode);
                CreateLobbyBtnVisbility = Visibility.Collapsed;
                InviteBtnVisibility = Visibility.Visible;
                JoinBtnVisibility = Visibility.Collapsed;
            }
            else
            {
                MessageBox.Show(Util.StatusToMessageMapper.GetLobbyServiceMessage(Util.LobbyOperationType.CREATE_PARTY, request.StatusCode));
            }
        }

        public void InviteToParty(Guid friendID)
        {
            if (_lobbyCode != string.Empty && PartyHost == _me)
            {
                Guid partyHostID = (Guid)_me.PlayerID;
                CodenamesGame.LobbyService.CommunicationRequest request = LobbyOperation.Instance.InviteToParty(partyHostID, friendID, _lobbyCode);
                if (!request.IsSuccess)
                {
                    MessageBox.Show(Util.StatusToMessageMapper.GetLobbyServiceMessage(Util.LobbyOperationType.INVITE_TO_PARTY, request.StatusCode));
                }
            }
        }

        private void HandlePlayerJoined(object sender, PlayerEventArgs e)
        {
            IsPartyFull = true;
            PartyHost = _me;
            PartyGuest = e.Player;
            CreateLobbyBtnVisbility = Visibility.Collapsed;
            GuestBtnVisibility = Visibility.Visible;
            InviteBtnVisibility = Visibility.Collapsed;
        }

        private void HandlePlayerLeft(object sender, Guid e)
        {
            IsPartyFull = false;
            PartyHost = _me;
            PartyGuest = null;
            CreateLobbyBtnVisbility = _lobbyCode != string.Empty ? Visibility.Collapsed : Visibility.Visible;
            GuestBtnVisibility = Visibility.Collapsed;
            InviteBtnVisibility = Visibility.Visible;
        }

        private void ConnectToMatchmakingService(PlayerDM player)
        {
            Guid playerID = (Guid)player.PlayerID;
            CodenamesGame.MatchmakingService.CommunicationRequest request = MatchmakingOperation.Instance.Initialize(playerID);
            if (request.IsSuccess)
            {
                SuscribeToMatchmakingEvents();
            }
            else
            {
                MessageBox.Show(Util.StatusToMessageMapper.GetMatchmakingServiceMessage(request.StatusCode));
            }
        }

        public void DisconnectFromMatchmakingService()
        {
            UnsuscribeFromMatchmakingEvents();
            MatchmakingOperation.Instance.Disconnect();
        }

        public async Task RequestArrangedMatch()
        {
            MatchConfigurationDM configuration = PrepareArrangedMatchRequest();
            if (configuration != null)
            {
                PlayBtnEnabled = false;
                CodenamesGame.MatchmakingService.CommunicationRequest request = await MatchmakingOperation.Instance.RequestArrangedMatch(configuration);
                if (!request.IsSuccess)
                {
                    PlayBtnEnabled = true;
                    MessageBox.Show(Util.StatusToMessageMapper.GetMatchmakingServiceMessage(request.StatusCode));
                }
                else
                {
                    MessageBox.Show("Your match has been requested");
                }
            }
        }

        private MatchConfigurationDM PrepareArrangedMatchRequest()
        {
            if (_partyHost != null && _partyGuest != null)
            {
                MatchConfigurationDM matchConfig = new MatchConfigurationDM();
                switch (_gamemode)
                {
                    case GamemodeDM.NORMAL:
                        matchConfig.Rules.gamemode = GamemodeDM.NORMAL;
                        break;
                    case GamemodeDM.CUSTOM:
                        matchConfig.Rules.gamemode = GamemodeDM.CUSTOM;
                        matchConfig.Rules.TurnTimer = TurnTimer;
                        matchConfig.Rules.TimerTokens = TimerTokens;
                        matchConfig.Rules.BystanderTokens = BystanderTokens;
                        break;
                    case GamemodeDM.COUNTERINTELLIGENCE:
                        matchConfig.Rules.gamemode = GamemodeDM.COUNTERINTELLIGENCE;
                        break;
                    default:
                        matchConfig.Rules.gamemode = GamemodeDM.NORMAL;
                        break;
                }
                matchConfig.Requester = _me;
                matchConfig.Companion = _me.PlayerID == _partyHost.PlayerID ? _partyGuest : _partyHost;
                return matchConfig;
            }
            return null;
        }

        private void ConfirmReady()
        {
            if (_match != null)
            {
                MatchmakingOperation.Instance.ConfirmMatch(_match.MatchID);
            }
        }

        private void RequestCancel()
        {
            MatchmakingOperation.Instance.CancelMatch();
        }

        private void SuscribeToMatchmakingEvents()
        {
            MatchmakingCallbackHandler.OnMatchPending += HandleMatchPending;
            MatchmakingCallbackHandler.OnMatchReady += HandleMatchReady;
            MatchmakingCallbackHandler.OnPlayersReady += HandlePlayersReady;
            MatchmakingCallbackHandler.OnMatchCanceled += HandleMatchCanceled;
        }

        public void UnsuscribeFromMatchmakingEvents()
        {
            MatchmakingCallbackHandler.OnMatchPending -= HandleMatchPending;
            MatchmakingCallbackHandler.OnMatchReady -= HandleMatchReady;
            MatchmakingCallbackHandler.OnPlayersReady -= HandlePlayersReady;
            MatchmakingCallbackHandler.OnMatchCanceled -= HandleMatchCanceled;
        }

        private void HandleMatchPending(object sender, MatchPendingEventArgs e)
        {
            PlayBtnEnabled = false;
        }

        private void HandleMatchReady(object sender, MatchDM match)
        {
            if (match != null)
            {
                ReadyOrCancelTgBtnVisibility = Visibility.Visible;
                PlayBtnVisibility = Visibility.Collapsed;
                _match = match;
                _canMatchBeCanceled = true;
                SetRulesToIncomingRules(match);
            }
        }

        private void SetRulesToIncomingRules(MatchDM match)
        {
            Gamemode = match.Rules.gamemode;
            TurnTimer = match.Rules.TurnTimer;
            TimerTokens = match.Rules.TimerTokens;
            BystanderTokens = match.Rules.BystanderTokens;
        }

        private void HandlePlayersReady(object sender, Guid e)
        {
            MatchDM matchToNavigate = _match;

            ReadyOrCancelTgBtnVisibility = Visibility.Collapsed;
            PlayBtnVisibility = Visibility.Visible;
            PlayBtnEnabled = true;
            _canMatchBeCanceled = false;
            //Turn it back from cancel to its "signal ready state"
            IsReadyOrCancelTgBtnActive = false;
            _match = null; //Free the match field

            if (BeginMatch != null && matchToNavigate != null)
            {
                BeginMatch.Invoke(matchToNavigate);
            }
        }

        private void HandleMatchCanceled(object sender, MatchCanceledEventArgs e)
        {
            if (_match != null && e.MatchID == _match.MatchID)
            {
                _match = null;
                PlayBtnEnabled = true;
                PlayBtnVisibility = Visibility.Visible;
                ReadyOrCancelTgBtnVisibility = Visibility.Collapsed;
                MessageBox.Show(Util.StatusToMessageMapper.GetMatchmakingServiceMessage(e.Reason));
            }
        }

        private void LoadFriends()
        {
            try
            {
                var allFriends = SocialOperation.Instance.GetFriends();

                var onlineFriendsList = _session.GetOnlineFriendsList();
                var onlineIds = new HashSet<Guid>(onlineFriendsList.Select(p => p.PlayerID.Value));

                Friends.Clear();
                foreach (var friend in allFriends)
                {
                    if (friend.PlayerID.HasValue)
                    {
                        bool isOnline = onlineIds.Contains(friend.PlayerID.Value);
                        Friends.Add(new FriendItem { Player = friend, IsOnline = isOnline });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading friends: " + ex.Message);
            }
        }

        public void SubscribeToSessionEvents()
        {
            SessionOperation.OnFriendOnline += HandleFriendOnline;
            SessionOperation.OnFriendOffline += HandleFriendOffline;
        }

        public void UnsubscribeFromSessionEvents()
        {
            SessionOperation.OnFriendOnline -= HandleFriendOnline;
            SessionOperation.OnFriendOffline -= HandleFriendOffline;
        }

        private void HandleFriendOnline(object sender, PlayerEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var friendItem = Friends.FirstOrDefault(f => f.Player.PlayerID == e.Player.PlayerID);
                if (friendItem != null)
                {
                    friendItem.IsOnline = true;
                }
                else
                {
                    Friends.Add(new FriendItem { Player = e.Player, IsOnline = true });
                }
            });
        }

        private void HandleFriendOffline(object sender, Guid playerId)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var friendItem = Friends.FirstOrDefault(f => f.Player.PlayerID == playerId);
                if (friendItem != null)
                {
                    friendItem.IsOnline = false;
                }
            });
        }

        public class FriendItem : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
            private bool _isOnline;
            public PlayerDM Player { get; set; }

            public bool IsOnline
            {
                get => _isOnline;
                set
                {
                    _isOnline = value;
                    OnPropertyChanged();
                }
            }
            protected void OnPropertyChanged([CallerMemberName] string name = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}