using CodenamesClient.Properties.Langs;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace CodenamesClient.GameUI
{
    public partial class MainMenuWindow : UserControl
    {
        private MediaPlayer mediaPlayer;

        public MainMenuWindow()
        {
            InitializeComponent();
            mediaPlayer = new MediaPlayer();
            mediaPlayer.Open(new Uri("Main Theme (NOT FINAL).mp3", UriKind.Relative));
            mediaPlayer.Play();
            mediaPlayer.MediaEnded += (s, e) => mediaPlayer.Position = TimeSpan.Zero;
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
            // TO DO
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

        private void GameMode_Button_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = (Button)sender;
            string gameMode = clickedButton.Content.ToString();
            MessageBox.Show($"Starting game in {gameMode} mode!");
            // Aquí podrías agregar la lógica para iniciar el juego real.
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
    }
}