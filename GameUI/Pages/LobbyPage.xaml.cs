using CodenamesClient.GameUI.BoardUI;
using CodenamesClient.GameUI.ViewModels;
using CodenamesGame.Domain.POCO;
using CodenamesGame.Domain.POCO.Match;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using static CodenamesClient.GameUI.ViewModels.LobbyViewModel;

namespace CodenamesClient.GameUI.Pages
{
    public partial class LobbyPage : Page
    {
        private LobbyViewModel _viewModel;
        private Guid _myID;
        private Storyboard _slideInOnlineFriends;
        private Storyboard _slideOutOnlineFriends;
        private Storyboard _slideInTypeCode;
        private Storyboard _slideOutTypeCode;
        private readonly Regex _regex = new Regex("[^0-9]+", RegexOptions.None, TimeSpan.FromMilliseconds(100));

        public LobbyPage(PlayerDM player, GamemodeDM gamemode)
        {
            InitializeComponent();
            _myID = (Guid)player.PlayerID;
            _viewModel = new LobbyViewModel(player, gamemode);
            DataContext = _viewModel;
            Loaded += OnLobbyPageLoaded;
            Unloaded += OnLobbyPageUnloaded;
        }

        private void OnLobbyPageLoaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is LobbyViewModel)
            {
                _slideInOnlineFriends = (Storyboard)FindResource("SlideInOnlineFriendsAnimation");
                _slideOutOnlineFriends = (Storyboard)FindResource("SlideOutOnlineFriendsAnimation");
                _slideInTypeCode = (Storyboard)FindResource("SlideInLobbyCodeAnimation");
                _slideOutTypeCode = (Storyboard)FindResource("SlideOutLobbyCodeAnimation");

                _viewModel.SubscribeToSessionEvents();

                _viewModel.BeginMatch += OnBeginMatch;
            }
        }

        private void OnLobbyPageUnloaded(object sender, RoutedEventArgs e)
        {
            _viewModel.UnsubscribeFromSessionEvents();

            _viewModel.BeginMatch -= OnBeginMatch;
        }

        private void NumberValidation(object sender, TextCompositionEventArgs e)
        {
            e.Handled = _regex.IsMatch(e.Text);
        }

        private void NumberValidationKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }

        private void TextBoxPasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                if (_regex.IsMatch(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private void TimerTokensRangeValidation(object sender, TextChangedEventArgs e)
        {
            int maxTokens = MatchRulesDM.MAX_TIMER_TOKENS;
            TextBox tbx = sender as TextBox;
            RangeValidation(tbx, maxTokens);
        }

        private void TimerTokensMinRangeValidation(object sender, RoutedEventArgs e)
        {
            const int minTokens = 4;
            TextBox tbx = sender as TextBox;
            MinRangeValidation(tbx, minTokens);
        }

        private void BystanderTokensRangeValidation(object sender, TextChangedEventArgs e)
        {
            int maxTokens = MatchRulesDM.MAX_BYSTANDER_TOKENS;
            TextBox tbx = sender as TextBox;
            RangeValidation(tbx, maxTokens);
        }

        private void BystanderTokensMinRangeValidation(object sender, RoutedEventArgs e)
        {
            const int minTokens = 0;
            TextBox tbx = sender as TextBox;
            MinRangeValidation(tbx, minTokens);
        }

        private void TurnTimerRangeValidation(object sender, TextChangedEventArgs e)
        {
            int maxTurnTimer = MatchRulesDM.MAX_TURN_TIMER;
            TextBox tbx = sender as TextBox;
            RangeValidation(tbx, maxTurnTimer);
        }

        private void TurnTimerMinRangeValidation(object sender, RoutedEventArgs e)
        {
            const int minTurnTimer = 15;
            TextBox tbx = sender as TextBox;
            MinRangeValidation(tbx, minTurnTimer);
        }

        private static void RangeValidation(TextBox tbx, int max)
        {
            if (string.IsNullOrEmpty(tbx.Text))
            {
                return;
            }

            if (int.TryParse(tbx.Text, out int value) && value > max)
            {
                tbx.Text = max.ToString();
                tbx.CaretIndex = tbx.Text.Length; // Move cursor to end
            }
        }

        private static void MinRangeValidation(TextBox tbx, int min)
        {
            if (string.IsNullOrEmpty(tbx.Text))
            {
                tbx.Text = min.ToString();
                return;
            }

            if (int.TryParse(tbx.Text, out int value) && value < min)
            {
                tbx.Text = min.ToString();
            }
        }

        private async void Click_StartGame(object sender, RoutedEventArgs e)
        {
            if (_viewModel.IsPartyFull)
            {
                await _viewModel.RequestArrangedMatch();
            }
        }

        private void Click_btnCreateLobby(object sender, RoutedEventArgs e)
        {
            _viewModel.CreateLobby();
        }

        private void OnBeginMatch(MatchDM match)
        {
            _viewModel.BeginMatch -= OnBeginMatch;
            BoardPage board = new BoardPage(match, _myID);
            NavigationService.Navigate(board);
        }

        private void Click_ReturnToLobby(object sender, RoutedEventArgs e)
        {
            _viewModel.DisconnectFromLobbyService();
            _viewModel.DisconnectFromMatchmakingService();
            _viewModel.UnsuscribeFromLobbyEvents();
            _viewModel.UnsubscribeFromSessionEvents();
            _viewModel.UnsuscribeFromMatchmakingEvents();
            NavigationService.GoBack();
        }

        private void Click_btnJoinParty(object sender, RoutedEventArgs e)
        {
            Overlay.Visibility = Visibility.Visible;
            gridTypeCode.Visibility = Visibility.Visible;
            _slideInTypeCode.Begin();
        }

        private void Click_btnSendCode(object sender, RoutedEventArgs e)
        {
            CloseTypeCodeGrid();
            _viewModel.JoinParty(tbkInputCode.Text);
        }

        private void Click_btnCloseTypeCode(object sender, RoutedEventArgs e)
        {
            CloseTypeCodeGrid();
        }

        private void CloseTypeCodeGrid()
        {
            EventHandler slideOutHandler = null;

            slideOutHandler = (s, a) =>
            {
                gridTypeCode.Visibility = Visibility.Collapsed;
                Overlay.Visibility = Visibility.Collapsed;

                _slideOutTypeCode.Completed -= slideOutHandler;
            };

            _slideOutTypeCode.Completed += slideOutHandler;
            _slideOutTypeCode.Begin();
        }

        private void Click_ShowOnlineFriends(object sender, RoutedEventArgs e)
        {
            Overlay.Visibility = Visibility.Visible;
            OnlineFriendsGrid.Visibility = Visibility.Visible;
            _slideInOnlineFriends.Begin();
        }

        private void Click_HideOnlineFriends(object sender, RoutedEventArgs e)
        {
            EventHandler slideOutHandler = null;

            slideOutHandler = (s, a) =>
            {
                OnlineFriendsGrid.Visibility = Visibility.Collapsed;
                Overlay.Visibility = Visibility.Collapsed;

                _slideOutOnlineFriends.Completed -= slideOutHandler;
            };

            _slideOutOnlineFriends.Completed += slideOutHandler;
            _slideOutOnlineFriends.Begin();
        }

        private void Click_InviteFriend(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is FriendItem friendItem)
            {
                PlayerDM friendToInvite = friendItem.Player;

                if (friendToInvite.PlayerID.HasValue)
                {
                    Guid friendID = (Guid)friendToInvite.PlayerID;
                    _viewModel.InviteToParty(friendID);

                    MessageBox.Show($"Invitación enviada a {friendToInvite.Username}");
                }
            }
        }
    }
}