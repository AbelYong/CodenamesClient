using CodenamesClient.GameUI.Pages.UserControls;
using CodenamesClient.GameUI.ViewModels;
using CodenamesClient.Properties.Langs;
using CodenamesGame.Domain.POCO;
using CodenamesGame.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CodenamesClient.GameUI.Pages
{
    /// <summary>
    /// Lógica de interacción para MainMenuPage.xaml
    /// </summary>
    public partial class MainMenuPage : Page
    {
        private MainMenuViewModel _viewModel;
        private ProfileControl _profileControl;
        private MediaPlayer _mediaPlayer;
        private List<PlayerDM> _friends = new List<PlayerDM>();
        private List<PlayerDM> _requests = new List<PlayerDM>();
        private List<PlayerDM> _search = new List<PlayerDM>();

        public MainMenuPage(PlayerDM player)
        {
            InitializeComponent();
            _viewModel = new MainMenuViewModel(player);
            this.DataContext = _viewModel;

            _mediaPlayer = new MediaPlayer();
            _mediaPlayer.Open(new Uri("Main Theme (NOT FINAL).mp3", UriKind.Relative));
            _mediaPlayer.Play();
            _mediaPlayer.MediaEnded += (s, e) => _mediaPlayer.Position = TimeSpan.Zero;
        }

        /*
         * ProfileControl is created in code, instead of the XAML, because WPF cannot find
         * the resources needed to generate ProfileControl in design time, so we instantiate it
         * directly in code, thus event handling logic (close, save profile) is more code reliant 
         */
        private void Click_btnPlayer(object sender, RoutedEventArgs e)
        {
            if (!_viewModel.IsPlayerGuest)
            {
                Overlay.Visibility = Visibility.Visible;
                _profileControl = new ProfileControl(_viewModel.Player);
                _profileControl.VerticalAlignment = VerticalAlignment.Center;
                _profileControl.HorizontalAlignment = HorizontalAlignment.Center;
                _profileControl.Visibility = Visibility.Visible;

                _profileControl.ClickCloseProfile += CloseProfile;
                _profileControl.ClickSaveProfile += SaveProfile;

                gridMainMenu.Children.Add(_profileControl);
            }
            else
            {
                MessageBox.Show(Lang.mainMenuGuestCannotAccess);
            }
        }

        private void SaveProfile()
        {
            SetPlayerAfterProfileUpdate(_viewModel.Player.User.UserID);
            CloseProfile();
        }

        private void CloseProfile()
        {
            if (_profileControl != null)
            {
                _profileControl.ClickCloseProfile -= CloseProfile;
                _profileControl.ClickSaveProfile -= SaveProfile;
            }
            _profileControl.Visibility = Visibility.Collapsed;
            gridMainMenu.Children.Remove(_profileControl);
            _profileControl = null;
            Overlay.Visibility = Visibility.Collapsed;
        }

        private void Click_btnLogout(object sender, RoutedEventArgs e)
        {
            if (_viewModel.Player != null)
            {
                _viewModel.Disconnect(_viewModel.Player);
            }
            LoginPage login = new LoginPage();
            NavigationService.Navigate(login);
        }

        private void ShowSettings_Click(object sender, RoutedEventArgs e)
        {
            var slideInAnimation = (Storyboard)FindResource("SlideInAnimation");
            SettingsGrid.Visibility = Visibility.Visible;
            slideInAnimation.Begin();
        }

        private void HideSettings_Click(object sender, RoutedEventArgs e)
        {
            var slideOutAnimation = (Storyboard)FindResource("SlideOutAnimation");
            slideOutAnimation.Completed += (s, ev) =>
            {
                SettingsGrid.Visibility = Visibility.Collapsed;
            };
            slideOutAnimation.Begin();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO
            HideSettings_Click(sender, e);
        }

        private void btnQuit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void ShowFriends_Click(object sender, RoutedEventArgs e)
        {
            var slideInAnimation = (Storyboard)FindResource("SlideInFriendsAnimation");
            FriendsGrid.Visibility = Visibility.Visible;
            slideInAnimation.Begin();

            RefreshFriendsUi();
        }

        private void HideFriends_Click(object sender, RoutedEventArgs e)
        {
            var slideOutAnimation = (Storyboard)FindResource("SlideOutFriendsAnimation");
            slideOutAnimation.Completed += (s, ev) =>
            {
                FriendsGrid.Visibility = Visibility.Collapsed;
            };
            slideOutAnimation.Begin();
        }

        private void ClearSearchBox_Click(object sender, RoutedEventArgs e)
        {
            SearchBox.Text = string.Empty;
            SearchBox.Focus();
            SearchResultsList.ItemsSource = null;
        }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchBox.Text == Lang.socialSearchForAFriend)
            {
                SearchBox.Text = string.Empty;
                SearchBox.FontStyle = FontStyles.Normal;
            }
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                SearchBox.Text = Lang.socialSearchForAFriend;
                SearchBox.FontStyle = FontStyles.Italic;
            }
        }

        private void ShowGameMode_Click(object sender, RoutedEventArgs e)
        {
            var slideInAnimation = (Storyboard)FindResource("SlideInGameModeAnimation");
            GameModeGrid.Visibility = Visibility.Visible;
            slideInAnimation.Begin();
        }

        private void HideGameMode_Click(object sender, RoutedEventArgs e)
        {
            var slideOutAnimation = (Storyboard)FindResource("SlideOutGameModeAnimation");
            slideOutAnimation.Completed += (s, ev) =>
            {
                GameModeGrid.Visibility = Visibility.Collapsed;
            };
            slideOutAnimation.Begin();
        }

        private void NormalGameMode_Click(object sender, RoutedEventArgs e)
        {
            GamemodeDM mode = new GamemodeDM(GamemodeDM.GamemodeName.NORMAL);
            GoToLobby(mode);
        }
        private void CustomGameMode_Click(object sender, RoutedEventArgs e)
        {
            GamemodeDM mode = new GamemodeDM(GamemodeDM.GamemodeName.CUSTOM);
            GoToLobby(mode);
        }
        private void CounterintelligenceMode_Click(object sender, RoutedEventArgs e)
        {
            GamemodeDM mode = new GamemodeDM(GamemodeDM.GamemodeName.COUNTERINTELLIGENCE);
            GoToLobby(mode);
        }

        private void GoToLobby(GamemodeDM mode)
        {
            LobbyPage lobby = new LobbyPage(mode);
            NavigationService.Navigate(lobby);
            GameModeGrid.Visibility = Visibility.Collapsed;
        }

        private void ShowScoreboards_Click(object sender, RoutedEventArgs e)
        {
            var slideInAnimation = (Storyboard)FindResource("SlideInScoreboardsAnimation");
            ScoreboardsGrid.Visibility = Visibility.Visible;
            slideInAnimation.Begin();
        }

        private void HideScoreboards_Click(object sender, RoutedEventArgs e)
        {
            var slideOutAnimation = (Storyboard)FindResource("SlideOutScoreboardsAnimation");
            slideOutAnimation.Completed += (s, ev) =>
            {
                ScoreboardsGrid.Visibility = Visibility.Collapsed;
            };
            slideOutAnimation.Begin();
        }

        private void SetPlayerAfterProfileUpdate(Guid? userID)
        {
            if (userID != null)
            {
                Guid auxUserID = (Guid)userID;
                PlayerDM player = UserOperation.GetPlayer(auxUserID);
                _viewModel.Player = player;
                btnPlayer.Content = player.Username;
            }
        }

        private void RefreshFriendsUi()
        {
            if (_viewModel.Player?.PlayerID == null) return;
            var me = _viewModel.Player.PlayerID.Value;

            _friends = SocialOperation.GetFriends(me);
            _requests = SocialOperation.GetIncomingRequests(me);

            FriendsList.ItemsSource = _friends;
            RequestsList.ItemsSource = _requests;
        }

        private static PlayerDM ItemFromButton(Button btn) => btn?.DataContext as PlayerDM;

        private void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            if (_viewModel.Player?.PlayerID == null) return;

            var q = SearchBox.Text?.Trim();
            if (string.IsNullOrEmpty(q) || q == Lang.socialSearchForAFriend) return;

            var me = _viewModel.Player.PlayerID.Value;
            _search = SocialOperation.SearchPlayers(me, q, 20);
            SearchResultsList.ItemsSource = _search;
        }

        private void SendRequest_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.Player?.PlayerID == null) return;
            var target = ItemFromButton((Button)sender);
            if (target?.PlayerID == null) return;

            var (ok, msg) = SocialOperation.SendFriendRequest(_viewModel.Player.PlayerID.Value, target.PlayerID.Value);
            MessageBox.Show(msg);
            RefreshFriendsUi();
        }

        private void AcceptRequest_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.Player?.PlayerID == null) return;
            var requester = ItemFromButton((Button)sender);
            if (requester?.PlayerID == null) return;

            var (ok, msg) = SocialOperation.AcceptFriendRequest(_viewModel.Player.PlayerID.Value, requester.PlayerID.Value);
            MessageBox.Show(msg);
            RefreshFriendsUi();
        }

        private void RejectRequest_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.Player?.PlayerID == null) return;
            var requester = ItemFromButton((Button)sender);
            if (requester?.PlayerID == null) return;

            var (ok, msg) = SocialOperation.RejectFriendRequest(_viewModel.Player.PlayerID.Value, requester.PlayerID.Value);
            MessageBox.Show(msg);
            RefreshFriendsUi();
        }

        private void RemoveFriend_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.Player?.PlayerID == null) return;
            var friend = ItemFromButton((Button)sender);
            if (friend?.PlayerID == null) return;

            var (ok, msg) = SocialOperation.RemoveFriend(_viewModel.Player.PlayerID.Value, friend.PlayerID.Value);
            MessageBox.Show(msg);
            RefreshFriendsUi();
        }
    }
}
