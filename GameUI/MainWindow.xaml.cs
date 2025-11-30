using CodenamesClient.GameUI.Pages;
using CodenamesGame.Network;
using CodenamesGame.SessionService;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Windows;

namespace CodenamesClient.GameUI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Closed += MainWindowClosed;
            SessionCallbackHandler.OnKicked += OnKickedFromServer;

            var gameTracks = new Dictionary<string, string>
            {
                { "Main", "Assets/AudioGame/Main Theme.mp3" },
                { "Guesser", "Assets/AudioGame/Guesser Theme.mp3" },
                { "Spymaster", "Assets/AudioGame/Spymaster Theme.mp3" },
                { "TieBreaker", "Assets/AudioGame/Tie-breaker Theme.mp3" }
            };

            AudioManager.Instance.LoadTracks(gameTracks);
        }

        private void MainWindowClosed(object sender, EventArgs e)
        {
            try
            {
                SessionOperation.Instance.Disconnect();
                SocialOperation.Instance.Terminate();
                LobbyOperation.Instance.Disconnect();
                MatchmakingOperation.Instance.Disconnect();
                MatchOperation.Instance.Disconnect();
            }
            catch (Exception ex) when (ex is CommunicationException || ex is CommunicationObjectFaultedException || ex is CommunicationObjectFaultedException)
            {
                //Do nothing
            }
        }

        private void OnKickedFromServer(object sender, BanReason reason)
        {
            Dispatcher.Invoke(() =>
            {
                string message = "";
                switch (reason)
                {
                    case BanReason.TemporaryBan:
                        message = Properties.Langs.Lang.kickMessageTemp;
                        break;
                    case BanReason.PermanentBan:
                        message = Properties.Langs.Lang.kickMessagePerm;
                        break;
                    default:
                        message = Properties.Langs.Lang.globalDisconnected;
                        break;
                }

                MessageBox.Show(message, Properties.Langs.Lang.kickTitle, MessageBoxButton.OK, MessageBoxImage.Stop);

                MainFrame.Navigate(new LoginPage());
            });
        }
    }
}
