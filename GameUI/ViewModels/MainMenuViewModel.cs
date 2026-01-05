using CodenamesClient.Operation;
using CodenamesClient.Operation.Network.Duplex;
using CodenamesClient.Properties.Langs;
using CodenamesClient.Util;
using CodenamesGame.Domain.POCO;
using CodenamesGame.Network;
using CodenamesGame.Network.EventArguments;
using CodenamesGame.Network.Proxies.CallbackHandlers;
using CodenamesGame.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;

namespace CodenamesClient.GameUI.ViewModels
{
    public class MainMenuViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event Action<InvitationReceivedEventArgs> OnInvitationReceived;
        private PlayerDM _player;
        private string _username;
        private bool _isPlayerGuest;

        private HashSet<Guid> _sentRequestIds = new HashSet<Guid>();

        public ObservableCollection<FriendItem> Friends { get; set; }
        public ObservableCollection<FriendItem> Requests { get; set; }
        public ObservableCollection<SearchItem> SearchResults { get; set; }
        public ObservableCollection<ScoreboardDM> TopWinsEntries { get; set; }
        public ObservableCollection<ScoreboardDM> TopBestTimeEntries { get; set; }
        public ObservableCollection<ScoreboardDM> TopAssassinsEntries { get; set; }

        public MainMenuViewModel(PlayerDM player, bool isGuest)
        {
            Friends = new ObservableCollection<FriendItem>();
            Requests = new ObservableCollection<FriendItem>();
            SearchResults = new ObservableCollection<SearchItem>();
            TopWinsEntries = new ObservableCollection<ScoreboardDM>();
            TopBestTimeEntries = new ObservableCollection<ScoreboardDM>();
            TopAssassinsEntries = new ObservableCollection<ScoreboardDM>();

            ScoreboardCallbackHandler.OnLeaderboardUpdateReceived += HandleLeaderboardUpdate;

            IsPlayerGuest = isGuest;
            _player = player;
            Username = Player.Username;

            if (!IsPlayerGuest)
            {
                ConnectSocialService();
                ConnectLobbyService();
                LoadInitialFriendData();
            }
        }

        public PlayerDM Player
        {
            get => _player;
            set 
            { 
                _player = value;
                OnPropertyChanged();
            }
        }

        public string Username
        {
            get => _username;
            set
            { 
                _username = value;
                OnPropertyChanged();
            }
        }

        public bool IsPlayerGuest
        {
            get => _isPlayerGuest;
            set
            { 
                _isPlayerGuest = value;
                OnPropertyChanged();
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void ConnectSocialService()
        {
            DuplexNetworkManager.Instance.ConnectToFriendService(Player.PlayerID.Value);
            SubscribeToFriendEvents();
        }

        public void ConnectLobbyService()
        {
            var request = DuplexNetworkManager.Instance.ConnectLobbyService(Player.PlayerID.Value);
            if (!request.IsSuccess)
            {
                MessageBox.Show(StatusToMessageMapper.GetLobbyServiceMessage(Util.LobbyOperationType.INTIALIZE, request.StatusCode));
            }
        }

        public void SuscribeToLobbyInvitations()
        {
            LobbyCallbackHandler.OnInvitationReceived += HandleLobbyInvitationReceived;
        }

        public void UnsuscribeFromLobbyInvitations()
        {
            LobbyCallbackHandler.OnInvitationReceived -= HandleLobbyInvitationReceived;
        }

        private void HandleLobbyInvitationReceived(object sender, InvitationReceivedEventArgs e)
        {
            OnInvitationReceived?.Invoke(e);
            
        }

        public void Disconnect()
        {
            if (_player != null)
            {
                DuplexNetworkManager.Instance.DisconnectFromSessionService();
                if (!IsPlayerGuest)
                {
                    UnsubscribeFromFriendEvents();
                    DuplexNetworkManager.Instance.DisconnectFromFriendService();
                }
                DuplexNetworkManager.Instance.DisconnectFromLobbyService();
            }
        }

        private void SubscribeToFriendEvents()
        {
            FriendCallbackHandler.OnNewFriendRequest += HandleNewFriendRequest;
            FriendCallbackHandler.OnFriendRequestAccepted += HandleFriendRequestAccepted;
            FriendCallbackHandler.OnFriendRequestRejected += HandleFriendRequestRejected;
            FriendCallbackHandler.OnFriendRemoved += HandleFriendRemoved;
        }

        private void UnsubscribeFromFriendEvents()
        {
            FriendCallbackHandler.OnNewFriendRequest -= HandleNewFriendRequest;
            FriendCallbackHandler.OnFriendRequestAccepted -= HandleFriendRequestAccepted;
            FriendCallbackHandler.OnFriendRequestRejected -= HandleFriendRequestRejected;
            FriendCallbackHandler.OnFriendRemoved -= HandleFriendRemoved;
        }

        private void HandleNewFriendRequest(object sender, PlayerEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                string message = string.Format(Lang.friendNotificationNewRequest, e.Player.Username);
                MessageBox.Show(message, Lang.globalSuccessTitle, MessageBoxButton.OK, MessageBoxImage.Information);

                Requests.Add(new FriendItem
                {
                    Player = e.Player,
                    ProfilePicturePath = PictureHandler.GetImagePath(e.Player.AvatarID),
                    IsOnline = false
                });
            });
        }

