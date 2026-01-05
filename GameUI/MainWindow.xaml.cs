using CodenamesClient.GameUI.Pages;
using CodenamesClient.Operation.Network.Duplex;
using CodenamesGame.Network.Proxies.CallbackHandlers;
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
            DuplexNetworkManager.Instance.ServerConnectionLost += OnServerConnectionLost;

            var gameTracks = new Dictionary<string, string>
            {
                { "Main", "Assets/AudioGame/Main Theme.mp3" },
                { "Guesser", "Assets/AudioGame/Guesser Theme.mp3" },
                { "Spymaster", "Assets/AudioGame/Spymaster Theme.mp3" },
                { "TieBreaker", "Assets/AudioGame/Tie-breaker Theme.mp3" }
            };

            AudioManager.Instance.LoadTracks(gameTracks);
        }

        private void OnServerConnectionLost(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (IsVisible)
                {
                    MessageBox.Show(
                        this,
                        Properties.Langs.Lang.globalDisconnected,
                        Properties.Langs.Lang.connectionLostTitle,
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );

                    NavigateToLogin();
                }
            });
        }

        private void NavigateToLogin()
        {
            DisconnectDuplexServices();

            MainFrame.Navigate(new LoginPage());
            MainFrame.NavigationService.RemoveBackEntry();
        }

        private static void DisconnectDuplexServices()
        {
            DuplexNetworkManager.Instance.DisconnectFromSessionService();
            DuplexNetworkManager.Instance.DisconnectFromFriendService();
            DuplexNetworkManager.Instance.DisconnectFromLobbyService();
            DuplexNetworkManager.Instance.DisconnectFromMatchmakingService();
            DuplexNetworkManager.Instance.DisconnectFromMatchService();
        }

        private void MainWindowClosed(object sender, EventArgs e)
        {
            try
            {
                DisconnectDuplexServices();
            }
            catch (Exception ex) when (ex is CommunicationException || ex is TimeoutException)
            {
                CodenamesGame.Util.CodenamesGameLogger.Log.Debug("Exception on main window closed: ", ex);
            }
            catch (Exception ex)
            {
                CodenamesGame.Util.CodenamesGameLogger.Log.Error("Unexpected exception on main window closed: ", ex);
            }
        }

        private void OnKickedFromServer(object sender, KickReason reason)
        {
            Dispatcher.Invoke(() =>
            {
                string message = "";
                switch (reason)
                {
                    case KickReason.TEMPORARY_BAN:
                        message = Properties.Langs.Lang.kickMessageTemp;
                        break;
                    case KickReason.PERMANTENT_BAN:
                        message = Properties.Langs.Lang.kickMessagePerm;
                        break;
                    case KickReason.DUPLICATE_LOGIN:
                        message = Properties.Langs.Lang.kickMessageDuplicateLogin;
                        break;
                    default:
                        message = Properties.Langs.Lang.globalDisconnected;
                        break;
                }

                MessageBox.Show(this, message, Properties.Langs.Lang.kickTitle, MessageBoxButton.OK, MessageBoxImage.Stop);

                NavigateToLogin();
            });
        }
    }
}
