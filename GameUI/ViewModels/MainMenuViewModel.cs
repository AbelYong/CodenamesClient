using CodenamesClient.Properties.Langs;
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
        private readonly SessionOperation _session;
        private PlayerDM _player;
        private string _username;
        private bool _isPlayerGuest;

        public ObservableCollection<PlayerDM> Friends { get; set; }
        public ObservableCollection<PlayerDM> Requests { get; set; }
        public ObservableCollection<PlayerDM> SearchResults { get; set; }

        public MainMenuViewModel(PlayerDM player, SessionOperation session, bool isGuest)
        {
            Friends = new ObservableCollection<PlayerDM>();
            Requests = new ObservableCollection<PlayerDM>();
            SearchResults = new ObservableCollection<PlayerDM>();

            _session = session;
            IsPlayerGuest = isGuest;
            Player = player ?? AssembleGuest();
            Username = Player.Username;

            ConnectSocialService(Player);
            if (!IsPlayerGuest)
            {
                LoadInitialFriendData();
                Guid playerID = (Guid)player.PlayerID;
            }
        }

        public SessionOperation Session
        {
            get => _session;
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
                    MessageBox.Show($"Error al conectar al servicio de amigos: {ex.Message}");
                }
            }
        }

        public void Disconnect(PlayerDM player)
        {
            if (_player != null)
            {
                _session.Disconnect(player);
                if (!IsPlayerGuest)
                {
                    SocialOperation.Instance.Terminate();
                    UnsubscribeFromFriendEvents();
                }
            }
        }

        private void SubscribeToFriendEvents()
        {
            FriendCallbackHandler.OnNewFriendRequest += HandleNewFriendRequest;
            FriendCallbackHandler.OnFriendRequestAccepted += HandleFriendRequestAccepted;
            FriendCallbackHandler.OnFriendRequestRejected += HandleFriendRequestRejected;
            FriendCallbackHandler.OnFriendRemoved += HandleFriendRemoved;
            FriendCallbackHandler.OnOperationSuccess += HandleOperationSuccess;
            FriendCallbackHandler.OnOperationFailure += HandleOperationFailure;
        }

        private void UnsubscribeFromFriendEvents()
        {
            FriendCallbackHandler.OnNewFriendRequest -= HandleNewFriendRequest;
            FriendCallbackHandler.OnFriendRequestAccepted -= HandleFriendRequestAccepted;
            FriendCallbackHandler.OnFriendRequestRejected -= HandleFriendRequestRejected;
            FriendCallbackHandler.OnFriendRemoved -= HandleFriendRemoved;
            FriendCallbackHandler.OnOperationSuccess -= HandleOperationSuccess;
            FriendCallbackHandler.OnOperationFailure -= HandleOperationFailure;
        }

        private void HandleNewFriendRequest(object sender, PlayerEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show($"¡Nueva solicitud de amistad de {e.Player.Username}!");
                Requests.Add(e.Player);
            });
        }

        private void HandleFriendRequestAccepted(object sender, PlayerEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show($"¡{e.Player.Username} aceptó tu solicitud!");
                LoadInitialFriendData();
            });
        }

        private void HandleFriendRequestRejected(object sender, PlayerEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show($"¡{e.Player.Username} rechazó tu solicitud!");
            });
        }

        private void HandleFriendRemoved(object sender, PlayerEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show($"¡{e.Player.Username} te ha eliminado de sus amigos!");
                var friend = Friends.FirstOrDefault(f => f.PlayerID == e.Player.PlayerID);
                if (friend != null)
                {
                    Friends.Remove(friend);
                }
            });
        }

        private void HandleOperationSuccess(object sender, OperationMessageEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show($"Éxito: {e.Message}", "Operación Exitosa", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadInitialFriendData();
            });
        }

        private void HandleOperationFailure(object sender, OperationMessageEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show($"Error: {e.Message}", "Operación Fallida", MessageBoxButton.OK, MessageBoxImage.Error);
            });
        }

        public void LoadInitialFriendData()
        {
            if (IsPlayerGuest)
            {
                return;
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                var friendsList = SocialOperation.Instance.GetFriends();
                var requestsList = SocialOperation.Instance.GetIncomingRequests();

                Friends.Clear();
                Requests.Clear();
                foreach (var friend in friendsList) Friends.Add(friend);
                foreach (var req in requestsList) Requests.Add(req);
            });
        }

        public void SearchPlayers(string query)
        {
            var searchList = SocialOperation.Instance.SearchPlayers(query);
            Application.Current.Dispatcher.Invoke(() =>
            {
                SearchResults.Clear();
                foreach (var player in searchList) SearchResults.Add(player);
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