        private void HandleFriendRequestAccepted(object sender, PlayerEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                string message = string.Format(Lang.friendNotificationAccepted, e.Player.Username);
                MessageBox.Show(message, Lang.globalSuccessTitle, MessageBoxButton.OK, MessageBoxImage.Information);

                LoadInitialFriendData();
            });
        }

        private static void HandleFriendRequestRejected(object sender, PlayerEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                string message = string.Format(Lang.friendNotificationRejected, e.Player.Username);
                MessageBox.Show(message, Lang.globalSuccessTitle, MessageBoxButton.OK, MessageBoxImage.Information);
            });
        }

        private void HandleFriendRemoved(object sender, PlayerEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                string message = string.Format(Lang.friendNotificationRemoved, e.Player.Username);
                MessageBox.Show(message, Lang.globalSuccessTitle, MessageBoxButton.OK, MessageBoxImage.Information);

                var friend = Friends.FirstOrDefault(f => f.Player.PlayerID == e.Player.PlayerID);
                if (friend != null)
                {
                    Friends.Remove(friend);
                }
            });
        }

        public void SendFriendRequest(SearchItem item)
        {
            if (IsPlayerGuest || item?.Player?.PlayerID == null)
            {
                return;
            }

            var response = DuplexNetworkManager.Instance.SendFriendRequest(item.Player.PlayerID.Value);
            string message = StatusToMessageMapper.GetFriendServiceMessage(response.StatusCode);

            if (response.IsSuccess)
            {
                MessageBox.Show(message, Lang.globalSuccessTitle, MessageBoxButton.OK, MessageBoxImage.Information);
                item.IsPending = true;
                _sentRequestIds.Add(item.Player.PlayerID.Value);
            }
            else
            {
                MessageBox.Show(message, Lang.globalErrorTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void AcceptFriendRequest(FriendItem item)
        {
            if (IsPlayerGuest || item?.Player?.PlayerID == null)
            {
                return;
            }

            var response = DuplexNetworkManager.Instance.AcceptFriendRequest(item.Player.PlayerID.Value);
            string message = StatusToMessageMapper.GetFriendServiceMessage(response.StatusCode);

            if (response.IsSuccess)
            {
                MessageBox.Show(message, Lang.globalSuccessTitle, MessageBoxButton.OK, MessageBoxImage.Information);

                Requests.Remove(item);
                Friends.Add(item);
            }
            else
            {
                MessageBox.Show(message, Lang.globalErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void RejectFriendRequest(FriendItem item)
        {
            if (IsPlayerGuest || item?.Player?.PlayerID == null)
            {
                return;
            }

            var response = DuplexNetworkManager.Instance.RejectFriendRequest(item.Player.PlayerID.Value);
            string message = StatusToMessageMapper.GetFriendServiceMessage(response.StatusCode);

            if (response.IsSuccess)
            {
                MessageBox.Show(message, Lang.globalSuccessTitle, MessageBoxButton.OK, MessageBoxImage.Information);
                Requests.Remove(item);
            }
            else
            {
                MessageBox.Show(message, Lang.globalErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void RemoveFriend(PlayerDM friendPlayer)
        {
            if (IsPlayerGuest || friendPlayer == null || friendPlayer.PlayerID == null)
            {
                return;
            }

            var response = DuplexNetworkManager.Instance.RemoveFriend(friendPlayer.PlayerID.Value);
            string message = StatusToMessageMapper.GetFriendServiceMessage(response.StatusCode);

            if (response.IsSuccess)
            {
                MessageBox.Show(message, Lang.globalSuccessTitle, MessageBoxButton.OK, MessageBoxImage.Information);

                var itemToRemove = Friends.FirstOrDefault(f => f.Player.PlayerID == friendPlayer.PlayerID);
                if (itemToRemove != null)
                {
                    Friends.Remove(itemToRemove);
                }
            }
            else
            {
                MessageBox.Show(message, Lang.globalErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void LoadInitialFriendData()
        {
            if (IsPlayerGuest)
            {
                return;
            }

            Task.Run(() =>
            {
                var friendsList = DuplexNetworkManager.Instance.GetFriends();
                var requestsList = DuplexNetworkManager.Instance.GetIncomingRequests();
                var sentList = DuplexNetworkManager.Instance.GetSentRequests();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Friends.Clear();
                    Requests.Clear();
                    _sentRequestIds.Clear();

                    foreach (var friend in friendsList)
                    {
                        Friends.Add(new FriendItem
                        {
                            Player = friend,
                            ProfilePicturePath = PictureHandler.GetImagePath(friend.AvatarID),
                            IsOnline = false
                        });
                    }

                    foreach (var req in requestsList)
                    {
                        Requests.Add(new FriendItem
                        {
                            Player = req,
                            ProfilePicturePath = PictureHandler.GetImagePath(req.AvatarID),
                            IsOnline = false
                        });
                    }

                    _sentRequestIds = sentList.Where(x => x.PlayerID.HasValue).Select(x => x.PlayerID.Value).ToHashSet();
                });
            });
        }

        public void SearchPlayers(string query)
        {
            if (IsPlayerGuest)
            {
                return;
            }

            Task.Run(() =>
            {
                var searchList = DuplexNetworkManager.Instance.SearchPlayers(query);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    SearchResults.Clear();
                    foreach (var player in searchList)
                    {
                        if (player.PlayerID != Player.PlayerID)
                        {
                            bool isAlreadySent = player.PlayerID.HasValue && _sentRequestIds.Contains(player.PlayerID.Value);

                            SearchResults.Add(new SearchItem
                            {
                                Player = player,
                                IsPending = isAlreadySent,
                                ProfilePicturePath = PictureHandler.GetImagePath(player.AvatarID)
                            });
                        }
                    }
                });
            });
        }

        public class SearchItem : INotifyPropertyChanged
        {
            private bool _isPending;
            private string _profilePicturePath;

            public PlayerDM Player { get; set; }

            public string ProfilePicturePath
            {
                get => _profilePicturePath;
                set
                { 
                    _profilePicturePath = value;
                    OnPropertyChanged();
                }
            }

            public bool IsPending
            {
                get => _isPending;
                set 
                { 
                    _isPending = value;
                    OnPropertyChanged(); 
                }
            }

            public Visibility ButtonVisibility
            {
                get => IsPending ? Visibility.Collapsed : Visibility.Visible;
            }

            public Visibility TextVisibility
            {
                get => IsPending ? Visibility.Visible : Visibility.Collapsed;
            }

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged([CallerMemberName] string name = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
                if (name == nameof(IsPending))
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ButtonVisibility)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TextVisibility)));
                }
            }
        }

        public class FriendItem : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
            private bool _isOnline;
            private string _profilePicturePath;
            public PlayerDM Player { get; set; }

            public string ProfilePicturePath
            {
                get => _profilePicturePath;
                set
                { 
                    _profilePicturePath = value;
                    OnPropertyChanged();
                }
            }

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

        private void HandleLeaderboardUpdate(object sender, ScoreboardEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                TopWinsEntries.Clear();
                TopBestTimeEntries.Clear();
                TopAssassinsEntries.Clear();

                if (e.Leaderboard != null)
                {
                    var wins = e.Leaderboard.OrderByDescending(x => x.GamesWon).Take(10);
                    foreach (var item in wins)
                    {
                        TopWinsEntries.Add(item);
                    }

                    var bestTime = e.Leaderboard
                                .Where(x => x.FastestMatch != "--:--" && !string.IsNullOrEmpty(x.FastestMatch))
                                .OrderBy(x => x.FastestMatch)
                                .Take(10);
                    foreach (var item in bestTime)
                    {
                        TopBestTimeEntries.Add(item);
                    }

                    var assassins = e.Leaderboard.OrderByDescending(x => x.AssassinsRevealed).Take(10);
                    foreach (var item in assassins)
                    {
                        TopAssassinsEntries.Add(item);
                    }
                }
            });
        }

        public void OpenScoreboard()
        {
            if (!IsPlayerGuest && Player?.PlayerID != null)
            {
                var playerId = Player.PlayerID.Value;

                Task.Run(() =>
                {
                    DuplexNetworkManager.Instance.ConnectToScoreboardService(playerId);
                });
            }
        }

        public void CloseScoreboard()
        {
            if (!IsPlayerGuest)
            {
                DuplexNetworkManager.Instance.DisconnectFromScoreboardService();
            }
        }

        public void ShowMyPersonalScore()
        {
            if (!IsPlayerGuest && Player?.PlayerID != null)
            {
                Task.Run(() =>
                {
                    var myScore = DuplexNetworkManager.Instance.GetMyScore(Player.PlayerID.Value);
                    if (myScore != null)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            TopWinsEntries.Clear();
                            TopBestTimeEntries.Clear();
                            TopAssassinsEntries.Clear();

                            TopWinsEntries.Add(myScore);
                            TopBestTimeEntries.Add(myScore);
                            TopAssassinsEntries.Add(myScore);
                        });
                    }
                });
            }
        }
    }
}