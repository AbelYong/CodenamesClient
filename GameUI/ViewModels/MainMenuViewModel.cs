using CodenamesClient.Properties.Langs;
using CodenamesClient.Util;
using CodenamesGame.Domain.POCO;
using CodenamesGame.Network;
using CodenamesGame.Network.EventArguments;
using CodenamesGame.SessionService;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;

namespace CodenamesClient.GameUI.ViewModels
{
    public class MainMenuViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private PlayerDM _player;
        private string _username;
        private bool _isPlayerGuest;

        public ObservableCollection<PlayerDM> Friends { get; set; }
        public ObservableCollection<PlayerDM> Requests { get; set; }
        public ObservableCollection<PlayerDM> SearchResults { get; set; }

        public MainMenuViewModel(PlayerDM player, bool isGuest)
        {
            Friends = new ObservableCollection<PlayerDM>();
            Requests = new ObservableCollection<PlayerDM>();
            SearchResults = new ObservableCollection<PlayerDM>();

            IsPlayerGuest = isGuest;
            Player = player ?? AssembleGuest();
            Username = Player.Username;

            ConnectSocialService(Player);
            if (!IsPlayerGuest)
            {
                LoadInitialFriendData();
            }
        }

        public PlayerDM Player
        {
            get => _player;
            set { _player = value; OnPropertyChanged(); }
        }

        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(); }
        }

        public bool IsPlayerGuest
        {
            get => _isPlayerGuest;
            set { _isPlayerGuest = value; OnPropertyChanged(); }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void ConnectSocialService(PlayerDM player)
        {
            if (!IsPlayerGuest)
            {
                try
                {
                    SocialOperation.Instance.Initialize(player.PlayerID.Value);
                    SubscribeToFriendEvents();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, Lang.globalErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public void Disconnect()
        {
            if (_player != null)
            {
                SessionOperation.Instance.Disconnect();
                if (!IsPlayerGuest)
                {
                    UnsubscribeFromFriendEvents();
                    SocialOperation.Instance.Terminate();
                }
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
                Requests.Add(e.Player);
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

                var friend = Friends.FirstOrDefault(f => f.PlayerID == e.Player.PlayerID);
                if (friend != null)
                {
                    Friends.Remove(friend);
                }
            });
        }

        public void SendFriendRequest(Guid targetPlayerId)
        {
            if (IsPlayerGuest) return;

            var response = SocialOperation.Instance.SendFriendRequest(targetPlayerId);
            string message = StatusToMessageMapper.GetFriendServiceMessage(response.StatusCode);

            if (response.IsSuccess)
            {
                MessageBox.Show(message, Lang.globalSuccessTitle, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(message, Lang.globalErrorTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void AcceptFriendRequest(PlayerDM requester)
        {
            if (IsPlayerGuest || requester == null || requester.PlayerID == null) return;

            var response = SocialOperation.Instance.AcceptFriendRequest(requester.PlayerID.Value);
            string message = StatusToMessageMapper.GetFriendServiceMessage(response.StatusCode);

            if (response.IsSuccess)
            {
                MessageBox.Show(message, Lang.globalSuccessTitle, MessageBoxButton.OK, MessageBoxImage.Information);

                Requests.Remove(requester);
                Friends.Add(requester);
            }
            else
            {
                MessageBox.Show(message, Lang.globalErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void RejectFriendRequest(PlayerDM requester)
        {
            if (IsPlayerGuest || requester == null || requester.PlayerID == null) return;

            var response = SocialOperation.Instance.RejectFriendRequest(requester.PlayerID.Value);
            string message = StatusToMessageMapper.GetFriendServiceMessage(response.StatusCode);

            if (response.IsSuccess)
            {
                MessageBox.Show(message, Lang.globalSuccessTitle, MessageBoxButton.OK, MessageBoxImage.Information);
                Requests.Remove(requester);
            }
            else
            {
                MessageBox.Show(message, Lang.globalErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void RemoveFriend(PlayerDM friend)
        {
            if (IsPlayerGuest || friend == null || friend.PlayerID == null) return;

            var response = SocialOperation.Instance.RemoveFriend(friend.PlayerID.Value);
            string message = StatusToMessageMapper.GetFriendServiceMessage(response.StatusCode);

            if (response.IsSuccess)
            {
                MessageBox.Show(message, Lang.globalSuccessTitle, MessageBoxButton.OK, MessageBoxImage.Information);
                Friends.Remove(friend);
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
                var friendsList = SocialOperation.Instance.GetFriends();
                var requestsList = SocialOperation.Instance.GetIncomingRequests();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Friends.Clear();
                    Requests.Clear();
                    foreach (var friend in friendsList)
                    {
                        Friends.Add(friend);
                    }
                    foreach (var req in requestsList)
                    {
                        Requests.Add(req);
                    }
                });
            });
        }

        public void SearchPlayers(string query)
        {
            if (IsPlayerGuest) return;

            Task.Run(() =>
            {
                var searchList = SocialOperation.Instance.SearchPlayers(query);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    SearchResults.Clear();
                    foreach (var player in searchList)
                    {
                        if (player.PlayerID != Player.PlayerID)
                            SearchResults.Add(player);
                    }
                });
            });
        }

        private static PlayerDM AssembleGuest()
        {
            const int DEFAULT_AVATAR = 0;
            PlayerDM guest = new PlayerDM();
            guest.PlayerID = Guid.NewGuid();
            guest.Username = Lang.globalGuest;
            guest.AvatarID = DEFAULT_AVATAR;
            return guest;
        }
    }
}