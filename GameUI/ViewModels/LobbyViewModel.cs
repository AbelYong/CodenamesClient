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

namespace CodenamesClient.GameUI.ViewModels
{
    public class LobbyViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private string _gamemodeName;
        private int _timerTokens;
        private int _bystanderTokens;
        private int _turnTimer;
        private bool _isCustomGame;

        private readonly SessionOperation _session;
        public ObservableCollection<PlayerDM> OnlineFriends { get; }

        public string GamemodeName
        {
            get => _gamemodeName;
            set
            {
                _gamemodeName = value;
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

        public LobbyViewModel(GamemodeDM gamemode, SessionOperation session)
        {
            _session = session;
            OnlineFriends = new ObservableCollection<PlayerDM>();
            LoadInitialOnlineFriends();

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