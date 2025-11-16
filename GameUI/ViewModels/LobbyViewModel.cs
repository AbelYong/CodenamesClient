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
using CodenamesGame.LobbyService;
using System.ServiceModel;

namespace CodenamesClient.GameUI.ViewModels
{
    public class LobbyViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private readonly PlayerDM _me;
        private PlayerDM _partyHost;
        private PlayerDM _partyGuest;
        private string _gamemodeName;
        private string _lobbyCode = string.Empty;
        private string _visibleLobbyCodeTag = string.Empty;
        private int _timerTokens;
        private int _bystanderTokens;
        private int _turnTimer;
        private bool _isCustomGame;
        private bool _isPartyFull;
        private Visibility _guestBtnVisibility;
        private Visibility _inviteBtnVisibility;
        private Visibility _jointBtnVisibility;
        private bool _createLobbyBtnEnabled;

        private readonly SessionOperation _session;
        public ObservableCollection<PlayerDM> OnlineFriends { get; }

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

        public bool CreateLobbyBtnEnabled
        {
            get => _createLobbyBtnEnabled;
            set
            {
                _createLobbyBtnEnabled = value;
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
            get => _isCustomGame;
            set
            {
                _isCustomGame = value;
                OnPropertyChanged();
            }
        }

        public LobbyViewModel(PlayerDM player, GamemodeDM gamemode, SessionOperation session)
        {
            CreateLobbyBtnEnabled = true;
            InviteBtnVisibility = Visibility.Collapsed;
            GuestBtnVisibility = Visibility.Collapsed;
            _me = player;
            _session = session;
            PartyHost = player;

            OnlineFriends = new ObservableCollection<PlayerDM>();
            LoadInitialOnlineFriends();
            ConnectToLobbyService(player);

            switch (gamemode)
            {
                case (GamemodeDM.NORMAL):
                    GamemodeName = Lang.gamemodeNormalGame;
                    LoadDefaultRules();
                    break;
                case (GamemodeDM.CUSTOM):
                    GamemodeName = Lang.gamemodeCustomGame;
                    LoadDefaultRules();
                    break;
                case (GamemodeDM.COUNTERINTELLIGENCE):
                    GamemodeName = Lang.gamemodeCounterintelligenceMode;
                    LoadDefaultRules();
                    break;
                default:
                    GamemodeName = "Gamemode";
                    LoadDefaultRules();
                    break;
            }
        }

        private void ConnectToLobbyService(PlayerDM player)
        {
            Guid playerID = (Guid)player.PlayerID;
            LobbyOperation.Instance.Initialize();
            CommunicationRequest request = LobbyOperation.Instance.Connect(playerID);
            if (request.IsSuccess)
            {
                SuscribeToLobbyEvents();
            }
            else
            {
                MessageBox.Show(Util.StatusToMessageMapper.GetLobbyServiceMessage(Util.LobbyOperationType.CONNECT, request.StatusCode));
            }
        }

        public void DisconnectFromLobbyService(PlayerDM player)
        {
            Guid playerID = (Guid)player.PlayerID;
            LobbyOperation.Instance.Disconnect(playerID);
        }

        private void SuscribeToLobbyEvents()
        {
            LobbyCallbackHandler.OnInvitationReceived += HandleInvitationReceived;
            LobbyCallbackHandler.OnPlayerJoined += HandlePlayerJoined;
            LobbyCallbackHandler.OnPlayerLeft += HandlePlayerLeft;
        }

        public void UnsucribeToLobbyEvents()
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
                JoinParty(e.Player, e.LobbyCode);
            }
        }

        public void JoinParty(PlayerDM me, string lobbyCode)
        {
            Guid myID = (Guid)_me.PlayerID;
            JoinPartyRequest request = LobbyOperation.Instance.JoinParty(myID, lobbyCode);
            if (request.IsSuccess)
            {
                PartyHost = PlayerDM.AssemblePlayer(request.Party.PartyHost);
                PartyGuest = _me;
                GuestBtnVisibility = Visibility.Visible;
                InviteBtnVisibility = Visibility.Collapsed;
            }
            else
            {
                MessageBox.Show(Util.StatusToMessageMapper.GetLobbyServiceMessage(Util.LobbyOperationType.JOIN_PARTY, request.StatusCode));
            }
        }

        public void CreateLobby()
        {
            Guid playerID = (Guid)_me.PlayerID;
            CreateLobbyRequest request = LobbyOperation.Instance.CreateLobby(playerID);
            if (request.IsSuccess)
            {
                _lobbyCode = request.LobbyCode;
                PartyHost = _me;
                VisibleLobbyCodeTag = string.Format("Lobby code: {0}", _lobbyCode);
                CreateLobbyBtnEnabled = false;
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
                CommunicationRequest request = LobbyOperation.Instance.InviteToParty(partyHostID, friendID, _lobbyCode);
                if (!request.IsSuccess)
                {
                    MessageBox.Show(Util.StatusToMessageMapper.GetLobbyServiceMessage(Util.LobbyOperationType.INVITE_TO_PARTY, request.StatusCode));
                }
            }
        }

        private void HandlePlayerJoined(object sender, PlayerEventArgs e)
        {
            PartyHost = _me;
            PartyGuest = e.Player;
            GuestBtnVisibility = Visibility.Visible;
            InviteBtnVisibility = Visibility.Collapsed;
        }

        private void HandlePlayerLeft(object sender, Guid e)
        {
            PartyHost = _me;
            PartyGuest = null;
            GuestBtnVisibility = Visibility.Collapsed;
            InviteBtnVisibility = Visibility.Visible;
        }

        /// <summary>
        /// Loads the initial list of online friends from the session instance.
        /// </summary>
        private void LoadInitialOnlineFriends()
        {
            var onlineFriendsList = _session.GetOnlineFriendsList();
            OnlineFriends.Clear();
            foreach (var friend in onlineFriendsList)
            {
                OnlineFriends.Add(friend);
            }
        }

        /// <summary>
        /// Subscribes to static events in the session.
        /// </summary>
        public void SubscribeToSessionEvents()
        {
            SessionOperation.OnFriendOnline += HandleFriendOnline;
            SessionOperation.OnFriendOffline += HandleFriendOffline;
            SessionOperation.OnOnlineFriendsReceived += HandleOnlineFriendsReceived;
        }

        /// <summary>
        /// Unsubscribes from static session events.
        /// </summary>
        public void UnsubscribeFromSessionEvents()
        {
            SessionOperation.OnFriendOnline -= HandleFriendOnline;
            SessionOperation.OnFriendOffline -= HandleFriendOffline;
            SessionOperation.OnOnlineFriendsReceived -= HandleOnlineFriendsReceived;
        }

        /// <summary>
        /// Handler for when a friend connects.
        /// </summary>
        private void HandleFriendOnline(object sender, PlayerEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (!OnlineFriends.Any(f => f.PlayerID == e.Player.PlayerID))
                {
                    OnlineFriends.Add(e.Player);
                }
            });
        }

        /// <summary>
        /// Handler for when a friend disconnects.
        /// </summary>
        private void HandleFriendOffline(object sender, Guid playerId)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var friend = OnlineFriends.FirstOrDefault(f => f.PlayerID == playerId);
                if (friend != null)
                {
                    OnlineFriends.Remove(friend);
                }
            });
        }

        private void HandleOnlineFriendsReceived(object sender, List<PlayerDM> friendsList)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                OnlineFriends.Clear();
                foreach (var friend in friendsList)
                {
                    OnlineFriends.Add(friend);
                }
            });
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

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}