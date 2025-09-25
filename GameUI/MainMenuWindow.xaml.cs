using CodenamesClient.Properties.Langs;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace CodenamesClient.GameUI
{
    public partial class MainMenuWindow : Window
    {
        public MainMenuWindow()
        {
            InitializeComponent();
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
    }
}